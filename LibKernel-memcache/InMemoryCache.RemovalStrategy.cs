using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibKernel;

namespace LibKernel_memcache
{
    public partial class InMemoryCache 
    {

        public int MaxResourcesInCache { get; set; }
        public int RemovalChunkSize { get; set; }
        public long MaxCacheSize { get; set; }
        public long MaxCacheDurationSeconds { get; set; }

        private Dictionary<string, int> _hitlist = new Dictionary<string, int>();
        private Dictionary<string, DateTime> _lastlist = new Dictionary<string, DateTime>();
        
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
                DoGarbageCollection();
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

        private long RemovalStrategy(ResourceRepresentation resource)
        {
            return
                resource.Size*EnergySizeTradeoffFactor - resource.Energy
                - (long)Math.Round((resource.Expires - DateTime.Now).TotalSeconds)
                - _hitlist[resource.NetResourceIdentifier]*1000
                + (long)Math.Round(((DateTime.Now-_lastlist[resource.NetResourceIdentifier]).TotalSeconds)*100)
                ;
        }

        private void DoGarbageCollection()
        {
            var now = DateTime.Now;
            var removal = _cache.Where(_ => _.Value.Expires < now || (now-_.Value.Modified).TotalSeconds>MaxCacheDurationSeconds).Select(_ => _.Key).ToList();
            foreach (var nri in removal) RemoveFromCache(nri);
        }

    }
}
