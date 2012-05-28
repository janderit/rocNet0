using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel
{
    public interface ResourceProvider
    {
        Response Get(Request request);
        void InformQueue(long backlog);
    }

    public interface KernelRegistration
    {
        ResourceRegistry Routes { get; }
        void AddHook(PostProcessHook hook);
    }

}
