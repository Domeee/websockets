using System;

namespace Tully.Server.Debug
{
    public class Program
    {
        public static void Main()
        {
            var server = new WebSocketServer("192.168.1.68", 80);
            server.Started += (sender, args) => Console.WriteLine("WebSocket server started!");
            server.Stopped += (sender, args) => Console.WriteLine("WebSocket server stopped!");
            server.Start();

            Console.WriteLine("Hit enter to continue...");
            Console.Read();
        }
    }
}