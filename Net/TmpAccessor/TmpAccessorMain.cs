﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using More;
using More.Net.TmpCommand;

namespace More.Net
{
    class TmpAccessorOptions : CLParser
    {
        public readonly CLGenericArgument<UInt16> tmpListenPort;
        public readonly CLSwitch noNfs;

        public TmpAccessorOptions()
        {
            tmpListenPort = new CLGenericArgument<UInt16>(UInt16.Parse, 't', "tmp-port", "The TMP (Tunnel Manipulation Protocol) listen port");
            tmpListenPort.SetDefault(Tmp.DefaultPort);
            Add(tmpListenPort);

            noNfs = new CLSwitch('n', "no-nfs", "Do not start the nfs control server");
            Add(noNfs);
        }
        public override void PrintUsageHeader()
        {
            Console.WriteLine("TmpAccessor.exe [options] <tunnel-listener>");
            Console.WriteLine("    <tunnel-listener>     <server-name>,<target-host>,<targetPort>[,<listen-port>]");
        }
    }
    public static class TmpAccessorMain
    {
        static TmpConnectionManager TmpConnectionManager;

        static void Main(string[] args)
        {
            TmpAccessorOptions options = new TmpAccessorOptions();
            List<String> nonOptionArgs = options.Parse(args);

            TlsSettings settings = new TlsSettings(false);

            TmpConnectionManager = new TmpConnectionManager(settings);

            //
            // Add Tunnel Listeners from command line arguments
            //
            if(nonOptionArgs.Count > 0)
            {
                for(int i = 0; i < nonOptionArgs.Count; i++)
                {
                    String tunnelListenerString = nonOptionArgs[i];

                    String[] tunnelListenerStrings = tunnelListenerString.Split(new Char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                    if(tunnelListenerStrings.Length < 3)
                    {
                        options.ErrorAndUsage("A tunnel-listener must have at least 3 comma seperated values but '{0}' only had {1}", tunnelListenerString, tunnelListenerStrings.Length);
                        Environment.Exit(1);
                    }

                    String serverName = tunnelListenerStrings[0];                    
                    String targetHost = tunnelListenerStrings[1];
                    UInt16 targetPort = UInt16.Parse(tunnelListenerStrings[2]);

                    if(tunnelListenerStrings.Length == 3)
                    {
                        TmpConnectionManager.AddTunnelListener(serverName, false, targetHost, targetPort);
                    }
                    else if(tunnelListenerStrings.Length == 4)
                    {
                        UInt16 listenPort = UInt16.Parse(tunnelListenerStrings[3]);
                        TmpConnectionManager.AddTunnelListener(serverName, false, targetHost, targetPort, listenPort);
                    }
                    else
                    {
                        options.ErrorAndUsage("A tunnel-listener may not have more than 4 comma seperated values but '{0}' had {1}", tunnelListenerString, tunnelListenerStrings.Length);
                        Environment.Exit(1);
                    }
                }
            }

            if (!options.noNfs.set)
            {
                //
                // Setup and start Npc Accessor Control Server
                //
                NpcReflector npcReflector = new NpcReflector(
                    new NpcExecutionObject(TmpConnectionManager, "Accessor", null, null));
                NpcServerSingleThreaded npcServer = new NpcServerSingleThreaded(
                    NpcServerConsoleLoggerCallback.Instance, npcReflector, new DefaultNpcHtmlGenerator("Accessor", npcReflector), 2030);
                Thread npcServerThread = new Thread(npcServer.Run);
                npcServerThread.Start();
            }

            //
            // Setup and run Tmp listen/handler thread
            //
            SelectControl selectControl = new SelectControl(false);

            IPAddress listenIP = IPAddress.Any;
            Socket listenSocket = new Socket(listenIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(listenIP, Tmp.DefaultPort));
            listenSocket.Listen(8);
            selectControl.AddListenSocket(listenSocket, AcceptClient);

            SelectServer selectServer = new SelectServer(selectControl, 1024, 1024, false);
            selectServer.Run();
        }
        public static void AcceptClient(ref SelectControl selectControl, Socket listenSocket, Buf safeBuffer)
        {
            Console.WriteLine("{0} [{1}] Accepted TmpServer Socket", DateTime.Now, listenSocket.SafeLocalEndPointString());
            selectControl.PerformAccept(listenSocket, TmpConnectionManager.HandleInitialConnectionInfo);
        }
    }
    [NpcInterface]
    public interface AccessorControlInterface
    {
        String[] GetServerNames();

