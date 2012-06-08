using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LibKernel;

namespace LibKernel_memcache
{
    public partial class SingleThreadedInMemoryCache : ICacheResources
    {
        private readonly List<string> _keys = new List<string>();
        private readonly Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();
        private int _cachesize = 0;
        private long _cacheenergy = 0;

        public void AddToCache(Request request, Response response)
        {
            
            if (_keys.Contains(response.Resource.NetResourceIdentifier))
            {
                if (_cache[response.Resource.NetResourceIdentifier].Resource.Modified < response.Resource.Modified)
                    RemoveFromCache(response.Resource.NetResourceIdentifier);
                else
                {
                    if (!IsCanonical(response, request)) AddAliasIfUnknown(response, request);
                    return;
                }
            }

            //Console.WriteLine("ADD "+Thread.CurrentThread.ManagedThreadId+" "+response.Resource.NetResourceIdentifier);

            var resource = CloneResource(response);

            response.XCache = XCache.Cached;
            response.Via.Add("rocnet-memcache");

            _cache.Add(resource.NetResourceIdentifier,
                       new CacheEntry
                           {
                               Resource = resource,
                               Hits = 0,
                               Last = DateTime.Now,
                               OriginalVia = response.Via.ToList(),
                               OriginalRetrievalTime = response.RetrievalTimeMs
                           });
            _cachesize += resource.Size;
            _cacheenergy += resource.Energy + response.RetrievalTimeMs;
            
            _keys.Add(resource.NetResourceIdentifier);
            if (!IsCanonical(response, request)) AddAliasIfUnknown(response, request);

            CheckForRemovals();

        }

        public void Revoke(Guid revocation)
        {
            var removal = _cache.Where(_ => _.Value.Resource.RevokationTokens.Contains(revocation)).Select(_=>_.Key).ToList();

            foreach (var r in removal)
            {
                _revoked++;
                if (_cache.ContainsKey(r)) RemoveFromCache(r);
            }

        }


        private static bool IsCanonical(Response response, Request request)
        {
            return request.NetResourceLocator == response.Resource.NetResourceIdentifier;
        }


        public void RemoveFromCache(string nri)
        {
            if (!_keys.Contains(nri)) return;

            //Console.WriteLine("DEL " + Thread.CurrentThread.ManagedThreadId + " " + nri);


           RemoveAliases(nri);
            _keys.Remove(nri);
            var removed = _cache[nri];
            _cache.Remove(nri);
            _cachesize -= removed.Resource.Size;
            _cacheenergy -= removed.Resource.Energy + removed.OriginalRetrievalTime;
        }

        public void ClearCache()
        {
            _alias.Clear();
            _keys.Clear();
            _cache.Clear();
            
            _cacheenergy = 0;
            _cachesize = 0;
        }


        private readonly Dictionary<string, string> _alias = new Dictionary<string, string>();

        private void AddAliasIfUnknown(Response response, Request request)
        {
            if (!_alias.ContainsKey(request.NetResourceLocator)) _alias.Add(request.NetResourceLocator, response.Resource.NetResourceIdentifier);
        }

        private void RemoveAliases(string nri)
        {
            var aliasesToRemove = _alias.Where(_ => _.Value == nri).Select(_ => _.Key).ToList();
            foreach (var nrl in aliasesToRemove) _alias.Remove(nrl);
        }

        private long _matchrequests = 0;
        private long _hits = 0;
        private int _faults = 0;
        private long _deliveredenergy = 0;
        private int _revoked = 0;

        public long StatHits { get { return _hits; } }
        public long StatFaults { get { return _faults; } }
        public long StatHitRatePermega { get { return (_matchrequests > 0 ? ((1000000 * _hits) / _matchrequests) : 0); } }
        public long StatDeliveredEnergy { get { return _deliveredenergy; } }
        public long StatRevocations { get { return _revoked; } }

        public int ResourcesCached
        {
            get { return _cache.Count; }
        }

        public int CacheSize
        {
            get { return _cachesize; }
        }

        public long CachedEnergyValue
        {
            get { return _cacheenergy; }
        }

        
        public bool Match(string nri)
        {
            var result = _keys.Contains(nri) || _alias.ContainsKey(nri);
            Interlocked.Increment(ref _matchrequests);
            if (result) Interlocked.Increment(ref _hits);

            return result;
        }

        public Response RetrieveOrNull(string nrl)
        {
            try
            {
                var nri = Dealias(nrl);
                if (!_keys.Contains(nri))
                {
                    Interlocked.Increment(ref _faults);
                    return null;
                }

                CacheEntry entry;
                if (!_cache.TryGetValue(nri, out entry)) {
                    Interlocked.Increment(ref _faults);
                    return null;
                }
                if (entry == null)
                {
                        Interlocked.Increment(ref _faults);
                        return null;
                }
                _deliveredenergy += entry.Resource.Energy + entry.OriginalRetrievalTime;
                entry.Hits++;
                entry.Last = DateTime.Now;
                return new Response { Resource = entry.Resource , Information="", RetrievalTimeMs=0, Status=ResponseCode.Ok, Via=entry.OriginalVia.ToList(), XCache=XCache.CacheHit};
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Interlocked.Increment(ref _faults);
                return null;
            }
        }

        private string Dealias(string netResourceLocator)
        {
            return _alias.ContainsKey(netResourceLocator) ? _alias[netResourceLocator] : netResourceLocator;
        }

        private static ResourceRepresentation CloneResource(Response response)
        {
            return new ResourceRepresentation()
            {
                Body = response.Resource.Body,
                Cacheable = true,
                Correlations = (response.Resource.Correlations ?? new List<Guid>()).ToList(),
                Energy = response.Resource.Energy,
                Expires = response.Resource.Expires,
                MediaType = response.Resource.MediaType,
                Modified = response.Resource.Modified,
                NetResourceIdentifier = response.Resource.NetResourceIdentifier,
                Relations = (response.Resource.Relations ?? new List<string>()),
                RevokationTokens = (response.Resource.RevokationTokens ?? new List<Guid>()).ToList(),
                Size = response.Resource.Size,
            };
        }

    }
}
