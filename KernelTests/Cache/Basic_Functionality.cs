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
        private InMemoryCache _cache;

        [SetUp]
        public void StartCache()
        {
            _cache = new InMemoryCache();
        }

        [Test]
        public void Does_not_deliver_non_cached_item()
        {
            Assert.IsFalse(_cache.Match(Guid.NewGuid().ToString()));
        }


        [Test]
        public void Cache_unknown_resource()
        {
            var nri = "net://" + Guid.NewGuid();

            Assert.IsFalse(_cache.Match(nri));

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

            _cache.PostProcess(req, rep);


            Assert.IsTrue(_cache.Match(nri));
            Assert.AreEqual(1, _cache.ResourcesCached);
            var a = _cache.Handler(req);
            Assert.IsNotNull(a);
        }

        [Test]
        public void Dont_cache_twice()
        {
            var nri = "net://" + Guid.NewGuid();

            Assert.IsFalse(_cache.Match(nri));

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
            var a = _cache.Handler(req);
            Assert.IsNotNull(a);

            _cache.PostProcess(req, rep);

            Assert.IsTrue(_cache.Match(nri));
            Assert.AreEqual(1, _cache.ResourcesCached);
            var b = _cache.Handler(req);
            Assert.IsNotNull(b);
        }

        [Test]
        public void Cache_returns_copy_of_resource()
        {
            var nri = "net://" + Guid.NewGuid();

            Assert.IsFalse(_cache.Match(nri));

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

            _cache.PostProcess(req, rep);


            Assert.IsTrue(_cache.Match(nri));
            Assert.AreEqual(1, _cache.ResourcesCached);

            var cached = _cache.Handler(req);

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
