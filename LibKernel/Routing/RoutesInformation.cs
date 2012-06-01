using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibKernel.Exceptions;
using Newtonsoft.Json;

namespace LibKernel.Routing
{
    [Serializable]
    public class RoutesInformation
    {
        public RoutesInformation() { Routes = new List<RouteEntry>(); }

        public static string Mediatype { get { return "vnd.rocnet.routesinformation;v=1"; } }
        public List<RouteEntry> Routes { get; set; }
    }
}
