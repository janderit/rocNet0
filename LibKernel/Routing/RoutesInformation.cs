using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using LibKernel.Exceptions;
using Newtonsoft.Json;

namespace LibKernel.Routing
{
    public class RoutesInformation
    {
        public RoutesInformation()
        {
            Routes = new List<RouteEntry>();
        }

        public static string Mediatype { get { return "application/vnd.rocnet.routesinformation+xml;v=1"; } }
        public List<RouteEntry> Routes { get; set; }

        public static RoutesInformation Parse(ResourceRepresentation resource)
        {
            if (resource.MediaType != Mediatype) throw MediaTypeException.Create(Mediatype, resource.MediaType, resource.NetResourceIdentifier);
            if (resource.MediaType.Contains("+json"))
            {
                return JsonConvert.DeserializeObject<RoutesInformation>(resource.Body);
            }
            if (resource.MediaType.Contains("+xml"))
            {
                var stream = new StringReader(resource.Body);
                return new XmlSerializer(typeof (RoutesInformation)).Deserialize(stream) as RoutesInformation;
            }
            return null;
        }

        public static ResourceRepresentation Pack(RoutesInformation that)
        {
            var body = "";

            if (Mediatype.Contains("+xml"))
            {
                var sw = new StringWriter();
                new XmlSerializer(typeof (RoutesInformation)).Serialize(sw, that);
                body = sw.ToString();
                sw.Dispose();
            }

            if (Mediatype.Contains("+json"))
            {
                body = JsonConvert.SerializeObject(that, Formatting.Indented);
            }

            return new ResourceRepresentation
                             {
                                 NetResourceIdentifier="net://@",
                                 Body= body,
                                 MediaType=Mediatype
                             };
        }
    }

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
