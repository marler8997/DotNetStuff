using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

using More;

namespace More.Net
{
    class TunnelOptions : CLParser
    {
        public readonly CLInt32Argument readBufferSize;
        public readonly CLInt32Argument socketBackLog;

        public TunnelOptions()
            : base()
        {
            readBufferSize = new CLInt32Argument('r', "Read Buffer Size", "The size of the buffer used to read from a socket");
            readBufferSize.SetDefault(8192);
            Add(readBufferSize);

            socketBackLog = new CLInt32Argument('s', "Server Socket BackLog", "The maximum length of the pending connections queue");
            socketBackLog.SetDefault(32);
            Add(socketBackLog);
        }
        public override void PrintUsageHeader()
        {
            //
            // specifier
            //
            Console.WriteLine("Tunnel.exe [options] <listener> <listener> ...");
            Console.WriteLine("  <listener>             :  <server-connector> <protocol-and-port-set> <server-connector> <protocol-and-port-set>...");
            Console.WriteLine("  <protocol-and-port-set>:  tcp|udp<listen-port-set>");
            Console.WriteLine("  <server-conector>      :  ip-address | hostname | proxy[:port]%<server-connector>");
        }
    }

    class TunnelProgram
    {
        static Int32 Main(string[] args)
        {
            TunnelOptions optionsParser = new TunnelOptions();
            List<String> nonOptionArgs = optionsParser.Parse(args);

            if (nonOptionArgs.Count <= 0)
            {
                Console.WriteLine("Must give at least one listener");
                optionsParser.PrintUsage();
                return -1;
            }

            SelectControl selectControl = new SelectControl(false);
            
            //
            // Parse listeners
            //
            int arg = 0;
            do
            {
                //
                // Get Command Line Arguments
                //
                String connector = nonOptionArgs[arg];
                arg++;
                if (arg >= nonOptionArgs.Count)
                {
                    Console.WriteLine("EndPoint '{0}' is missing a protocol 'tcp|udp' and a listen port set", connector);
                    optionsParser.PrintUsage();
                    return -1;
                }
                //
                // Parse End Point
                //
                HostWithOptionalProxy forwardHost = ConnectorParser.ParseConnectorWithPortAndOptionalProxy(connector);

                //
                // Parse Protocol and Port Set
                //
                String protocolAndPortSet = nonOptionArgs[arg++];

                Boolean isTcp;
                if (protocolAndPortSet.StartsWith("tcp", StringComparison.CurrentCultureIgnoreCase))
                {
                    isTcp = true;
                }
                else if (protocolAndPortSet.StartsWith("udp", StringComparison.CurrentCultureIgnoreCase))
                {
                    isTcp = false;
                }
                else
                {
                    throw new InvalidOperationException(String.Format("Unknown protocol '{0}', expected 'tcp' or 'udp'",
                        (protocolAndPortSet.Length < 3) ? protocolAndPortSet : protocolAndPortSet.Remove(3)));
                }

                if (protocolAndPortSet.Length < 4)
                {
                    Console.WriteLine("ProtocolAndPortSet '{0}' Protocol '{1}' is missing a listen port set", connector, protocolAndPortSet);
                    optionsParser.PrintUsage();
                    return -1;
                }

                String portSetString = protocolAndPortSet.Substring(3);
                if (String.IsNullOrEmpty(portSetString))
                {
                    Console.WriteLine("EndPoint '{0}' Protocol '{1}' is missing a listen port set", connector, protocolAndPortSet);
                    optionsParser.PrintUsage();
                    return -1;
                }

                PortSet portSet = ParseUtilities.ParsePortSet(portSetString);

                if (isTcp)
                {
                    TcpCallback tcpCallback = new TcpCallback(forwardHost);

                    for (int i = 0; i < portSet.Length; i++)
                    {
                        Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        listenSocket.Bind(new IPEndPoint(IPAddress.Any, portSet[i]));
                        listenSocket.Listen(optionsParser.socketBackLog.ArgValue);
                        selectControl.AddListenSocket(listenSocket, tcpCallback.HandleAccept);
                    }
                }
                else
                {
                    if (forwardHost.proxy == null)
                    {
                        IPEndPoint serverEndPoint = forwardHost.directEndPoint.GetOrResolveToIPEndPoint();
                        DirectUdpCallback udpCallback = new DirectUdpCallback(serverEndPoint, 30 * 60); // 30 minutes
                        for (int i = 0; i < portSet.Length; i++)
                        {
                            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                            udpSocket.Bind(new IPEndPoint(IPAddress.Any, portSet[i]));
                            selectControl.AddReceiveSocket(udpSocket, udpCallback.ReceiveHandler);
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                        /*
                        UdpThroughProxyCallback udpCallback = new UdpThroughProxyCallback(serverEndPoint, proxyConnector,
                            1000 * 60 * 4, 1000 * 60 * 10);
                        for (int i = 0; i < portSet.Length; i++)
                        {
                            udpListenerList.Add(new UdpSelectListener(new IPEndPoint(IPAddress.Any, portSet[i]), udpCallback));
                        }
                        */
                    }
                }
            } while (arg < nonOptionArgs.Count);

            SelectServer selectServer = new SelectServer(selectControl, new Buf(8192));
            selectServer.Run();

            return -1;
        }
    }


