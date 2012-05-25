using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel
{
    public class Response
    {
        public ResponseCode Status { get; set; }
        public ResourceRepresentation Resource { get; set; }
        public string Information { get; set; }
    }
}
