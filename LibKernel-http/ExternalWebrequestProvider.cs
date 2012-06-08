using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using LibKernel;

namespace LibKernel_http
{
    public class ExternalWebrequestProvider 
    {
        private static readonly Guid MyGuid = new Guid("77B04DC3-6D54-4BA6-AFB2-E4935D5DA248");
        public void Register(KernelRegistration kernel)
        {
            kernel.Routes.DeleteRoute(MyGuid);
            kernel.Routes.RegisterResourceMapping(MyGuid, "^http://(?<url>.+)$" , "net://www?url=${url}");
            kernel.Routes.RegisterResourceHandlerRegex(MyGuid, @"^net://www\?url=(?<url>.*)$", 250,true, Get);
        }

        public ResourceRepresentation Get(Request request)
        {

            var url = Regex.Replace(request.NetResourceLocator, @"^net://www\?url=(?<url>.*)$", "${url}");

            var t0 = Environment.TickCount;
            var wc = new WebClient();
            var s = new StreamReader(wc.OpenRead("http://"+url)).ReadToEnd();
            return /*new Response
                       {
                           Status=ResponseCode.Ok,
                           Resource=*/new ResourceRepresentation
                       {
                           Body = s,
                           Cacheable = true,
                           Correlations = null,
                           Energy = Environment.TickCount - t0,
                           Expires = DateTime.Now.AddDays(1),
                           //Headers = wc.ResponseHeaders.AllKeys.Select(_ => "HTTP-"+ _ + ": " + wc.ResponseHeaders[_]),
                           MediaType = "text/html",
                           Modified = DateTime.Now,
                           NetResourceIdentifier = "net://www?url="+url,
                           Relations = null,
                           RevokationTokens = null,
                           Size = s.Length,
                           //Via = new[] {"WebrequestProvider v0.1"}.ToList()
                       /*}*/
                       };
        }

    }
}
