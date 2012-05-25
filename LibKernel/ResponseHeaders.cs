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

        public const string MediaType = "MediaType";
        public const string Cacheable = "Allow-Cache";
        public const string Expires = "Expires";
        public const string NetResourceIdentifier = "NetResourceIdentifier";
        public const string Modified = "Modified";
        public const string Energy = "Energy";
        public const string Size = "Size";
        public const string Via = "Seen-by";
        public const string Correlation = "Correlated";
        public const string Relation = "Link-related";
        public const string Revokation = "Revoke-on";
    }
}
