﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LibKernel;
using LibKernel_memcache;
using NUnit.Framework;

namespace KernelTests.Cache
{
    [TestFixture]
    class Invalidation
    {
        private ResourceCacheKernelAdapter _cacheKernelAdapter;
        private ICacheResources _backend;

        [SetUp]
        public void StartCache()
        {
            _backend = new SingleThreadedInMemoryCache();
            _cacheKernelAdapter = new ResourceCacheKernelAdapter(_backend);
        }

        [Test, Category("Smoke")]
        public void Clear_cache()
        {
            AssertCacheEmpty();
            FillCache();
            _cacheKernelAdapter.Clear();
            AssertCacheEmpty();
        }

        [Test, Category("Unit")]
        public void Invalidate_single_resource()
        {
            AssertCacheEmpty();
            FillCache();
            _cacheKernelAdapter.Revoke(revocation);
            Assert.IsFalse(_cacheKernelAdapter.Match(nri1));
            Assert.IsTrue(_cacheKernelAdapter.Match(nri2));
            Assert.AreEqual(1, _cacheKernelAdapter.Statistics.ResourcesCached);
            Assert.AreEqual(10, _cacheKernelAdapter.Statistics.CacheSize);
            Assert.AreEqual(50000, _cacheKernelAdapter.Statistics.CachedEnergyValue);
        }

        private string nri1 = "net://" + Guid.NewGuid();
        private Guid revocation = Guid.NewGuid();
        private string nri2 = "net://" + Guid.NewGuid();
        

        private void FillCache()
        {
            var req = new Request { NetResourceLocator = nri1 };
            var rep = new Response
            {
                Status = ResponseCode.Ok,
                Resource = new ResourceRepresentation
                {
                    Body = "TEST",
                    Cacheable = true,
                    Energy = 100000,
                    Expires = DateTime.MaxValue,
                    MediaType = "text/plain",
                    NetResourceIdentifier = nri1,
                    Size = 4,
                    RevokationTokens = new[] { revocation }
                }
            };

            _cacheKernelAdapter.PostProcess(req, rep);

            req = new Request { NetResourceLocator = nri2 };
            rep = new Response
            {
                Status = ResponseCode.Ok,
                Resource = new ResourceRepresentation
                {
                    Body = "TESTCACHED",
                    Cacheable = true,
                    Energy = 50000,
                    Expires = DateTime.MaxValue,
                    MediaType = "text/plain",
                    NetResourceIdentifier = nri2,
                    Size = 10
                }
            };

            _cacheKernelAdapter.PostProcess(req, rep);



            Assert.IsTrue(_cacheKernelAdapter.Match(nri1));
            Assert.IsTrue(_cacheKernelAdapter.Match(nri2));
            Assert.AreEqual(2, _cacheKernelAdapter.Statistics.ResourcesCached);
            Assert.AreEqual(14, _cacheKernelAdapter.Statistics.CacheSize);
            Assert.AreEqual(150000, _cacheKernelAdapter.Statistics.CachedEnergyValue);
        }

        private void AssertCacheEmpty()
        {
            Assert.IsFalse(_cacheKernelAdapter.Match(nri1));
            Assert.IsFalse(_cacheKernelAdapter.Match(nri2));
            Assert.AreEqual(0, _cacheKernelAdapter.Statistics.ResourcesCached);
            Assert.AreEqual(0, _cacheKernelAdapter.Statistics.CacheSize);
            Assert.AreEqual(0, _cacheKernelAdapter.Statistics.CachedEnergyValue);
        }


        [Test, Category("Unit")]
        public void Garbage_collection()
        {
            AssertCacheEmpty();
            var req = new Request { NetResourceLocator = nri1 };
            var rep = new Response
            {
                Status = ResponseCode.Ok,
                Resource = new ResourceRepresentation
                {
                    Body = "TEST",
                    Cacheable = true,
                    Energy = 2000,
                    Expires = DateTime.Now.AddSeconds(2),
                    MediaType = "text/plain",
                    NetResourceIdentifier = nri1,
                    Size = 4,
                    RevokationTokens = new[] { revocation }
                }
            };

            _backend.MinimumExpirationTimesEnergyFactor = 0;
            _cacheKernelAdapter.PostProcess(req, rep);
            Assert.IsTrue(_cacheKernelAdapter.Match(nri1));
            Thread.Sleep(3000);
            _backend.TriggerGarbageCollection();
            AssertCacheEmpty();


        }

    }
}
