using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
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

        private void Handler(Socket socket, IOMultiPlex revents)
        {
            try
            {
                requests++;
                var request = _formatter.DeserializeRequest(socket.RecvAll(Encoding.UTF8));
                _kernel.InformQueue(requests);
                var response = _kernel.Get(request);
                socket.SendDatagram(_formatter.Serialize(response));
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private volatile bool terminate = false;

        private void Worker()
        {
            try
            {
                _context = new Context();
                _socket = _context.Socket(SocketType.REP);
                _socket.PollInHandler += Handler;
                _socket.PollErrHandler += Error;
                _socket.Bind(_serverUri);

                Thread.CurrentThread.IsBackground = true;
                _barrier.SignalAndWait();

                while (!terminate)
                {
                    Context.Poller(new List<Socket> { _socket }.ToArray(), IdleCheckInterval_ms * 1000);
                    _idleTick.OnNext(Environment.TickCount);
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
