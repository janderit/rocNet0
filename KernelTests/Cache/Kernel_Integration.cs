﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibKernel;
using LibKernel_memcache;
using NUnit.Framework;
using rocNet.Kernel;

namespace KernelTests.Cache
{
    [TestFixture]
    public class Kernel_Integration
    {
        private InProcessKernel _kernel;
        private ResourceCacheKernelAdapter _cacheKernelAdapter;
        private ICacheResources _cache;

        [SetUp]
        public void Start()
        {
            _kernel = new InProcessKernel();
            _cache = new SingleThreadedInMemoryCache();
            _cacheKernelAdapter = ResourceCacheKernelAdapter.AttachTo(_kernel, ResourceCacheKernelAdapter.GenerateFallback(_kernel.Get), _cache);
        }

        [TearDown]
        public void Stop()
        {
            _cacheKernelAdapter.Clear();
            _kernel.Reset();
        }


        private string nri1 = "net://" + Guid.NewGuid();

        private ResourceRepresentation DeliverResource(bool cacheable, Request arg)
        {
            return new ResourceRepresentation
            {
                NetResourceIdentifier=nri1,
                Body = "TheBody",
                Cacheable = cacheable,
                Energy = 100,
                Expires = DateTime.MaxValue,
                MediaType = "text/plain"
            };
        }

        [Test, Category("Integration")]
        public void Kernel_does_not_try_to_cache_noncachable_Resources_delivered_by_kernel()
        {

            _kernel.Routes.RegisterResourceHandler(Guid.NewGuid(), nri1, 100, true, r => DeliverResource(false, r));

            var reply1 = _kernel.Get(new Request {NetResourceLocator = nri1, AcceptableMediaTypes = new[] {"*/*"}});
            Assert.AreEqual(XCache.None, reply1.XCache);

            var reply2 = _kernel.Get(new Request {NetResourceLocator = nri1, AcceptableMediaTypes = new[] {"*/*"}});
            Assert.AreEqual(XCache.None, reply2.XCache);
        }
        
        [Test, Category("Integration")]
        public void Cache_caches_cachable_Resources_delivered_by_kernel()
        {

            _kernel.Routes.RegisterResourceHandler(Guid.NewGuid(), nri1, 100, true, r => DeliverResource(true, r));

            var reply1 = _kernel.Get(new Request { NetResourceLocator = nri1, AcceptableMediaTypes = new[] { "*/*" } });
            Assert.AreEqual(XCache.Cached, reply1.XCache);

            var reply2 = _kernel.Get(new Request { NetResourceLocator = nri1, AcceptableMediaTypes = new[] { "*/*" } });
            Assert.AreEqual(XCache.CacheHit, reply2.XCache);

        }

        [Test, Category("Integration")]
        public void Kernel_ignores_cachable_Resources_on_request()
        {

            _kernel.Routes.RegisterResourceHandler(Guid.NewGuid(), nri1, 100, true, r => DeliverResource(true, r));

            var reply1 = _kernel.Get(new Request { NetResourceLocator = nri1, AcceptableMediaTypes = new[] { "*/*" } });
            Assert.AreEqual(XCache.Cached, reply1.XCache);

            var reply2 = _kernel.Get(new Request {NetResourceLocator = nri1, AcceptableMediaTypes = new[] {"*/*"}, IgnoreCached = true});
            Assert.AreEqual(XCache.None, reply2.XCache);

        }

    }


}
