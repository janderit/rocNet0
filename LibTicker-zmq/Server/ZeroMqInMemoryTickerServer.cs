using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using LibTicker;
using LibTicker.Clients;
using LibTicker.Server;
using ZMQ;

namespace LibTicker_zmq.Server
{
    public class ZeroMqInMemoryTickerServer
    {
        private readonly string _postUri;
        private readonly string _pubsubUri;
        private readonly string _outofbandUri;
        private readonly Action<string> _log;
        private readonly string _uri;
        private Context _ctx;
        private Socket _post;
        private Socket _pubsub;
        private Socket _outofband;
        private TickerPublisher _sink;
        private IDisposable _observer;
        private Thread _thread;
        private bool _terminated = false;

        public ZeroMqInMemoryTickerServer(string postUri, string pubsubUri, string outofbandUri, Action<string> log)
        {
            _postUri = postUri;
            _pubsubUri = pubsubUri;
            _outofbandUri = outofbandUri;
            _log = log;

            _ctx = new Context();
            _post = _ctx.Socket(SocketType.REP);
            _pubsub = _ctx.Socket(SocketType.PUB);
            _outofband = _ctx.Socket(SocketType.REP);

            _sink = new LoopbackPublisher();
            _observer = new LoopbackListener().AsObservable().Subscribe(Publish);

            _post.Bind(postUri);
            _pubsub.Bind(pubsubUri);
            _outofband.Bind(outofbandUri);

            _post.PollInHandler += OnPost;
            _outofband.PollInHandler += OnRequest;

            log("TickerServer (0mq) listening on " + postUri + " & " + outofbandUri + " & serving on " + pubsubUri);

        }

        private void Publish(Tick tick)
        {
            _log("Publish "+tick.Serial);
            var pubsub = _pubsub;
            if (pubsub == null) return;

//            Trace.WriteLine("0MQ TICK XX " + tick.Serial + " " + tick.Subject + " " + tick.Trigger);

            Send(pubsub, Encode(tick));
        }

        private static void Send(Socket socket, IEnumerable<string> lines)
        {
            foreach (var line in lines.Take(lines.Count()-1)) socket.SendMore(line, Encoding.UTF8);
            socket.Send(lines.Last(), Encoding.UTF8);
        }

        private void OnRequest(Socket socket, IOMultiPlex revents)
        {
            var message = socket.RecvAll(Encoding.UTF8).ToArray();
            if (message.First()!="RECALL") return;

            var from = Int64.Parse(message[1]);
            var to = message[2] == "ALL" ? Int64.MaxValue : Int64.Parse(message[2]);

            var ticks = LoopbackDevice.Retrieve(from, to);

            Send(socket, Encode(ticks));
        }

        private static IEnumerable<string> Encode(IEnumerable<Tick> ticks)
        {
            foreach (var tick in ticks) foreach (var line in Encode(tick)) yield return line;
        }

        private static IEnumerable<string> Encode(Tick tick)
        {
            yield return tick.Trigger + " " + tick.Subject;
            yield return tick.Serial.ToString();
            yield return tick.Publication.ToShortDateString() + " " + tick.Publication.ToLongTimeString();
            yield return tick.Data;
        }

        private void OnPost(Socket socket, IOMultiPlex revents)
        {
            var message = socket.RecvAll(Encoding.UTF8).ToArray();
            if (message[0] != "POST")
            {
                _log("Bad request [" + message[0] + "]");
                socket.Send("400 Bad Request", Encoding.UTF8);
                return;
            }
            _log("POST from " + message[1]);
            _sink.Publish(new Guid(message[2]), new Guid(message[3]), message[4]);
            socket.Send("202 ACCEPTED", Encoding.UTF8);
        }

        public ZeroMqInMemoryTickerServer Start()
        {
            _thread = new Thread(Worker);
            _thread.Start();
            return this;
        }

        private void Worker()
        {
            Thread.CurrentThread.IsBackground = true;
            _log("Worker running");
            while (!_terminated) Context.Poller(new List<Socket> {_post, _outofband}.ToArray(), 10000);
            _log("Worker terminated");
        }

        public void Shutdown()
        {
            _terminated = true;

            Thread.Sleep(400);

            var o = _observer;
            _observer = null;
            if (o!=null) o.Dispose();

            _post.Dispose();
            _pubsub.Dispose();
            _outofband.Dispose();

            _pubsub = null;

            _ctx.Dispose();
            _log("Shutdown");
        }

    }
}
