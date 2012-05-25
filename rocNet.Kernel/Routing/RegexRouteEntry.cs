using System;
using System.Text.RegularExpressions;

namespace LibKernel
{
    internal class RegexRouteEntry:RouteEntry
    {
        private Regex _regex;
        public string NriRegex { get; private set; }

        public Guid GroupId { get; private set; }
        public long Energy { get; private set; }
        public Func<ResourceRequest, ResourceRepresentation> Handler { get; private set; }

        public RegexRouteEntry(Guid groupId, string nriregex, int energy, Func<ResourceRequest, ResourceRepresentation> handler)
        {
            NriRegex = nriregex;
            _regex = new Regex(nriregex);
            GroupId = groupId;
            Energy = energy;
            Handler = handler;
        }

        public bool Match(string nri)
        {
            return _regex.IsMatch(nri);
        }
    }
}