    public class TcpBridge
    {
        readonly String clientLogString;
        readonly String serverLogString;
        readonly Socket client, server;
        public TcpBridge(String clientLogString, Socket client, String serverLogString, Socket server)
        {
            Console.WriteLine("New Tunnel  {0,21} > {1,-21}", clientLogString, serverLogString);
            this.clientLogString = clientLogString;
            this.client = client;
            this.serverLogString = serverLogString;
            this.server = server;
        }
        public void ReceiveHandler(ref SelectControl selectControl, Socket socket, Buf safeBuffer)
        {
            try
            {
                int bytesReceived = socket.Receive(safeBuffer.array);
                if (bytesReceived <= 0)
                {
                    if (socket == client)
                    {
                        Console.WriteLine("{0} > {1} TCP:Client Disconnected", clientLogString, serverLogString);
                        selectControl.DisposeAndRemoveReceiveSocket(client);
                        selectControl.ShutdownIfConnectedDisposeAndRemoveReceiveSocket(server);
                    }
                    else
                    {
                        Console.WriteLine("{0} > {1} TCP:Server Disconnected", clientLogString, serverLogString);
                        selectControl.DisposeAndRemoveReceiveSocket(server);
                        selectControl.ShutdownIfConnectedDisposeAndRemoveReceiveSocket(client);
                    }
                }
                else
                {
                    if (socket == client)
                    {
                        Console.WriteLine("{0} > {1} TCP:{2} bytes",  clientLogString, serverLogString, bytesReceived);
                        server.Send(safeBuffer.array, bytesReceived, SocketFlags.None);
                    }
                    else
                    {
                        Console.WriteLine("{0} < {1} TCP:{2} bytes", clientLogString, serverLogString, bytesReceived);
                        client.Send(safeBuffer.array, bytesReceived, SocketFlags.None);
                    }
                }
            }
            catch (Exception)
            {
                selectControl.ShutdownIfConnectedDisposeAndRemoveReceiveSocket(client);
                selectControl.ShutdownIfConnectedDisposeAndRemoveReceiveSocket(server);
            }
        }
    }
    class TcpCallback
    {
        HostWithOptionalProxy serverHost;
        public TcpCallback(HostWithOptionalProxy serverHost)
        {
            this.serverHost = serverHost;
        }

        // TODO: catch and handle exceptions
        public void HandleAccept(ref SelectControl selectControl, Socket listenSocket, Buf safeBuffer)
        {
            Socket newClientSocket = listenSocket.Accept();

            Socket newRemoteServerSocket = new Socket(serverHost.directEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            newRemoteServerSocket.ConnectTcpSocketThroughProxy(serverHost);

            TcpBridge bridge = new TcpBridge(newClientSocket.SafeRemoteEndPointString(), newClientSocket,
                newRemoteServerSocket.SafeRemoteEndPointString(), newRemoteServerSocket);
            selectControl.AddReceiveSocket(newClientSocket, bridge.ReceiveHandler);
            selectControl.AddReceiveSocket(newRemoteServerSocket, bridge.ReceiveHandler);
        }
    }
    struct EndPointAndLastAccessTime
    {
        public readonly EndPoint endPoint;
        public Int64 expireTime;
        public EndPointAndLastAccessTime(EndPoint endPoint, Int64 expireTime)
        {
            this.endPoint = endPoint;
            this.expireTime = expireTime;
        }
    }
    class DirectUdpCallback
    {
        // Can be shared because this application is single threaded
        static EndPoint From = new IPEndPoint(IPAddress.Any, 0);

        readonly EndPoint remoteServerEndPoint;
        readonly Int64 udpTimeoutTicks;

        // Note: these clients timeout after a certain amount of no activity
        readonly Dictionary<EndPoint,EndPointAndLastAccessTime> currentClients;

