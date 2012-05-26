using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibKernel;
using LibKernel.MediaFormats;
using ZMQ;

namespace LibKernel_zmq
{
    public class ZeroMqResourceProviderConnector : ResourceProvider
    {
        private string _zmqUrl;
        private ZeroMqDatagramFormatter _formatter;
        private ZeroMqConnector _conn;

        public ZeroMqResourceProviderConnector(string zmqUrl)
        {
            if (zmqUrl == null) throw new ArgumentNullException("zmqUrl");
            _zmqUrl = zmqUrl;
            _formatter = new ZeroMqDatagramFormatter();
            _conn = new ZeroMqConnector(zmqUrl, false);
        }


        public Response Get(Request request)
        {            
            return _formatter.DeserializeResponse(_conn.Transact(_formatter.Serialize(request)));
        }

        public IEnumerable<string> GetRaw(Request request)
        {
            return (_conn.Transact(_formatter.Serialize(request))).ToList();
        }

    }
}
