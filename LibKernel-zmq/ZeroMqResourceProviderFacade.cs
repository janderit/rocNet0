using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using LibKernel;
using LibKernel.MediaFormats;
using LibKernel.Provider;
using ZMQ;
using Exception = System.Exception;

namespace LibKernel_zmq
{
    public class ZeroMqResourceProviderFacade
    {
        private readonly string _serverUri;
        private static Context _context;
        private Socket _socket;
        private readonly ZeroMqDatagramFormatter _formatter;
        private Thread _worker;
        private Barrier _barrier;

        static ZeroMqResourceProviderFacade()
        {
            _context = new Context();
        }

        public ZeroMqResourceProviderFacade EnableProviderRouteScan(ResourceRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            _providerRouteScanEnabled = true;
            return this;
        }

        public int IdleCheckInterval_ms = 10;
        private readonly Subject<long> _idleTick = new Subject<long>();

        public IObservable<long> OnIdle()
        {
            return _idleTick;
        }

        public ZeroMqResourceProviderFacade(MultithreadedResourceProviderFacade facade, string serverUri):this(serverUri)
        {
            if (facade == null) throw new ArgumentNullException("facade");

            _work = facade.GetAndContinue;
            _sendPendingResults = facade.SynchronizedDeliverResponses;
        }

        public ZeroMqResourceProviderFacade(ResourceProvider provider, string serverUri)
            : this(serverUri)
        {
            if (provider == null) throw new ArgumentNullException("provider");

            _work = (request, reply) => { reply(provider.Get(request)); };
            _sendPendingResults = () => { };
        }

        private ZeroMqResourceProviderFacade(string serverUri)
        {
            if (serverUri == null) throw new ArgumentNullException("serverUri");
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
            _terminate = true;
            _barrier.SignalAndWait();
            _barrier.Dispose();
            _barrier = null;
        }

        private List<CometHandler> _comets = new List<CometHandler>();

        private void Handler(Socket socket, IOMultiPlex revents)
        {
            try
            {
                var id = socket.Recv();
                DiscardEmptyLine(socket);
                var datagram = socket.RecvAll(Encoding.UTF8);

                if (_providerRouteScanEnabled && datagram.Count == 2 && datagram.First() == "@listen")
                {
                    _comets.Add(new CometHandler(id, new Guid(datagram.Skip(1).First())));
                    return;
                }

                var request = _formatter.DeserializeRequest(datagram);

                _work(request, response => socket.SendDatagram(id, _formatter.Serialize(response)));
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private readonly Action<Request, Action<Response>> _work;
        private Action _sendPendingResults;

        private static void DiscardEmptyLine(Socket socket)
        {
            socket.Recv();
        }

        private volatile bool _terminate = false;
        private bool _providerRouteScanEnabled;

        private void Worker()
        {
            try
            {
                _context = new Context();
                _socket = _context.Socket(SocketType.ROUTER);
                _socket.PollInHandler += Handler;
                _socket.Bind(_serverUri);

                Thread.CurrentThread.IsBackground = true;
                _barrier.SignalAndWait();

                while (!_terminate)
                {
                    Context.Poller(new List<Socket> { _socket }.ToArray(), IdleCheckInterval_ms * 1000);
                    _idleTick.OnNext(Environment.TickCount);

                    _sendPendingResults();
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

    }
}
