using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel
{
    public class Response
    {
        public Response()
        {
            Via = new List<string>();
            XCache = XCache.None;
        }

        public ResponseCode Status { get; set; }
        public ResourceRepresentation Resource { get; set; }
        public string Information { get; set; }
        public List<string> Via { get; set; }
        public XCache XCache { get; set; }
        public Int32 RetrievalTimeMs { get; set; }
    }

    public enum XCache { None, CacheHit, Cached, Ignored }

}
