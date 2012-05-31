using System;

namespace LibKernel
{
    public interface KernelRoute
    {
        Guid GroupId { get; }
        long Energy { get; }
        bool Match(string nri);
        bool IsAuthoritative { get; }
        Func<Request, ResourceRepresentation> Handler { get; }
    }
}