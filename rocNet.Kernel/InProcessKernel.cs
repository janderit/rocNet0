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

        public InProcessKernel()
        {
            _router = new Router(this);
        }

        public ResourceRepresentation Get(string nrl)
        {
            var response = Get(new ResourceRequest {NetResourceLocator = nrl, AcceptableMediaTypes = new[] {@"*\+*"}});
            if (response.Status==ResponseCode.Ok) return response.Resource;

            if (response.Status == ResponseCode.NotFound) throw ResourceNotFoundException.Create(response.Information, "");
            if (response.Status == ResponseCode.InternalError) throw ResourceUnavailableException.Create("Internal error", response.Information);
            throw ResourceUnavailableException.Create("Unknown status code (" + response.Status + ")", response.Information);
        }

        public Response Get(ResourceRequest request)
        {
            var handler = _router.Lookup(request.NetResourceLocator);
            if (handler == null) return new Response{Status=ResponseCode.NotFound, Information="No route to resource"};
            try
            {
                return new Response { Status = ResponseCode.Ok, Information = "Ok", Resource = handler(request) };
            }
            catch (Exception ex)
            {
                return new Response {Status = ResponseCode.InternalError, Information = ex.Message + " " + ex.GetType().FullName};
            }
        }

        public void Get(ResourceRequest request, Action<Response> response)
        {
            throw new NotImplementedException();
        }

        public void Get(ResourceRequest request, Action<ResourceRepresentation> resource, Action<Response> onFailure)
        {
            throw new NotImplementedException();
        }

        public ResourceRegistry Routes { get { return _router; } }


        public void Reset()
        {
            _router.Reset();
        }
    }
}
