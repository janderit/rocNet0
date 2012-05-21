using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibTicker;
using LibTicker.Server;
using NUnit.Framework;

namespace TickerTests.Base
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
        public void Smoke()
        {
            Assert.True(true);
        }

        [Test]
        public void Sending_a_single_Tick()
        {
            var publisher = Ticker.Publisher.ZeroMq("tcp://localhost:32991", "UnitTest");
            publisher.Publish(new Guid("BA8519EA-A067-45A7-8D38-9AC12E3EF6DE"), new Guid("7FF4B4DF-6775-4680-AA4F-9A86C2832D5D"), "data");
        }

        [Test]
        public void Sending_and_receiving_a_single_Tick()
        {
            var subject1 = new Guid("690A2327-FDA5-4C50-886C-5AE17EF46829");
            var trigger1 = new Guid("F6D70D2C-1690-4605-8468-10490360973D");

            var ticks = new List<Tick>();
            var publisher = Ticker.Publisher.ZeroMq("tcp://localhost:32991", "UnitTest");
            var listener = Ticker.Listener.ZeroMq("tcp://localhost:32992", "tcp://localhost:32993").Listener(ticks.Add);

            publisher.Publish(subject1, trigger1, "data");
            Thread.Sleep(200);

            Assert.AreEqual(1, ticks.Count);
            var tick = ticks.Single();
            Assert.AreEqual(subject1, tick.Subject);
            Assert.AreEqual(trigger1, tick.Trigger);
            Assert.AreEqual("data", tick.Data);
        }

        [Test]
        public void Sending_and_receiving_100_Ticks()
        {
            var subject1 = new Guid("690A2327-FDA5-4C50-886C-5AE17EF46829");
            var trigger1 = new Guid("F6D70D2C-1690-4605-8468-10490360973D");

            var ticks = new List<Tick>();
            var publisher = Ticker.Publisher.ZeroMq("tcp://localhost:32991", "UnitTest");
            var listener = Ticker.Listener.ZeroMq("tcp://localhost:32992", "tcp://localhost:32993").Listener(ticks.Add);

            for (int i = 0; i < 99; i++) publisher.Publish(Guid.NewGuid(), Guid.NewGuid(), "data");
            publisher.Publish(subject1, trigger1, "data");

            Thread.Sleep(100);

            Assert.AreEqual(100, ticks.Count);
            var tick = ticks.Last();
            Assert.AreEqual(subject1, tick.Subject);
            Assert.AreEqual(trigger1, tick.Trigger);
            Assert.AreEqual("data", tick.Data);
        }

        [Test,Ignore]
        public void Sending_and_receiving_1000_Ticks()
        {
            var max = 1000;
            int i = 0;
            var publisher = Ticker.Publisher.ZeroMq("tcp://localhost:32991", "UnitTest");
            var listener = Ticker.Listener.ZeroMq("tcp://localhost:32992", "tcp://localhost:32993").Listener(t => Interlocked.Increment(ref i));

            var publications = new List<Action>();
            for (int j = 0; j < max; j++) publications.Add(() => publisher.Publish(Guid.NewGuid(), Guid.NewGuid(), "data"));

            Parallel.Invoke(publications.ToArray());

            Thread.Sleep(15000);

            Assert.AreEqual(max, i);
        }

    }
}
