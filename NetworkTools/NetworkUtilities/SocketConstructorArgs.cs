using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Marler.NetworkTools
{
    public interface ISocketFactory
    {
        Socket New();
    }

    public class SocketFactory : ISocketFactory
    {        
        public readonly AddressFamily addressFamily;
        public readonly SocketType socketType;
        public readonly ProtocolType protocolType;
        public SocketFactory(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            this.addressFamily = addressFamily;
            this.socketType = socketType;
            this.protocolType = protocolType;
        }

        public Socket New()
        {
            return new Socket(addressFamily, socketType, protocolType);
        }
    }
}
