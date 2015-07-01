using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Tully.Server.Debug
{
    internal class MyTcpListener
    {
        public static void Main()
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 8080.
                var port = 80;

                // Get the ip for the default gateway
                IPAddress localAddr = IPAddress.Parse("192.168.1.68");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    while (true)
                    {
                        while (!stream.DataAvailable)
                        {
                        }

                        var bytes = new byte[client.Available];
                        stream.Read(bytes, 0, bytes.Length);

                        string data = Encoding.UTF8.GetString(bytes);

                        if (new Regex("^GET").IsMatch(data))
                        {
                            byte[] response =
                                Encoding.UTF8.GetBytes(
                                    "HTTP/1.1 101 Switching Protocols" + Environment.NewLine + 
                                    "Connection: Upgrade" + Environment.NewLine + 
                                    "Upgrade: websocket" + Environment.NewLine + 
                                    "Sec-WebSocket-Accept: " + 
                                    Convert.ToBase64String(
                                        SHA1.Create()
                                          .ComputeHash(
                                              Encoding.UTF8.GetBytes(
                                                  new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim(
                                                      ) + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")))
                                    + Environment.NewLine + Environment.NewLine);

                            stream.Write(response, 0, response.Length);
                            var msg = Encoding.UTF8.GetBytes("Hello from server");
                            stream.Write(msg, 0,  msg.Length);
                        }
                        else
                        {
                            var decoded = new byte[3];
                            var encoded = new byte[3] { 112, 16, 109 };
                            var key = new byte[4] { 61, 84, 35, 6 };

                            for (var i = 0; i < encoded.Length; i++)
                            {
                                decoded[i] = (byte)(encoded[i] ^ key[i % 4]);
                            }

                            Console.WriteLine("Data: " + Encoding.UTF8.GetString(decoded));
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }
}