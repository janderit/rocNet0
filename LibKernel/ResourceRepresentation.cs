using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel
{
    public class ResourceRepresentation
    {
        public ResourceRepresentation()
        {
            Cacheable = false;
            Expires = DateTime.Now;
            Modified = DateTime.Now;

            Relations = new List<string>();
            Via = new List<string>();
            RevokationTokens = new List<Guid>();
            Correlations = new List<Guid>();
            Headers = new List<string>();
        }


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

        public void AddHeader(string key, string value)
        {
            Headers = Headers.Union(new[] {Header(key, value)}).ToList();
        }

        public static string Header(string key, string value) { return string.Format("{0}: {1}", key, value); }

    }
}
