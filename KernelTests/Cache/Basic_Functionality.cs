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

        [Test, Category("Negative")]
        public void Does_not_deliver_non_cached_item()
        {
            Assert.IsFalse(_cacheKernelAdapter.Match(Guid.NewGuid().ToString()));
        }


        [Test, Category("Unit")]
        public void Cache_unknown_resource()
        {
            var nri = "net://" + Guid.NewGuid();

            Assert.IsFalse(_cacheKernelAdapter.Match(nri));

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


            Assert.IsTrue(_cacheKernelAdapter.Match(nri));
            Assert.AreEqual(1, _cacheKernelAdapter.Statistics.ResourcesCached);
            var a = _cacheKernelAdapter.Handler(req);
            Assert.IsNotNull(a);
        }

        [Test, Category("Unit")]
        public void Dont_cache_twice()
        {
            var nri = "net://" + Guid.NewGuid();

            Assert.IsFalse(_cacheKernelAdapter.Match(nri));

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
            var a = _cacheKernelAdapter.Handler(req);
            Assert.IsNotNull(a);

            _cacheKernelAdapter.PostProcess(req, rep);

            Assert.IsTrue(_cacheKernelAdapter.Match(nri));
            Assert.AreEqual(1, _cacheKernelAdapter.Statistics.ResourcesCached);
            var b = _cacheKernelAdapter.Handler(req);
            Assert.IsNotNull(b);
        }

        [Test, Category("Unit")]
        public void Cache_returns_copy_of_resource()
        {
            var nri = "net://" + Guid.NewGuid();

            Assert.IsFalse(_cacheKernelAdapter.Match(nri));

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


            Assert.IsTrue(_cacheKernelAdapter.Match(nri));
            Assert.AreEqual(1, _cacheKernelAdapter.Statistics.ResourcesCached);

            var cached = _cacheKernelAdapter.Handler(req);

            Assert.IsNotNull(cached);
            Assert.IsNotNull(cached.Resource);

            Assert.AreEqual(original.Body, cached.Resource.Body);
            Assert.AreEqual(original.NetResourceIdentifier, cached.Resource.NetResourceIdentifier);
            Assert.AreEqual(original.Cacheable, cached.Resource.Cacheable);
            Assert.AreEqual(original.Energy, cached.Resource.Energy);
            Assert.AreEqual(original.Expires, cached.Resource.Expires);
            Assert.AreEqual(original.MediaType, cached.Resource.MediaType);
            Assert.AreEqual(original.Modified, cached.Resource.Modified);
            Assert.AreEqual(original.Size, cached.Resource.Size);

            original.Body = "MODIFIED";
            Assert.AreNotEqual(original.Body, cached.Resource.Body);

        }

    }
}
