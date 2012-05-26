using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibKernel;
using LibKernel.Exceptions;
using LibKernel_zmq;

namespace rni
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("rni - rocNet inspector");
            Console.WriteLine("");
            Console.Write("0MQ [localhost:]> tcp://");
            var url = Console.ReadLine();
            if (!url.Contains(":")) url = "127.0.0.1:" + url;
            var conn = new ZeroMqResourceProviderConnector("tcp://"+url);

            var done = false;
            while (!done)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine();
                Console.Write("kernel> ");
                var nri = Console.ReadLine();
                if (nri == "") done = true; else
                {
                    try
                    {
                        var r = conn.GetRaw(new Request { NetResourceLocator = nri });
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        foreach (var l in r)
                        {
                            if (l == "") Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine(l);
                        }
                    }
                    catch (KernelException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.GetType().FullName);
                        Console.WriteLine(ex.StackTrace);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.GetType().FullName);
                        Console.WriteLine(ex.StackTrace);
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                }
            }

        }
    }
}
