using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel.Exceptions
{
    [Serializable]
    public class MediaFormatNotSupportedException : KernelException
    {
        public MediaFormatNotSupportedException()
        {
        }

        private MediaFormatNotSupportedException(string format)
            : base("media format not supported: " + format)
        {
        }

        public static MediaFormatNotSupportedException Create(string format, string info)
        {
            var ex = new MediaFormatNotSupportedException(info);
            ex.Data.Add("Requested", format);
            ex.Data.Add("Resource", info);
            return ex;
        }

    }
}
