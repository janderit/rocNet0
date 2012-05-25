using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibKernel.Exceptions
{
    [Serializable]
    public class MediaTypeException : KernelException
    {
        public MediaTypeException()
        {
        }

        private MediaTypeException(string resource)
            : base("Resource with unexpected media type: " + resource)
        {
        }

        public static MediaTypeException Create(string expectedMediaType, string encounteredMediaType, string info)
        {
            var ex = new MediaTypeException(info);
            ex.Data.Add("Expected", expectedMediaType);
            ex.Data.Add("Found", encounteredMediaType);
            ex.Data.Add("Resource", info);
            return ex;
        }

    }
}
