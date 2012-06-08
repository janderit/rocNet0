using System;
using System.Text.RegularExpressions;
using LibKernel;
using LibKernel.Routing;

namespace rocNet.Kernel.Routing
{
    internal class RegexKernelRoute : KernelRouteBase, KernelRoute
    {
        private Regex _regex;
        public string NriRegex { get; private set; }

        public Guid GroupId { get; private set; }

        public bool IsAuthoritative { get; private set; }

        public Func<Request, Response> Handler { get; private set; }

        public RegexKernelRoute(Guid groupId, string nriregex, int energy, bool auth, Func<Request, ResourceRepresentation> handler)
        {
            IsAuthoritative = auth;
            NriRegex = nriregex;
            _regex = new Regex(nriregex);
            GroupId = groupId;
            DeliveryTime = energy;
            Handler = r =>
            {
                var t0 = Environment.TickCount;
                var x = handler(r);
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
            return _regex.IsMatch(nri);
        }
    }
}