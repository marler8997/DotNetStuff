using System;
using System.Threading;
using System.Net.Sockets;

using More.Net;

namespace TcpGateway
{
    public class TcpGateway
    {
        public readonly PortSet listenPortSet;
        public readonly Int32 socketBackLog;

        public TcpGateway(PortSet listenPortSet, Int32 socketBackLog)
        {
            this.listenPortSet = listenPortSet;
            this.socketBackLog = socketBackLog;
        }

        public void StartThreads()
        {
            PortSetListener listener = new PortSetListener(listenPortSet, AcceptedNewClient, socketBackLog);
            listener.Start();
        }

        public void AcceptedNewClient(UInt32 socketID, UInt16 port, IncomingConnection incomingConnection)
        {
            /*
            Socket incomingSocket = incomingConnection.socket;




            Socket clientSideSocket;

            
            
            

            clientSideSocket = clientSideConnector.Connect();






            ConnectionMessageLogger connectionMessageLogger = ConnectionMessageLogger.NullConnectionMessageLogger;

            IConnectionDataLogger connectionDataLogger = new ConnectionDataLoggerPrettyLog(socketID, ConsoleDataLogger.Instance,
                clientSideConnectionSpecifier, incomingConnection.endPointName);

            SocketTunnel socketTunnel = new SocketTunnel(connectionMessageLogger, connectionDataLogger,
                clientSideSocket, incomingConnection.socket, readBufferSize);
            socketTunnel.Start();
            */
        }
    }

}
