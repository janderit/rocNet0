using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LibKernel;
using LibKernel_http;
using LibKernel_zmq;
using NUnit.Framework;

namespace KernelTests.ZeroMq_Transport
{
    [TestFixture]
    class Resource_Access
    {

        private InProcessKernel _kernel;
        private ZeroMqResourceProviderFacade _facade;

        [SetUp]
        public void StartKernel()
        {
            _kernel = new InProcessKernel();
            _facade = new ZeroMqResourceProviderFacade(_kernel, "tcp://127.0.0.1:15674");
            _facade.Start();
        }

        [TearDown]
        public void StopKernel()
        {
            _facade.Stop();
            _kernel.Reset();
            _facade = null;
            _kernel = null;
        }

        private const string MyRegex = @"^net://fib/(?<i>[0-9]+)$";




        [Test]
        public void Fibonacci()
        {
            _kernel.Routes.RegisterResourceHandlerRegex(Guid.NewGuid(), MyRegex, 1, ServeFibonacci);

            var conn = new ZeroMqResourceProviderConnector("tcp://localhost:15674");

            var resp = conn.Get(new Request {NetResourceLocator = "net://fib/10"});

            Assert.AreEqual(55, Int32.Parse(resp.Resource.Body));
        }

        private Int64 Fibonacci(Int64 p)
        {
            return
                Int64.Parse(
                    _kernel.Get(new Request
                                    {
                                        NetResourceLocator = "net://fib/" + p,
                                        AcceptableMediaTypes = new[] {"text/longint"}
                                    }).Resource.Body);
        }

        private ResourceRepresentation ServeFibonacci(Request req)
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
                           Size=result.ToString().Length
                       };
        }


    }
}
