using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel.Exceptions
{
    [Serializable]
    public class ResourceNotFoundException : KernelException
    {
        public ResourceNotFoundException()
        {
        }

        private ResourceNotFoundException(string resource):base("Resource not found: "+resource)
        {
        }

        public static ResourceNotFoundException Create(string resource, string info)
        {
            var ex = new ResourceNotFoundException(resource);
            ex.Data.Add("Resource", resource);
            ex.Data.Add("Details", info);
            return ex;
        }

    }
}
