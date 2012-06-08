using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel
{
    public static class ResponseHeaders
    {
        public static string Assemble(string header, string value)
        {
            return header.Trim() + ": " + value.Trim();
        }

        public const string Retrieval = "Retrieval";
        public const string XCache = "X-CACHE";
        public const string Size = "Size";
        public const string Via = "Seen-by";
    }
}
