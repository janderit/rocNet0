using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel
{
    public struct ResourceRepresentation
    {
        public string NetResourceIdentifier;
        public string MediaType;
        public IEnumerable<string> Relations;
        public IEnumerable<string> Via;
        public long Energy;
        public int Size;

        public IEnumerable<Guid> Correlations;
        public IEnumerable<Guid> RevokationTokens;

        public bool Cacheable;
        public DateTime Modified;
        public DateTime Expires;

        public IEnumerable<string> Headers;
        public string Body;
    }
}
