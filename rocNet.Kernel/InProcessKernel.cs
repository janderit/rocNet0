using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibKernel.Exceptions;

namespace LibKernel
{
    public class InProcessKernel : ResourceProvider, KernelRegistration
    {

        private readonly Router _router;
        private List<PostProcessHook> _hooks = new List<PostProcessHook>();

        public InProcessKernel()
        {
            _router = new Router(this);
        }

        public void AddHook(PostProcessHook hook)
        {
            _hooks.Add(hook);
        }

        public ResourceRepresentation Get(string nrl)
        {
            var response = Get(new Request {NetResourceLocator = nrl, AcceptableMediaTypes = new[] {@"*\+*"}});
            if (response.Status==ResponseCode.Ok) return response.Resource;

            if (response.Status == ResponseCode.NotFound) throw ResourceNotFoundException.Create(response.Information, "");
            if (response.Status == ResponseCode.InternalError) throw ResourceUnavailableException.Create("Internal error", response.Information);
            throw ResourceUnavailableException.Create("Unknown status code (" + response.Status + ")", response.Information);
        }

        public Response Get(Request request)
        {
            return _hooks.Aggregate(DoGet(request), (current, postProcessHook) => postProcessHook.PostProcess(request, current));
        }

        private Response DoGet(Request request)
        {
            if (!request.NetResourceLocator.Contains("://")) request.NetResourceLocator = "net://" + request.NetResourceLocator;

            var route = _router.Lookup(request.NetResourceLocator);
            if (route == null) return new Response{Status=ResponseCode.NotFound, Information="No route to resource"};
            try
            {
                var resp = new Response { Status = ResponseCode.Ok, Information = "Ok", Resource = route.Handler(request) };
                return resp;
            }
            catch (Exception ex)
            {
                return new Response {Status = ResponseCode.InternalError, Information = ex.Message + " " + ex.GetType().FullName};
            }
        }

        public ResourceRegistry Routes { get { return _router; } }


        public void Reset()
        {
            _router.Reset();
        }
    }
}
