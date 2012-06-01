using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LibKernel;
using LibKernel_http;
using NUnit.Framework;
using rocNet.Kernel;

namespace KernelTests.Basic_Functionality
{
    [TestFixture]
    class Examples
    {

        private InProcessKernel _kernel;

        [SetUp]
        public void StartKernel()
        {
            _kernel = new InProcessKernel();
        }

        [TearDown]
        public void StopKernel()
        {
            _kernel.Reset();
        }

        private const string MyRegex = @"^net://fib/(?<i>[0-9]+)$";

        [Test]
        public void Fibonacci()
        {
            _kernel.Routes.RegisterResourceHandlerRegex(Guid.NewGuid(), MyRegex, 1, true, ServeFibonacci);
            Assert.AreEqual(55, Fibonacci(10));
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
                           Correlations = null,
                           Expires = DateTime.MaxValue,
                           Headers = null,
                           MediaType = "text/longint",
                           Modified = DateTime.Now,
                           NetResourceIdentifier = "net://fib/" + p,
                           Relations = null,
                           RevokationTokens = null,
                           Via = null,
                           Energy = Environment.TickCount - t0,
                           Size=result.ToString().Length
                       };
        }


        [Test]
        public void Google()
        {
            var provider = new ExternalWebrequestProvider();
            provider.Register(_kernel);

            var resource = _kernel.Get("http://www.google.com");
            Assert.IsTrue(resource.Body.Contains("<html"));
        }


        [Test]
        public void Google_via_Map()
        {
            var provider = new ExternalWebrequestProvider();
            provider.Register(_kernel);

            _kernel.Routes.RegisterResourceMapping(Guid.NewGuid(), "net://google", "http://www.google.de");

            var resource = _kernel.Get("net://google");
            Assert.IsTrue(resource.Body.Contains("<html"));
        }

    }
}
