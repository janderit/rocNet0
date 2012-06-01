using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel_memcache
{
    [Serializable]
    public class CacheInformation
    {

        public static string Mediatype { get { return "vnd.rocnet.cacheinformation;v=1"; } }

        public CacheInformation()
        {
            Configuration = new CacheConfiguration();
            Statistics = new CacheStatistics();
        }

        public CacheConfiguration Configuration { get; set; }
        public CacheStatistics Statistics { get; set; }
    }
}
