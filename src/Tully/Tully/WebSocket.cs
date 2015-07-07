using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Tully
{
    public class WebSocket : IDisposable
    {
        private readonly TcpClient _client;

        private NetworkStream _stream;

        public event EventHandler Opened;

        public WebSocket(string serverAddr, ushort port)
        {
            _client = new TcpClient(serverAddr, port);
        }

        public void Open()
        {
            _stream = _client.GetStream();
            OnOpen(EventArgs.Empty);

            Handshake();
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

            var data = Encoding.UTF8.GetBytes(message.ToString());
            _stream.Write(data, 0, data.Length);
            Debug.WriteLine(string.Format("Sent: {0}", message));
        }

        public void SendMdnString()
        {
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

            _stream.Write(frame, 0, frame.Length);
        }

        public void Dispose()
        {
            if (_stream != Stream.Null)
            {
                _stream.Close();
            }

            _client.Close();
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
