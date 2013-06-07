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
        private readonly ISocketConnector clientSideConnector;
        //public readonly String clientSideConnectionSpecifier;

        private readonly PortSet listenPorts;
        public readonly Int32 socketBackLog;
        public readonly Int32 readBufferSize;

        public ClientServer(EndPoint clientSideServerEndPoint, ISocketConnector clientSideConnector,
            ClientConnectWaitMode clientWaitMode, PortSet listenPorts, Int32 socketBackLog, Int32 readBufferSize)
        {
            this.clientSideConnector = clientSideConnector;

            this.listenPorts = listenPorts;
            this.socketBackLog = socketBackLog;

            this.readBufferSize = readBufferSize;
        }

        public void Start()
        {
            PortSetListener listener = new PortSetListener(listenPorts, AcceptedNewClient, socketBackLog);
            listener.Start();
        }

        public void AcceptedNewClient(UInt32 socketID, UInt16 port, IncomingConnection incomingConnection)
        {
            Socket clientSideSocket = new Socket(clientSideServerEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            clientSideConnector.Connect(clientSideSocket, clientSideServerEndPoint);

            ConnectionMessageLogger messageLogger = ConnectionMessageLogger.NullConnectionMessageLogger;
            IConnectionDataLogger dataLogger = new ConnectionDataLoggerPrettyLog(socketID, ConsoleDataLogger.Instance,
                clientSideServerEndPoint.ToString(), incomingConnection.endPointName);

            TwoWaySocketTunnel socketTunnel = new TwoWaySocketTunnel(
                clientSideSocket, incomingConnection.socket, readBufferSize, messageLogger, dataLogger);
            new Thread(socketTunnel.StartOneAndRunOne).Start();
        }


    }
}
