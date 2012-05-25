using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LibKernel.Routing;
using rocNet.Kernel.Routing;

namespace LibKernel
{
    class Router : ResourceRegistry
    {

        private readonly List<RouteEntry> _routes = new List<RouteEntry>();
        private readonly Dictionary<Guid, ResourceProvider> _provider = new Dictionary<Guid, ResourceProvider>();
        private readonly ResourceProvider _kernel;

        public Router(ResourceProvider kernel)
        {
            if (kernel == null) throw new ArgumentNullException("kernel");
            _kernel = kernel;
        }

        public void RegisterResourceHandler(Guid routeGroupId, string nri, int energy, Func<ResourceRequest, ResourceRepresentation> handler)
        {
            _routes.Add(new ImmediateRouteEntry(routeGroupId, nri, energy, handler));
        }

        public void RegisterResourceHandlerRegex(Guid routeGroupId, string nriRegEx, int energy, Func<ResourceRequest, ResourceRepresentation> handler)
        {
            _routes.Add(new RegexRouteEntry(routeGroupId, nriRegEx, energy, handler));
        }

        public void RegisterResourceMapping(Guid routeGroupId, string nriregex, string replacement)
        {
            RegisterResourceHandlerRegex(routeGroupId, nriregex, 1, new MappedRoute(nriregex, replacement, this, true).Map);
        }

        public void RegisterResourceProvider(ResourceProvider provider)
        {
            var providerid = Guid.NewGuid();

            RegisterResourceHandler(providerid, RoutesRequest.Get(providerid).NetResourceLocator, 1, _=>provider.Get(RoutesRequest.GetGlobal()).Guard());

            _provider.Add(providerid, provider);
            var routeresource = _kernel.Get(RoutesRequest.Get(providerid)).Guard();

            var routelist = RoutesInformation.Parse(routeresource);

            foreach (var routeEntry in routelist.Routes)
            {
                if (!String.IsNullOrEmpty(routeEntry.Identifier)) RegisterResourceForwarder(providerid, IdentifierToRegex(routeEntry.Identifier), provider, routeEntry.Energy + routeresource.Energy);
                else RegisterResourceForwarder(providerid, routeEntry.Regex, provider, routeEntry.Energy + routeresource.Energy);
            }
        }

        private string IdentifierToRegex(string identifier)
        {
            return identifier
                .Replace("\\", "\\\\")
                .Replace(".", "\\.")
                .Replace("+", "\\+")
                .Replace("*", "\\*")
                .Replace("$", "\\$")
                .Replace("?", "\\?")
                .Replace("#", "\\#")
                ;
        }

        private IEnumerable<Routing.RouteEntry> PublicRoutes()
        {
            var ri = _routes.OfType<ImmediateRouteEntry>().Select(_=>new Routing.RouteEntry{Identifier=_.Nri, Energy = _.Energy});
            ri = ri.Union(_routes.OfType<RegexRouteEntry>().Select(_ => new Routing.RouteEntry { Regex = _.NriRegex, Energy = _.Energy }));
            return ri.ToList();
        }

        public void EnableRoutePublication()
        {
            RegisterResourceHandler(Guid.NewGuid(), "net://@", 0, r => RoutesInformation.Pack(new RoutesInformation { Routes = PublicRoutes().ToList() }));
        }

        public void RegisterResourceForwarder(Guid routeGroupId, string nriregex, ResourceProvider provider, long energy)
        {
            _routes.Add(new ForwardRouteEntry(routeGroupId, nriregex, energy, provider));
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
