using System;
using System.Collections;
using System.IO;
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
                var port = 80;
                IPAddress localAddr = IPAddress.Parse("192.168.1.68");
                server = new TcpListener(localAddr, port);
                server.Start();

                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");
                    NetworkStream stream = client.GetStream();

                    while (true)
                    {
                        while (!stream.DataAvailable)
                        {
                        }

                        var frameBytes = new byte[client.Available];
                        stream.Read(frameBytes, 0, frameBytes.Length);

                        string request = Encoding.UTF8.GetString(frameBytes);

                        if (new Regex("^GET").IsMatch(request))
                        {
                            var response = new StringBuilder();
                            response.AppendLine("HTTP/1.1 101 Switching Protocols");
                            response.AppendLine("Connection: Upgrade");
                            response.AppendLine("Upgrade: websocket");
                            response.AppendLine(
                                "Sec-WebSocket-Accept: "
                                + Convert.ToBase64String(
                                    SHA1.Create()
                                      .ComputeHash(
                                          Encoding.UTF8.GetBytes(
                                              new Regex("Sec-WebSocket-Key: (.*)").Match(request).Groups[1].Value.Trim()
                                              + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"))));
                            response.AppendLine();

                            var data = Encoding.UTF8.GetBytes(response.ToString());
                            stream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            var frame = new Frame(frameBytes);
                            var result = Encoding.UTF8.GetString(frame.ApplicationData);
                            Console.WriteLine("Data: " + result);
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

        static bool IsSet(byte data, byte bitNumber)
        {
            return (data & (1 << bitNumber - 1)) != 0;
        }
    }
}