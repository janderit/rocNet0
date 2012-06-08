using System;
using System.Collections.Generic;
using LibKernel;

namespace LibKernel_memcache
{
    class CacheEntry
    {
        internal ResourceRepresentation Resource;
        internal int Hits;
        internal DateTime Last;
        internal List<string> OriginalVia;
        internal int OriginalRetrievalTime;
    }
}