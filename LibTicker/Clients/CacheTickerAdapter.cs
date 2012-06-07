using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibTicker;
using LibTicker.WellKnown;

namespace LibTicker.Clients
{
    
    public class CacheTickerAdapter : IDisposable
    {
        private Action<Guid> _revoke;
        private IDisposable _disposeOnDone;

        public CacheTickerAdapter(TickerService listener, Action<Guid> revoke)
        {
            _revoke = revoke;
            _disposeOnDone = listener.Filter(Trigger.ResourceChanged).AsObservable().Subscribe(Revoke);
        }

        private void Revoke(Tick tick)
        {
            var revoke = _revoke;
            if (revoke != null) revoke(tick.Subject);
        }

        public void Dispose()
        {
            var dod = _disposeOnDone;
            _disposeOnDone = null;
            if (dod!=null) dod.Dispose();
            _revoke = null;
        }
    }
     
}
