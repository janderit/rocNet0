using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LibKernel
{
    class Router : IRegisterRoutes
    {

        private List<RouteEntry> _routes = new List<RouteEntry>();

        public void RegisterImmediate(Guid routeGroupId, string nri, int energy, Func<ResourceRequest, ResourceRepresentation> handler)
        {
            _routes.Add(new ImmediateRouteEntry(routeGroupId, nri, energy, handler));
        }

        public void RegisterRegex(Guid routeGroupId, string nriRegEx, int energy, Func<ResourceRequest, ResourceRepresentation> handler)
        {
            _routes.Add(new RegexRouteEntry(routeGroupId, nriRegEx, energy, handler));
        }

        public void RegisterMap(Guid routeGroupId, string nriregex, string replacement)
        {
            RegisterRegex(routeGroupId, nriregex, 1, new MappedRoute(nriregex, replacement, this, true).Map);
        }

        public void DeleteRoute(Guid routeGroupId)
        {
            _routes.Where(_ => _.GroupId == routeGroupId).ToList().ForEach(_ => _routes.Remove(_));
        }

        public Func<ResourceRequest, ResourceRepresentation> Lookup(string nri)
        {
            return _routes.Where(_ => _.Match(nri)).OrderBy(_ => _.Energy).Select(_=>_.Handler).FirstOrDefault();
        }



        public void Reset()
        {
            _routes.Clear();
        }
    }

    internal class MappedRoute
    {
        private readonly string _nriregex;
        private readonly string _replacement;
        private readonly Router _router;
        private readonly bool _rewritenrl;

        public MappedRoute(string nriregex, string replacement, Router router, bool rewritenrl)
        {
            _nriregex = nriregex;
            _replacement = replacement;
            _router = router;
            _rewritenrl = rewritenrl;
        }

        public ResourceRepresentation Map(ResourceRequest req)
        {
            var mapped = Regex.Replace(req.NetResourceLocator, _nriregex, _replacement);
            var handler = _router.Lookup(mapped);
            return handler(_rewritenrl ? new ResourceRequest {NetResourceLocator = mapped, AcceptableMediaTypes = req.AcceptableMediaTypes} : req);
        }
    }
}
