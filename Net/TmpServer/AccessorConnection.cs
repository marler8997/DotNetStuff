using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using More.Net.TmpCommand;

namespace More.Net
{
    public class AccessorConnection
    {
        public readonly String accessorIPOrHost;
        public readonly EndPoint accessorEndPoint;
        public readonly ISocketConnector connector;

        Socket accessorSocket;
        DataFilterHandler accessorReceiveHandler;

        public Boolean Connected
        {
            get
            {
                return accessorSocket != null && accessorSocket.Connected;
            }
        }
        public void SendHeartbeat()
        {
            if (accessorSocket != null)
            {
                try
                {
                    accessorSocket.Send(FrameProtocol.HeartbeatSendPacket);
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} [{1}] Sending heartbeat threw exception : {2}", DateTime.Now, accessorEndPoint, e.ToString());
                    accessorSocket = null;
                    accessorReceiveHandler = null;
                }
            }
        }

        public AccessorConnection(String connectorString)
        {
            String accessorIPOrHostAndOptionalPort = ConnectorParser.ParseConnector(connectorString, out connector);
            this.accessorIPOrHost = EndPoints.RemoveOptionalPort(accessorIPOrHostAndOptionalPort);

            this.accessorEndPoint = EndPoints.EndPointFromIPOrHostAndOptionalPort(accessorIPOrHostAndOptionalPort, Tmp.DefaultPort);
        }
        public Socket MakeNewSocketAndConnect()
        {
            Socket socket = new Socket(accessorEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (connector == null)
            {
                socket.Connect(accessorEndPoint);
            }
            else
            {
                connector.Connect(socket, accessorEndPoint);
            }
            return socket;
        }
        public Socket MakeNewSocketAndConnectOnPort(UInt16 port)
        {
            EndPoint thisPortEndPoint = EndPoints.EndPointFromIPOrHost(accessorIPOrHost, port);

            Socket socket = new Socket(thisPortEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (connector == null)
            {
                socket.Connect(thisPortEndPoint);
            }
            else
            {
                connector.Connect(socket, thisPortEndPoint);
            }
            return socket;
        }
        public Boolean TryConnectAndInitialize(TlsSettings tlsSettings, Buf sendBuffer, ServerInfo serverInfo, SelectTunnelsThread tunnelsThread)
        {
            if (Connected) throw new InvalidOperationException(String.Format(
                "You called Connect() on accessor '{0}' but its already connected", accessorEndPoint));

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                if (connector == null)
                {
                    socket.Connect(accessorEndPoint);
                }
                else
                {
                    connector.Connect(socket, accessorEndPoint);
                }

                this.accessorSocket = socket;

                //
                // Send initial connection information
                //
                Boolean setupTls = tlsSettings.requireTlsForTmpConnections;

                Byte[] connectionInfoPacket = new Byte[] {Tmp.CreateConnectionInfoFromTmpServerToAccessor(
                    tlsSettings.requireTlsForTmpConnections, false)};
                socket.Send(connectionInfoPacket);

                //
                // Only receive packet if tls was not required
                //
                if (!tlsSettings.requireTlsForTmpConnections)
                {
                    int bytesRead = socket.Receive(connectionInfoPacket, 0, 1, SocketFlags.None);
                    if (bytesRead <= 0) throw new SocketException();

                    setupTls = Tmp.ReadTlsRequirementFromAccessorToTmpServer(connectionInfoPacket[0]);
                }


                DataHandler accessorSendHandler = new SocketSendDataHandler(socket).HandleData;
                this.accessorReceiveHandler = new DataFilterHandler(new FrameProtocolFilter(),
                        new TmpServerHandler(this, tlsSettings, tunnelsThread));

                //
                // Initiate a tls connection if required
                //
                if (setupTls)
                {
                    throw new NotImplementedException("Tls not yet implemented");
                }

                //
                // Send server info
                //
                UInt32 commandLength = Tmp.SerializeCommand(ServerInfo.Serializer, Tmp.ToAccessorServerInfoID, serverInfo, sendBuffer, 0);
                accessorSendHandler(sendBuffer.array, 0, commandLength);

                return true;
            }
            catch (Exception)
            {
                this.accessorSocket = null;
                this.accessorReceiveHandler = null;
                return false;
            }
        }
        public void ReceiveWithTimeout(Byte[] receiveBuffer, Int32 timeoutMillis)
        {
            Console.WriteLine("{0} [{1}] Select timeout is {2} seconds", DateTime.Now, accessorEndPoint, timeoutMillis / 1000);

            SingleObjectList list = new SingleObjectList(accessorSocket);

            try
            {
                Socket.Select(list, null, null, timeoutMillis * 1000);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} [{1}] Select threw exception : {2}", DateTime.Now, accessorEndPoint, e.ToString());
                accessorSocket = null;
                accessorReceiveHandler = null;
                return;
            }

            if (list.obj != null)
            {
                Int32 bytesRead = 0;
                try
                {
                    bytesRead = accessorSocket.Receive(receiveBuffer);
                }
                catch (Exception)
                {
                }

                if (bytesRead <= 0)
                {
                    accessorSocket = null;
                    accessorReceiveHandler = null;
                }
                else
                {
                    accessorReceiveHandler.HandleData(receiveBuffer, 0, (UInt32)bytesRead);
                }
            }
        }
    }
}
