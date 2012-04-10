using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Marler.NetworkTools
{

    public interface ISocketConnector
    {
        String ConnectionSpecifier { get; }
        Socket Connect();
        Socket Connect(TimeSpan timeout);
    }

    public interface IDirectSocketConnector : ISocketConnector
    {
        String HostAndPort { get; }
    }

    public class SocketConnectorFromHost : IDirectSocketConnector
    {
        public readonly String host;
        public readonly UInt16 port;

        public SocketConnectorFromHost(String host, UInt16 port)
        {
            this.host = host;
            this.port = port;
        }

        public String HostAndPort { get { return String.Format("{0}:{1}", host, port); } }
        public String ConnectionSpecifier { get { return String.Format("{0}:{1}", host, port); } }

        public Socket Connect()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(host, port);
            return socket;
        }

        public Socket Connect(TimeSpan timeout)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (socket.ConnectWithTimeout(host, port, timeout))
            {
                return socket;
            }
            return null;
        }

        public override String ToString()
        {
            return ConnectionSpecifier;
        }
    }


    public class SocketConnectorFromIPAddress : IDirectSocketConnector
    {
        public readonly IPAddress ipAddress;
        public readonly UInt16 port;

        public SocketConnectorFromIPAddress(IPAddress ipAddress, UInt16 port)
        {
            this.ipAddress = ipAddress;
            this.port = port;
        }

        public String HostAndPort { get { return String.Format("{0}:{1}", ipAddress.ToString(), port); } }
        public String ConnectionSpecifier { get { return String.Format("{0}:{1}", ipAddress.ToString(), port); } }

        public Socket Connect()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipAddress, port);
            return socket;
        }
        public Socket Connect(TimeSpan timeout)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (socket.ConnectWithTimeout(new IPEndPoint(ipAddress, port), timeout))
            {
                return socket;
            }
            return null;
        }


        public override String ToString()
        {
            return ConnectionSpecifier;
        }
    }

    public class SocketConnectorFromProxy4 : ISocketConnector
    {
        private readonly IDirectSocketConnector proxyConnector;
        private readonly byte[] userID;

        public readonly IPAddress hostIP;
        public readonly UInt16 hostPort;

        public SocketConnectorFromProxy4(IDirectSocketConnector proxyConnector, byte[] userID, IPAddress hostIP, UInt16 hostPort)
        {
            this.proxyConnector = proxyConnector;
            this.userID = userID;

            this.hostIP = hostIP;
            this.hostPort = hostPort;
        }

        public String ConnectionSpecifier { get { return GenericUtilities.Proxy4Name(userID, proxyConnector, hostIP, hostPort); } }

        public Socket Connect()
        {
            Proxy4ConnectSocket proxySocket = new Proxy4ConnectSocket(userID, proxyConnector);
            return proxySocket.Connect(hostIP, hostPort);
        }
        public Socket Connect(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }
        public override String ToString()
        {
            return ConnectionSpecifier;
        }
    }
    public class SocketConnectorFromProxy4a : ISocketConnector
    {
        private readonly IDirectSocketConnector proxyConnector;
        public readonly String proxyHost;
        public readonly UInt16 proxyPort;
        private readonly byte[] userID;

        public readonly String host;
        public readonly UInt16 hostPort;

        public SocketConnectorFromProxy4a(IDirectSocketConnector proxyConnector, byte[] userID, String host, UInt16 hostPort)
        {
            this.proxyConnector = proxyConnector;
            this.userID = userID;

            this.host = host;
            this.hostPort = hostPort;
        }

        public String ConnectionSpecifier { get { return GenericUtilities.Proxy4aName(userID, proxyConnector, host, hostPort); } }

        public Socket Connect()
        {
            Proxy4aConnectSocket proxySocket = new Proxy4aConnectSocket(userID, proxyConnector);
            return proxySocket.Connect(host, hostPort);
        }
        public Socket Connect(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public override String ToString()
        {
            return ConnectionSpecifier;
        }
    }
    public class SocketConnectorFromProxy5NoAuthentication : ISocketConnector
    {
        private readonly IDirectSocketConnector proxyConnector;
        public readonly String proxyHost;
        public readonly UInt16 proxyPort;

        public readonly String host;
        public readonly UInt16 hostPort;

        public SocketConnectorFromProxy5NoAuthentication(IDirectSocketConnector proxyConnector, String host, UInt16 hostPort)
        {
            this.proxyConnector = proxyConnector;

            this.host = host;
            this.hostPort = hostPort;
        }

        public String ConnectionSpecifier { get { throw new NotImplementedException(); } }

        public Socket Connect()
        {
            Proxy5NoAuthenticationConnectSocket proxySocket = new Proxy5NoAuthenticationConnectSocket(ProtocolType.Tcp, proxyConnector);
            return proxySocket.Connect(GenericUtilities.ResolveHost(host), hostPort);
        }
        public Socket Connect(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }
        public override String ToString()
        {
            return ConnectionSpecifier;
        }
    }

    public class SocketConnectorFromProxy5UsernamePassword : ISocketConnector
    {
        private readonly IDirectSocketConnector proxyConnector;

        public readonly String username;
        public readonly String password;

        public readonly String host;
        public readonly UInt16 hostPort;

        public SocketConnectorFromProxy5UsernamePassword(IDirectSocketConnector proxyConnector, String username, String password,
            String host, UInt16 hostPort)
        {
            this.proxyConnector = proxyConnector;

            this.username = username;
            this.password = password;

            this.host = host;
            this.hostPort = hostPort;
        }

        public String ConnectionSpecifier { get { return String.Format("{0}:{1}", host, hostPort); } }
        public String Name { get { throw new NotImplementedException(); } }

        public Socket Connect()
        {
            Proxy5UsernamePasswordConnectSocket proxySocket = new Proxy5UsernamePasswordConnectSocket(
                ProtocolType.Tcp, proxyConnector, username, password);
            return proxySocket.Connect(GenericUtilities.ResolveHost(host), hostPort);
        }
        public Socket Connect(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }
        public override String ToString()
        {
            return ConnectionSpecifier;
        }
    }
}
