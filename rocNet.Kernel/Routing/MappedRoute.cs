using System.Text.RegularExpressions;
using LibKernel;

namespace rocNet.Kernel.Routing
{
    internal class MappedRoute
    {
        private readonly string _nriregex;
        private readonly string _replacement;
        private readonly ResourceProvider _kernel;

        public MappedRoute(string nriregex, string replacement, ResourceProvider kernel)
        {
            _nriregex = nriregex;
            _replacement = replacement;
            _kernel = kernel;
        }

        public ResourceRepresentation Map(Request req)
        {
            var mapped = Regex.Replace(req.NetResourceLocator, _nriregex, _replacement);
            return
                _kernel.Get(new Request
                                {
                                    Timestamp = req.Timestamp,
                                    NetResourceLocator = mapped,
                                    AcceptableMediaTypes = req.AcceptableMediaTypes
                                }).Resource;
        }
    }
}