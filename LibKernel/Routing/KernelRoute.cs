using System;

namespace LibKernel
{
    public interface KernelRoute
    {
        Guid GroupId { get; }
        long Energy { get; }
        bool Match(string nri);
        Func<Request, ResourceRepresentation> Handler { get; }
    }
}