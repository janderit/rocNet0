using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel.Provider
{
    public interface IProvideResources
    {
        bool Heartbeat();
        IEnumerable<RouteInfo> GetRoutes();
    }
}
