using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel.Exceptions
{
    [Serializable]
    public class RouteUnavailableException : KernelException
    {
        public RouteUnavailableException()
        {
        }

        private RouteUnavailableException(string resource)
            : base("Route to resource currently not available: " + resource)
        {
        }

        public static RouteUnavailableException Create(string resource, string info)
        {
            var ex = new RouteUnavailableException(resource);
            ex.Data.Add("Resource", resource);
            ex.Data.Add("Details", info);
            return ex;
        }

    }
}
