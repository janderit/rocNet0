using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibKernel;
using LibKernel.MediaFormats;
using ZMQ;

namespace LibKernel_zmq
{
    public class ZeroMqKernelConnector : Kernel
    {
        private string _zmqUrl;
        private ZeroMqDatagramFormatter _formatter;
        private ZeroMqConnector _conn;

        public void Configure(string zmqUrl)
        {
            if (zmqUrl == null) throw new ArgumentNullException("zmqUrl");
            _zmqUrl = zmqUrl;
            _formatter = new ZeroMqDatagramFormatter();
            _conn = new ZeroMqConnector(zmqUrl, false);
        }


        public Response Get(ResourceRequest request)
        {
            return _formatter.DeserializeResponse(_conn.Transact(_formatter.Serialize(request)));
        }

        public void Get(ResourceRequest request, Action<Response> response)
        {
            throw new NotImplementedException();
        }

        public void Get(ResourceRequest request, Action<ResourceRepresentation> resource, Action<Response> onFailure)
        {
            throw new NotImplementedException();
        }



    }
}
