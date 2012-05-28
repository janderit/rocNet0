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
        private InMemoryCache _cache;

        [SetUp]
        public void StartCache()
        {
            _cache = new InMemoryCache();
        }

        [Test]
        public void Test()
        {
            var nri = "net://" + Guid.NewGuid();

            Assert.IsFalse(_cache.Match(nri));
            Assert.AreEqual(0, _cache.ResourcesCached);
            Assert.AreEqual(0, _cache.CacheSize);
            Assert.AreEqual(0, _cache.CachedEnergyValue);

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

            _cache.PostProcess(req, rep);


            
            Assert.IsTrue(_cache.Match(nri));
            Assert.AreEqual(1, _cache.ResourcesCached);
            Assert.AreEqual(rep.Resource.Size, _cache.CacheSize);
            Assert.AreEqual(rep.Resource.Energy, _cache.CachedEnergyValue);
            
        }

    }
}
