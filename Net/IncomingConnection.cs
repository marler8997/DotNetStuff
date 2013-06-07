using System;
using System.Net.Sockets;

namespace More.Net
{
    public class IncomingConnection
    {
        public readonly String endPointName;
        public readonly Socket socket;
        public readonly UInt16 localPort;

        public IncomingConnection(Socket socket, UInt16 localPort)
        {
            this.endPointName = socket.SafeRemoteEndPointString();
            this.socket = socket;
            this.localPort = localPort;
        }

        public override string ToString()
        {
            return endPointName;
        }
    }
}
