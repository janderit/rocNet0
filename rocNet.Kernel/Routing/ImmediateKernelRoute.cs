using System;
using LibKernel;
using LibKernel.Routing;

namespace rocNet.Kernel.Routing
{
    internal class ImmediateKernelRoute : KernelRoute
    {
        public readonly string Nri;
        public readonly string Route;

        public Guid GroupId { get; private set; }
        public long Energy { get; private set; }

        public Func<Request, ResourceRepresentation> Handler { get; private set; }

        public ImmediateKernelRoute(Guid groupId, string nri, int energy, bool auth, Func<Request, ResourceRepresentation> handler)
        {
            IsAuthoritative = auth;
            GroupId = groupId;
            Nri = nri;
            Energy = energy;
            Handler = r=>
                          {
                              var t0 = Environment.TickCount;
                              var x =handler(r);
                              var t1 = Environment.TickCount;
                              if (x.Energy < t1 - t0) x.Energy = t1 - t0;
                              if (Energy < t1 - t0) Energy = t1 - t0;
                              return x;
                          };
        }

        public bool Match(string nri)
        {
            return Nri == nri;
        }

        public bool IsAuthoritative { get; private set; }
    }
}
