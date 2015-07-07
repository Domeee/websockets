using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Tully
{
    public class WebSocketServer : IDisposable
    {
        private readonly TcpListener _server;

        public WebSocketServer(string localaddr, ushort port)
        {
            var ipaddr = IPAddress.Parse(localaddr);
            _server = new TcpListener(ipaddr, port);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public event EventHandler Started;

        public event EventHandler Stopped;

        public void Start()
        {
            try
            {
                _server.Start();
                OnStarted(EventArgs.Empty);

                while (true)
                {
                    Debug.Write("Waiting for a connection...");

                    TcpClient client = _server.AcceptTcpClient();
                    Debug.WriteLine("Connected!");
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

                            byte[] data = Encoding.UTF8.GetBytes(response.ToString());
                            stream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            var frame = new Frame(frameBytes);
                            string result = Encoding.UTF8.GetString(frame.ApplicationData);
                            Debug.WriteLine("Data: " + result);
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                Debug.WriteLine(string.Format("SocketException: {0}", e));
            }
            finally
            {
                // Stop listening for new clients.
                _server.Stop();
            }
        }

        public void Stop()
        {
            _server.Stop();
            OnStopped(EventArgs.Empty);
        }

        protected virtual void OnStarted(EventArgs e)
        {
            if (Started != null)
            {
                Started(this, e);
            }
        }

        protected virtual void OnStopped(EventArgs e)
        {
            if (Stopped != null)
            {
                Stopped(this, e);
            }
        }
    }
}