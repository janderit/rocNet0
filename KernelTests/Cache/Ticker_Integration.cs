using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using LibKernel;
using LibKernel_memcache;
using LibTicker;
using LibTicker.Clients;
using LibTicker.Server;
using LibTicker.WellKnown;
using NUnit.Framework;
using rocNet.Kernel;

namespace KernelTests.Cache
{
    [TestFixture]
    public class Ticker_Integration
    {
        private InProcessKernel _kernel;
        private ResourceCacheKernelAdapter _cacheKernelAdapter;
        private ICacheResources _cache;
        private LoopbackPublisher _publisher;
        private LoopbackListener _listener;

        [SetUp]
        public void Start()
        {
            LoopbackDevice.Reset();
            _kernel = new InProcessKernel();
            _cache = new SingleThreadedInMemoryCache();
            _cacheKernelAdapter = ResourceCacheKernelAdapter.AttachTo(_kernel, ResourceCacheKernelAdapter.GenerateFallback(_kernel.Get), _cache);
            _publisher = new LoopbackPublisher();
            _listener = new LoopbackListener();
            _listener.Listener(Trace);
            _cacheTickerAdapter = new CacheTickerAdapter(_listener, _cache.Revoke);
        }

        private void Trace(Tick tick)
        {
//            System.Diagnostics.Trace.WriteLine(tick.Subject+" "+tick.Trigger);
        }

        [TearDown]
        public void Stop()
        {
            _cacheTickerAdapter.Dispose();
            _listener.Dispose();
            _cacheKernelAdapter.Clear();
            _kernel.Reset();
            LoopbackDevice.Reset();
        }


        private readonly string _nri1 = "net://" + Guid.NewGuid();
        private readonly Guid _revocation = Guid.NewGuid();
        private CacheTickerAdapter _cacheTickerAdapter;

        private ResourceRepresentation DeliverResource(string body, Request arg)
        {
            return new ResourceRepresentation
                       {
                           NetResourceIdentifier = _nri1,
                           Body = body,
                           Cacheable = true,
                           Energy = 100,
                           Expires = DateTime.MaxValue,
                           MediaType = "text/plain",
                           Modified = DateTime.Now,
                           RevokationTokens = new[] { _revocation }
                       };
        }

        [Test, Category("Unit")]
        public void Cache_delivers_old_items_if_not_invalidated()
        {
            string body = "Take1";

            _kernel.Routes.RegisterResourceHandler(Guid.NewGuid(), _nri1, 100, true, r=>DeliverResource(body,r));

            var reply1 = _kernel.Get(new Request { NetResourceLocator = _nri1, AcceptableMediaTypes = new[] { "*/*" } });
            Assert.IsFalse(reply1.Resource.Headers.Any(_ => _.Contains("CACHE")));
            Assert.AreEqual("Take1", reply1.Resource.Body);


            body = "Take2";

            Thread.Sleep(350);

            var reply2 = _kernel.Get(new Request { NetResourceLocator = _nri1, AcceptableMediaTypes = new[] { "*/*" } });

            Assert.IsTrue(reply2.Resource.Headers.Any(_ => _.Contains("CACHE")));
            Assert.AreEqual("Take1", reply2.Resource.Body);
        }

        [Test, Category("Integration")]
        public void Tickerintegration_allows_cache_to_invalidate_changed_items()
        {


            string body = "Take1";

            _kernel.Routes.RegisterResourceHandler(Guid.NewGuid(), _nri1, 100, true, r => DeliverResource(body, r));
            

            var reply1 = _kernel.Get(new Request { NetResourceLocator = _nri1, AcceptableMediaTypes = new[] { "*/*" } });
            Assert.IsFalse(reply1.Resource.Headers.Any(_ => _.Contains("CACHE")));
            Assert.AreEqual("Take1", reply1.Resource.Body);

//            System.Diagnostics.Trace.WriteLine("2a");

            body = "Take2";
            Assert.IsTrue(_publisher.Publish(_revocation, Trigger.ResourceChanged, ""), "Failed to publish ticker info");

            
            Thread.Sleep(350);

//            System.Diagnostics.Trace.WriteLine("2b");


            var reply2 = _kernel.Get(new Request { NetResourceLocator = _nri1, AcceptableMediaTypes = new[] { "*/*" } });

//            System.Diagnostics.Trace.WriteLine("2c");

            Assert.IsFalse(reply2.Resource.Headers.Any(_ => _.Contains("CACHE")));
            Assert.AreEqual("Take2", reply2.Resource.Body);
        }

    }
}
