using System;
using System.Diagnostics;
using System.Threading;

namespace LibKernel.Provider
{
    class AsyncRequestHandler
    {
        private readonly Action<Response> _onresponse;
        public Response Response;
        public bool Done;
        public DateTime Started = DateTime.Now;
        private readonly Action _work;
        private int _thread;

        public AsyncRequestHandler(ResourceProvider provider, Request request, Action<Response> onresponse)
        {
            _onresponse = onresponse;
            _work = () => Response = provider.Get(request);
        }

        public AsyncRequestHandler Start()
        {
            _thread = Thread.CurrentThread.ManagedThreadId;
            ThreadPool.QueueUserWorkItem(o =>
                                             {
                                                 _work();
                                                 Done = true;
                                             });
            return this;
        }

        public void SyncFinish()
        {
            Debug.Assert(Done);
            Debug.Assert(null!=_onresponse);
            Debug.Assert(_thread == Thread.CurrentThread.ManagedThreadId);

            _onresponse(Response);
        }

    }
}