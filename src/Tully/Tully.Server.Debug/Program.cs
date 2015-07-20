using System;

namespace Tully.Server.Debug
{
    public class Program
    {
        public static void Main()
        {
            var server = new WebSocketServer("192.168.1.68", 80);
            server.Started += (sender, args) => Console.WriteLine("Server started!");
            server.Stopped += (sender, args) => Console.WriteLine("Server stopped!");

            Console.WriteLine("Tully WebSocket server at your command");
            var exit = false;

            while (!exit)
            {
                var cmd = Console.ReadLine();

                switch (cmd)
                {
                    case "/exit":
                        exit = true;
                        break;
                    case "/start":
                        server.Start();
                        break;
                    case "/stop":
                        server.Stop();
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