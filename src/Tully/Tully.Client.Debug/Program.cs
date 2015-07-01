using System;
using System.Net.Sockets;
using System.Text;

namespace Tully.Client.Debug
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Connect("192.168.1.68", "Hello World!");
        }

        private static void Connect(string server, string message)
        {
            try
            {
                // Create a TcpClient. 
                // Note, for this client to work you need to have a TcpServer  
                // connected to the same address as specified by the server, port 
                // combination.
                var port = 8080;
                var client = new TcpClient(server, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                var data = Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing. 
                var stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response. 

                // Buffer to store the response bytes.
                data = new byte[256];

                // String to store the response ASCII representation.
                var responseData = string.Empty;

                // Read the first batch of the TcpServer response bytes.
                var bytes = stream.Read(data, 0, data.Length);
                responseData = Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);

                // Close everything.
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