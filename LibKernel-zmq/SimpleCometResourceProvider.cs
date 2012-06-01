using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LibKernel;
using LibKernel.MediaFormats;
using LibKernel.Provider;

namespace LibKernel_zmq
{
    public class SimpleCometResourceProvider : IDisposable
    {
        private readonly Guid _myid;
        private readonly string _zmqUrl;
        private ZeroMqConnector _conn;
        private readonly Thread _listenthread;
        private readonly Thread _replythread;
        private readonly Barrier _start;
        private readonly Barrier _barrier;
        private bool _kill;
        private readonly ZeroMqDatagramFormatter _formatter;
        private ZeroMqConnector _replyconn;
        private Action<Request, Action<Response>> _get;
        private readonly Action _synctick;


        public SimpleCometResourceProvider(Guid myid, string zmqUrl, ResourceProvider provider)
            : this(myid, zmqUrl)
        {
            if (provider == null) throw new ArgumentNullException("provider");

            _get = (request, reply) => { reply(provider.Get(request)); };

            _start.SignalAndWait();
            _start.Dispose();
        }

        public SimpleCometResourceProvider(Guid myid, string zmqUrl, MultithreadedResourceProviderFacade provider):this(myid, zmqUrl)
        {
            if (provider == null) throw new ArgumentNullException("provider");

            _get = provider.GetAndContinue;
            _synctick = provider.SynchronizedDeliverResponses;

            _start.AddParticipant();
            _replythread = new Thread(Poller);
            _replythread.Start();
            
            _start.SignalAndWait();
            _start.Dispose();
        }

        private SimpleCometResourceProvider(Guid myid, string zmqUrl)
        {
            _myid = myid;
            _zmqUrl = zmqUrl;
            _formatter = new ZeroMqDatagramFormatter();

            _start = new Barrier(2);
            _barrier = new Barrier(3);

            _conn = new ZeroMqConnector(zmqUrl, true) { TimeoutMicroseconds = 1000 * 1000, ThrowOnTimeout = false };
            _listenthread = new Thread(Worker);
            _listenthread.Start();
        }


        private void Poller()
        {
            Thread.CurrentThread.IsBackground = true;
            _replyconn = new ZeroMqConnector(_zmqUrl, true) { TimeoutMicroseconds = 1000 * 1000, ThrowOnTimeout = false };

            _start.SignalAndWait();
            try
            {
                while (!_kill)
                {
                    _synctick();
                    Thread.Sleep(15);
                }
            }
            finally
            {
                _replyconn.Dispose();
                _barrier.SignalAndWait();
            }
        }

        private void Worker()
        {
            _start.SignalAndWait();

            Thread.CurrentThread.IsBackground = true;
            try
            {
                while (!_kill)
                {
                    var datagram = _conn.Transact0B1B(new[] {"@listen", _myid.ToString()});
                    if (datagram != null)
                    {
                        var id = datagram.Item1;
                        var request = _formatter.DeserializeRequest(datagram.Item2);
                        _get(request, r => _replyconn.Transact1B0B(id, _formatter.Serialize(r)));
                    }
                }
            }
            finally
            {
                _barrier.SignalAndWait();
            }
        }

        public void Dispose()
        {
            _kill = true;
            _barrier.SignalAndWait();
            _barrier.Dispose();
            var c = _conn;
            _conn = null;
            if (c!=null) c.Dispose();
        }
    }
}
