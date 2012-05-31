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

        private ResourceCacheKernelAdapter _cacheKernelAdapter;

        [SetUp]
        public void StartCache()
        {
            _cacheKernelAdapter = new ResourceCacheKernelAdapter(new SingleThreadedInMemoryCache());
        }

        [Test]
        public void Cache_noncanonical_nrl()
        {
            var nri = "net://" + Guid.NewGuid();
            var nrl = "net://" + Guid.NewGuid();
            
            Assert.IsFalse(_cacheKernelAdapter.Match(nri));
            Assert.IsFalse(_cacheKernelAdapter.Match(nrl));
            Assert.AreEqual(0, _cacheKernelAdapter.Statistics.ResourcesCached);

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

            _cacheKernelAdapter.PostProcess(req, rep);
            
            Assert.IsTrue(_cacheKernelAdapter.Match(nri));
            Assert.IsTrue(_cacheKernelAdapter.Match(nrl));
            Assert.AreEqual(1, _cacheKernelAdapter.Statistics.ResourcesCached);
            var a = _cacheKernelAdapter.Handler(req);
            var b = _cacheKernelAdapter.Handler(req2);
            Assert.IsNotNull(a);
            Assert.AreSame(a,b);
        }
    }
}
