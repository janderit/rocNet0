using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LibKernel;

namespace LibKernel_memcache
{
    public partial class InMemoryCache 
    {

        private readonly Dictionary<string, ResourceRepresentation> _cache = new Dictionary<string, ResourceRepresentation>();
        private int _cachesize = 0;
        private long _cacheenergy = 0;

        private void AddToCache(Response response)
        {
            //Console.WriteLine("ADD "+Thread.CurrentThread.ManagedThreadId+" "+response.Resource.NetResourceIdentifier);
            _hitlist.Add(response.Resource.NetResourceIdentifier, 0);
            _lastlist.Add(response.Resource.NetResourceIdentifier, DateTime.Now);
            _cache.Add(response.Resource.NetResourceIdentifier, CloneResource(response));
            _cachesize += response.Resource.Size;
            _cacheenergy += response.Resource.Energy;
        }

        private static ResourceRepresentation CloneResource(Response response)
        {
            return new ResourceRepresentation()
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
        }

        private void RemoveFromCache(string nri)
        {
            //Console.WriteLine("DEL " + Thread.CurrentThread.ManagedThreadId + " " + nri);
            var removed = _cache[nri];
            RemoveAliases(nri);
            _cache.Remove(nri);
            _hitlist.Remove(nri);
            _lastlist.Remove(nri);
            _cachesize -= removed.Size;
            _cacheenergy -= removed.Energy;
        }

        private void DoClearCache()
        {
            _cache.Clear();
            _cacheenergy = 0;
            _cachesize = 0;
        }
    }
}
