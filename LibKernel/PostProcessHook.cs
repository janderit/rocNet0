using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel
{
    public interface PostProcessHook
    {
        Response PostProcess(Request request, Response response);
    }
}
