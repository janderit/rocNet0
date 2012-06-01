using System;
using System.Collections.Generic;
using System.Linq;

namespace LibKernel.Provider
{
    public class MultithreadedResourceProviderFacade
    {

        public long InThreadHandlingLimit { get; set; }

        private readonly ResourceProvider _provider;
        private readonly List<AsyncRequestHandler> _pending = new List<AsyncRequestHandler>();

        public MultithreadedResourceProviderFacade(ResourceProvider provider)
        {
            _provider = provider;
        }

        public int Count
        {
            get { return _pending.Count; }
        }

        private void StartAsyncRequest(Request request, Action<Response> onresponse)
        {
            _pending.Add(new AsyncRequestHandler(_provider, request, onresponse).Start());
        }

        public void SynchronizedDeliverResponses()
        {
            var pending = _pending.Where(_ => _.Done).ToList();
            foreach (var requestHandler in pending)
            {
                _pending.Remove(requestHandler);
                requestHandler.SyncFinish();
            }

            UpdateStatistics();
        }

        private long _requests = 0;

        public void GetAndContinue(Request request, Action<Response> reply)
        {
            _requests++;

            _provider.InformLag(DateTime.Now - request.Timestamp);

            if (_provider.Estimate(request) < InThreadHandlingLimit)
                reply(_provider.Get(request));
            else
                StartAsyncRequest(request, reply);
        }

        private void UpdateStatistics()
        {
            _provider.InformRequestNumber(_requests);
            _provider.InformQueue(_pending.Count);
            
        }
        
    }
}
