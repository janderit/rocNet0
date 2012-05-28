using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibKernel;

namespace LibKernel_memcache
{
    public partial class InMemoryCache : KernelRoute, PostProcessHook
    {

        public InMemoryCache()
        {
            SetCachingStrategyDefaults();
            SetRemovalStrategyDefaults();
        }

        public static InMemoryCache AttachTo(KernelRegistration kernel)
        {
            var cache = new InMemoryCache
                            {
                                GroupId = Guid.NewGuid()
                            };

            kernel.AddHook(cache);
            kernel.Routes.RegisterRoute(cache.GroupId, cache);
            kernel.Routes.RegisterResourceHandler(cache.GroupId, "net://@cache", 0, r => cache.CacheInfo());
            kernel.Routes.RegisterResourceHandler(cache.GroupId, "net://@cacheids", 0, r => cache.CacheIds());
            return cache;
        }

        public Guid GroupId { get; set; }

        private readonly Dictionary<string, string> _alias = new Dictionary<string, string>();

        public long Energy
        {
            get { return 1; }
        }



        private DateTime _lastgc = DateTime.MinValue;
        public TimeSpan GarbageCollectionInterval = TimeSpan.FromSeconds(10);
        public bool Match(string nri)
        {
            if (DateTime.Now > _lastgc + GarbageCollectionInterval)
            {
                _lastgc = DateTime.Now;
                TriggerGarbageCollection();
            }

            var result = _cache.ContainsKey(nri) || _alias.ContainsKey(nri);
            _matchrequests++;
            if (result) _hits++;
            return result;
        }

        public Func<Request, ResourceRepresentation> Handler
        {
            get
            {
                return r =>
                           {
                               var nri = Dealias(r.NetResourceLocator);
                               var result = _cache[nri];
                               _deliveredenergy += result.Energy;
                               _hitlist[nri]++;
                               _lastlist[nri] = DateTime.Now;
                               return result;
                           };
            }
        }

        private string Dealias(string netResourceLocator)
        {
            return _alias.ContainsKey(netResourceLocator) ? _alias[netResourceLocator] : netResourceLocator;
        }

        public Response PostProcess(Request request, Response response)
        {
            if (response.Status!=ResponseCode.Ok) return response;

            if (_cache.ContainsKey(response.Resource.NetResourceIdentifier))
            { // assuming that the response originated in this cache
                if (!IsCanonical(response, request)) AddAliasIfUnknown(response, request);
                return response;
            }

            if (response.Resource.Cacheable && CheckCachingStrategy(response.Resource))
            {
                CheckForRemovals();
                AddToCache(response);

                if (!IsCanonical(response, request)) AddAliasIfUnknown(response, request);
                ModifyHeaderAddCached(response);
            }
            else
            {
                ModifyHeaderAddCacheMiss(response);
            }
            return response;
        }

        private static bool IsCanonical(Response response, Request request)
        {
            return request.NetResourceLocator == response.Resource.NetResourceIdentifier;
        }

        private static void ModifyHeaderAddCacheMiss(Response response)
        {
            response.Resource.Headers = (response.Resource.Headers ?? new List<string>()).Union(new[] { "X-Cache: MISS,IGNORED" }).ToList();
        }

        private static void ModifyHeaderAddCached(Response response)
        {
            response.Resource.Headers = (response.Resource.Headers??new List<string>()).Union(new[] { "X-Cache: MISS,CACHED" }).ToList();
        }

        private void AddAliasIfUnknown(Response response, Request request)
        {
            if (!_alias.ContainsKey(request.NetResourceLocator)) _alias.Add(request.NetResourceLocator, response.Resource.NetResourceIdentifier);
        }

        
        public void Clear()
        {
            _alias.Clear();
            DoClearCache();
            _hitlist.Clear();
            _lastlist.Clear();
        }

        

        public void Revoke(Guid revocation)
        {
            var removal = _cache.Where(_ => _.Value.RevokationTokens.Contains(revocation)).Select(_=>_.Key).ToList();

            foreach (var r in removal)
            {
                _revoked++;
                if (_cache.ContainsKey(r)) RemoveFromCache(r);
            }

        }

        private void RemoveAliases(string nri)
        {
            var aliasesToRemove = _alias.Where(_ => _.Value == nri).Select(_ => _.Key).ToList();
            foreach (var nrl in aliasesToRemove) _alias.Remove(nrl);
        }

        public void TriggerGarbageCollection()
        {
            DoGarbageCollection();
        }

        
    }
}
