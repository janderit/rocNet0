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
    class Provider_Registration
    {
        private InProcessKernel _kernel;
        private InProcessKernel _provider;
        private ZeroMqResourceProviderFacade _host;
        private SimpleCometResourceProvider _comet;

        [SetUp]
        public void Start()
        {
            _kernel = new InProcessKernel();
            _provider = new InProcessKernel();

            _provider.Routes.RegisterResourceHandler(Guid.NewGuid(), "net://hello", 10, true,
                                                     r =>
                                                     new ResourceRepresentation
                                                         {
                                                             Body = "World",
                                                             //MediaType = "text/plain",
                                                             NetResourceIdentifier = "net://hello"
                                                         });

            _host = new ZeroMqResourceProviderFacade(_kernel as ResourceProvider, "tcp://127.0.0.1:17999");
            _host.Start();

            _comet = new SimpleCometResourceProvider(Guid.NewGuid(), "tcp://127.0.0.1:17999", _provider);
        }

        [TearDown]
        public void Stop()
        {
            _comet.Dispose();
            _host.Stop();
        }

        [Test, Ignore("Not finished")]
        public void DirectAccess()
        {

            var rep = _provider.Get("net://hello");
            Assert.IsNotNull(rep);
            Assert.AreEqual("World", rep.Body);

        }

        [Test, Ignore("Not finished")]
        public void CometAccess()
        {
            var conn = new ZeroMqResourceProviderConnector("tcp://127.0.0.1:17999");
            var rep = conn.Get(new Request {NetResourceLocator = "net://hello"});
            conn.Close();
            Assert.IsNotNull(rep);
            Assert.IsNotNull(rep.Resource);
            Assert.AreEqual("World", rep.Resource.Body);

        }

        [Test, Ignore("Not finished")]
        public void FullZmqAccess()
        {

            var rep = _kernel.Get("net://hello");
            Assert.IsNotNull(rep);
            Assert.AreEqual("World", rep.Body);

        }

    }
}
