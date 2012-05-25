using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel.Exceptions
{
    [Serializable]
    public abstract class KernelException : Exception
    {
        protected KernelException(){}

        protected KernelException(string message):base(message){}
    }
}
