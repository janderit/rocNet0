using System;
using System.Collections.Generic;
using LibKernel;

namespace LibKernel_memcache
{
    public interface ICacheResources
    {
        void TriggerGarbageCollection();
        void AddToCache(Request request, Response response);
        void Revoke(Guid revocation);
        void ClearCache();

        bool Match(string nri);
        ResourceRepresentation RetrieveOrNull(string nrl);

        bool CheckCachingStrategy(ResourceRepresentation resource);

        int MaxResourcesInCache { set; }
        int RemovalChunkSize { set; }
        long MaxCacheSize { set; }
        long MaxCacheDurationSeconds { set; }

        long MinCachableEnergy { set; }
        long MaxCachableSize { set; }
        int MinimumExpirationTimesEnergyFactor { set; }
        int EnergySizeTradeoffFactor { set; }

        IEnumerable<string> CurrentKeys { get; }

        CacheConfiguration Configuration { get; }
        CacheStatistics Statistics { get; }
    }
}