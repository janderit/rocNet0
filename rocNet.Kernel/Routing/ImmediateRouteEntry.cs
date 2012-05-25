using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel
{
    internal class ImmediateRouteEntry : RouteEntry
    {
        public readonly string Nri;
        public readonly string Route;

        public Guid GroupId { get; private set; }
        public long Energy { get; private set; }
        public Func<ResourceRequest, ResourceRepresentation> Handler { get; private set; }

        public ImmediateRouteEntry(Guid groupId, string nri, int energy, Func<ResourceRequest, ResourceRepresentation> handler)
        {
            GroupId = groupId;
            Nri = nri;
            Energy = energy;
            Handler = handler;
        }

        public bool Match(string nri)
        {
            return Nri == nri;
        }
    }
}
