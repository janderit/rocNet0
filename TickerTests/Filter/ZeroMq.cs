using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using LibTicker;
using LibTicker.Clients;
using LibTicker.Server;
using LibTicker_zmq.Server;
using NUnit.Framework;

namespace TickerTests.Filter
{
    [TestFixture]
    class ZeroMq
    {

        private ZeroMqInMemoryTickerServer _server;

        [SetUp]
        public void StartServer()
        {
            _server = new ZeroMqInMemoryTickerServer("tcp://127.0.0.1:32991", "tcp://127.0.0.1:32992", "tcp://127.0.0.1:32993", s => Trace.WriteLine(s)).Start();
            Thread.Sleep(200);
        }

        [TearDown]
        public void KillServer()
        {
            _server.Shutdown();
        }

        [Test]
        public void No_filtering()
        {
            var subject1 = new Guid("690A2327-FDA5-4C50-886C-5AE17EF46829");
            var trigger1 = new Guid("F6D70D2C-1690-4605-8468-10490360973D");
            var subject2 = new Guid("1C7F409E-4ACE-4B92-BF9A-9FB5C2B2748E");
            var trigger2 = new Guid("0F9359B3-62FD-40EC-B259-802BA4AD8522");

            var ticks = new List<Tick>();
            var publisher = Ticker.Publisher.ZeroMq("tcp://localhost:32991", "UnitTest");
            var listener = Ticker.Listener.ZeroMq("tcp://localhost:32992", "tcp://localhost:32993").Listener(ticks.Add);

            publisher.Publish(subject1, trigger1, "data1");
            publisher.Publish(subject2, trigger2, "data2");

            Thread.Sleep(200);

            Assert.AreEqual(2, ticks.Count);
        }

        [Test]
        public void Filter_by_Subject()
        {
            var subject1 = new Guid("690A2327-FDA5-4C50-886C-5AE17EF46829");
            var trigger1 = new Guid("F6D70D2C-1690-4605-8468-10490360973D");
            var subject2 = new Guid("1C7F409E-4ACE-4B92-BF9A-9FB5C2B2748E");
            var trigger2 = new Guid("0F9359B3-62FD-40EC-B259-802BA4AD8522");

            var ticks = new List<Tick>();
            var publisher = Ticker.Publisher.ZeroMq("tcp://localhost:32991", "UnitTest");
            var listener = Ticker.Listener.ZeroMq("tcp://localhost:32992", "tcp://localhost:32993").ListenTo(subject2).Listener(ticks.Add);

            publisher.Publish(subject1, trigger1, "data1");
            publisher.Publish(subject2, trigger2, "data2");

            Thread.Sleep(100);

            Assert.AreEqual(1, ticks.Count);
            var tick = ticks.Single();
            Assert.AreEqual(subject2, tick.Subject);
            Assert.AreEqual(trigger2, tick.Trigger);
            Assert.AreEqual("data2", tick.Data);
        }

        [Test]
        public void Filter_by_Trigger()
        {
            var subject1 = new Guid("690A2327-FDA5-4C50-886C-5AE17EF46829");
            var trigger1 = new Guid("F6D70D2C-1690-4605-8468-10490360973D");
            var subject2 = new Guid("1C7F409E-4ACE-4B92-BF9A-9FB5C2B2748E");
            var trigger2 = new Guid("0F9359B3-62FD-40EC-B259-802BA4AD8522");

            var ticks = new List<Tick>();
            var publisher = Ticker.Publisher.ZeroMq("tcp://localhost:32991", "UnitTest");
            var listener = Ticker.Listener.ZeroMq("tcp://localhost:32992", "tcp://localhost:32993").Filter(trigger2).Listener(ticks.Add);

            publisher.Publish(subject1, trigger1, "data1");
            publisher.Publish(subject2, trigger2, "data2");

            Thread.Sleep(100);

            Assert.AreEqual(1, ticks.Count);
            var tick = ticks.Single();
            Assert.AreEqual(subject2, tick.Subject);
            Assert.AreEqual(trigger2, tick.Trigger);
            Assert.AreEqual("data2", tick.Data);
        }

        [Test]
        public void Exclusive_configuration()
        {
            var subject1 = new Guid("690A2327-FDA5-4C50-886C-5AE17EF46829");
            var trigger1 = new Guid("F6D70D2C-1690-4605-8468-10490360973D");
            var subject2 = new Guid("1C7F409E-4ACE-4B92-BF9A-9FB5C2B2748E");
            var trigger2 = new Guid("0F9359B3-62FD-40EC-B259-802BA4AD8522");

            var ticks = new List<Tick>();
            var publisher = Ticker.Publisher.ZeroMq("tcp://localhost:32991", "UnitTest");
            var listener = Ticker.Listener.ZeroMq("tcp://localhost:32992", "tcp://localhost:32993").ListenTo(subject1).Filter(trigger2).Listener(ticks.Add);

            publisher.Publish(subject1, trigger1, "data1");
            publisher.Publish(subject2, trigger2, "data2");

            Thread.Sleep(100);

            Assert.AreEqual(0, ticks.Count);
        }

    }
}
