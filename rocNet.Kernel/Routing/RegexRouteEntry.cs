using System;
using System.Text.RegularExpressions;

namespace LibKernel
{
    internal class RegexRouteEntry:RouteEntry
    {
        private readonly Regex _regex;

        public Guid GroupId { get; private set; }
        public int Energy { get; private set; }
        public Func<ResourceRequest, ResourceRepresentation> Handler { get; private set; }

        public RegexRouteEntry(Guid groupId, string nriregex, int energy, Func<ResourceRequest, ResourceRepresentation> handler)
        {
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