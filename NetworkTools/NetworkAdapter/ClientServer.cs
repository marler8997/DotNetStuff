using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace Marler.NetworkTools
{
    public class ClientServer
    {
        private readonly ISocketConnector clientSideConnector;
        public readonly ClientConnectWaitMode clientWaitMode;
        public readonly String clientSideConnectionSpecifier;

        private readonly PortSet listenPorts;
        public readonly Int32 socketBackLog;
        public readonly Int32 readBufferSize;

        private Socket nextClientSideSocket;

        public ClientServer(ISocketConnector clientSideConnector,
            ClientConnectWaitMode clientWaitMode, PortSet listenPorts, Int32 socketBackLog, Int32 readBufferSize)
        {
            this.clientSideConnector = clientSideConnector;
            this.clientWaitMode = clientWaitMode;
            this.clientSideConnectionSpecifier = clientSideConnector.ConnectionSpecifier;

            this.listenPorts = listenPorts;
            this.socketBackLog = socketBackLog;
            this.readBufferSize = readBufferSize;

            this.nextClientSideSocket = null;
        }

        public void Start()
        {
            if (clientWaitMode == ClientConnectWaitMode.Immediate)
            {
                nextClientSideSocket = clientSideConnector.Connect();
            }

            PortSetListener listener = new PortSetListener(
                listenPorts, AcceptedNewClient, socketBackLog);
            listener.Start();
        }

        public void AcceptedNewClient(UInt32 socketID, UInt16 port, IncomingConnection incomingConnection)
        {
            Socket clientSideSocket;

            lock(clientSideConnector)
            {
                if(clientWaitMode == ClientConnectWaitMode.Immediate)
                {
                    if(nextClientSideSocket == null) throw new InvalidOperationException(
                        "clientWaitMode = Immediate, but the nextClientSocketSocket is null?");
                    clientSideSocket = nextClientSideSocket;
                }
            }
            if(clientWaitMode == ClientConnectWaitMode.OtherEndToConnect)
            {
                clientSideSocket = clientSideConnector.Connect();
            }
            else if(clientWaitMode == ClientConnectWaitMode.OtherEndReceiveConnectRequest)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new InvalidOperationException(String.Format("clientWaitMode is unrecognized '{0}' ({1})",
                    clientWaitMode, (Int32) clientWaitMode));
            }

            ConnectionMessageLogger messageLogger = ConnectionMessageLogger.NullConnectionMessageLogger;
            /*
            ConnectionMessageLogger connectionMessageLogger = new ConnectionMessageLoggerSingleLog(
                new ConsoleMessageLogger("Tunnel"), String.Format("{0} to {1}", clientSideConnectionSpecifier, incomingConnection.endPointName),
                String.Format("{0} to {1}", incomingConnection.endPointName, clientSideConnectionSpecifier));
            */
            IConnectionDataLogger dataLogger = new ConnectionDataLoggerPrettyLog(socketID, ConsoleDataLogger.Instance,
                clientSideConnectionSpecifier, incomingConnection.endPointName);

            TwoWaySocketTunnel socketTunnel = new TwoWaySocketTunnel(null,
                clientSideSocket, incomingConnection.socket, readBufferSize, messageLogger, dataLogger);
            new Thread(socketTunnel.StartOneAndRunOne).Start();
        }


    }
}
