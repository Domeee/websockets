using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Tully
{
    /// <summary>
    /// WebSocket client implementation.
    /// </summary>
    /// <remarks>The <see cref="WebSocket" /> class implementation is related to the 
    /// WebSocket API specification of the W3C. 
    /// See <a href="http://www.w3.org/TR/websockets/">The WebSocket API</a></remarks>
    public class WebSocket : IDisposable
    {
        private const ushort BufferSize = 1024;

        /// <summary>
        /// The connection is either not established or closed.
        /// </summary>
        private const byte ClosedState = 0;

        /// <summary>
        /// The connectino is established.
        /// </summary>
        private const byte OpenState = 1;

        /// <summary>
        /// Closing handshake in progress.
        /// </summary>
        private const byte ClosingState = 2;

        private readonly byte[] _buffer = new byte[BufferSize];

        private readonly ushort _port;

        private readonly string _serverAddr;

        private TcpClient _client;

        private byte _readyState = ClosedState;

        private NetworkStream _stream;

        public WebSocket(string serverAddr, ushort port)
        {
            _serverAddr = serverAddr;
            _port = port;
        }

        public event EventHandler Opened;

        public event EventHandler Closed;

        public event EventHandler MessageReceived;

        public void Open()
        {
            if (_readyState != ClosedState)
            {
                throw new InvalidOperationException("The WebSocket has to be closed before it can be opened again.");
            }

            // TODO: bad design => stream assigned but not used local...
            _client = new TcpClient(_serverAddr, _port);
            _stream = _client.GetStream();
            OpeningHandshake();
            _readyState = OpenState;
            new Thread(Listen).Start();

            // Connection established
            OnOpen(EventArgs.Empty);
        }

        public void Close()
        {
            if (_readyState != OpenState)
            {
                throw new InvalidOperationException("The WebSocket has to be opened before it can be closed.");
            }

            _readyState = ClosingState;
            ClosingHandshake();
            OnClose(EventArgs.Empty);
        }

        private void ClosingHandshake()
        {
            byte byte1 = 136;
            byte byte2 = 128;
            var frame = new byte[6];
            frame[0] = byte1;
            frame[1] = byte2;

            // TODO: Refactor to method
            var maskingKey = new byte[4];
            var rnd = new Random();
            rnd.NextBytes(maskingKey);
            frame[2] = maskingKey[0];
            frame[3] = maskingKey[1];
            frame[4] = maskingKey[2];
            frame[5] = maskingKey[3];

            _stream.Write(frame, 0, frame.Length);
        }

        public void SendMdnString()
        {
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

        protected virtual void OnClose(EventArgs args)
        {
            Closed?.Invoke(this, args);
        }

        protected virtual void OnOpen(EventArgs e)
        {
            Opened?.Invoke(this, e);
        }

        protected virtual void OnMessageReceived(WebSocketEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        private void Listen()
        {
            while (_readyState == OpenState)
            {
                if (_stream.DataAvailable)
                {
                    _stream.BeginRead(_buffer, 0, BufferSize, OnRead, _stream);
                }
            }
        }

        private void OnRead(IAsyncResult ar)
        {
            var stream = (NetworkStream)ar.AsyncState;
            stream.EndRead(ar);
            var frame = new WebSocketFrame(_buffer);
            if (frame.IsMasked)
            {
                throw new ProtocolException("Payload must not be masked.", WebSocketStatusCode.ProtocolError);
            }

            object result = null;

            if (frame.OpCode == 1)
            {
                result = Encoding.UTF8.GetString(frame.ApplicationData);
            }
            else if (frame.OpCode == 2)
            {
                result = BitConverter.ToSingle(frame.ApplicationData, 0);
            }

            OnMessageReceived(new WebSocketEventArgs { Data = result });

            Debug.WriteLine(result);
        }

        private void OpeningHandshake()
        {
            var message = new StringBuilder();
            message.AppendLine("GET / HTTP/1.1");
            message.AppendLine("Host: 127.0.0.1");
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

        #region IDisposable Members

        public void Dispose()
        {
            if (_stream != Stream.Null)
            {
                _stream.Close();
            }

            _client.Close();
        }

        #endregion
    }
}