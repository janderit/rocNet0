using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel
{
    public struct ResourceRequest
    {
        public string NetResourceLocator;
        public IEnumerable<string> AcceptableMediaTypes;
    }
}
