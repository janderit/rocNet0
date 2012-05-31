using System;

namespace LibKernel
{
    public interface KernelRoute
    {
        Guid GroupId { get; }
        long Energy { get; }
        bool Match(string nri, bool ignoreCache=false);
        Func<Request, ResourceRepresentation> Handler { get; }
    }
}