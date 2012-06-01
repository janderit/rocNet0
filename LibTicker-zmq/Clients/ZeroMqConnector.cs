using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZMQ;

namespace LibTicker_zmq.Clients
{
    class ZeroMqConnector : IDisposable
    {
        private readonly string _uri;
        private readonly bool _singletonSocket;
        private static readonly Context _context;
        private readonly Socket _control;

        private int CommandTimeoutMicroseconds { get; set; }
        private int QueryTimeoutMicroseconds { get; set; }

        static ZeroMqConnector()
        {
            _context = new Context();
        }

        public ZeroMqConnector(string uri, bool singletonSocket = false)
        {
            CommandTimeoutMicroseconds = 5000000;
            QueryTimeoutMicroseconds = 5000000;

            _uri = uri;
            _singletonSocket = singletonSocket;
            
            if (singletonSocket)
            {
                _control = _context.Socket(SocketType.REQ);
                _control.Connect(_uri);
            }
        }

        public IEnumerable<string> Transact(IEnumerable<string> message)
        {
            Func<Socket> create = () =>
            {
                var c = _context.Socket(SocketType.REQ);
                c.Connect(_uri);
                return c;
            };

            var control = _singletonSocket ? _control : create();

            List<string> response = null;

            var handle = new PollHandler((s, m) => response = s.RecvAll(Encoding.UTF8).ToList());

            control.PollInHandler += handle;

            message.Take(message.Count() - 1).ToList().ForEach(line=>control.SendMore(line, Encoding.UTF8));

            if (control.Send(message.Last(), Encoding.UTF8) != SendStatus.Sent) return null;

            Context.Poller(CommandTimeoutMicroseconds, control);
            control.PollInHandler -= handle;

            if (!_singletonSocket) control.Dispose();

            if (response == null) throw new TimeoutException(string.Format("Endpoint {0} did not reply within {1} ms.", _uri, (CommandTimeoutMicroseconds/1000)));
            return response;
        }

        public void Dispose()
        {
            if (_singletonSocket)
            {
                var c = _control;
                if (c!=null) c.Dispose();
            }
        }
    }
}
