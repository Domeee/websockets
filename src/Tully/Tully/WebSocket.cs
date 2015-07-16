using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Tully
{
    public class WebSocket : IDisposable
    {
        private readonly TcpClient _client;

        private NetworkStream _stream;

        public WebSocket(string serverAddr, ushort port)
        {
            _client = new TcpClient(serverAddr, port);
        }

        public void Dispose()
        {
            if (_stream != Stream.Null)
            {
                _stream.Close();
            }

            _client.Close();
        }

        public event EventHandler Opened;

        public void Open()
        {
            _stream = _client.GetStream();

            Handshake();

            // Handshake completed
            OnOpen(EventArgs.Empty);
        }

        private void Handshake()
        {
            // Client handshake
            var message = new StringBuilder();
            message.AppendLine("GET / HTTP/1.1");
            message.AppendLine("Host: 192.168.1.68");
            message.AppendLine("Upgrade: websocket");
            message.AppendLine("Connection: Upgrade");
            message.AppendLine("Sec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==");
            message.AppendLine("Sec-WebSocket-Version: 13");
            message.AppendLine();
            byte[] data = Encoding.UTF8.GetBytes(message.ToString());
            _stream.Write(data, 0, data.Length);
            Debug.WriteLine(string.Format("Sent: {0}", message));

            // Wait for connection upgrade
            var connectionUpgraded = false;
            while (!connectionUpgraded)
            {
                while (!_stream.DataAvailable)
                {
                }

                var frameBytes = new byte[_client.Available];
                _stream.Read(frameBytes, 0, frameBytes.Length);

                string request = Encoding.UTF8.GetString(frameBytes);

                connectionUpgraded = new Regex("Connection: Upgrade").Match(request).Success
                                     && new Regex("Upgrade: websocket").Match(request).Success;
            }
        }

        public void SendMdnString()
        {
            // Client single message
            byte byte1 = 129;
            byte byte2 = 131;
            var maskingKey = new byte[4];
            var rnd = new Random();
            rnd.NextBytes(maskingKey);
            byte[] decoded = Encoding.UTF8.GetBytes("MDN");
            var encoded = new byte[decoded.Length];

            for (var i = 0; i < encoded.Length; i++)
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

            _stream.Write(frame, 0, frame.Length);
        }

        protected virtual void OnOpen(EventArgs e)
        {
            if (Opened != null)
            {
                Opened(this, e);
            }
        }
    }
}