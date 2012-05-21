using System;

namespace LibTicker
{
    public static class Ticker
    {
        public static class Publisher
        {
            public static TickerPublisher Loopback()
            {
                return new Clients.LoopbackPublisher();
            }

            public static TickerPublisher ZeroMq(string uri, string ident)
            {
                return new Clients.ZeroMqPublisher(uri, ident);
            }
        }


        public static class Listener
        {
            public static TickerService Loopback()
            {
                return new Clients.LoopbackListener();
            }

            public static TickerService ZeroMq(string inbandUri, string outofbandUri)
            {
                return new Clients.ZeroMqListener(inbandUri, outofbandUri);
            }
        }
    }
}
