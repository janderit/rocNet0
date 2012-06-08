using System;
using LibKernel;
using LibKernel.Routing;

namespace rocNet.Kernel.Routing
{
    internal class ImmediateKernelRoute : KernelRouteBase, KernelRoute
    {
        public readonly string Nri;
        public readonly string Route;

        public Guid GroupId { get; private set; }

        public Func<Request, Response> Handler { get; private set; }

        public ImmediateKernelRoute(Guid groupId, string nri, int energy, bool auth, Func<Request, ResourceRepresentation> handler)
        {
            IsAuthoritative = auth;
            GroupId = groupId;
            Nri = nri;
            DeliveryTime = energy;
            Handler = r=>
                          {
                              var t0 = Environment.TickCount;
                              var x =handler(r);
                              var t1 = Environment.TickCount;
                              return new Response
                                         {
                                             Information = "",
                                             Resource = x,
                                             RetrievalTimeMs = t1 - t0,
                                             Status = ResponseCode.Ok
                                         };
                          };
        }

        public bool Match(string nri)
        {
            return Nri == nri;
        }

        public bool IsAuthoritative { get; private set; }
    }
}
