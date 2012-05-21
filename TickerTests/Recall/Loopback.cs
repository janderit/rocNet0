using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LibTicker;
using LibTicker.Clients;
using LibTicker.Server;
using NUnit.Framework;

namespace TickerTests.Recall
{
    [TestFixture]
    class Loopback
    {

        [SetUp]
        public void Setup()
        {
            LoopbackDevice.Reset();
        }

        [TearDown]
        public void Teardown()
        {
            LoopbackDevice.Reset();
        }

        [Test]
        public void No_recall()
        {
            var subject1 = new Guid("690A2327-FDA5-4C50-886C-5AE17EF46829");
            var trigger1 = new Guid("F6D70D2C-1690-4605-8468-10490360973D");
            var subject2 = new Guid("1C7F409E-4ACE-4B92-BF9A-9FB5C2B2748E");
            var trigger2 = new Guid("0F9359B3-62FD-40EC-B259-802BA4AD8522");

            var ticks = new List<Tick>();
            var publisher = Ticker.Publisher.Loopback();
            
            publisher.Publish(subject1, trigger1, "data1");
            publisher.Publish(subject2, trigger2, "data2");

            Thread.Sleep(100);

            var listener = Ticker.Listener.Loopback().Listener(ticks.Add);

            Thread.Sleep(100);

            Assert.AreEqual(0, ticks.Count);
        }

        [Test]
        public void Recall()
        {
            var subject1 = new Guid("690A2327-FDA5-4C50-886C-5AE17EF46829");
            var trigger1 = new Guid("F6D70D2C-1690-4605-8468-10490360973D");
            var subject2 = new Guid("1C7F409E-4ACE-4B92-BF9A-9FB5C2B2748E");
            var trigger2 = new Guid("0F9359B3-62FD-40EC-B259-802BA4AD8522");

            var ticks = new List<Tick>();
            var publisher = Ticker.Publisher.Loopback();

            publisher.Publish(subject1, trigger1, "data1");
            publisher.Publish(subject2, trigger2, "data2");

            Thread.Sleep(100);

            var listener = Ticker.Listener.Loopback().Listener(ticks.Add).RecallFrom(0);

            Thread.Sleep(100);

            Assert.AreEqual(2, ticks.Count);
        }

    }
}