        //
        // A tunnel listener is a port that the accessor listens on.
        // When accessor gets a connection on that port, it opens a tunnel to the TmpServer and forwards all
        // comunication to the tunnel
        //
        void AddTunnelListener(String serverName, Boolean requireTls, String targetHost, UInt16 targetPort, UInt16 listenPort);
        // returns listen port
        UInt16 AddTunnelListener(String serverName, Boolean requireTls, String targetHost, UInt16 targetPort);
    }
    public class TmpConnectionManager : AccessorControlInterface
    {
        public const Int32 DefaultTunnelListenerBacklog = 8;

        TlsSettings tlsSettings;

        readonly Random random;

        readonly List<TmpControlConnection> tmpControlConnections;
        readonly Dictionary<String, TmpControlConnection> serverNameToControlConnection;

        readonly SelectServerDynamicTcpListeners tunnelSelectServer;
        readonly Dictionary<Int32,DisconnectedTunnel> incompleteTunnels;

        public TmpConnectionManager(TlsSettings tlsSettings)
        {
            this.tlsSettings = tlsSettings;

            this.random = new Random((Int32)Stopwatch.GetTimestamp());

            this.tmpControlConnections = new List<TmpControlConnection>();
            this.serverNameToControlConnection = new Dictionary<String, TmpControlConnection>();

            this.tunnelSelectServer = new SelectServerDynamicTcpListeners(1024, 1024, false);
            new Thread(tunnelSelectServer.Run).Start();

            this.incompleteTunnels = new Dictionary<Int32, DisconnectedTunnel>();
        }
        public TmpControlConnection TryGetTmpControlConnection(String serverName)
        {
            TmpControlConnection tmpControlConnection;
            if (serverNameToControlConnection.TryGetValue(serverName, out tmpControlConnection))
            {
                return tmpControlConnection;
            }
            return null;
        }
        /*
        void SocketFromTmpServerClosed(Socket closedSocket)
        {
            Console.WriteLine("{0} WARNING: Socket closed using the default initial SocketCloseHandler", DateTime.Now);
        }
        */
        public void GotServerName(String oldServerName, String newServerName, TmpControlConnection controlConnection)
        {
            if(oldServerName != null)
            {
                if(oldServerName.Equals(newServerName)) return;                
                serverNameToControlConnection.Remove(oldServerName);
            }

            //
            // Check if this server name already exists, if so, dispose that connection
            //
            TmpControlConnection existingConnection;
            if (serverNameToControlConnection.TryGetValue(newServerName, out existingConnection))
            {
                if (existingConnection == controlConnection) return;

                Console.WriteLine("{0} Got ServerInfo from new TmpControlConnection with Name='{1}', however, a connection with that name already exists...disposing the old one and using the new one",
                    DateTime.Now, newServerName);
                existingConnection.Dispose();
            }

            serverNameToControlConnection[newServerName] = controlConnection;
        }
        public void LostTmpControlConnection(TmpControlConnection tmpControlConnection)
        {
            tmpControlConnections.Remove(tmpControlConnection);
            serverNameToControlConnection.Remove(tmpControlConnection.ServerInfoName);
        }
        /*
        public SimpleSelectHandler HandleConnectionFromTmpServer(Socket listenSocket, Socket socket, Buf safeBuffer)
        {
            Console.WriteLine("{0} [{1}] Accepted TmpServer Socket", DateTime.Now, socket.SafeRemoteEndPointString());
            return HandleInitialConnectionInfo;
        }
        */
        public void HandleInitialConnectionInfo(ref SelectControl selectControl, Socket socket, Buf safeBuffer)
        {
            Int32 bytesRead = socket.Receive(safeBuffer.array, 1, SocketFlags.None);
            if (bytesRead <= 0)
            {
                Console.WriteLine("{0} WARNING: Socket closed", DateTime.Now);
                selectControl.DisposeAndRemoveReceiveSocket(socket);
                //receiveHandler = null;
                return;
            }

            Byte connectionInfo = safeBuffer.array[0];

            Boolean accessorRequiresTls, isTunnel;
            Tmp.ReadConnectionInfoFromTmpServer(connectionInfo, out accessorRequiresTls, out isTunnel);

            //
            // Determine if TLS should be set up
            //
            Boolean setupTls;
            if(accessorRequiresTls)
            {
                setupTls = true;
            }
            else if (!isTunnel)
            {
                // The TmpServer is waiting for a response to indicate whether it should setup TLS
                setupTls = tlsSettings.requireTlsForTmpConnections;
                socket.Send(new Byte[] { setupTls ? (Byte)1 : (Byte)0 });
            }
            else
            {
                setupTls = false;
            }

            IDataHandler sendDataHandler = new SocketSendDataHandler(socket);
            IDataFilter receiveDataFilter = null;

            //
            // Setup TLS if necessary
            //
            if (setupTls)
            {
                //
                // Negotiate TLS, setup sendDataHandler and receiveDataFilter
                //
                Console.WriteLine("{0} [{1}] This connection requires tls but it is not currently supported",
                    DateTime.Now, socket.SafeRemoteEndPointString());
                selectControl.ShutdownDisposeAndRemoveReceiveSocket(socket);
                return;             
            }

            IPEndPoint remoteEndPoint = (IPEndPoint)(socket.RemoteEndPoint);

            if (isTunnel)
            {
                Console.WriteLine("{0} [{1}] Is a Tunnel Connection", DateTime.Now, remoteEndPoint.ToString());
                TmpServerSideTunnelKeyReceiver keyReceiver = new TmpServerSideTunnelKeyReceiver(this);
                selectControl.UpdateHandler(socket, keyReceiver.SocketReceiverHandler);
            }
            else
            {
                Console.WriteLine("{0} [{1}] Is a Control Connection", DateTime.Now, remoteEndPoint.ToString());
                TmpControlConnection tmpControlConnection = new TmpControlConnection(this, tlsSettings,
                    remoteEndPoint, socket, sendDataHandler, receiveDataFilter);
                lock (tmpControlConnections)
                {
                    tmpControlConnections.Add(tmpControlConnection);
                }
                selectControl.UpdateHandler(socket, tmpControlConnection.SocketReceiverHandler);
            }
        }
        internal void ReceivedTunnelKey(ref SelectControl selectControl, Socket socket, Byte[] receivedKey)
        {
            //
            // Get Tunnel
            //
            if (receivedKey.Length != 4)
            {
                Console.WriteLine("{0} Expected tunnel key to be 4 byte but is {1}", DateTime.Now, receivedKey.Length);
                selectControl.ShutdownDisposeAndRemoveReceiveSocket(socket);
                return;
            }

            Int32 key = (Int32)(
                (0xFF000000 & (receivedKey[0] << 24)) |
                (0x00FF0000 & (receivedKey[1] << 16)) |
                (0x0000FF00 & (receivedKey[2] <<  8)) |
                (0x000000FF & (receivedKey[3]      )) );

            DisconnectedTunnel disconnectedTunnel;
            if (!incompleteTunnels.TryGetValue(key, out disconnectedTunnel))
            {
                Console.WriteLine("{0} Could not find tunnel for key {1}", DateTime.Now, key);
                selectControl.ShutdownDisposeAndRemoveReceiveSocket(socket);
                return;
            }

            disconnectedTunnel.CompleteTunnel(ref selectControl, socket);
        }
        public SimpleSelectHandler AcceptAndInitiateTunnel(TunnelListenerHandler listener, Socket clientSocket, Buf safeBuffer)
        {
            //
            // Check if server is connected
            //
            TmpControlConnection tmpControlConnection = TryGetTmpControlConnection(listener.serverName);
            if (tmpControlConnection == null)
            {
                return null;
            }

            Console.WriteLine("{0} [{1}] Received tunnel connection for server '{2}' to connect to target '{3}:{4}'",
                DateTime.Now, clientSocket.SafeRemoteEndPointString(), listener.serverName, listener.targetHost, listener.targetPort);

            //
            // Generate a tunnel key
            //
            Int32 randomKey = random.Next();

            //
            // TODO: This would generate an infinite loop if every single key was taken up in the dictionary,
            //       however, I'm not sure if I should worry about this or not? Maybe there's a better way to do
            //       this?
            //
            while (true)
            {
                if (!incompleteTunnels.ContainsKey(randomKey)) break;
                randomKey++;
            }
                        
            DisconnectedTunnel disconnectedTunnel = new DisconnectedTunnel(clientSocket);
            incompleteTunnels.Add(randomKey, disconnectedTunnel);

            Byte[] tunnelKey = new Byte[4];
            tunnelKey[0] = (Byte)(randomKey >> 24);
            tunnelKey[1] = (Byte)(randomKey >> 16);
            tunnelKey[2] = (Byte)(randomKey >>  8);
            tunnelKey[3] = (Byte)(randomKey      );
            
            //
            // Send Open Tunnel command to TmpServer
            //
            OpenAccessorTunnelRequest request = new OpenAccessorTunnelRequest(0,
                listener.targetHostBytes, listener.targetPort, tunnelKey);
            UInt32 commandLength = Tmp.SerializeCommand<OpenAccessorTunnelRequest>(OpenAccessorTunnelRequest.Serializer,
                Tmp.ToServerOpenAccessorTunnelRequestID, request, safeBuffer, 0);
            tmpControlConnection.dataSender.HandleData(safeBuffer.array, 0, commandLength);

            //
            // Create a diconnected tunnel handler
            //
            return disconnectedTunnel.ConnectedSocketReceiveHandler;
        }

