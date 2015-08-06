using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using More.Net.TmpCommand;

namespace More.Net
{
    public class AccessorConnection
    {
        HostWithOptionalProxy accessorHost;
        public readonly String accessorHostString;

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
                    Console.WriteLine("{0} [{1}] Sending heartbeat threw exception : {2}", DateTime.Now, accessorHost, e.ToString());
                    accessorSocket = null;
                    accessorReceiveHandler = null;
                }
            }
        }

        public AccessorConnection(String connectorString)
        {
            this.accessorHost = ConnectorParser.ParseConnectorWithOptionalPortAndProxy(connectorString, Tmp.DefaultPort);
            this.accessorHostString = accessorHost.TargetString();
        }
        public Socket MakeNewSocketAndConnect()
        {
            Socket socket = new Socket(accessorHost.directEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.ConnectTcpSocketThroughProxy(accessorHost);
            return socket;
        }
        public Socket MakeNewSocketAndConnectOnPort(UInt16 port)
        {
            HostWithOptionalProxy newHost = new HostWithOptionalProxy(accessorHost, port);
            Socket socket = new Socket(newHost.directEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.ConnectTcpSocketThroughProxy(newHost);
            return socket;
        }
        public Boolean TryConnectAndInitialize(TlsSettings tlsSettings, Buf sendBuffer, ServerInfo serverInfo, SelectTunnelsThread tunnelsThread)
        {
            if (Connected) throw new InvalidOperationException(String.Format(
                "You called Connect() on accessor '{0}' but its already connected", accessorHostString));

            Socket socket = new Socket(accessorHost.directEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.ConnectTcpSocketThroughProxy(accessorHost);

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
            Console.WriteLine("{0} [{1}] Select timeout is {2} seconds", DateTime.Now, accessorHostString, timeoutMillis / 1000);

            SingleObjectList list = new SingleObjectList(accessorSocket);

            try
            {
                Socket.Select(list, null, null, timeoutMillis * 1000);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} [{1}] Select threw exception : {2}", DateTime.Now, accessorHostString, e.ToString());
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
