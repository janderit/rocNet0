using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibKernel;
using NUnit.Framework;
using rocNet.Kernel;

namespace KernelTests.Basic_Functionality
{
    [TestFixture]
    class Static_Serving
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
            Assert.True(true);
        }



    }
}