        //
        // Accessor Control Interface
        //
        public String[] GetServerNames()
        {
            String[] names;
            lock (tmpControlConnections)
            {
                names = new String[tmpControlConnections.Count];
                for (int i = 0; i < tmpControlConnections.Count; i++)
                {
                    names[i] = tmpControlConnections[i].ServerInfoName;
                }
            }
            return names;
        }
        public void AddTunnelListener(String serverName, Boolean requireTls, String targetHost, UInt16 targetPort, UInt16 listenPort)
        {
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Any, listenPort));

            AddTunnelListener(serverName, requireTls, targetHost, targetPort, listenSocket, DefaultTunnelListenerBacklog);
        }
        public UInt16 AddTunnelListener(String serverName, Boolean requireTls, String targetHost, UInt16 targetPort)
        {
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            listenSocket.Bind(endPoint);
            UInt16 listenPort = (UInt16)((IPEndPoint)listenSocket.LocalEndPoint).Port;

            AddTunnelListener(serverName, requireTls, targetHost, targetPort, listenSocket, DefaultTunnelListenerBacklog);

            return listenPort;
        }
        void AddTunnelListener(String serverName, Boolean requireTls, String targetHost, UInt16 targetPort, Socket boundSocket, Int32 backlog)
        {
            TunnelListenerHandler tunnelListenerHandler = new TunnelListenerHandler(this, serverName, targetHost, targetPort, requireTls);
            tunnelSelectServer.AddListener(boundSocket, backlog, tunnelListenerHandler.AcceptClientHandler);
            Console.WriteLine("{0} Added tunnel listener (ServerName={1}, Tls={2}, Target={3}:{4}, ListenPort={5}",
                DateTime.Now, serverName, requireTls, targetHost, targetPort, ((IPEndPoint)boundSocket.LocalEndPoint).Port);
        }
    }

    public class TmpControlConnection : IDisposable
    {
        readonly TmpConnectionManager tmpConnectionManager;
        readonly TlsSettings tlsSettings;

        public readonly IPEndPoint remoteEndPoint;
        readonly Socket socket;
        public readonly IDataHandler dataSender;
        readonly IDataFilter receiveDataFilter;
        //readonly FrameAndHeartbeatReceiverHandler frameAndHeartbeatReceiveHandler;
        readonly FrameProtocolReceiverHandler frameProtocolReceiveHandler;

        ServerInfo serverInfo;
        String serverInfoName;
        public ServerInfo ServerInfo { get { return serverInfo; } }
        public String ServerInfoName { get { return serverInfoName; } }

        public TmpControlConnection(TmpConnectionManager tmpConnectionManager, TlsSettings tlsSettings,
            IPEndPoint remoteEndPoint, Socket socket, IDataHandler dataSender, IDataFilter receiveDataFilter)
        {
            this.tmpConnectionManager = tmpConnectionManager;
            this.tlsSettings = tlsSettings;
            this.remoteEndPoint = remoteEndPoint;

            this.dataSender = dataSender;
            this.receiveDataFilter = receiveDataFilter;
            //this.frameAndHeartbeatReceiveHandler = new FrameAndHeartbeatReceiverHandler(HandleCommand, HandleHeartbeat, null);
            this.frameProtocolReceiveHandler = new FrameProtocolReceiverHandler(HandleCommand, null);
        }
        public void SocketClosedHandler(Socket socket)
        {
            Dispose();
        }
        public void Dispose()
        {
            Console.WriteLine("{0} TmpControlConnection 'Name={1}' Closed", DateTime.Now,
                (serverInfoName == null) ? "<null>" : serverInfoName);
            tmpConnectionManager.LostTmpControlConnection(this);
        }
        /*
        public void SendCommand(Byte commandID, IReflector reflector, Object command, ByteBuffer sendBuffer)
        {
            Tmp.SerializeCommand(
            Byte[] packet = Tmp.CreateCommandPacket(commandID, reflector, command, 0);
            dataSender.HandleData(packet, 0, packet.Length);
        }
        */
        /*
        void HandleHeartbeat()
        {
            Console.WriteLine("{0} [{1}] [TmpControl] Got heartbeat", DateTime.Now, remoteEndPoint);
        }
        */
        public void SocketReceiverHandler(ref SelectControl selectControl, Socket socket, Buf safeBuffer)
        {
            Int32 bytesRead = socket.Receive(safeBuffer.array);
            if (bytesRead <= 0)
            {
                selectControl.DisposeAndRemoveReceiveSocket(socket);
                Dispose();
                return;
            }

            //Console.WriteLine("{0} [{1}] [Debug] Got {2} Bytes: {3}", DateTime.Now, remoteEndPoint, bytesRead, safeBuffer.array.ToHexString(0, bytesRead));

            if (receiveDataFilter == null)
            {
                frameProtocolReceiveHandler.HandleData(safeBuffer.array, 0, (UInt32)bytesRead);
            }
            else
            {
                receiveDataFilter.FilterTo(frameProtocolReceiveHandler.HandleData,
                    safeBuffer.array, 0, (UInt32)bytesRead);
            }
        }
        void HandleCommand(Byte[] data, UInt32 offset, UInt32 length)
        {
            if (length == 0)
            {
                Console.WriteLine("{0} [{1}] [TmpControl] Got heartbeat", DateTime.Now, remoteEndPoint);
                return;
            }

            Byte commandID = data[offset];

            switch (commandID)
            {
                case Tmp.ToAccessorServerInfoID:
                    String oldServerName = this.serverInfoName;

                    ServerInfo.Serializer.Deserialize(data, offset + 1, offset + length, out this.serverInfo);
                    if (serverInfo.Name == null || serverInfo.Name.Length <= 0)
                    {
                        Console.WriteLine("{0} [{1}] [TmpControl] TmpServer did not provide a ServerName. Closing the connection.", DateTime.Now, remoteEndPoint);
                        socket.ShutdownAndDispose();
                        return;
                    }
                    this.serverInfoName = Encoding.ASCII.GetString(serverInfo.Name);

                    tmpConnectionManager.GotServerName(oldServerName, serverInfoName, this);
                    Console.WriteLine("{0} [{1}] [TmpControl] Got ServerInfo(Name='{2}')", DateTime.Now, remoteEndPoint, serverInfoName);

                    break;
                default:
                    Console.WriteLine("{0} Unknown command id {1}", DateTime.Now, commandID);
                    break;
            }
        }
    }

    public class TmpServerSideTunnelKeyReceiver
    {
        readonly TmpConnectionManager tmpConnectionManager;

        Byte[] receivedKey;
        Byte receivedLength;

        public TmpServerSideTunnelKeyReceiver(TmpConnectionManager tmpConnectionManager)
        {
            this.tmpConnectionManager = tmpConnectionManager;
            receivedKey = null;
        }
        public void CloseHandler()
        {
            Console.WriteLine("{0} TmpServer Tunnel connection closing while reading its TunnelKey", DateTime.Now);
        }
        public void SocketReceiverHandler(ref SelectControl selectControl, Socket socket, Buf safeBuffer)
        {
            Int32 bytesRead;

            if(receivedKey == null)
            {
                bytesRead = socket.Receive(safeBuffer.array, 1, SocketFlags.None);
                if(bytesRead <= 0)
                {
                    CloseHandler();
                    selectControl.DisposeAndRemoveReceiveSocket(socket);
                    return;
                }
                receivedKey = new Byte[safeBuffer.array[0]];
                receivedLength = 0;
                if (socket.Available <= 0) return;
            }

            if(receivedLength >= receivedKey.Length)
            {
                throw new InvalidOperationException("CodeBug: This SocketReceiveHandler should have been set to something else after the receivedKey was completely received");
            }

            bytesRead = socket.Receive(receivedKey, receivedLength, receivedKey.Length - receivedLength, SocketFlags.None);
            if (bytesRead <= 0)
            {
                CloseHandler();
                selectControl.DisposeAndRemoveReceiveSocket(socket);
                return;
            }

            receivedLength += (Byte)bytesRead;

            if (receivedLength >= receivedKey.Length)
            {
                tmpConnectionManager.ReceivedTunnelKey(ref selectControl, socket, receivedKey);
            }
        }
    }

    public class TunnelListenerHandler
    {
        readonly TmpConnectionManager tmpConnectionManager;

        public readonly String serverName;
        public readonly String targetHost;
        public readonly UInt16 targetPort;
        public readonly Boolean requireTls;

        public readonly Byte[] targetHostBytes;

        public TunnelListenerHandler(TmpConnectionManager tmpConnectionManager, String serverName,
            String targetHost, UInt16 targetPort, Boolean requireTls)
        {
            this.tmpConnectionManager = tmpConnectionManager;

            this.serverName = serverName;
            this.targetHost = targetHost;
            this.targetPort = targetPort;
            this.requireTls = requireTls;

            this.targetHostBytes = Encoding.ASCII.GetBytes(targetHost);
        }
        public SimpleSelectHandler AcceptClientHandler(Socket listenSocket, Socket socket, Buf safeBuffer)
        {
            return tmpConnectionManager.AcceptAndInitiateTunnel(this, socket, safeBuffer);
        }
    }

    public class ConnectedTunnel
    {
        readonly Socket a, b;
        public ConnectedTunnel(Socket a, Socket b)
        {
            this.a = a;
            this.b = b;
        }
        public void SocketCloseHandler(Socket socket)
        {
            Socket otherSocket = (socket == a) ? b : a;
            otherSocket.ShutdownAndDispose();
        }
        public void Dispose(ref SelectControl selectControl)
        {
            selectControl.ShutdownDisposeAndRemoveReceiveSocket(a);
            selectControl.ShutdownDisposeAndRemoveReceiveSocket(b);
        }
        public void AToBHandler(ref SelectControl selectControl, Socket socket, Buf safeBuffer)
        {
            Handle(ref selectControl, a, b, safeBuffer);
        }
        public void BToAHandler(ref SelectControl selectControl, Socket socket, Buf safeBuffer)
        {
            Handle(ref selectControl, b, a, safeBuffer);
        }
        // return true to close
        void Handle(ref SelectControl selectControl, Socket receiveFrom, Socket sendTo, Buf safeBuffer)
        {
            Int32 bytesRead = 0;
            try
            {
                bytesRead = receiveFrom.Receive(safeBuffer.array, SocketFlags.None);
                if (bytesRead > 0)
                {
                    sendTo.Send(safeBuffer.array, 0, bytesRead, SocketFlags.None);
                    return;
                }
            }
            catch (Exception)
            {
            }

            Dispose(ref selectControl);
        }
    }

    public class DisconnectedTunnel// : IDisposable
    {
        readonly Socket connectedSocket;

        Buf buffer;
        Int32 bufferLength;

        ConnectedTunnel connectedTunnel; // gets set when the other end connects

        public DisconnectedTunnel(Socket connectedSocket)
        {
            this.connectedSocket = connectedSocket;

            this.buffer = null;
            this.bufferLength = 0;

            this.connectedTunnel = null;
        }
        public void Dispose(ref SelectControl selectControl)
        {
            if (connectedTunnel != null)
            {
                connectedTunnel.Dispose(ref selectControl);
            }
            else
            {
                selectControl.ShutdownDisposeAndRemoveReceiveSocket(connectedSocket);
            }
        }
        public void CompleteTunnel(ref SelectControl selectControl, Socket socket)
        {
            lock (connectedSocket)
            {
                if (connectedTunnel != null)
                {
                    throw new InvalidOperationException("CodeBug: This tunnel has already been completed");
                }

                //
                // Send all the buffered data
                //
                if (bufferLength > 0)
                {
                    try
                    {
                        socket.Send(buffer.array, 0, bufferLength, SocketFlags.None);
                    }
                    catch (Exception)
                    {
                        selectControl.ShutdownDisposeAndRemoveReceiveSocket(connectedSocket);
                        selectControl.ShutdownDisposeAndRemoveReceiveSocket(socket);
                        return;
                    }
                }

                connectedTunnel = new ConnectedTunnel(connectedSocket, socket);
                selectControl.UpdateHandler(socket, connectedTunnel.BToAHandler);
            }
        }
        public void ConnectedSocketReceiveHandler(ref SelectControl selectControl, Socket socket, Buf safeBuffer)
        {
            //
            // Check if the connection has been made
            //
            if (this.connectedTunnel != null)
            {
                selectControl.UpdateHandler(socket, this.connectedTunnel.AToBHandler);
                this.connectedTunnel.AToBHandler(ref selectControl, socket, safeBuffer);
                return;
            }

            //
            // Put the data into the byte buffer
            //
            if (buffer == null)
            {
                buffer = new Buf(256, 256);
                bufferLength = 0;
            }
            else
            {
                buffer.EnsureCapacityCopyData(bufferLength + 256);
            }

            //
            // Receive the data into the buffer
            //
            Int32 bytesRead = socket.Receive(buffer.array, bufferLength, buffer.array.Length - bufferLength, SocketFlags.None);

            if (bytesRead > 0)
            {
                bufferLength += bytesRead;
            }
            else
            {
                Dispose(ref selectControl);
            }
        }
    }

}
