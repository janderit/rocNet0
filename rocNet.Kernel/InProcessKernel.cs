using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using LibKernel.Exceptions;

namespace LibKernel
{
    public class InProcessKernel : ResourceProvider, KernelRegistration
    {

        private readonly Router _router;
        private List<PostProcessHook> _hooks = new List<PostProcessHook>();

        public InProcessKernel()
        {
            _router = new Router(this);
            _statthread = new Thread(StatOut);
            _statthread.Start();
        }

        public void AddHook(PostProcessHook hook)
        {
            _hooks.Add(hook);
        }

        private void StatOut()
        {
            try
            {
                Thread.CurrentThread.IsBackground = true;
                bool persecond = false;
                while (true)
                {
                    try
                    {
                        Console.Clear();
                        Console.WriteLine("");
                        Console.WriteLine("InProcess kernel stats");

                        var stat = _stat.ToList();

                        var threads = stat.Select(_ => _.ThreadId).Distinct().Count();

                        var freqs = 0.0;
                        if (stat.Count > 0)
                        {
                            var delta = stat.Last().Facaderequests - stat.First().Facaderequests;
                            freqs = persecond
                                        ? (delta < 1000)
                                              ? delta*60
                                              : (delta/((stat.Max(_ => _.Zeit) - stat.Min(_ => _.Zeit)).TotalMinutes))
                                        : (delta < 1000)
                                              ? delta
                                              : (delta/((stat.Max(_ => _.Zeit) - stat.Min(_ => _.Zeit)).TotalMinutes));
                        }

                        var ops = persecond
                                      ? (stat.Count < 1000)
                                            ? stat.Count*60
                                            : (stat.Count/((stat.Max(_ => _.Zeit) - stat.Min(_ => _.Zeit)).TotalMinutes))
                                      : (stat.Count < 1000)
                                            ? stat.Count
                                            : (stat.Count/((stat.Max(_ => _.Zeit) - stat.Min(_ => _.Zeit)).TotalMinutes))
                            ;

                        var avg = (stat.Count > 0) ? stat.Sum(_ => _.Ticks)/stat.Count : 0;

                        Console.WriteLine();
                        if (ops < 600)
                            Console.WriteLine(" Operations per minute: " + (int) Math.Round(ops));
                        else
                            Console.WriteLine(" Operations per second: " + (int) Math.Round(ops/60.0));

                        if (freqs < 600)
                            Console.WriteLine(" Requests   per minute: " + (int)Math.Round(freqs));
                        else
                            Console.WriteLine(" Requests   per second: " + (int)Math.Round(freqs / 60.0));

                        var last = stat.LastOrDefault();
                        if (last != null)
                            Console.WriteLine(" Last resource access : " + last.nrl + " [" + last.Zeit + "]");

                        Console.WriteLine(" Average duration :     " + avg);
                        Console.WriteLine(" Current msg delay:     " + _lag);
                        Console.WriteLine(" Pending queue    :     " + _queuelength);
                        Console.WriteLine(" Worker threads   :     " + threads);

                        Console.WriteLine(" Last error :           " + LastEx);

                        if (!persecond && stat.Count > 6000) persecond = true;
                        if (persecond && stat.Count < 100) persecond = false;

                        while (_stat.Count > 1000 || PeekSeesOldMessage(persecond))
                        {
                            ReqStat dummy;
                            if (!_stat.TryDequeue(out dummy)) break;
                        }
                        Thread.Sleep(500);
                    }
                    catch (Exception ex)
                    {
                        Console.Clear();
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        Thread.Sleep(500);
                    }
                }
            }
            catch
            {
                Console.WriteLine("stat term");
            }
        }

        private bool PeekSeesOldMessage(bool persecond)
        {
            ReqStat msg;
            if (_stat.TryPeek(out msg))
            {
                if (msg != null && (DateTime.Now - msg.Zeit).TotalSeconds> (persecond?1:60)) return true;
            }
            return false;
        }

        public ResourceRepresentation Get(string nrl)
        {
            var response = Get(new Request {NetResourceLocator = nrl, AcceptableMediaTypes = new[] {@"*\+*"}});
            if (response.Status==ResponseCode.Ok) return response.Resource;

            if (response.Status == ResponseCode.NotFound) throw ResourceNotFoundException.Create(response.Information, "");
            if (response.Status == ResponseCode.InternalError) throw ResourceUnavailableException.Create("Internal error", response.Information);
            throw ResourceUnavailableException.Create("Unknown status code (" + response.Status + ")", response.Information);
        }

        public Response Get(Request request)
        {
            return _hooks.ToList().Aggregate(DoGet(request), (current, postProcessHook) => postProcessHook.PostProcess(request, current));
        }

        public void InformRequestNumber(long backlog)
        {
            _facaderequests = backlog;
        }

        public long Estimate(Request request)
        {
            return 0;
        }

        public void InformLag(TimeSpan delay)
        {
            _lag = (int)Math.Round(delay.TotalMilliseconds);
        }

        public void InformQueue(int count)
        {
            _queuelength = count;
        }

        private ConcurrentQueue<ReqStat> _stat = new ConcurrentQueue<ReqStat>();
        public string LastEx = "-none-";
        private Thread _statthread;
        private long _facaderequests;
        private int _queuelength;
        private int _lag;

        class ReqStat
        {
            public DateTime Zeit;
            public bool ok;
            public string nri;
            public string nrl;
            public string fail;
            public int Start;
            public int Ticks;
            public long Facaderequests;
            public int ThreadId;
        }

        private Response DoGet(Request request)
        {
            var routes = FindRoutes(request).ToList();
            if (routes.Count()==0) return new Response{Status=ResponseCode.NotFound, Information="No route to resource"};

            var s = new ReqStat { nrl = request.NetResourceLocator, Start = Environment.TickCount, Zeit = DateTime.Now, Facaderequests = _facaderequests, ThreadId = Thread.CurrentThread.ManagedThreadId };

            try
            {
                var resp = new Response { Status = ResponseCode.NotFound, Information = "No available route", Resource = null };

                while (routes.Count>0)
                {
                    var res = routes[0].Handler(request);
                    if (res == null)
                    {
                        routes.RemoveAt(0);
                        continue;
                    }
                    resp = new Response { Status = ResponseCode.Ok, Information = "Ok", Resource = res };
                    break;
                }

                if (resp.Status==ResponseCode.Ok)
                {
                    s.ok = true;
                    s.nri = resp.Resource.NetResourceIdentifier;
                }

                s.Ticks = Environment.TickCount - s.Start;

                _stat.Enqueue(s);

                return resp;
            }
            catch (Exception ex)
            {
                s.fail = ex.Message;
                s.ok = false;
                s.Ticks = Environment.TickCount - s.Start;
                _stat.Enqueue(s);
                LastEx = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz ")+ ex.Message;
                return new Response {Status = ResponseCode.InternalError, Information = ex.Message + " " + ex.GetType().FullName};
            }
        }

        private IEnumerable<KernelRoute> FindRoutes(Request request)
        {
            if (!request.NetResourceLocator.Contains("://")) request.NetResourceLocator = "net://" + request.NetResourceLocator;
            return _router.Lookup(request.NetResourceLocator, request.IgnoreCached);
        }

        public ResourceRegistry Routes { get { return _router; } }


        public void Reset()
        {
            _router.Reset();
        }

        
    }
}
