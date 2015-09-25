using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        private const ushort BufferSize = 1024;

        private const byte StartedState = 0;

        private const byte StoppedState = 1;

        private readonly byte[] _buffer = new byte[BufferSize];

        private readonly ConcurrentDictionary<string, TcpClient> _clients =
            new ConcurrentDictionary<string, TcpClient>();

        private readonly TcpListener _server;

        private byte _state = StoppedState;

        public WebSocketServer(string localaddr, ushort port)
        {
            IPAddress ipaddr = IPAddress.Parse(localaddr);
            _server = new TcpListener(ipaddr, port);
        }

        public event EventHandler Started;

        public event EventHandler Stopped;

        public void Start()
        {
            if (_state != StoppedState)
            {
                throw new InvalidOperationException("The WebSocket server was already started.");
            }

            _server.Start();
            new Thread(Listen).Start();
            _state = StartedState;
            OnStarted(EventArgs.Empty);
        }

        public void Stop()
        {
            _state = StoppedState;

            foreach (KeyValuePair<string, TcpClient> kvp in _clients)
            {
                var client = kvp.Value;
                SendClosingHandshake(client.GetStream());
                client.Close();
            }

            _server.Stop();
            _clients.RemoveAll();
            OnStopped(EventArgs.Empty);
        }

        protected virtual void OnStarted(EventArgs e)
        {
            Started?.Invoke(this, e);
        }

        protected virtual void OnStopped(EventArgs e)
        {
            Stopped?.Invoke(this, e);
        }

        private void Listen()
        {
            while (_state == StartedState)
            {
                try
                {
                    TcpClient client = _server.AcceptTcpClient();
                    new Thread(() => HandleClient(client)).Start();
                }
                catch (SocketException sex)
                {
                    if (sex.ErrorCode == 10004)
                    {
                        Debug.WriteLine("Server stopped");
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
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

        private void HandleClient(TcpClient client)
        {
            _clients.Add(client.Client.RemoteEndPoint.ToString(), client);
            NetworkStream stream = client.GetStream();

            while (client.Connected && _state == StartedState)
            {
                if (stream.DataAvailable)
                {
                    stream.BeginRead(_buffer, 0, BufferSize, OnRead, client);
                }
            }
        }

        private void OnRead(IAsyncResult ar)
        {
            var client = (TcpClient)ar.AsyncState;
            var stream = client.GetStream();
            int read = stream.EndRead(ar);

            string request = Encoding.UTF8.GetString(_buffer, 0, read);

            try
            {
                if (NetworkPackageSniffer.IsOpeningHandshake(request))
                {
                    string key = CalculateWebSocketAccept(request);
                    byte[] response = NetworkMessage.GetClosingHandshake(key);
                    stream.Write(response, 0, response.Length);
                }
                else
                {
                    var frame = new WebSocketFrame(_buffer);
                    if (!frame.IsMasked)
                    {
                        throw new ProtocolException("Payload has to be masked.", WebSocketStatusCode.ProtocolError);
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
                    else if (frame.OpCode == 8)
                    {
                        // Closing handshake
                        SendClosingHandshake(stream);

                        // Cleanup
                        _clients.Remove(client.Client.RemoteEndPoint.ToString());
                        client.Close();
                    }

                    Debug.WriteLine(result);

                    var response = new byte[2 + frame.ApplicationData.Length];
                    int byte1 = frame.OpCode == 1 ? 129 : 130;
                    int byte2 = frame.ApplicationData.Length;

                    response[0] = (byte) byte1;
                    response[1] = (byte) byte2;

                    for (var i = 0; i < frame.ApplicationData.Length; i++)
                    {
                        response[i + 2] = frame.ApplicationData[i];
                    }

                    foreach (KeyValuePair<string, TcpClient> kvp in _clients)
                    {
                        TcpClient c = kvp.Value;

                        if (client.Client.RemoteEndPoint.ToString() == c.Client.RemoteEndPoint.ToString())
                        {
                            continue;
                        }

                        NetworkStream clientStream = c.GetStream();
                        clientStream.Write(response, 0, response.Length);
                    }
                }
            }
            catch (ProtocolException pox)
            {
                SendClosingHandshake(stream, WebSocketStatusCode.ProtocolError);
            }
            catch (Exception ex)
            {
                SendClosingHandshake(stream, WebSocketStatusCode.ProtocolError);
            }
            finally
            {
                _clients.Remove(client.Client.RemoteEndPoint.ToString());
                client.Close();
            }
        }

        private void SendClosingHandshake(NetworkStream stream, WebSocketStatusCode statusCode = WebSocketStatusCode.Closure)
        {
            var response = new byte[4];
            response[0] = 136;

            if (statusCode == WebSocketStatusCode.ProtocolError)
            {
                response[1] = 8;
                // status code 1002 over 2 bytes
                response[2] = 3;
                response[3] = 234;
            }

            
            stream.Write(response, 0, response.Length);
        }

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}