using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZMQ;

namespace LibKernel_zmq
{
    static class SocketExtender
    {
        public static void SendDatagram (this Socket socket, IEnumerable<string> datagram, Encoding encoding=null)
        {
            encoding = encoding ?? Encoding.UTF8;
            datagram.Take(datagram.Count()-1).ToList().ForEach(_=>socket.SendMore(_, encoding));
            socket.Send(datagram.Last(), encoding);
        }
    }
}
