﻿using System;
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
            RevokationTokens = new List<Guid>();
            Correlations = new List<Guid>();            
        }


        public string NetResourceIdentifier;
        public string MediaType;
        public IEnumerable<string> Relations;
        public long Energy;
        public int Size;

        public IEnumerable<Guid> Correlations;
        public IEnumerable<Guid> RevokationTokens;

        public bool Cacheable;
        public DateTime Modified;
        public DateTime Expires;
        public string Body;
    }
}
