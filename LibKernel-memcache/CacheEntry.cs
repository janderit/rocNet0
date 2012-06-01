using System;
using LibKernel;

namespace LibKernel_memcache
{
    class CacheEntry
    {
        internal ResourceRepresentation Resource;
        internal int Hits;
        internal DateTime Last;
    }
}