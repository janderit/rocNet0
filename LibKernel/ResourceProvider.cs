using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel
{
    public interface ResourceProvider
    {
        Response Get(ResourceRequest request);
    }

    public interface KernelRegistration
    {
        IRegisterRoutes Routes { get; }
    }

}
