using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using LibTicker.Server;

namespace LibTicker.Clients
{
    public class LoopbackListener : TickerService, IDisposable
    {

        private Subject<Tick> _observer;
        private readonly List<Guid> _subjects = new List<Guid>();
        private readonly List<Guid> _triggers = new List<Guid>();
        private IDisposable _observation;

        public LoopbackListener()
        {
            LoopbackDevice.EnsureActive();

            var s = new Subject<Tick>();
            s.SubscribeOn(Scheduler.TaskPool);
            LoopbackDevice.Tickobserver.Where(Filter).Subscribe(s);
            _observer=s;
        }

        public TickerService ListenTo(Guid subject)
        {
            _subjects.Add(subject);
            return this;
        }

        public TickerService Filter(Guid trigger)
        {
            _triggers.Add(trigger);
            return this;
        }

        public TickerService RecallFrom(long serial)
        {
            LoopbackDevice.Retrieve(serial, Int64.MaxValue).Where(Filter).ToList().ForEach(_observer.OnNext);

            return this;
        }

        public TickerService Listener(Action<Tick> listener)
        {
            _observation = _observer.ObserveOn(Scheduler.TaskPool).Subscribe(listener);
            return this;
        }

        private bool Filter(Tick tick)
        {
            return (_subjects.Count == 0 || _subjects.Contains(tick.Subject)) &&
                   (_triggers.Count == 0 || _triggers.Contains(tick.Trigger));
        }

        public IObservable<Tick> AsObservable()
        {
            return _observer;
        }

        public void Dispose()
        {
            var oo = _observation;
            _observation = null;
            if (oo!=null) oo.Dispose();


            var o = _observer;
            if (o == null) return;

            _observer = null;
            o.OnCompleted();
            o.Dispose();
        }
    }
}
