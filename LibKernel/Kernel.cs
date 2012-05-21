using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel
{
    public interface Kernel
    {
        ResourceRepresentation Get(ResourceRequest request);
    }

    public interface KernelConfigurator
    {
        
    }

    public interface KernelRegistration
    {
        IRegisterRoutes Routes { get; }
    }

}
