using System;

namespace Tully.Client.Debug
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var client = new WebSocket("127.0.0.1", 8080);
            client.Opened += (sender, eventArgs) => Console.WriteLine("Connection open!");
            client.Closed += (sender, eventArgs) => Console.WriteLine("Connection closed");
            client.MessageReceived += (sender, eventArgs) => Console.WriteLine(((WebSocketEventArgs)eventArgs).Data);

            var exit = false;

            while (!exit)
            {
                var cmd = Console.ReadLine();

                switch (cmd)
                {
                    case "/close":
                        client.Close();
                        break;
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