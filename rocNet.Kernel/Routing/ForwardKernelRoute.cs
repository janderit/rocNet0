using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LibKernel;
using LibKernel.Routing;

namespace rocNet.Kernel.Routing
{
    internal class ForwardKernelRoute : KernelRouteBase, KernelRoute
    {
        private readonly ResourceProvider _provider;
        private readonly Regex _regex;

        public ForwardKernelRoute(Guid routeGroupId, string nriregex, long energy, bool auth, ResourceProvider provider)
        {
            IsAuthoritative = auth;
            _regex = new Regex(nriregex);
            _provider = provider;
            GroupId = routeGroupId;
            DeliveryTime = energy; 
        }

        public Guid GroupId { get; private set; }

        public bool Match(string nri)
        {
            return _regex.IsMatch(nri);
        }

        public bool IsAuthoritative { get; private set; }

        public Func<Request, Response> Handler
        {
            get { return Handle; }
        }

        private Response Handle(Request arg)
        {
            var t0 = Environment.TickCount;
            var x = _provider.Get(arg).Guard();
            var t1 = Environment.TickCount;

            x.RetrievalTimeMs = t1 - t0;

            return x;
        }
    }
}
