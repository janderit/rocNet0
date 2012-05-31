using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibKernel;

namespace LibKernel_memcache
{
    public partial class SingleThreadedInMemoryCache : ICacheResources
    {

        public SingleThreadedInMemoryCache()
        {
            LastGarbageCollection = DateTime.MinValue;
            SetCachingStrategyDefaults();
            SetRemovalStrategyDefaults();
        }

        public int MaxResourcesInCache { get; set; }
        public int RemovalChunkSize { get; set; }
        public long MaxCacheSize { get; set; }
        public long MaxCacheDurationSeconds { get; set; }

        public long MinCachableEnergy { get; set; }
        public long MaxCachableSize { get; set; }
        public int MinimumExpirationTimesEnergyFactor { get; set; }
        public int EnergySizeTradeoffFactor { get; set; }
        public IEnumerable<string> CurrentKeys
        {
            get
            {
                return _keys.ToList();
            }
        }

        private void SetCachingStrategyDefaults()
        {
            MinCachableEnergy = 100;
            MaxCachableSize = 1024 * 1024;
            MinimumExpirationTimesEnergyFactor = 2;
            EnergySizeTradeoffFactor = 1;
        }



        public bool CheckCachingStrategy(ResourceRepresentation resource)
        {
            return
                resource.Cacheable
                && resource.Expires > DateTime.Now.AddMilliseconds(250)
                && resource.Energy >= MinCachableEnergy
                && resource.Size <= MaxCachableSize
                && (resource.Expires - DateTime.Now).TotalMilliseconds
                    >= resource.Energy * MinimumExpirationTimesEnergyFactor
                && resource.Energy >= resource.Size * EnergySizeTradeoffFactor
                ;
        }
        
        private void SetRemovalStrategyDefaults()
        {
            MaxResourcesInCache = 2000000;
            RemovalChunkSize = 100;
            MaxCacheSize = 50*1024*1024;
            MaxCacheDurationSeconds = 3600;
        }

        private void CheckForRemovals()
        {
            Func<bool> cacheLimitsObeyed = () => _cache.Count <= MaxResourcesInCache && _cachesize <= MaxCacheSize;

            if (!cacheLimitsObeyed())
            {
                TriggerGarbageCollection();
                if (!cacheLimitsObeyed())
                {
                    var removalList = CreatePriorityRemovalList();
                    while (!cacheLimitsObeyed())
                    {
                        for (int i = 0; i < RemovalChunkSize && removalList.Count > 0; i++)
                        {
                            RemoveFromCache(removalList.First());
                            removalList.RemoveAt(0);
                        }
                    }
                }
            }

        }

        private List<string> CreatePriorityRemovalList()
        {
            return _cache.OrderBy(_=>RemovalStrategy(_.Value)).Select(_ => _.Key).ToList();
        }

        private long RemovalStrategy(CacheEntry entry)
        {
            return
                entry.Resource.Size * EnergySizeTradeoffFactor - entry.Resource.Energy
                - (long)Math.Round((entry.Resource.Expires - DateTime.Now).TotalSeconds)
                - entry.Hits * 1000
                + (long)Math.Round(((DateTime.Now-entry.Last).TotalSeconds)*100)
                ;
        }


        public DateTime LastGarbageCollection { get; private set; }


        public void TriggerGarbageCollection()
        {
            var now = DateTime.Now;
            LastGarbageCollection = now;

            var removal = _cache.Where(_ => _.Value.Resource.Expires < now || (now - _.Value.Resource.Modified).TotalSeconds > MaxCacheDurationSeconds).Select(_ => _.Key).ToList();
            foreach (var nri in removal) RemoveFromCache(nri);
        }

        public CacheConfiguration Configuration { get { return new CacheConfiguration
                                                                   {
                                                                       EnergySizeTradeoffFactor=EnergySizeTradeoffFactor,
                                                                       MaxCachableSize=MaxCachableSize,
                                                                       MaxCacheDurationSeconds=MaxCacheDurationSeconds,
                                                                       MaxCacheSize=MaxCacheSize,
                                                                       MaxResourcesInCache=MaxResourcesInCache,
                                                                       MinCachableEnergy=MinCachableEnergy,
                                                                       MinimumExpirationTimesEnergyFactor=MinimumExpirationTimesEnergyFactor,
                                                                       RemovalChunkSize = RemovalChunkSize,
                                                                   }; } }

        public CacheStatistics Statistics { get { return new CacheStatistics
                                                             {
                                                                 AliasCount=_alias.Count,
                                                                 CachedEnergyValue=_cacheenergy,
                                                                 CacheSize=_cachesize,
                                                                 Requests=_matchrequests,
                                                                 Faults=_faults,
                                                                 HitRatePermega=StatHitRatePermega,
                                                                 Hits=_hits,
                                                                 ResourcesCached=_keys.Count,
                                                                 RevokationTokenCount=0,
                                                                 StatDeliveredEnergy=_deliveredenergy,
                                                                 StatRevocations = _revoked
                                                             }; } }

    }
}
