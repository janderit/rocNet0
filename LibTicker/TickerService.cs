using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibTicker
{

    public interface TickerService : IDisposable
    {
        TickerService ListenTo(Guid subject);
        TickerService Filter(Guid trigger);
        TickerService RecallFrom(long serial);
        TickerService Listener(Action<Tick> listener);
        IObservable<Tick> AsObservable();
    }
}
