using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel.Exceptions
{
    [Serializable]
    public class ResourceUnavailableException : KernelException
    {
        public ResourceUnavailableException()
        {
        }

        private ResourceUnavailableException(string resource)
            : base("Resource not available: " + resource)
        {
        }

        public static ResourceUnavailableException Create(string resource, string info)
        {
            var ex = new ResourceUnavailableException(resource);
            ex.Data.Add("Resource", resource);
            ex.Data.Add("Details", info);
            return ex;
        }

    }
}
