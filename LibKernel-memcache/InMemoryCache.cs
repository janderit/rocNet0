using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibKernel;

namespace LibKernel_memcache
{
    public class InMemoryCache : KernelRoute, PostProcessHook
    {

        public static void AttachTo(KernelRegistration kernel)
        {
            var cache = new InMemoryCache
                            {
                                GroupId = Guid.NewGuid()
                            };

            kernel.AddHook(cache);
            kernel.Routes.RegisterRoute(cache.GroupId, cache);
            kernel.Routes.RegisterResourceHandler(cache.GroupId, "net://@cache", 0, r => cache.CacheInfo());
        }

        public Guid GroupId { get; set; }

        private readonly Dictionary<string, ResourceRepresentation> _cache = new Dictionary<string, ResourceRepresentation>();
        private readonly Dictionary<string, string> _alias = new Dictionary<string, string>();

        public long Energy
        {
            get { return 1; }
        }

        public int MaxResourcesInCache { get; set; }
        public long MaxCacheSize { get; set; }
        public long MaxCacheDurationSeconds { get; set; }
        public long MinCachableEnergy { get; set; }
        public long MaxCachableSize { get; set; }

        public bool Match(string nri)
        {
            return _cache.ContainsKey(nri) || _alias.ContainsKey(nri);
        }

        public Func<Request, ResourceRepresentation> Handler
        {
            get { return r => _cache[Dealias(r.NetResourceLocator)]; }
        }

        private ResourceRepresentation CacheInfo()
        {
            return new ResourceRepresentation
                       {
                           Body=
                           "rocNet kerlen memcache\r\n"
                           +_cache.Count+" resources cached \r\n"
                           +_alias.Count+" aliases registered\r\n"
                           + "? cached bytes\r\n\r\n"
                           + "? cache energy value\r\n\r\n"
                           + "? revokation tokens\r\n\r\n"
                           + "HitRate: -not evaluated- \r\n"
                           + "EnergySaved: -not evaluated- \r\n"
                           + "Revocations: -not evaluated- \r\n\r\n"
                           + "MaxCacheItems: "+MaxResourcesInCache+" \r\n"
                           + "MaxCacheSize: " + MaxCacheSize + " \r\n"
                           + "MinCachableEnergy: " + MinCachableEnergy + " \r\n"
                           + "MaxCachableSize: " + MaxCachableSize + " \r\n"
                           + "MaxCacheDurationSeconds: " + MaxCacheDurationSeconds + " \r\n"
                           
                           ,
                           Cacheable=false,
                           Energy=1,
                           MediaType="text/plain",
                           NetResourceIdentifier = "net://@cache"                           
                       };
        }

        private string Dealias(string netResourceLocator)
        {
            return _alias.ContainsKey(netResourceLocator) ? _alias[netResourceLocator] : netResourceLocator;
        }

        public Response PostProcess(Request request, Response response)
        {
            if (response.Status!=ResponseCode.Ok) return response;

            if (_cache.ContainsKey(response.Resource.NetResourceIdentifier))
            {
                if (request.NetResourceLocator != response.Resource.NetResourceIdentifier) if (!_alias.ContainsKey(request.NetResourceLocator)) _alias.Add(request.NetResourceLocator, response.Resource.NetResourceIdentifier);
                return response;
            }

            if (response.Resource.Cacheable)
            {
                var cacheitem = new ResourceRepresentation()
                                    {
                                        Body = response.Resource.Body,
                                        Cacheable=true,
                                        Correlations = (response.Resource.Correlations ?? new List<Guid>()).ToList(),
                                        Energy=response.Resource.Energy,
                                        Expires = response.Resource.Expires,
                                        Headers = (response.Resource.Headers ?? new List<string>()).Union(new[] { "X-Cache: HIT" }).ToList(),
                                        MediaType = response.Resource.MediaType,
                                        Modified=response.Resource.Modified,
                                        NetResourceIdentifier=response.Resource.NetResourceIdentifier,
                                        Relations = (response.Resource.Relations ?? new List<string>()),
                                        RevokationTokens = (response.Resource.RevokationTokens ?? new List<Guid>()).ToList(),
                                        Size=response.Resource.Size,
                                        Via = (response.Resource.Via ?? new List<string>()).Union(new[] { "rocNet-memcache" }).ToList()
                                    };

                _cache.Add(response.Resource.NetResourceIdentifier, cacheitem);

                if (request.NetResourceLocator != response.Resource.NetResourceIdentifier) if (!_alias.ContainsKey(request.NetResourceLocator)) _alias.Add(request.NetResourceLocator, response.Resource.NetResourceIdentifier);

                response.Resource.Headers = (response.Resource.Headers??new List<string>()).Union(new[] { "X-Cache: MISS,CACHED" }).ToList();
            }
            else
            {
                response.Resource.Headers = (response.Resource.Headers ?? new List<string>()).Union(new[] { "X-Cache: MISS,IGNORED" }).ToList();
            }
            return response;
        }
    }
}
