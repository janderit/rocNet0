using System;

namespace LibTicker
{
    public interface TickerPublisher
    {
        bool Publish(Guid subject, Guid trigger, string data);
    }
}