using LibTicker;
using LibTicker.Clients;
using LibTicker_zmq.Clients;

namespace TickerTests
{
    public static class Ticker
    {
        public static class Publisher
        {
            public static TickerPublisher Loopback()
            {
                return new LoopbackPublisher();
            }

            public static TickerPublisher ZeroMq(string uri, string ident)
            {
                return new ZeroMqPublisher(uri, ident);
            }
        }


        public static class Listener
        {
            public static TickerService Loopback()
            {
                return new LoopbackListener();
            }

            public static TickerService ZeroMq(string inbandUri, string outofbandUri)
            {
                return new ZeroMqListener(inbandUri, outofbandUri);
            }
        }
    }
}
