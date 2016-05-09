using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using More.Net.TmpCommand;

namespace More.Net
{
    public class AccessorConnection
    {
        InternetHost accessorHost;
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
            Proxy proxy;
            String ipOrHostOptionalPort = Proxy.StripAndParseProxies(connectorString, DnsPriority.IPv4ThenIPv6, out proxy);
            UInt16 port = Tmp.DefaultPort;
            String ipOrHost = EndPoints.SplitIPOrHostAndOptionalPort(ipOrHostOptionalPort, ref port);
            this.accessorHost = new InternetHost(ipOrHost, port, DnsPriority.IPv4ThenIPv6, proxy);
            this.accessorHostString = accessorHost.CreateTargetString();
        }
        public Socket MakeNewSocketAndConnect()
        {
            return NewSocketConnection(ref accessorHost);
        }
        public Socket MakeNewSocketAndConnectOnPort(UInt16 port)
        {
            InternetHost tempHost = new InternetHost(accessorHost, port);
            return NewSocketConnection(ref tempHost);
        }
        public static Socket NewSocketConnection(ref InternetHost host)
        {
            Socket socket = new Socket(host.GetAddressFamilyForTcp(), SocketType.Stream, ProtocolType.Tcp);
            BufStruct dataLeftOverFromProxyConnect = default(BufStruct);
            host.Connect(socket, DnsPriority.IPv4ThenIPv6, ProxyConnectOptions.None, ref dataLeftOverFromProxyConnect);
            if (dataLeftOverFromProxyConnect.contentLength > 0)
            {
                throw new NotImplementedException();
            }
            return socket;
        }
        public Boolean TryConnectAndInitialize(TlsSettings tlsSettings, Buf sendBuffer, ServerInfo serverInfo, SelectTunnelsThread tunnelsThread)
        {
            if (Connected) throw new InvalidOperationException(String.Format(
                "You called Connect() on accessor '{0}' but its already connected", accessorHostString));

            try
            {
                accessorSocket = NewSocketConnection(ref accessorHost);

                //
                // Send initial connection information
                //
                Boolean setupTls = tlsSettings.requireTlsForTmpConnections;

                Byte[] connectionInfoPacket = new Byte[] {Tmp.CreateConnectionInfoFromTmpServerToAccessor(
                    tlsSettings.requireTlsForTmpConnections, false)};
                accessorSocket.Send(connectionInfoPacket);

                //
                // Only receive packet if tls was not required
                //
                if (!tlsSettings.requireTlsForTmpConnections)
                {
                    int bytesRead = accessorSocket.Receive(connectionInfoPacket, 0, 1, SocketFlags.None);
                    if (bytesRead <= 0) throw new SocketException();

                    setupTls = Tmp.ReadTlsRequirementFromAccessorToTmpServer(connectionInfoPacket[0]);
                }


                DataHandler accessorSendHandler = new SocketSendDataHandler(accessorSocket).HandleData;
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
