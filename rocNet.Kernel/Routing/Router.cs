using System;
using System.Collections.Generic;
using System.Linq;
using LibKernel;
using LibKernel.MediaFormats;
using LibKernel.Routing;

namespace rocNet.Kernel.Routing
{
    class Router : ResourceRegistry
    {

        private readonly List<KernelRoute> _routes = new List<KernelRoute>();
        private readonly Dictionary<Guid, ResourceProvider> _provider = new Dictionary<Guid, ResourceProvider>();
        private readonly ResourceProvider _kernel;

        public Router(ResourceProvider kernel)
        {
            if (kernel == null) throw new ArgumentNullException("kernel");
            _kernel = kernel;
        }

        public void RegisterRoute(Guid routeGroupId, KernelRoute route)
        {
            _routes.Add(route);
        }

        public void RegisterResourceHandler(Guid routeGroupId, string nri, int energy, bool isAuthoritative, Func<Request, ResourceRepresentation> handler)
        {
            _routes.Add(new ImmediateKernelRoute(routeGroupId, nri, energy, isAuthoritative, handler));
        }

        public void RegisterResourceHandlerRegex(Guid routeGroupId, string nriRegEx, int energy, bool isAuthoritative, Func<Request, ResourceRepresentation> handler)
        {
            _routes.Add(new RegexKernelRoute(routeGroupId, nriRegEx, energy, isAuthoritative, handler));
        }

        public void RegisterResourceMapping(Guid routeGroupId, string nriregex, string replacement)
        {
            RegisterResourceHandlerRegex(routeGroupId, nriregex, 1, true, new MappedRoute(nriregex, replacement, _kernel).Map);
        }

        public void RegisterResourceProvider(ResourceProvider provider, int energy, bool isAuthoritative)
        {
            var providerid = Guid.NewGuid();

            RegisterResourceHandler(providerid, RoutesRequest.Get(providerid).NetResourceLocator,energy, isAuthoritative,  _=>provider.Get(RoutesRequest.GetGlobal()).Guard());

            _provider.Add(providerid, provider);
            var routeresource = _kernel.Get(RoutesRequest.Get(providerid)).Guard();

            var routelist = MediaFormatter<RoutesInformation>.Parse(routeresource, RoutesInformation.Mediatype);

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

        private IEnumerable<RouteEntry> PublicRoutes()
        {
            var ri = _routes.OfType<ImmediateKernelRoute>().Select(_=>new RouteEntry{Identifier=_.Nri, Energy = _.Energy});
            ri = ri.Union(_routes.OfType<RegexKernelRoute>().Select(_ => new RouteEntry { Regex = _.NriRegex, Energy = _.Energy }));
            return ri.ToList();
        }

        public void EnableRoutePublication()
        {
            RegisterResourceHandler(Guid.NewGuid(), "net://@", 0, true, r => MediaFormatter<RoutesInformation>.Pack("net://@", new RoutesInformation { Routes = PublicRoutes().ToList() }, "json", RoutesInformation.Mediatype));
        }

        public void RegisterResourceForwarder(Guid routeGroupId, string nriregex, ResourceProvider provider, long energy)
        {
            _routes.Add(new ForwardKernelRoute(routeGroupId, nriregex, energy,true,  provider));
        }

        public void DeleteRoute(Guid routeGroupId)
        {
            _routes.Where(_ => _.GroupId == routeGroupId).ToList().ForEach(_ => _routes.Remove(_));
        }

        public IEnumerable<KernelRoute> Lookup(string nri, bool ignorecache)
        {
            return _routes.Where(_=>!ignorecache||_.IsAuthoritative).Where(_ => _.Match(nri)).OrderBy(_ => _.Energy).ToList();
        }



        public void Reset()
        {
            _routes.Clear();
        }
    }
}
