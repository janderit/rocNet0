using System;

namespace LibKernel
{
    public interface IRegisterRoutes
    {
        void RegisterImmediate(Guid routeGroupId, string nri, int energy, Func<ResourceRequest, ResourceRepresentation> handler);
        void RegisterRegex(Guid routeGroupId, string nriRegEx, int energy, Func<ResourceRequest, ResourceRepresentation> handler);
        void DeleteRoute(Guid routeGroupId);
        void RegisterMap(Guid routeGroupId, string nriregex, string replacement);
    }
}