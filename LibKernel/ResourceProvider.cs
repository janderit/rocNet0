using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel
{
    public interface ResourceProvider
    {
        Response Get(Request request);
        void InformRequestNumber(long backlog);
        long Estimate(Request request);
        void InformLag(TimeSpan delay);
        void InformQueue(int count);
    }
}
