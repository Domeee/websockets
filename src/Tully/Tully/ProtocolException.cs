using System;

namespace Tully
{
    internal class ProtocolException : Exception
    {
        internal ProtocolException(WebSocketStatusCode statusCode)
        {
            StatusCode = statusCode;
        }

        public ProtocolException(string p, WebSocketStatusCode webSocketStatusCode)
            : base(p)
        {
            StatusCode = webSocketStatusCode;
        }

        internal WebSocketStatusCode StatusCode { get; set; }
    }
}