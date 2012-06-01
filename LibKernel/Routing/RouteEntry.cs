using System;
using System.Xml.Serialization;

namespace LibKernel.Routing
{
    [Serializable]
    public class RouteEntry
    {
        [XmlAttribute]
        public string Identifier { get; set; }
        [XmlText]
        public string Regex { get; set; }
        [XmlAttribute]
        public long Energy { get; set; }
    }
}