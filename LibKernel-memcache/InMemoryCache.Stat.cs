using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibKernel;

namespace LibKernel_memcache
{
    public partial class InMemoryCache 
    {

        private long _matchrequests = 0;
        private long _hits = 0;
        private long _deliveredenergy = 0;
        private long _revoked = 0;

        public long StatHits { get { return _hits; } }
        public long StatHitRatePermega { get { return _matchrequests>0?((1000000*_hits)/_matchrequests):0; } }
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

        private ResourceRepresentation CacheInfo()
        {
            return new ResourceRepresentation
                       {
                           Body=
                           "rocNet kernel memcache\r\n"
                           +ResourcesCached+" resources cached \r\n"
                           +_alias.Count+" aliases registered\r\n"
                           + CacheSize+" cached bytes\r\n\r\n"
                           + CachedEnergyValue+"? cache energy value\r\n\r\n"
                           //+ "? revokation tokens\r\n\r\n"
                           + "HitRate: " + StatHitRatePermega + "/1000000 requests \r\n"
                           + "Hits: " + StatHits + " \r\n"
                           + "EnergySaved: " + StatDeliveredEnergy + " \r\n"
                           + "Revocations: "+StatRevocations+" \r\n\r\n"
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

        private ResourceRepresentation CacheIds()
        {
            return new ResourceRepresentation
            {
                Body =
                "rocNet kernel memcache\r\n"
                +_cache.Aggregate("", (c,kv)=>c+kv.Key+Environment.NewLine),
                Cacheable = false,
                Energy = 1,
                MediaType = "text/plain",
                NetResourceIdentifier = "net://@cache"
            };
        }

    }
}
