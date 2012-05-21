using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibKernel;
using NUnit.Framework;

namespace KernelTests.Basic_Functionality
{
    [TestFixture]
    class Routing
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

        [Test]
        public void Smoke()
        {
            _kernel.Routes.RegisterImmediate(Guid.NewGuid(), "net://test", 1, a => { throw new NotImplementedException(); });
            Assert.True(true);
        }

        [Test,ExpectedException(typeof(FileNotFoundException))]
        public void FileNotFound()
        {
            _kernel.Get("net://nonesense.HJGASD");
            Assert.Fail();
        }

        [Test]
        public void Immediate()
        {
            _kernel.Routes.RegisterImmediate(Guid.NewGuid(), "net://test1", 1, a => new ResourceRepresentation
                                                                                       {
                                                                                           Body="TEST1",
                                                                                           Cacheable=false
                                                                                       });

            _kernel.Routes.RegisterImmediate(Guid.NewGuid(), "net://test2", 1, a => new ResourceRepresentation
            {
                Body = "TEST2",
                Cacheable = false
            });

            _kernel.Routes.RegisterImmediate(Guid.NewGuid(), "net://test3", 1, a => new ResourceRepresentation
            {
                Body = "TEST3",
                Cacheable = false
            });

            Assert.AreEqual("TEST2", _kernel.Get("net://test2").Body);
        }

        [Test, ExpectedException(typeof(FileNotFoundException))]
        public void Route_Removal()
        {
            _kernel.Routes.RegisterImmediate(Guid.NewGuid(), "net://test1", 1, a => new ResourceRepresentation
            {
                Body = "TEST1",
                Cacheable = false
            });

            var id = Guid.NewGuid();
            _kernel.Routes.RegisterImmediate(id, "net://test2", 1, a => new ResourceRepresentation
            {
                Body = "TEST2",
                Cacheable = false
            });

            _kernel.Routes.RegisterImmediate(Guid.NewGuid(), "net://test3", 1, a => new ResourceRepresentation
            {
                Body = "TEST3",
                Cacheable = false
            });

            _kernel.Routes.DeleteRoute(id);

            _kernel.Get("net://test2");
            Assert.Fail();
        }

        [Test]
        public void Simple_Regex()
        {
            _kernel.Routes.RegisterRegex(Guid.NewGuid(), "^net://test.$", 1, a => new ResourceRepresentation
            {
                Body = a.NetResourceLocator.Substring(6).ToUpper(),
                Cacheable = false
            });

            Assert.AreEqual("TEST2", _kernel.Get("net://test2").Body);
        }

    }
}
