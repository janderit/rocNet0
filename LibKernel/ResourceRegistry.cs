﻿using System;

namespace LibKernel
{
    public interface ResourceRegistry
    {
        void RegisterResourceHandler(Guid routeGroupId, string nri, int energy, Func<ResourceRequest, ResourceRepresentation> handler);
        void RegisterResourceHandlerRegex(Guid routeGroupId, string nriRegEx, int energy, Func<ResourceRequest, ResourceRepresentation> handler);
        void DeleteRoute(Guid routeGroupId);
        void RegisterResourceMapping(Guid routeGroupId, string nriregex, string replacement);
    }
}