using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static string Mediatype { get { return "application/vnd.rocnet.routesinformation+json;v=1"; } }
        public List<RouteEntry> Routes { get; set; }

        public static RoutesInformation Parse(ResourceRepresentation resource)
        {
            if (resource.MediaType != Mediatype) throw MediaTypeException.Create(Mediatype, resource.MediaType, resource.NetResourceIdentifier);
            return JsonConvert.DeserializeObject<RoutesInformation>(resource.Body);
        }

        public static ResourceRepresentation Pack(RoutesInformation that)
        {
            return new ResourceRepresentation
                             {
                                 NetResourceIdentifier="net://@",
                                 Body=JsonConvert.SerializeObject(that, Formatting.Indented),
                                 MediaType=Mediatype
                             };
        }
    }

    public class RouteEntry
    {
        public string Identifier { get; set; }
        public string Regex { get; set; }
        public long Energy { get; set; }
    }

}
