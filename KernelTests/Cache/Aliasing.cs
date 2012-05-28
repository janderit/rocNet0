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
    class Aliasing
    {

        private InMemoryCache _cache;

        [SetUp]
        public void StartCache()
        {
            _cache = new InMemoryCache();
        }

        [Test]
        public void Cache_noncanonical_nrl()
        {
            var nri = "net://" + Guid.NewGuid();
            var nrl = "net://" + Guid.NewGuid();
            
            Assert.IsFalse(_cache.Match(nri));
            Assert.IsFalse(_cache.Match(nrl));
            Assert.AreEqual(0, _cache.ResourcesCached);

            var req = new Request { NetResourceLocator = nrl };
            var req2 = new Request { NetResourceLocator = nri };
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
            Assert.IsTrue(_cache.Match(nrl));
            Assert.AreEqual(1, _cache.ResourcesCached);
            var a = _cache.Handler(req);
            var b = _cache.Handler(req2);
            Assert.IsNotNull(a);
            Assert.AreSame(a,b);
        }
    }
}
