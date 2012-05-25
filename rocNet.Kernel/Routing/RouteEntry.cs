﻿using System;

namespace LibKernel
{
    public interface RouteEntry
    {
        Guid GroupId { get; }
        long Energy { get; }
        bool Match(string nri);
        Func<ResourceRequest, ResourceRepresentation> Handler { get; }
    }
}