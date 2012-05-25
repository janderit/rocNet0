using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LibKernel;
using LibKernel.MediaFormats;
using ZMQ;

namespace LibKernel_zmq
{
    public class ZeroMqKernelFacade
    {
        private readonly Kernel _kernel;
        private readonly string _serverUri;
        private Context _context;
        private Socket _socket;
        private ZeroMqDatagramFormatter _formatter;
        private Thread _worker;
        private Barrier _barrier;

        public ZeroMqKernelFacade(Kernel kernel, string serverUri)
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
            if (_socket == null) return;
            _socket.PollInHandler -= Handler;
            _worker.Abort();
            _worker.Join();
            _worker = null;
            _socket.Dispose();
            _context.Dispose();
            _socket = null;
            _context = null;
        }

        private void Handler(Socket socket, IOMultiPlex revents)
        {
            var request = _formatter.DeserializeRequest(socket.RecvAll(Encoding.UTF8));
            var response = _kernel.Get(request);
            socket.SendDatagram(_formatter.Serialize(response));
        }

        private void Worker()
        {
            _context = new Context();
            _socket = _context.Socket(SocketType.REP);
            _socket.PollInHandler += Handler;
            _socket.Bind(_serverUri);

            Thread.CurrentThread.IsBackground = true;
            _barrier.SignalAndWait();

            while (true)
            {
                Context.Poller(new List<Socket> { _socket }.ToArray(), 250000);
            }
        }

    }
}
