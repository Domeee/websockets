using System;
using System.Net.Sockets;
using System.Text;

namespace Tully.Client.Debug
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Connect("192.168.1.68");
        }

        private static void Connect(string server)
        {
            try
            {
                var port = 80;
                
                // This constructor also opens a connection to the specified URI.
                var client = new TcpClient(server, port);

                // Client handshake
                var message = new StringBuilder();
                message.AppendLine("GET / HTTP/1.1");
                message.AppendLine("Host: 192.168.1.68");
                message.AppendLine("Upgrade: websocket");
                message.AppendLine("Connection: Upgrade");
                message.AppendLine("Sec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==");
                message.AppendLine("Sec-WebSocket-Version: 13");
                message.AppendLine();
                
                var data = Encoding.UTF8.GetBytes(message.ToString());
                var stream = client.GetStream();
                stream.Write(data, 0, data.Length);
                Console.WriteLine("Sent: {0}", message);
                
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }
    }
}