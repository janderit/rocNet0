using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibTicker.WellKnown
{
    public static class Trigger
    {
        public static readonly Guid Hourly = new Guid("458AF725-EBF0-43C5-BAC6-946BDAE78DAB");
        public static readonly Guid Daily = new Guid("0036F9BB-5115-4C09-8839-E10B40F6FFB7");
        public static readonly Guid Weekly = new Guid("EAC245EF-E8A0-4712-8085-DEB9C24D03FF");
        public static readonly Guid Monthly = new Guid("281AA61C-EDD6-4F86-A5B6-727CAA97ECCF");

        public static readonly Guid ResourceChanged = new Guid("034164E5-7333-4FA1-9D7A-EEA171F762B5");
    }
}
