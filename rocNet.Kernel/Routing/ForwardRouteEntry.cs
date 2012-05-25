using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LibKernel;

namespace rocNet.Kernel.Routing
{
    internal class ForwardRouteEntry : RouteEntry
    {
        private readonly ResourceProvider _provider;
        private readonly Regex _regex;

        public ForwardRouteEntry(Guid routeGroupId, string nriregex, long energy, ResourceProvider provider)
        {
            _regex = new Regex(nriregex);
            _provider = provider;
            GroupId = routeGroupId;
            Energy = energy; 
        }

        public Guid GroupId { get; private set; }
        public long Energy { get; private set; }

        public bool Match(string nri)
        {
            return _regex.IsMatch(nri);
        }

        public Func<ResourceRequest, ResourceRepresentation> Handler
        {
            get { return Handle; }
        }

        private ResourceRepresentation Handle(ResourceRequest arg)
        {
            return _provider.Get(arg).Guard();
        }
    }
}
