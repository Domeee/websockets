using System.Text;

namespace Tully
{
    internal class NetworkMessage
    {
        internal static byte[] GetOpeningHandshake(string host, string webSocketKey)
        {
            var message = new StringBuilder();
            message.AppendLine("GET / HTTP/1.1");
            message.Append("Host: ");
            message.AppendLine(host);
            message.AppendLine("Upgrade: websocket");
            message.AppendLine("Connection: Upgrade");
            message.Append("Sec-WebSocket-Key: ");
            message.AppendLine(webSocketKey);
            message.AppendLine("Sec-WebSocket-Version: 13");
            message.AppendLine();
            return Encoding.UTF8.GetBytes(message.ToString());
        }

        internal static byte[] GetClosingHandshake(string webSocketAccept)
        {
            var response = new StringBuilder();
            response.AppendLine("HTTP/1.1 101 Switching Protocols");
            response.AppendLine("Connection: Upgrade");
            response.AppendLine("Upgrade: websocket");
            response.Append("Sec-WebSocket-Accept: ");
            response.AppendLine(webSocketAccept);
            response.AppendLine();

            return Encoding.UTF8.GetBytes(response.ToString());
        }
    }
}