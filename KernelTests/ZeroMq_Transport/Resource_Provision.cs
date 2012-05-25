using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibKernel;
using LibKernel_zmq;
using NUnit.Framework;

namespace KernelTests.ZeroMq_Transport
{
    [TestFixture]
    class Resource_Provision
    {

        private InProcessKernel _kernel;
        private ResourceProvider _service;
        private ZeroMqResourceProviderFacade _provider;


        [SetUp]
        public void Start()
        {

            StartKernel();
            StartProvider();
        }

        private void StartKernel(){

            _kernel = new InProcessKernel();
            var plugin = new ZeroMqResourceProviderConnector("tcp://localhost:15700");
            _kernel.Routes.RegisterResourceHandler(Guid.NewGuid(), "net://test", 10, r => plugin.Get(r).Resource);
        }

        private void StartProvider()
        {
            var s = new InProcessKernel();
            s.Routes.RegisterResourceHandler(Guid.NewGuid(), "net://test", 0, r => new ResourceRepresentation { NetResourceIdentifier="net://test" ,MediaType="text",Body = "Hello World" });
            _service = s;
            _provider = new ZeroMqResourceProviderFacade(_service, "tcp://127.0.0.1:15700");
            _provider.Start();
        }

        [TearDown]
        public void StopAll()
        {
            _provider.Stop();
            _kernel.Reset();
            _provider = null;
            _kernel = null;
        }

        [Test]
        public void Loopback()
        {
            Assert.AreEqual("Hello World", _service.Get(new ResourceRequest { NetResourceLocator = "net://test" }).Resource.Body);
        }

        [Test]
        public void Direct()
        {
            Assert.AreEqual("Hello World", new ZeroMqResourceProviderConnector("tcp://localhost:15700").Get(new ResourceRequest { NetResourceLocator = "net://test" }).Resource.Body);
        }

        [Test]
        public void Chained()
        {
            Assert.AreEqual("Hello World", _kernel.Get("net://test").Body);
        }

    }
}
