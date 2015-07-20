# Tully WebSockets
websockets

# Usage
## Server
```csharp
var server = new WebSocketServer("192.168.1.68", 80);
server.Started += (sender, args) => Console.WriteLine("WebSocket server started!");
server.Stopped += (sender, args) => Console.WriteLine("WebSocket server stopped!");
server.Start();
```
##Client
```csharp
using (var client = new WebSocket("192.168.1.68", 80))
{
	client.Opened += (sender, eventArgs) => Console.WriteLine("Connection open!");
	client.Open();
	client.SendMdnString();
}
```
# Features
According to RFC 6455

# Motivation

# Credits

# License

The MIT License (MIT)

Copyright (c) 2015 

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
