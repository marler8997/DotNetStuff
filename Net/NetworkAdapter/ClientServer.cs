using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace More.Net
{
    public class ClientServer
    {
        private readonly EndPoint clientSideServerEndPoint;
        private readonly ISocketConnector clientSideProxyConnector;

        private readonly PortSet listenPorts;
        public readonly Int32 socketBackLog;
        public readonly Int32 readBufferSize;
        readonly Boolean logData;

        public ClientServer(EndPoint clientSideServerEndPoint, ISocketConnector clientSideProxyConnector,
            ClientConnectWaitMode clientWaitMode, PortSet listenPorts, Int32 socketBackLog, Int32 readBufferSize,
            Boolean logData)
        {
            this.clientSideServerEndPoint = clientSideServerEndPoint;
            this.clientSideProxyConnector = clientSideProxyConnector;

            this.listenPorts = listenPorts;
            this.socketBackLog = socketBackLog;

            this.readBufferSize = readBufferSize;
            this.logData = logData;
        }

        public void Start()
        {
            PortSetListener listener = new PortSetListener(listenPorts, AcceptedNewClient, socketBackLog);
            listener.Start();
        }

        public void AcceptedNewClient(UInt32 socketID, UInt16 port, IncomingConnection incomingConnection)
        {
            Socket clientSideSocket = new Socket(clientSideServerEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            if (clientSideProxyConnector == null)
            {
                clientSideSocket.Connect(clientSideServerEndPoint);
            }
            else
            {
                clientSideProxyConnector.Connect(clientSideSocket, clientSideServerEndPoint);
            }

            ConnectionMessageLogger messageLogger = ConnectionMessageLogger.NullConnectionMessageLogger;
            IConnectionDataLogger dataLogger = logData ? new ConnectionDataLoggerPrettyLog(socketID, ConsoleDataLogger.Instance,
                clientSideServerEndPoint.ToString(), incomingConnection.endPointName) :
                ConnectionDataLogger.Null;

            TwoWaySocketTunnel socketTunnel = new TwoWaySocketTunnel(
                clientSideSocket, incomingConnection.socket, readBufferSize, messageLogger, dataLogger);
            new Thread(socketTunnel.StartOneAndRunOne).Start();
        }
    }
}
