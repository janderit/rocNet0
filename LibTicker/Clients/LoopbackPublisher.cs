using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibTicker.Server;

namespace LibTicker.Clients
{
    public class LoopbackPublisher : TickerPublisher
    {
        public LoopbackPublisher()
        {
            LoopbackDevice.EnsureActive();
        }

        public bool Publish(Guid subject, Guid trigger, string data)
        {
            LoopbackDevice.Publish(subject, trigger, data);
            return true;
        }
    }
}
