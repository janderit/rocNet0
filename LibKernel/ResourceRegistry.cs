using System;

namespace LibKernel
{
    public interface ResourceRegistry
    {
        void RegisterRoute(Guid routeGroupId, KernelRoute route);
        void RegisterResourceHandler(Guid routeGroupId, string nri, int energy, bool isAuthoritative, Func<Request, ResourceRepresentation> handler);
        void RegisterResourceHandlerRegex(Guid routeGroupId, string nriRegEx, int energy, bool isAuthoritative, Func<Request, ResourceRepresentation> handler);
        void DeleteRoute(Guid routeGroupId);
        void RegisterResourceMapping(Guid routeGroupId, string nriregex, string replacement);
        void RegisterResourceForwarder(Guid routeGroupId, string nriregex, ResourceProvider provider, long energy);

        void RegisterResourceProvider(ResourceProvider provider, int energy, bool isAuthoritative);
        void EnableRoutePublication();
    }
}