using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Text.RegularExpressions;
using System.Reactive;
using System.Reactive.Linq;
using LibKernel;
using LibKernel_http;
using LibKernel_memcache;
using LibKernel_zmq;

namespace kernel
{
    class Program
    {
        private static InProcessKernel _kernel;

        private const string MyRegex = @"^net://fib/(?<i>[0-9]+)$";

        static void Main(string[] args)
        {

            Console.Write("0MQ port>");
            var port = Console.ReadLine();            
            var uri = "tcp://127.0.0.1:"+port;
            
            _kernel = new InProcessKernel();

            _kernel.Routes.RegisterResourceHandler(Guid.NewGuid(), "net://test", 0, r => new ResourceRepresentation { NetResourceIdentifier = "net://hello", MediaType = "text", Body = "Hello World" });

            _kernel.Routes.RegisterResourceMapping(Guid.NewGuid(), "^net://morning/(?<a>.+)$", "net://greeting/morning/${a}");
            _kernel.Routes.RegisterResourceHandlerRegex(Guid.NewGuid(), "^net://greeting/morning/.+$", 2, Morning );
            _kernel.Routes.RegisterResourceHandlerRegex(Guid.NewGuid(), MyRegex, 1, ServeFibonacci);
            var web = new ExternalWebrequestProvider();
            web.Register(_kernel);
            _kernel.Routes.EnableRoutePublication();

            var cache = new SingleThreadedInMemoryCache();
            //var cache = new CacheMultithreadingFacade(new SingleThreadedInMemoryCache());
            ResourceCacheKernelAdapter.AttachTo(_kernel, ResourceCacheKernelAdapter.GenerateFallback(_kernel.Get), cache);

            cache.EnergySizeTradeoffFactor = 1;
            cache.MinCachableEnergy = 2;
            cache.MinimumExpirationTimesEnergyFactor = 4;

            cache.MaxResourcesInCache = 30;
            cache.RemovalChunkSize = 6;
            cache.MaxCacheDurationSeconds = 30;

            var p = new ZeroMqResourceProviderFacade(_kernel, uri);

            p.InThreadHandlingLimit = Int64.MaxValue;

            p.Start();

            Console.WriteLine("Kernel running on "+uri+" ...");
            Console.ReadLine();
            p.Stop();
        }

        private static ResourceRepresentation Morning(Request arg)
        {
            var name = arg.NetResourceLocator.Replace("net://", "")
                .Replace("greeting/", "")
                .Replace("morning/", "");

            return new ResourceRepresentation
                {
                    Cacheable=true,
                    Expires=DateTime.Now.AddSeconds(30),
                    NetResourceIdentifier = "net://greeting/morning/" + name,
                    MediaType = "text",
                    Body = "Good morning to you, " + name + ", too!"
                };
        }

        private static Int64 Fibonacci(Int64 p)
        {
            return
                Int64.Parse(
                    _kernel.Get(new Request
                    {
                        NetResourceLocator = "net://fib/" + p,
                        AcceptableMediaTypes = new[] { "text/longint" }
                    }).Resource.Body);
        }

        private static ResourceRepresentation ServeFibonacci(Request req)
        {
            var p = Int64.Parse(Regex.Replace(req.NetResourceLocator, MyRegex, "${i}"));

            var t0 = Environment.TickCount;
            var result = (p < 2) ? p : Fibonacci(p - 1) + Fibonacci(p - 2);

            return new ResourceRepresentation
            {
                Body = result.ToString(),
                Cacheable = true,
                Expires = DateTime.MaxValue,
                MediaType = "text/longint",
                Modified = DateTime.Now,
                NetResourceIdentifier = "net://fib/" + p,
                Energy = Environment.TickCount - t0,
                Size = result.ToString().Length
            };
        }

    }
}
