using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibKernel;
using LibKernel.MediaFormats;
using ZMQ;
using Exception = System.Exception;

namespace LibKernel_zmq
{
    public class ZeroMqResourceProviderFacade
    {
        private readonly ResourceProvider _kernel;
        private readonly string _serverUri;
        private static Context _context;
        private Socket _socket;
        private ZeroMqDatagramFormatter _formatter;
        private Thread _worker;
        private Barrier _barrier;

        static ZeroMqResourceProviderFacade()
        {
            _context = new Context();
        }


        public int IdleCheckInterval_ms = 250;
        private readonly Subject<long> _idleTick = new Subject<long>();

        public IObservable<long> OnIdle()
        {
            return _idleTick;
        }

        public ZeroMqResourceProviderFacade(ResourceProvider kernel, string serverUri)
        {
            if (kernel == null) throw new ArgumentNullException("kernel");
            if (serverUri == null) throw new ArgumentNullException("serverUri");
            _kernel = kernel;
            _serverUri = serverUri;
            _formatter = new ZeroMqDatagramFormatter();
        }

        public void Start()
        {
            _barrier = new Barrier(2);
            _worker = new Thread(Worker);
            _worker.Start();
            _barrier.SignalAndWait();
            _barrier.Dispose();
            _barrier = null;
        }
        
        public void Stop()
        {
            TerminateWorker();

            if (_socket == null) return;
            _socket.PollInHandler -= Handler;
            _worker = null;
            _socket.Dispose();
            _socket = null;
        }

        private void TerminateWorker()
        {
            _barrier = new Barrier(2);
            terminate = true;
            _barrier.SignalAndWait();
            _barrier.Dispose();
            _barrier = null;
        }

        private long requests = 0;

        class RequestHandler
        {
            public byte[] Id;
            public Request Request;
            public Response Response;
            public bool Done;
            public DateTime Started;
            public Socket Socket;
        }

        private List<RequestHandler> _pending = new List<RequestHandler>();

        public long InThreadHandlingLimit { get; set; }

        private void Handler(Socket socket, IOMultiPlex revents)
        {
            try
            {
                requests++;
                var id = socket.Recv();
                var empty = socket.Recv();
                var request = _formatter.DeserializeRequest(socket.RecvAll(Encoding.UTF8));
                _kernel.InformRequestNumber(requests);
                _kernel.InformQueue(_pending.Count);
                _kernel.InformLag(DateTime.Now - request.Timestamp);

                var energy = _kernel.Estimate(request);

                if ((energy) < InThreadHandlingLimit)
                {
                    var response = _kernel.Get(request);
                    socket.SendDatagram(id, _formatter.Serialize(response));
                }
                else
                {
                    var r = new RequestHandler { Id=id, Done=false, Request=request,Started=DateTime.Now,Socket=socket };
                    _pending.Add(r);

                    Action worker = () =>
                                        {
                                            var response = _kernel.Get(request);
                                            r.Response = response;
                                            r.Done = true;
                                        };

                    Task.Factory.StartNew(worker);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void SendPendingResults()
        {
            var pending = _pending.Where(_ => _.Done).ToList();
            foreach (var requestHandler in pending)
            {
                requestHandler.Socket.SendDatagram(requestHandler.Id, _formatter.Serialize(requestHandler.Response));
                _pending.Remove(requestHandler);
            }
        }


        private volatile bool terminate = false;

        private void Worker()
        {
            try
            {
                _context = new Context();
                _socket = _context.Socket(SocketType.ROUTER);
                _socket.PollInHandler += Handler;
                _socket.PollErrHandler += Error;
                _socket.Bind(_serverUri);

                Thread.CurrentThread.IsBackground = true;
                _barrier.SignalAndWait();

                while (!terminate)
                {
                    Context.Poller(new List<Socket> { _socket }.ToArray(), IdleCheckInterval_ms * 1000);
                    _idleTick.OnNext(Environment.TickCount);
                    _kernel.InformRequestNumber(requests);
                    _kernel.InformQueue(_pending.Count);
                    SendPendingResults();
                }

                _idleTick.OnCompleted();
                _barrier.SignalAndWait();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor= ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                _idleTick.OnError(new Exception());
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
            }

        }

        
        private void Error(Socket socket, IOMultiPlex revents)
        {
            Console.WriteLine("ERROR");
            Console.ReadLine();
        }
    }
}
