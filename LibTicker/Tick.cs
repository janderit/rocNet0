using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibTicker
{
    public struct Tick
    {
        public Guid Subject;
        public Guid Trigger;
        public string Data;
        public long Serial;
        public DateTime Publication;
    }
}
