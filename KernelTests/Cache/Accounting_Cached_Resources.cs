using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibKernel;
using LibKernel_memcache;
using NUnit.Framework;

namespace KernelTests.Cache
{
    [TestFixture]
    class Accounting_Cached_Resources
    {
        private ResourceCacheKernelAdapter _cacheKernelAdapter;

        [SetUp]
        public void StartCache()
        {
            _cacheKernelAdapter = new ResourceCacheKernelAdapter(new SingleThreadedInMemoryCache());
        }

        [Test, Category("Unit")]
        public void Test()
        {
            var nri = "net://" + Guid.NewGuid();

            Assert.IsFalse(_cacheKernelAdapter.Match(nri));
            Assert.AreEqual(0, _cacheKernelAdapter.Statistics.ResourcesCached);
            Assert.AreEqual(0, _cacheKernelAdapter.Statistics.CacheSize);
            Assert.AreEqual(0, _cacheKernelAdapter.Statistics.CachedEnergyValue);

            var req = new Request { NetResourceLocator = nri };
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
                    NetResourceIdentifier = nri,
                    Size = 4
                }
            };

            _cacheKernelAdapter.PostProcess(req, rep);


            
            Assert.IsTrue(_cacheKernelAdapter.Match(nri));
            Assert.AreEqual(1, _cacheKernelAdapter.Statistics.ResourcesCached);
            Assert.AreEqual(rep.Resource.Size, _cacheKernelAdapter.Statistics.CacheSize);
            Assert.AreEqual(rep.Resource.Energy, _cacheKernelAdapter.Statistics.CachedEnergyValue);
            
        }

    }
}
