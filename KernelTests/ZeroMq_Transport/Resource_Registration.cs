using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibKernel;
using LibKernel_zmq;
using NUnit.Framework;
using rocNet.Kernel;

namespace KernelTests.ZeroMq_Transport
{

    [TestFixture]
    class Resource_Registration
    {

        private InProcessKernel _kernel;
        private ResourceProvider _service;
        private ZeroMqResourceProviderFacade _provider;
        private ZeroMqResourceProviderConnector _plugin;


        [SetUp]
        public void Start()
        {
            StartProvider();
            StartKernel();
        }

        private void StartKernel()
        {

            _kernel = new InProcessKernel();
            _plugin = new ZeroMqResourceProviderConnector("tcp://localhost:15700");
            _kernel.Routes.RegisterResourceProvider(_plugin,2,true);
        }

        private void StartProvider()
        {
            var s = new InProcessKernel();
            s.Routes.RegisterResourceHandler(Guid.NewGuid(), "net://test", 0,true, r => new ResourceRepresentation { NetResourceIdentifier = "net://test", MediaType = "text", Body = "Hello World" });
            s.Routes.EnableRoutePublication();
            _service = s;
            _provider = new ZeroMqResourceProviderFacade(_service, "tcp://127.0.0.1:15700");
            _provider.Start();
        }

        [TearDown]
        public void StopAll()
        {
            _plugin.Close();
            _provider.Stop();
            _kernel.Reset();
            _provider = null;
            _kernel = null;
        }

        [Test, Category("ZeroMQ")]
        public void Chained()
        {
            Assert.AreEqual("Hello World", _kernel.Get("net://test").Body);
        }

    }
}
