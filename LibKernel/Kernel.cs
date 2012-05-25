using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel
{
    public interface Kernel
    {

        Response Get(ResourceRequest request);
        
        void Get(ResourceRequest request, Action<Response> response);
        void Get(ResourceRequest request, Action<ResourceRepresentation> resource, Action<Response> onFailure);
        
    }

    public interface KernelConfigurator
    {
        
    }

    public interface KernelRegistration
    {
        IRegisterRoutes Routes { get; }
    }

}
