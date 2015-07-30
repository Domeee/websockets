using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tully
{
    public class WebSocketEventArgs : EventArgs
    {
        public object Data { get; set; }
    }
}
