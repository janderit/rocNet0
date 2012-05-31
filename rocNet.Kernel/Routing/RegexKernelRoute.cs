using System;
using System.Text.RegularExpressions;

namespace LibKernel
{
    internal class RegexKernelRoute:KernelRoute
    {
        private Regex _regex;
        public string NriRegex { get; private set; }

        public Guid GroupId { get; private set; }
        public long Energy { get; private set; }

        public bool IsAuthoritative { get; private set; }

        public Func<Request, ResourceRepresentation> Handler { get; private set; }

        public RegexKernelRoute(Guid groupId, string nriregex, int energy, bool auth, Func<Request, ResourceRepresentation> handler)
        {
            IsAuthoritative = auth;
            NriRegex = nriregex;
            _regex = new Regex(nriregex);
            GroupId = groupId;
            Energy = energy;
            Handler = r =>
            {
                var t0 = Environment.TickCount;
                var x = handler(r);
                var t1 = Environment.TickCount;
                if (x.Energy < t1 - t0) x.Energy = t1 - t0;
                if (Energy < t1 - t0) Energy = t1 - t0;
                return x;
            }; 
        }

        public bool Match(string nri)
        {
            return _regex.IsMatch(nri);
        }
    }
}