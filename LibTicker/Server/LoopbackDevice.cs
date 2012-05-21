using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using LibTicker.Clients;

namespace LibTicker.Server
{
    public static class LoopbackDevice
    {
        private static long _memoryLimit = 1*1024*1024;
        private static long _memory = 0;
        private static long _head = -1;
        private static long _base = 0;
        private static bool _active = false;
        private static readonly object Lock = new object();
        private static readonly ConcurrentQueue<Request> Requests = new ConcurrentQueue<Request>();
        private static readonly ConcurrentDictionary<long, Tick> Ticks = new ConcurrentDictionary<long, Tick>();
        private static Thread _runner;
        internal static readonly Subject<Tick> Tickobserver = new Subject<Tick>();
        private static readonly ManualResetEventSlim Barrier = new ManualResetEventSlim();

        internal static void EnsureActive()
        {
            if (_active) return;
            lock (Lock) DoStart();
        }

        public static void Reset()
        {
            EnsureActive();
            
            Ticks.Clear();

            _memory = 0;
            _head = -1;
            _base = 0;
        }

        private static void DoStart()
        {
            if (_active) return;
            _active = true;
            _runner = new Thread(Worker);
            _runner.Start();
        }

        private static void Worker()
        {
            Thread.CurrentThread.IsBackground = true;
            while (true)
            {
                if (Barrier.IsSet) Barrier.Reset();
                if (Requests.Count > 0)
                {
                    Request request;
                    if (!Requests.TryPeek(out request))
                    {
                        Thread.Yield();
                        break;
                    }

                    var size = 24 + (request.Data ?? "").Length;
                    var tick = new Tick
                                   {
                                       Subject = request.Subject,
                                       Trigger = request.Trigger,
                                       Serial = _head + 1,
                                       Data = request.Data ?? "",
                                       Publication = DateTime.Now
                                   };


                    while (!Ticks.TryAdd(_head + 1, tick)) Thread.Yield();

                    if (!Requests.TryDequeue(out request))
                    {
                        Thread.Yield();
                        if (Requests.IsEmpty) break;
                        Requests.TryDequeue(out request);
                    }

                    _memory += Size(tick);

                    Interlocked.Increment(ref _head);

                    Tickobserver.OnNext(tick);

                    while (_memory > _memoryLimit) Purge();
                }
                else
                {
                    Barrier.Wait(100);
                }
            }
        }

        private static long Size(Tick tick)
        {
            return 80 + tick.Data.Length;
        }

        internal static void Publish(Guid subject, Guid trigger, string data)
        {
            if ((data ?? "").Length > 2048) throw new ArgumentException("Data string length is limited to 2048 characters.");
            var request = new Request {Subject = subject, Trigger = trigger, Data = data ?? ""};
            Requests.Enqueue(request);
            Barrier.Set();
        }

        private static void Purge()
        {
            var b = _base;
            Interlocked.Increment(ref _base);
            Tick removed;
            while (!Ticks.TryRemove(b, out removed)) Thread.Yield();
            _memory -= Size(removed);
        }

        public static IEnumerable<Tick> Retrieve(long startAtSerial, long stopAtSerial)
        {
            return Ticks.Keys.ToList().Where(k => startAtSerial <= k && k <= stopAtSerial).Select(_ => Ticks[_]).ToList();
        }
    }
}
