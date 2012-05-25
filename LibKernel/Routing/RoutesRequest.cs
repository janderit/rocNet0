using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel.Routing
{
    public static class RoutesRequest
    {
        public static ResourceRequest Get(Guid provider)
        {
            return new ResourceRequest {NetResourceLocator = "net://"+provider, AcceptableMediaTypes = new[] {RoutesInformation.Mediatype}};
        }

        public static ResourceRequest GetGlobal()
        {
            return new ResourceRequest {NetResourceLocator = "net://@", AcceptableMediaTypes = new[] {RoutesInformation.Mediatype}};
        }
    }
}
