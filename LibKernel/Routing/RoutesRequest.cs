using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel.Routing
{
    public static class RoutesRequest
    {
        public static Request Get(Guid provider)
        {
            return new Request {NetResourceLocator = "net://"+provider, AcceptableMediaTypes = new[] {RoutesInformation.Mediatype}};
        }

        public static Request GetGlobal()
        {
            return new Request {NetResourceLocator = "net://@", AcceptableMediaTypes = new[] {RoutesInformation.Mediatype}};
        }
    }
}
