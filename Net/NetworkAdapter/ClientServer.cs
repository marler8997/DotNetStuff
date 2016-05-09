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
        readonly InternetHost server;
        readonly String clientSideServerLogString;

        readonly PortSet listenPorts;
        public readonly Int32 socketBackLog;
        public readonly Int32 readBufferSize;
        readonly Boolean logData;

        public ClientServer(InternetHost server, ClientConnectWaitMode clientWaitMode,
            PortSet listenPorts, Int32 socketBackLog, Int32 readBufferSize,
            Boolean logData)
        {
            this.server = server;
            this.clientSideServerLogString = server.CreateTargetString();

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
            Socket clientSideSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            BufStruct dataLeftOverFromServer = default(BufStruct);
            clientSideSocket.Connect(server, DnsPriority.IPv4ThenIPv6, ProxyConnectOptions.None, ref dataLeftOverFromServer);
            if (dataLeftOverFromServer.contentLength > 0)
            {
                throw new NotImplementedException(String.Format("There are {0} bytes left over from negotiating proxy with the server, this is not implemented", dataLeftOverFromServer.contentLength));
            }

            ConnectionMessageLogger messageLogger = ConnectionMessageLogger.NullConnectionMessageLogger;
            IConnectionDataLogger dataLogger = logData ? new ConnectionDataLoggerPrettyLog(socketID, ConsoleDataLogger.Instance,
                clientSideServerLogString, incomingConnection.endPointName) :
                ConnectionDataLogger.Null;

            TwoWaySocketTunnel socketTunnel = new TwoWaySocketTunnel(
                clientSideSocket, incomingConnection.socket, readBufferSize, messageLogger, dataLogger);
            new Thread(socketTunnel.StartOneAndRunOne).Start();
        }
    }
}
