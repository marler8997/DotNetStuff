using System;
using System.Net;
using System.Net.Sockets;

namespace More.Net
{
    public class NpcFrameAndHeartbeatHandler : IDataAndHeartbeatHandler
    {
        readonly Accessor accessor;
        readonly NpcDataHandler dataHandler;

        public NpcFrameAndHeartbeatHandler(Accessor accessor, NpcDataHandler dataHandler)
        {
            this.accessor = accessor;
            this.dataHandler = dataHandler;
        }
        public void HandleHeartbeat()
        {
            Console.WriteLine("{0} [{1}] Got hearbeat", DateTime.Now, accessor.accessorEndPoint);
        }
        public void HandleData(byte[] data, int offset, int length)
        {
            this.dataHandler.Handle(data, offset, length);
        }
        public void Dispose()
        {
            dataHandler.Dispose();
        }
    }

    public class Accessor
    {
        public readonly String accessorIPOrHost;
        public readonly EndPoint accessorEndPoint;
        public readonly ISocketConnector connector;

        Socket accessorSocket;
        FrameAndHeartbeatDataReceiver dataHandler;

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
                    accessorSocket.Send(FrameAndHeartbeatData.HeartBeatPacket);
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} [{1}] Sending heartbeat threw exception : {2}", DateTime.Now, accessorEndPoint, e.ToString());
                    accessorSocket = null;
                    dataHandler = null;
                }
            }
        }

        public Accessor(String connectorString)
        {
            String accessorIPOrHostAndOptionalPort = ConnectorParser.ParseConnector(connectorString, out connector);
            this.accessorIPOrHost = EndPoints.RemoveOptionalPort(accessorIPOrHostAndOptionalPort);

            this.accessorEndPoint = EndPoints.EndPointFromIPOrHostAndOptionalPort(accessorIPOrHostAndOptionalPort, Tmp.TmpAccessorPort);
        }
        public Socket MakeSocketAndConnectOnPort(UInt16 port)
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
        // return true on success
        public Boolean TryConnect(ref Int32 connectedCount, INpcServerCallback npcServerCallback, NpcExecutor npcExecutor)
        {
            Boolean connected = TryConnect(npcServerCallback, npcExecutor);
            if (connected)
            {
                connectedCount++;
            }
            return connected;
        }
        public Boolean TryConnect(INpcServerCallback npcServerCallback, NpcExecutor npcExecutor)
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

                this.dataHandler = new FrameAndHeartbeatDataReceiver(new NpcFrameAndHeartbeatHandler(this,
                    new NpcDataHandler(accessorEndPoint.ToString(), npcServerCallback,
                    new FrameAndHeartbeatDataSender(new SocketDataHandler(accessorSocket)),
                    npcExecutor, new DefaultNpcHtmlGenerator("Tunnel Manipulation", npcExecutor))));
                return true;
            }
            catch (Exception)
            {
                this.accessorSocket = null;
                this.dataHandler = null;
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
                dataHandler = null;
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
                    dataHandler = null;
                }
                else
                {
                    dataHandler.HandleData(receiveBuffer, 0, bytesRead);
                }
            }
        }
    }
}
