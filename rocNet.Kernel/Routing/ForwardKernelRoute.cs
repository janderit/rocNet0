using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LibKernel;

namespace rocNet.Kernel.Routing
{
    internal class ForwardKernelRoute : KernelRoute
    {
        private readonly ResourceProvider _provider;
        private readonly Regex _regex;

        public ForwardKernelRoute(Guid routeGroupId, string nriregex, long energy, ResourceProvider provider)
        {
            _regex = new Regex(nriregex);
            _provider = provider;
            GroupId = routeGroupId;
            Energy = energy; 
        }

        public Guid GroupId { get; private set; }
        public long Energy { get; private set; }

        public bool Match(string nri, bool ignorecache)
        {
            return _regex.IsMatch(nri);
        }

        public Func<Request, ResourceRepresentation> Handler
        {
            get { return Handle; }
        }

        private ResourceRepresentation Handle(Request arg)
        {
            var t0 = Environment.TickCount;
            var x = _provider.Get(arg).Guard();
            var t1 = Environment.TickCount;
            if (Energy < t1 - t0) Energy = t1 - t0;
            return x;
        }
    }
}
