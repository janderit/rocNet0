using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel
{
    public enum ResponseCode
    {
        Ok = 200,
        NotFound = 404,
        InternalError = 500,
        CommunicationsError=600
    }
}
