using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibKernel;
using LibKernel.Exceptions;
using NUnit.Framework;
using rocNet.Kernel;

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

        [Test, Category("Smoke")]
        public void Smoke()
        {
            _kernel.Routes.RegisterResourceHandler(Guid.NewGuid(), "net://test", 1, true, a => { throw new NotImplementedException(); });
            Assert.True(true);
        }

        [Test, ExpectedException(typeof(ResourceNotFoundException)), Category("Negative")]
        public void FileNotFound()
        {
            _kernel.Get("net://nonesense.HJGASD");
            Assert.Fail();
        }

        [Test, Category("Unit")]
        public void Immediate()
        {
            _kernel.Routes.RegisterResourceHandler(Guid.NewGuid(), "net://test1", 1, true, a => new ResourceRepresentation
                                                                                       {
                                                                                           Body="TEST1",
                                                                                           Cacheable=false
                                                                                       });

            _kernel.Routes.RegisterResourceHandler(Guid.NewGuid(), "net://test2", 1, true, a => new ResourceRepresentation
            {
                Body = "TEST2",
                Cacheable = false
            });

            _kernel.Routes.RegisterResourceHandler(Guid.NewGuid(), "net://test3", 1, true, a => new ResourceRepresentation
            {
                Body = "TEST3",
                Cacheable = false
            });

            Assert.AreEqual("TEST2", _kernel.Get("net://test2").Body);
        }

        [Test, Category("Unit"), ExpectedException(typeof(ResourceNotFoundException))]
        public void Route_Removal()
        {
            _kernel.Routes.RegisterResourceHandler(Guid.NewGuid(), "net://test1", 1, true, a => new ResourceRepresentation
            {
                Body = "TEST1",
                Cacheable = false
            });

            var id = Guid.NewGuid();
            _kernel.Routes.RegisterResourceHandler(id, "net://test2", 1, true, a => new ResourceRepresentation
            {
                Body = "TEST2",
                Cacheable = false
            });

            _kernel.Routes.RegisterResourceHandler(Guid.NewGuid(), "net://test3", 1, true, a => new ResourceRepresentation
            {
                Body = "TEST3",
                Cacheable = false
            });

            _kernel.Routes.DeleteRoute(id);

            _kernel.Get("net://test2");
            Assert.Fail();
        }

        [Test, Category("Unit")]
        public void Simple_Regex()
        {
            _kernel.Routes.RegisterResourceHandlerRegex(Guid.NewGuid(), "^net://test.$", 1, true, a => new ResourceRepresentation
            {
                Body = a.NetResourceLocator.Substring(6).ToUpper(),
                Cacheable = false
            });

            Assert.AreEqual("TEST2", _kernel.Get("net://test2").Body);
        }

    }
}
