using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LibKernel;
using LibKernel_zmq;

namespace kernelstressfib
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("kernel stressor FIB");
            Console.WriteLine("");
            Console.Write("0MQ [localhost:]> tcp://");
            var url = Console.ReadLine();
            if (!url.Contains(":")) url = "127.0.0.1:" + url;


            var soll = 0;

            var s = new List<Stressor>();
            bool done = false;
            while (!done)
            {

                Console.Clear();
                var means = s.Select(_ => _.mean).ToList();
                if (means.Count == 0) Console.WriteLine("no stressors active. Press + or - or ESC");
                else
                {
                    Console.WriteLine(means.Count+" active (of "+soll+"), mean: "+(means.Sum()/means.Count)+" ms");
                }

                while (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        done = true;                        
                    }

                    if (key.Key == ConsoleKey.OemPlus)
                    {
                        soll++;
                    }
                    if (key.Key == ConsoleKey.OemMinus)
                    {
                        soll--;
                        if (soll < 0) soll = 0;
                    }
                }

                s.Where(_=>_.Dead).ToList().ForEach(_=>s.Remove(_));

                while (s.Count < soll) s.Add(new Stressor(url));
                while (s.Count > soll)
                {
                    if (s.Count == 0) continue;
                    s.First().Kill();
                    s.RemoveAt(0);
                }

                Thread.Sleep(2000);

            }

            s.ForEach(_ => _.Kill());
            
            
        }





    }

    public class Stressor
    {
        private string _url;
        private Thread _thread;

        public volatile int mean = 0;

        public bool Dead = false;
        private bool _killed;

        public Stressor(string url)
        {
            _url = url;

            _thread = new Thread(Worker);
            _thread.Start();

        }

        public void Kill()
        {
            _killed = true;
        }


        public void Worker()
        {
            var conn = new ZeroMqResourceProviderConnector("tcp://" + _url, true);

            var r = new Random();

            var count = 0;
            var total = 0;

            try
            {
                var start = DateTime.Now;
                while (true)
                {
                    var f = r.Next(0, 92);

                    var t0 = Environment.TickCount;
                    conn.Get(new Request { NetResourceLocator = "net://fib/" + f });
                    var t1 = Environment.TickCount;
                    count++;
                    total = total + t1 - t0;
                    mean = total / count;
                    Thread.Sleep(50);
                    if (DateTime.Now > start.AddSeconds(20) ||_killed)
                    {
                        Thread.CurrentThread.Abort();
                    }
                }

            }
            catch
            {
                ;
            }
            finally
            {
                Dead = true;
                conn.Close();
            }
        }

    }

    

}
