using System;
using System.Net.Sockets;

namespace Marler.Net
{
    public class TcpSocket : Socket
    {
        public TcpSocket(AddressFamily addressFamily)
            : base(addressFamily, SocketType.Stream, ProtocolType.Tcp)
        {
        }
    }
    public class UdpSocket : Socket
    {
        public UdpSocket(AddressFamily addressFamily)
            : base(addressFamily, SocketType.Dgram, ProtocolType.Udp)
        {
        }
    }
}
