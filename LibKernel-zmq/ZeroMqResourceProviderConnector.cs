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
                ResetSocket();
                throw;
            }
        }

        private void ResetSocket()
        {
            Console.WriteLine("*************** SOCKET RESET ******************");
            var cnew = new ZeroMqConnector(_zmqUrl, true);
            var cold = _conn;
            cnew.ResetSingleton();
            _conn = cnew;
            cold.Dispose();
        }

        public void InformRequestNumber(long backlog)
        {
            // N/A
        }

        public long Estimate(Request request)
        {
            return 0;
        }

        public void InformLag(TimeSpan delay)
        {
        }

        public void InformQueue(int count)
        {
            
        }

        public IEnumerable<string> GetRaw(Request request)
        {
            try
            {
                return ((_conn.Transact(_formatter.Serialize(request))) ?? new List<string> { "DATA ERROR" }).ToList();
            }
            catch
            {
                ResetSocket();
                throw;
            }
            
        }

        public void Close()
        {
            _conn.Dispose();
        }

    }
}
