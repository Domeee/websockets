using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Tully
{
    /// <summary>
    /// WebSocket server implementation according to RFC 6455. See <a href="http://tools.ietf.org/html/rfc6455">RFC 6455</a>
    /// </summary>
    public class WebSocketServer : IDisposable
    {
        private const string WebSocketServerMagicString = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        private const ushort BufferSize = 1024;

        private readonly byte[] _buffer = new byte[BufferSize];

        private readonly ConcurrentList<TcpClient> _clients = new ConcurrentList<TcpClient>();

        private readonly TcpListener _server;

        private bool _started;

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
            if (_started)
            {
                throw new InvalidOperationException("The WebSocket server was already started.");
            }

            _server.Start();
            _started = true;
            OnStarted(EventArgs.Empty);

            _server.BeginAcceptTcpClient(OnAcceptClient, _server);
        }

        public void Stop()
        {
            _started = false;

            foreach (TcpClient client in _clients)
            {
                client.Close();
            }

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

        private void OnAcceptClient(IAsyncResult ar)
        {
            var listener = (TcpListener)ar.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(ar);
            client.ReceiveBufferSize = BufferSize;
            client.SendBufferSize = BufferSize;
            _clients.Add(client);
            NetworkStream stream = client.GetStream();

            while (_started)
            {
                if (stream.DataAvailable)
                {
                    stream.BeginRead(_buffer, 0, BufferSize, OnRead, stream);
                }
            }
        }

        private void OnRead(IAsyncResult ar)
        {
            var stream = (NetworkStream)ar.AsyncState;
            int read = stream.EndRead(ar);

            string request = Encoding.UTF8.GetString(_buffer, 0, read);

            if (NetworkPackageSniffer.IsOpeningHandshake(request))
            {
                string key = CalculateWebSocketAccept(request);
                byte[] response = NetworkMessage.GetClosingHandshake(key);
                stream.Write(response, 0, response.Length);
            }
            else
            {
                var frame = new WebSocketFrame(_buffer);
                string result = Encoding.UTF8.GetString(frame.ApplicationData);
                Debug.WriteLine("Data: " + result);
            }
        }
    }
}