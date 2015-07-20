using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Tully
{
    /// <summary>
    /// WebSocket server implementation according to RFC 6455. See <a href="http://tools.ietf.org/html/rfc6455">RFC 6455</a>
    /// </summary>
    public class WebSocketServer : IDisposable
    {
        private const string WebSocketServerMagicString = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        private readonly TcpListener _server;

        private readonly List<TcpClient> _clients = new List<TcpClient>();

        private bool _isStarted;

        public WebSocketServer(string localaddr, ushort port)
        {
            IPAddress ipaddr = IPAddress.Parse(localaddr);
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
            if (_isStarted)
            {
                throw new InvalidOperationException("The WebSocket server was already started.");
            }

            _server.Start();
            _isStarted = true;
            OnStarted(EventArgs.Empty);
            ListenLoop();
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

        private string CalculateWebSocketAccept(string openingHandshake)
        {
            string key = NetworkPackageSniffer.GetWebSocketKey(openingHandshake);
            SHA1 sha1Encrypter = SHA1.Create();
            byte[] acceptKeyBytes = Encoding.UTF8.GetBytes(key + WebSocketServerMagicString);
            byte[] hash = sha1Encrypter.ComputeHash(acceptKeyBytes);
            return Convert.ToBase64String(hash);
        }

        private void ListenLoop()
        {
            while (true)
            {
                TcpClient client = _server.AcceptTcpClient();
                new Thread(() => HandleClient(client)).Start();
                _clients.Add(client);
                Debug.WriteLine("Current clients count: " + _clients.Count);
            }
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            while (true)
            {
                while (!stream.DataAvailable)
                {
                }

                var frameBytes = new byte[client.Available];
                stream.Read(frameBytes, 0, frameBytes.Length);

                string request = Encoding.UTF8.GetString(frameBytes);

                if (NetworkPackageSniffer.IsOpeningHandshake(request))
                {
                    string key = CalculateWebSocketAccept(request);
                    byte[] response = NetworkMessage.GetClosingHandshake(key);
                    stream.Write(response, 0, response.Length);
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
}