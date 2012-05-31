using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace LibKernel_memcache
{
    [Serializable]
    public class CacheInformation
    {

        public static string Mediatype { get { return "vnd.rocnet.cacheinformation;v=1"; } }

        public CacheInformation()
        {
            Configuration = new CacheConfiguration();
            Statistics = new CacheStatistics();
        }

        public CacheConfiguration Configuration { get; set; }
        public CacheStatistics Statistics { get; set; }
    }

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

    [Serializable]
    public class CacheStatistics
    {
        [XmlAttribute]
        public int ResourcesCached { get; set; }
        [XmlAttribute]
        public int AliasCount { get; set; }
        [XmlAttribute]
        public long CacheSize { get; set; }
        [XmlAttribute]
        public long CachedEnergyValue { get; set; }

        [XmlAttribute]
        public long Hits { get; set; }
        [XmlAttribute]
        public int HitRatePermega { get; set; }
        [XmlAttribute]
        public int Faults { get; set; }
        [XmlAttribute]
        public int RevokationTokenCount { get; set; }
        [XmlAttribute]
        public int StatRevocations { get; set; }
        [XmlAttribute]
        public long StatDeliveredEnergy { get; set; }
    }

}
