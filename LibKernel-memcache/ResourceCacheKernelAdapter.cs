using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibKernel;
using LibKernel.MediaFormats;
using LibKernel.Routing;

namespace LibKernel_memcache
{
    public partial class ResourceCacheKernelAdapter : KernelRoute, PostProcessHook
    {

        private readonly ICacheResources _cache;
        private static KernelRegistration _kernel;
        private static Func<Request, Response> _fallback;

        public ResourceCacheKernelAdapter(ICacheResources cache)
        {
            _cache = cache;
        }

        public static ResourceCacheKernelAdapter AttachTo(KernelRegistration kernel, Func<Request, Response> fallback, ICacheResources backend)
        {
            if (fallback == null) throw new ArgumentNullException("fallback");
            var cache = new ResourceCacheKernelAdapter(backend)
                            {
                                GroupId = Guid.NewGuid()
                            };

            _kernel = kernel;
            _fallback = fallback;

            kernel.AddHook(cache);
            kernel.Routes.RegisterRoute(cache.GroupId, cache);
            kernel.Routes.RegisterResourceHandler(cache.GroupId, "net://@cache", 0,true, r => cache.CacheInfo("json"));
            kernel.Routes.RegisterResourceHandler(cache.GroupId, "net://@cacheids", 0,true, r => cache.CacheIds());
            return cache;
        }

        public Guid GroupId { get; set; }

        
        public long Energy
        {
            get { return 1; }
        }

        public long DeliveryTime
        {
            get { return 1; }
            set { }
        }

        public bool IsAuthoritative
        {
            get { return false; }
        }

        public bool Match(string nri)
        {
            return _cache.Match(nri);
        }
        
        public Func<Request, Response> Handler
        {
            get { return req => _cache.RetrieveOrNull(req.NetResourceLocator) ?? _fallback(req); }
        }
        

        public Response PostProcess(Request request, Response response)
        {
            if (response.Status!=ResponseCode.Ok) return response;

            if (response.Resource.Cacheable && response.XCache!=XCache.Cached && !_cache.Match(response.Resource.NetResourceIdentifier))
            {
                _cache.AddToCache(request, response);
            }

            return response;
        }

        public CacheConfiguration Configuration{get { return _cache.Configuration; }}
        public CacheStatistics Statistics { get { return _cache.Statistics; } }

        

        public void Clear()
        {
            _cache.ClearCache();
        }

        public void Revoke(Guid id)
        {
            _cache.Revoke(id);
        }


        public static Func<Request, Response> GenerateFallback(Func<Request, Response> get)
        {
            return req => get(new Request
                                  {
                                      Timestamp=req.Timestamp,
                                      NetResourceLocator = req.NetResourceLocator,
                                      AcceptableMediaTypes = req.AcceptableMediaTypes.ToList(),
                                      IgnoreCached = true
                                  }).Guard();
        }


        private ResourceRepresentation CacheInfo(string mediaformat)
        {
            return MediaFormatter<CacheInformation>.Pack("net://@cache",
                                                         new CacheInformation
                                                             {Configuration = Configuration, Statistics = Statistics},
                                                         mediaformat, CacheInformation.Mediatype);
        }

        private ResourceRepresentation CacheIds()
        {
            return new ResourceRepresentation
            {
                Body =
                "rocNet kernel memcache\r\n"
                + _cache.CurrentKeys.Aggregate("", (c, k) => c + k + Environment.NewLine),
                Cacheable = false,
                Energy = 1,
                MediaType = "text/plain",
                NetResourceIdentifier = "net://@cache"
            };
        }

    }
}
