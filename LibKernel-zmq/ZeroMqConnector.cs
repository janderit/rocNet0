using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ZMQ;

namespace LibKernel_zmq
{
    class ZeroMqConnector : IDisposable
    {
        private readonly string _uri;
        private readonly bool _singletonSocket;
        private static Context _context;
        private Socket _control;

        public int TimeoutMicroseconds { get; set; }

        static ZeroMqConnector()
        {
            _context = new Context();
        }

        public ZeroMqConnector(string uri, bool singletonSocket = false)
        {

            TimeoutMicroseconds = 5000000;

            _uri = uri;
            _singletonSocket = singletonSocket;
            
            if (singletonSocket)
            {
                _control = _context.Socket(SocketType.REQ);
                _control.SndBuf= 65535;
                _control.RcvBuf = 65535;
                _control.Connect(_uri);
            }
        }

        private bool _throwOnTimeout=true;
        public bool ThrowOnTimeout
        {
            get { return _throwOnTimeout; }
            set { _throwOnTimeout = value; }
        }

        public IEnumerable<string> Transact1B0B(byte[] id, IEnumerable<string> message)
        {
            try
            {
                Func<Socket> create = () =>
                {
                    var c = _context.Socket(SocketType.REQ);
                    c.SndBuf = 65535;
                    c.RcvBuf = 65535;
                    c.Connect(_uri);
                    return c;
                };

                var control = _singletonSocket ? _control : create();

                List<string> response = null;

                var handle = new PollHandler((s, m) => response = s.RecvAll(Encoding.UTF8).ToList());

                control.PollInHandler += handle;

                if (control.SendMore(id) != SendStatus.Sent) throw new ApplicationException("Transmission error");
                if (control.SendMore(new byte[0]) != SendStatus.Sent) throw new ApplicationException("Transmission error");
                message.Take(message.Count() - 1).ToList().ForEach(line => { if (control.SendMore(line, Encoding.UTF8) != SendStatus.Sent) throw new ApplicationException("Transmission error"); });

                if (control.Send(message.Last(), Encoding.UTF8) != SendStatus.Sent) return null;

                Context.Poller(TimeoutMicroseconds, control);
                control.PollInHandler -= handle;

                if (!_singletonSocket) control.Dispose();

                if (response == null)
                {
                    if (ThrowOnTimeout)
                        throw new TimeoutException(string.Format("Endpoint {0} did not reply within {1} ms.", _uri,
                                                                 (TimeoutMicroseconds / 1000)));
                    else return null;
                }
                return response;
            }
            catch (SEHException ex)
            {
                Console.WriteLine("SEH Exception" + ex.StackTrace);
                return null;
            }
        }

        public Tuple<byte[], IEnumerable<string>> Transact0B1B(IEnumerable<string> message)
        {
            try
            {
                Func<Socket> create = () =>
                {
                    var c = _context.Socket(SocketType.REQ);
                    c.SndBuf = 65535;
                    c.RcvBuf = 65535;
                    c.Connect(_uri);
                    return c;
                };

                var control = _singletonSocket ? _control : create();

                byte[] response1 = null;
                List<string> response2 = null;

                var handle = new PollHandler((s, m) =>
                                                 {
                                                     response1 = s.Recv();
                                                     s.Recv(); // throw away empty line
                                                     response2 = s.RecvAll(Encoding.UTF8).ToList();
                                                 });

                control.PollInHandler += handle;

                message.Take(message.Count() - 1).ToList().ForEach(line => { if (control.SendMore(line, Encoding.UTF8) != SendStatus.Sent) throw new ApplicationException("Transmission error"); });

                if (control.Send(message.Last(), Encoding.UTF8) != SendStatus.Sent) return null;

                Context.Poller(TimeoutMicroseconds, control);
                control.PollInHandler -= handle;

                if (!_singletonSocket) control.Dispose();

                if (response1 == null)
                {
                    if (ThrowOnTimeout)
                        throw new TimeoutException(string.Format("Endpoint {0} did not reply within {1} ms.", _uri,
                                                                 (TimeoutMicroseconds / 1000)));
                    else return null;
                }
                return new Tuple<byte[], IEnumerable<string>>(response1, response2);
            }
            catch (SEHException ex)
            {
                Console.WriteLine("SEH Exception" + ex.StackTrace);
                return null;
            }
        }

        public IEnumerable<string> Transact(IEnumerable<string> message)
        {
            try
            {
                Func<Socket> create = () =>
                                          {
                                              var c = _context.Socket(SocketType.REQ);
                                              c.SndBuf = 65535;
                                              c.RcvBuf = 65535;
                                              c.Connect(_uri);
                                              return c;
                                          };

                var control = _singletonSocket ? _control : create();

                List<string> response = null;

                var handle = new PollHandler((s, m) => response = s.RecvAll(Encoding.UTF8).ToList());

                control.PollInHandler += handle;

                message.Take(message.Count() - 1).ToList().ForEach(line => { if (control.SendMore(line, Encoding.UTF8)!=SendStatus.Sent) throw new ApplicationException("Transmission error"); });

                if (control.Send(message.Last(), Encoding.UTF8) != SendStatus.Sent) return null;

                Context.Poller(TimeoutMicroseconds, control);
                control.PollInHandler -= handle;

                if (!_singletonSocket) control.Dispose();

                if (response == null)
                {
                    if (ThrowOnTimeout)
                        throw new TimeoutException(string.Format("Endpoint {0} did not reply within {1} ms.", _uri,
                                                                 (TimeoutMicroseconds / 1000)));
                    else return null;
                }
                return response;
            }
            catch (SEHException ex)
            {
                Console.WriteLine("SEH Exception" +ex.StackTrace);
                return null;
            }
        }

        public void Dispose()
        {
            if (_singletonSocket)
            {
                var c = _control;
                if (c!=null) c.Dispose();
            }
        }

        public void ResetSingleton()
        {
            if (_singletonSocket)
            {
                var c = _control;

                var n = _context.Socket(SocketType.REQ);
                n.SndBuf = 65535;
                n.RcvBuf = 65535;
                n.Connect(_uri);

                _control = n;

                if (c != null) c.Dispose();
            }
        }
    }
}
