using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel
{
    public class Request
    {
        public Request()
        {
            Timestamp = DateTime.Now;
        }

        public string NetResourceLocator;
        public bool IgnoreCached;
        public IEnumerable<string> AcceptableMediaTypes;
        public DateTime Timestamp;
    }
}
