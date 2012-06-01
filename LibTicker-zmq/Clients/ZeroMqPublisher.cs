using System;
using System.Linq;
using LibTicker;

namespace LibTicker_zmq.Clients
{
    public class ZeroMqPublisher : TickerPublisher, IDisposable
    {
        private readonly string _ident;
        private ZeroMqConnector _conn;


        public ZeroMqPublisher(string uri, string ident)
        {
            _ident = ident;
            _conn = new ZeroMqConnector(uri, false);
        }

        public bool Publish(Guid subject, Guid trigger, string data)
        {
            var resp = _conn.Transact(new[] {"POST", _ident ?? "ANONYMOUS", subject.ToString(), trigger.ToString(), data ?? ""}.ToList()).ToList();
            return resp.Count == 1 && (resp.Single() == "200 OK" || resp.Single() == "202 ACCEPTED");
        }

        public void Dispose()
        {
            _conn.Dispose();
            _conn = null;
        }
    }
}
