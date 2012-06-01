using System;
using System.Xml.Serialization;

namespace LibKernel_memcache
{
    [Serializable]
    public class CacheConfiguration
    {
        [XmlAttribute]
        public int MaxResourcesInCache { get; set; }
        [XmlAttribute]
        public long MaxCacheSize { get; set; }
        [XmlAttribute]
        public long MaxCacheDurationSeconds { get; set; }

        [XmlAttribute]
        public long MinCachableEnergy { get; set; }
        [XmlAttribute]
        public long MaxCachableSize { get; set; }
        [XmlAttribute]
        public int MinimumExpirationTimesEnergyFactor { get; set; }
        [XmlAttribute]
        public int EnergySizeTradeoffFactor { get; set; }

        [XmlAttribute]
        public int RemovalChunkSize { get; set; }
    }
}