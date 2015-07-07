using System;

namespace Tully.Client.Debug
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var client = new WebSocket("192.168.1.68", 80))
            {
                client.Opened += (sender, eventArgs) => Console.WriteLine("Connection open!");
                client.Open();

                Console.WriteLine("Press Enter to send MDN");
                Console.ReadKey();
                client.SendMdnString();
            }

            Console.WriteLine("Press Enter to continue...");
            Console.Read();
        }
    }
}