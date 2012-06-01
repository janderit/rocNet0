using System;
using System.Xml.Serialization;

namespace LibKernel_memcache
{
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
        public long Requests { get; set; }
        [XmlAttribute]
        public long Hits { get; set; }
        [XmlAttribute]
        public long HitRatePermega { get; set; }
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