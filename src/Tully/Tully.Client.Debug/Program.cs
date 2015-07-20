using System;

namespace Tully.Client.Debug
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var client = new WebSocket("192.168.1.68", 80);
            client.Opened += (sender, eventArgs) => Console.WriteLine("Connection open!");

            var exit = false;

            while (!exit)
            {
                var cmd = Console.ReadLine();

                switch (cmd)
                {
                    case "/exit":
                        exit = true;
                        break;
                    case "/open":
                        client.Open();
                        break;
                    case "/send":
                        client.SendMdnString();
                        break;
                    default:
                        Console.WriteLine("Unknown command " + cmd);
                        break;
                }
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}