using System;

namespace LibKernel.Routing
{
    public interface KernelRoute
    {
        Guid GroupId { get; }
        long DeliveryTime { get; set; }
        bool Match(string nri);
        bool IsAuthoritative { get; }
        Func<Request, Response> Handler { get; }
    }



}