        public DirectUdpCallback(EndPoint remoteServerEndPoint, Int64 mappingTimeoutSeconds)
        {
            this.remoteServerEndPoint = remoteServerEndPoint;
            this.udpTimeoutTicks = Stopwatch.Frequency * mappingTimeoutSeconds;
            this.currentClients = new Dictionary<EndPoint, EndPointAndLastAccessTime>();
        }
        public void ReceiveHandler(ref SelectControl selectControl, Socket socket, Buf safeBuffer)
        {
            int bytesRead = socket.ReceiveFrom(safeBuffer.array, ref From);
            // TODO: Handle disconnects somehow?
            // Note: make sure that 0 doesn't also mean disconnect
            if (bytesRead < 0)
                throw new InvalidOperationException("Error: udp socket.ReceiveFrom returned negative");
            
            var now = Stopwatch.GetTimestamp();
            if (From.Equals(remoteServerEndPoint))
            {
                List<EndPoint> removeList = null;
                foreach (var pair in currentClients)
                {
                    if (pair.Value.expireTime >= now)
                    {
                        Console.WriteLine("Udp Client '{0}' expired", pair.Key.ToString());
                        if (removeList == null)
                        {
                            removeList = new List<EndPoint>();
                        }
                        removeList.Add(pair.Key);
                    }
                    else
                    {
                        // Should I update the expire times here?
                        Console.WriteLine("{0} > {1} UDP:{2} bytes",
                            remoteServerEndPoint.ToString(), pair.Key.ToString(), bytesRead);
                        socket.SendTo(safeBuffer.array, bytesRead, SocketFlags.None, pair.Key);
                    }
                }

                if (removeList != null)
                {
                    foreach (var endpoint in removeList)
                    {
                        currentClients.Remove(endpoint);
                    }
                }
            }
            else
            {
                currentClients[From] = new EndPointAndLastAccessTime(From, now + udpTimeoutTicks);
                Console.WriteLine("{0} > {1} UDP:{2} bytes", From.ToString(), remoteServerEndPoint.ToString(), bytesRead);
                socket.SendTo(safeBuffer.array, bytesRead, SocketFlags.None, remoteServerEndPoint);
            }
        }
    }
    /*
    class UdpThroughProxyCallback : DatagramSelectServerCallback
    {
        readonly EndPoint remoteServerEndPoint;
        readonly ISocketConnector proxyConnector;

        readonly Dictionary<EndPoint, SocketAndLastAccessTime> proxyDictionary;
        readonly Int32 proxyTimeoutMilliseconds;

        Int32 cleanWaitTime;
        Int64 nextCleanStopwatchTicks;

        public UdpThroughProxyCallback(EndPoint remoteServerEndPoint, ISocketConnector proxyConnector, Int32 proxyTimeoutMilliseconds, Int32 cleanWaitTime)
        {
            this.remoteServerEndPoint = remoteServerEndPoint;
            this.proxyConnector = proxyConnector;

            this.proxyDictionary = new Dictionary<EndPoint, SocketAndLastAccessTime>();
            this.proxyTimeoutMilliseconds = proxyTimeoutMilliseconds;

            this.cleanWaitTime = cleanWaitTime;
            this.nextCleanStopwatchTicks = Stopwatch.GetTimestamp() + cleanWaitTime.MillisToStopwatchTicks();
        }
        public void ServerStopped()
        {
        }
        public ServerInstruction ListenSocketClosed(UInt32 clientCount)
        {
            throw new NotImplementedException();
        }
        public ServerInstruction DatagramPacket(EndPoint endPoint, Socket socket, Byte[] bytes, UInt32 bytesRead)
        {
            Console.WriteLine("Datagram {0} bytes from EndPoint '{1}'", bytesRead, endPoint);
            SocketAndLastAccessTime remoteSocketAndAccessTime;

            if (proxyDictionary.TryGetValue(endPoint, out remoteSocketAndAccessTime))
            {
                if (Stopwatch.GetTimestamp() >= remoteSocketAndAccessTime.expireTime)
                {
                    remoteSocketAndAccessTime.socket.Close();
                    remoteSocketAndAccessTime.socket = null;
                }
            }
            else
            {
                remoteSocketAndAccessTime = new SocketAndLastAccessTime(null, Stopwatch.GetTimestamp() + proxyTimeoutMilliseconds.MillisToStopwatchTicks());
            }

            if (remoteSocketAndAccessTime.socket == null)
            {
                remoteSocketAndAccessTime.socket = new Socket(remoteServerEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                Console.WriteLine("Connecting...");
                proxyConnector.Connect(remoteSocketAndAccessTime.socket, remoteServerEndPoint);
                Console.WriteLine("Connected");
                TunnelProgram.selectServer.AddUdpSocket(remoteSocketAndAccessTime.socket, this);
            }

            //
            // Send Datagram
            //
            Console.WriteLine("Sending udp datagram...");
            remoteSocketAndAccessTime.socket.Send(bytes, (Int32)bytesRead, SocketFlags.None);

            
            //
            // Check all sockets to clean expired ones
            //
            Int64 time = Stopwatch.GetTimestamp();
            if (time >= nextCleanStopwatchTicks)
            {
                Console.WriteLine("Cleaning Old UDP Proxy Connections...");
                this.nextCleanStopwatchTicks = Stopwatch.GetTimestamp() + cleanWaitTime.MillisToStopwatchTicks();

                foreach (KeyValuePair<EndPoint, SocketAndLastAccessTime> pair in proxyDictionary)
                {
                    if (pair.Value.expireTime < time)
                    {
                        pair.Value.socket.Close();
                        proxyDictionary.Remove(pair.Key);
                    }
                }
                Console.WriteLine("Done Cleaning");
            }

            return ServerInstruction.NoInstruction;
        }
    }
    */

}