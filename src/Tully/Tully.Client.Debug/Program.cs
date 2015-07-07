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

                // Client single message
                byte byte1 = 129;
                byte byte2 = 131;
                var maskingKey = new byte[4];
                var rnd = new Random();
                rnd.NextBytes(maskingKey);
                var decoded = Encoding.UTF8.GetBytes("MDN");
                var encoded = new byte[decoded.Length];

                for (int i = 0; i < encoded.Length; i++)
                {
                    encoded[i] = (byte)(decoded[i] ^ maskingKey[i % 4]);
                }

                var frame = new byte[9];
                frame[0] = byte1;
                frame[1] = byte2;
                frame[2] = maskingKey[0];
                frame[3] = maskingKey[1];
                frame[4] = maskingKey[2];
                frame[5] = maskingKey[3];
                frame[6] = encoded[0];
                frame[7] = encoded[1];
                frame[8] = encoded[2];

                stream.Write(frame, 0, frame.Length);
                
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