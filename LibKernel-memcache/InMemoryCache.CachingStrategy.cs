using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibKernel;

namespace LibKernel_memcache
{
	public partial class InMemoryCache 
	{

        private void SetCachingStrategyDefaults()
        {
            MinCachableEnergy = 100;
            MaxCachableSize = 1024*1024;
            MinimumExpirationTimesEnergyFactor = 2;
            EnergySizeTradeoffFactor = 1;
        }

	    public long MinCachableEnergy { get; set; }
		public long MaxCachableSize { get; set; }
		public int MinimumExpirationTimesEnergyFactor { get; set; }
		public int EnergySizeTradeoffFactor { get; set; }

		private bool CheckCachingStrategy(ResourceRepresentation resource)
		{
		    return 
                resource.Expires>DateTime.Now.AddMilliseconds(250)
                && resource.Energy>=MinCachableEnergy
                && resource.Size<=MaxCachableSize
                && (resource.Expires-DateTime.Now).TotalMilliseconds 
                    >=resource.Energy*MinimumExpirationTimesEnergyFactor
                && resource.Energy>=resource.Size*EnergySizeTradeoffFactor
                ;
		}
	}
}
