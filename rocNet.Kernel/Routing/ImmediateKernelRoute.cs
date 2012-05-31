using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel
{
    internal class ImmediateKernelRoute : KernelRoute
    {
        public readonly string Nri;
        public readonly string Route;

        public Guid GroupId { get; private set; }
        public long Energy { get; private set; }

        public Func<Request, ResourceRepresentation> Handler { get; private set; }

        public ImmediateKernelRoute(Guid groupId, string nri, int energy, Func<Request, ResourceRepresentation> handler)
        {
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

        public bool Match(string nri, bool ignorecache)
        {
            return Nri == nri;
        }
    }
}
