using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibKernel.Exceptions;

namespace LibKernel
{
    public static class ResponseGuardExtension
    {
        public static Response Guard(this Response response, Action<ResponseCode, string> onError=null)
        {
            if (response == null) throw new ArgumentNullException("response");

            if (response.Status == ResponseCode.Ok && response.Resource==null) throw ResourceUnavailableException.Create("Internal error, missing resource", "");
            if (response.Status==ResponseCode.Ok) return response;

            if (onError != null) onError(response.Status, response.Information);

            if (response.Status==ResponseCode.NotFound) throw ResourceNotFoundException.Create(response.Information, "");
            if (response.Status==ResponseCode.InternalError) throw ResourceUnavailableException.Create("Internal error", response.Information);
            throw ResourceUnavailableException.Create("Unknown status code ("+response.Status+")", response.Information);
        }
    }
}
