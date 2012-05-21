using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibKernel
{
    public class InProcessKernel : Kernel, KernelRegistration
    {

        private readonly Router _router = new Router();

        public ResourceRepresentation Get(string nrl)
        {
            return Get(new ResourceRequest {NetResourceLocator = nrl, AcceptableMediaTypes = new[] {@"*\+*"}});
        }
        
        public ResourceRepresentation Get(ResourceRequest request)
        {
            var handler = _router.Lookup(request.NetResourceLocator);
            if (handler == null) throw new FileNotFoundException(request.NetResourceLocator);
            return handler(request);
        }

        public IRegisterRoutes Routes { get { return _router; } }


        public void Reset()
        {
            _router.Reset();
        }
    }
}
