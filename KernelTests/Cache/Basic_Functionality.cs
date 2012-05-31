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
    class Basic_Functionality
    {
        private ResourceCacheKernelAdapter _cacheKernelAdapter;

        [SetUp]
        public void StartCache()
        {
            _cacheKernelAdapter = new ResourceCacheKernelAdapter(new SingleThreadedInMemoryCache());
        }

        [Test]
        public void Does_not_deliver_non_cached_item()
        {
            Assert.IsFalse(_cacheKernelAdapter.Match(Guid.NewGuid().ToString(), false));
        }


        [Test]
        public void Cache_unknown_resource()
        {
            var nri = "net://" + Guid.NewGuid();

            Assert.IsFalse(_cacheKernelAdapter.Match(nri,false));

            var req = new Request {NetResourceLocator=nri};
            var rep = new Response
                          {
                              Status = ResponseCode.Ok,
                              Resource = new ResourceRepresentation
                                             {
                                                 Body="TEST",
                                                 Cacheable=true,
                                                 Energy=100000,
                                                 Expires=DateTime.MaxValue,
                                                 MediaType="text/plain",
                                                 NetResourceIdentifier=nri,
                                                 Size=4
                                             }
                          };

            _cacheKernelAdapter.PostProcess(req, rep);


            Assert.IsTrue(_cacheKernelAdapter.Match(nri, false));
            Assert.AreEqual(1, _cacheKernelAdapter.Statistics.ResourcesCached);
            var a = _cacheKernelAdapter.Handler(req);
            Assert.IsNotNull(a);
        }

        [Test]
        public void Dont_cache_twice()
        {
            var nri = "net://" + Guid.NewGuid();

            Assert.IsFalse(_cacheKernelAdapter.Match(nri,false));

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


            Assert.IsTrue(_cacheKernelAdapter.Match(nri, false));
            Assert.AreEqual(1, _cacheKernelAdapter.Statistics.ResourcesCached);
            var a = _cacheKernelAdapter.Handler(req);
            Assert.IsNotNull(a);

            _cacheKernelAdapter.PostProcess(req, rep);

            Assert.IsTrue(_cacheKernelAdapter.Match(nri, false));
            Assert.AreEqual(1, _cacheKernelAdapter.Statistics.ResourcesCached);
            var b = _cacheKernelAdapter.Handler(req);
            Assert.IsNotNull(b);
        }

        [Test]
        public void Cache_returns_copy_of_resource()
        {
            var nri = "net://" + Guid.NewGuid();

            Assert.IsFalse(_cacheKernelAdapter.Match(nri,false));

            var original = new ResourceRepresentation
                               {
                                   Body = "TEST",
                                   Cacheable = true,
                                   Energy = 100000,
                                   Expires = DateTime.MaxValue,
                                   MediaType = "text/plain",
                                   NetResourceIdentifier = nri,
                                   Size = 4
                               };

            var req = new Request { NetResourceLocator = nri };
            var rep = new Response
            {
                Status = ResponseCode.Ok,
                Resource = original
            };

            _cacheKernelAdapter.PostProcess(req, rep);


            Assert.IsTrue(_cacheKernelAdapter.Match(nri, false));
            Assert.AreEqual(1, _cacheKernelAdapter.Statistics.ResourcesCached);

            var cached = _cacheKernelAdapter.Handler(req);

            Assert.IsNotNull(cached);

            Assert.AreEqual(original.Body, cached.Body);
            Assert.AreEqual(original.NetResourceIdentifier, cached.NetResourceIdentifier);
            Assert.AreEqual(original.Cacheable, cached.Cacheable);
            Assert.AreEqual(original.Energy, cached.Energy);
            Assert.AreEqual(original.Expires, cached.Expires);
            Assert.AreEqual(original.MediaType, cached.MediaType);
            Assert.AreEqual(original.Modified, cached.Modified);
            Assert.AreEqual(original.Size, cached.Size);

            original.Body = "MODIFIED";
            Assert.AreNotEqual(original.Body, cached.Body);

        }

    }
}
