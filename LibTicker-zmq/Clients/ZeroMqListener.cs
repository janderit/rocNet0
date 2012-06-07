using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using LibTicker;
using ZMQ;

namespace LibTicker_zmq.Clients
{
    public class ZeroMqListener : TickerService, IDisposable
    {
        private readonly string _inbandUri;
        private readonly string _outofbandUri;
        private Context _ctx;
        private Subject<Tick> _subject;
        private readonly List<Guid> _subjects = new List<Guid>();
        private readonly List<Guid> _triggers = new List<Guid>();
        private Thread _worker;
        private bool _terminated;
        private bool _recvall=true;
        private IDisposable _trace;

        public ZeroMqListener(string inbandUri, string outofbandUri)
        {
            _inbandUri = inbandUri;
            _outofbandUri = outofbandUri;

            _ctx = new Context();

            _subject = new Subject<Tick>();
            _subject.SubscribeOn(Scheduler.TaskPool);
            _trace = _subject.Subscribe(tick => Trace.WriteLine("0MQ TICK rx " + tick.Serial + " " + tick.Subject + " " + tick.Trigger));

            _worker = new Thread(Worker);
            _worker.Start();
        }

        private void Worker()
        {
            var sock = _ctx.Socket(SocketType.SUB);
            sock.SetSockOpt(SocketOpt.SUBSCRIBE, new byte[] { });
            sock.PollInHandler += OnTick;
            sock.Connect(_inbandUri);

            Thread.CurrentThread.IsBackground = true;
//            Trace.WriteLine("0mq listener >> @ "+_inbandUri);
            while (!_terminated) Context.Poller(new List<Socket> { sock }.ToArray(), 250000);
            //Trace.WriteLine("0mq listener ##");

            sock.Dispose();
        }

        private void OnTick(Socket socket, IOMultiPlex revents)
        {
            var data = socket.RecvAll(Encoding.UTF8).ToArray();
            if (data.Count() != 4) return;

//            Trace.Write("RECV ");
//            Trace.WriteLine(data[2]);

            if (data[0].Length != 73) return;

            DecodeFilterAndPublish(data);

        }


        public TickerService RecallFrom(long serial)
        {
            var conn = new ZeroMqConnector(_outofbandUri);
            var stream = new Queue<string>(conn.Transact(new[] {"RECALL", serial.ToString(), "ALL"}));

            while (stream.Count>0)
            {
                var dat = new List<string> {stream.Dequeue(), stream.Dequeue(), stream.Dequeue(), stream.Dequeue()};
                var data = dat.ToArray();

                DecodeFilterAndPublish(data);
            }

            return this;
        }

        private void DecodeFilterAndPublish(string[] data)
        {
            var trigger = new Guid(data[0].Substring(0, 36));
            var subject = new Guid(data[0].Substring(37, 36));

            var tick = new Tick
                           {
                               Subject = subject,
                               Trigger = trigger,
                               Publication = DateTime.Now,
                               Serial = long.Parse(data[1]),
                               Data = data[3]
                           };

            if (Filter(tick))
            {
//                Trace.WriteLine("0MQ TICK RX " + tick.Serial + " " + tick.Subject + " " + tick.Trigger);
                _subject.OnNext(tick);
            }
        }

        public TickerService Listener(Action<Tick> listener)
        {
            _subject.ObserveOn(Scheduler.TaskPool).Subscribe(listener);
            return this;
        }

        public IObservable<Tick> AsObservable()
        {
            var res = _subject.Publish();
            res.Connect();
            return res;
        }

        public void Dispose()
        {
            _terminated = true;
            _worker.Join(TimeSpan.FromSeconds(10));

            _ctx.Dispose();
            _subject.OnCompleted();
            _trace.Dispose();
            _subject.Dispose();
        }

        public TickerService ListenTo(Guid subject)
        {
            _subjects.Add(subject);
            return this;
        }

        public TickerService Filter(Guid trigger)
        {
            _triggers.Add(trigger);
            if (_recvall)
            {
                _recvall = false; 
            //    _sock.SetSockOpt(SocketOpt.UNSUBSCRIBE, Encoding.UTF8.GetBytes(""));
            }
            //_sock.SetSockOpt(SocketOpt.SUBSCRIBE, Encoding.UTF8.GetBytes(trigger.ToString()));
            return this;
        }

        private bool Filter(Tick tick)
        {
            return (_subjects.Count == 0 || _subjects.Contains(tick.Subject)) &&
                   (_triggers.Count == 0 || _triggers.Contains(tick.Trigger));
        }

    }
}
