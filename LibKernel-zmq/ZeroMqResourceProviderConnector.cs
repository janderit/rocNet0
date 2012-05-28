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

        public ZeroMqResourceProviderConnector(string zmqUrl, bool singletonsocket=true)
        {
            if (zmqUrl == null) throw new ArgumentNullException("zmqUrl");
            _zmqUrl = zmqUrl;
            _formatter = new ZeroMqDatagramFormatter();
            _conn = new ZeroMqConnector(zmqUrl, singletonsocket);
        }

        public int Timeout {get { return _conn.TimeoutMicroseconds; }
            set { _conn.TimeoutMicroseconds = value; }
        }


        public Response Get(Request request)
        {
            try
            {
                return _formatter.DeserializeResponse(_conn.Transact(_formatter.Serialize(request)));
            }
            catch
            {
                Console.WriteLine("*************** SOCKET RESET ******************");
                _conn.ResetSingleton();
                _conn = new ZeroMqConnector(_zmqUrl, true);
                throw;
            }
        }

        public void InformQueue(long backlog)
        {
            // N/A
        }

        public IEnumerable<string> GetRaw(Request request)
        {
            try
            {
                return ((_conn.Transact(_formatter.Serialize(request))) ?? new List<string> { "DATA ERROR" }).ToList();
            }
            catch
            {
                Console.WriteLine("*************** SOCKET RESET ******************");
                _conn.ResetSingleton();
                _conn = new ZeroMqConnector(_zmqUrl, true);
                throw;
            }
            
        }

    }
}
