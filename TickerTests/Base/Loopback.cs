using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibTicker;
using LibTicker.Clients;
using LibTicker.Server;
using NUnit.Framework;

namespace TickerTests.Base
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
        public void Smoke()
        {
            Assert.True(true);
        }

        [Test]
        public void Sending_a_single_Tick()
        {
            var publisher = Ticker.Publisher.Loopback();
            publisher.Publish(new Guid("BA8519EA-A067-45A7-8D38-9AC12E3EF6DE"), new Guid("7FF4B4DF-6775-4680-AA4F-9A86C2832D5D"), "data");
        }

        [Test]
        public void Sending_and_receiving_a_single_Tick()
        {
            var subject1 = new Guid("690A2327-FDA5-4C50-886C-5AE17EF46829");
            var trigger1 = new Guid("F6D70D2C-1690-4605-8468-10490360973D");

            var ticks = new List<Tick>();
            var publisher = Ticker.Publisher.Loopback();
            var listener = Ticker.Listener.Loopback().Listener(ticks.Add);

            publisher.Publish(subject1, trigger1, "data");
            Thread.Sleep(100);

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
            var publisher = Ticker.Publisher.Loopback();
            var listener = Ticker.Listener.Loopback().Listener(ticks.Add);

            for (int i = 0; i < 99;i++ ) publisher.Publish(Guid.NewGuid(), Guid.NewGuid(), "data");
            publisher.Publish(subject1, trigger1, "data");

            Thread.Sleep(100);

            Assert.AreEqual(100, ticks.Count);
            var tick = ticks.Last();
            Assert.AreEqual(subject1, tick.Subject);
            Assert.AreEqual(trigger1, tick.Trigger);
            Assert.AreEqual("data", tick.Data);
        }

        [Test]
        public void Sending_and_receiving_100000_Ticks()
        {
            var max = 100000;
            int i = 0;
            var publisher = Ticker.Publisher.Loopback();
            var listener = Ticker.Listener.Loopback().Listener(t=>Interlocked.Increment(ref i));

            var publications = new List<Action>();
            for (int j = 0; j < max;j++ ) publications.Add(()=> publisher.Publish(Guid.NewGuid(), Guid.NewGuid(), "data"));

            Parallel.Invoke(publications.ToArray());

            Thread.Sleep(5000);

            Assert.AreEqual(max, i);
        }

    }
}
