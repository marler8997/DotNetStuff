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
        public static MultipleListenersSelectServer selectServer = new MultipleListenersSelectServer();

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

            List<TcpSelectListener> tcpListenerList = new List<TcpSelectListener>();
            List<UdpSelectListener> udpListenerList = new List<UdpSelectListener>();
            
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
                ISocketConnector proxyConnector;
                String ipOrHostAndPort = ConnectorParser.ParseConnector(connector, out proxyConnector);
                EndPoint serverEndPoint = EndPoints.EndPointFromIPOrHostAndPort(ipOrHostAndPort);

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
                    TcpCallback tcpCallback = new TcpCallback(serverEndPoint, proxyConnector);

                    for (int i = 0; i < portSet.Length; i++)
                    {
                        tcpListenerList.Add(new TcpSelectListener(new IPEndPoint(IPAddress.Any, portSet[i]),
                            optionsParser.socketBackLog.ArgValue, tcpCallback));
                    }
                }
                else
                {
                    if (proxyConnector == null)
                    {
                        DirectUdpCallback udpCallback = new DirectUdpCallback(serverEndPoint);

                        for (int i = 0; i < portSet.Length; i++)
                        {
                            udpListenerList.Add(new UdpSelectListener(new IPEndPoint(IPAddress.Any, portSet[i]), udpCallback));
                        }
                    }
                    else
                    {
                        UdpThroughProxyCallback udpCallback = new UdpThroughProxyCallback(serverEndPoint, proxyConnector,
                            1000 * 60 * 4, 1000 * 60 * 10);
                        for (int i = 0; i < portSet.Length; i++)
                        {
                            udpListenerList.Add(new UdpSelectListener(new IPEndPoint(IPAddress.Any, portSet[i]), udpCallback));
                        }
                    }
                }
            } while (arg < nonOptionArgs.Count);

            selectServer.PrepareToRun();
            selectServer.Run(Console.Out, new byte[8192], tcpListenerList.ToArray(), udpListenerList.ToArray());

            return -1;
        }
    }
    class TcpCallback : StreamSelectServerCallback
    {
        readonly EndPoint remoteServerEndPoint;
        readonly ISocketConnector proxyConnector;

        readonly Dictionary<Socket, Socket> socketPairs;
        Int32 expectedSocketCount;

        public TcpCallback(EndPoint remoteServerEndPoint, ISocketConnector proxyConnector)
        {
            this.remoteServerEndPoint = remoteServerEndPoint;
            this.proxyConnector = proxyConnector;

            this.socketPairs = new Dictionary<Socket, Socket>();
            expectedSocketCount = 0;
        }
        public void ServerListening(Socket listenSocket)
        {
        }
        public void ServerStopped()
        {
        }
        public ServerInstruction ListenSocketClosed(int clientCount)
        {
            return ServerInstruction.StopServer;
        }
        void VerifyClientCount()
        {
            if (socketPairs.Count != expectedSocketCount)
            {
                throw new InvalidOperationException(String.Format("Expected there to be {0} sockets but there are {1} entries in the socket dictionary",
                    expectedSocketCount, socketPairs.Count));
            }
        }
        public ServerInstruction ClientOpenCallback(Int32 clientCount, Socket socket)
        {
            Socket newRemoteServerSocket = new Socket(remoteServerEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            if (proxyConnector == null)
            {
                newRemoteServerSocket.Connect(remoteServerEndPoint);
            }
            else
            {
                proxyConnector.Connect(newRemoteServerSocket, remoteServerEndPoint);
            }

            socketPairs.Add(socket, newRemoteServerSocket);
            socketPairs.Add(newRemoteServerSocket, socket);
            expectedSocketCount += 2;
            VerifyClientCount();

            TunnelProgram.selectServer.AddReadTcpSocket(newRemoteServerSocket, this);

            Console.WriteLine("New Tunnel  {0,21} > {1,-21}", socket.RemoteEndPoint, newRemoteServerSocket.RemoteEndPoint);
            return ServerInstruction.NoInstruction;
        }
        public ServerInstruction ClientCloseCallback(Int32 clientCount, Socket socket)
        {
            Console.WriteLine("Closed      {0,21}", socket.RemoteEndPoint);

            Socket socketPair;
            if (socketPairs.TryGetValue(socket, out socketPair))
            {
                socketPairs.Remove(socket);
                expectedSocketCount--;

                // Close and remove the client socket as well                
                if (socketPair.Connected)
                {
                    try { socketPair.Shutdown(SocketShutdown.Both); } catch (IOException) { }
                }

                VerifyClientCount();

                return ServerInstruction.NoInstruction;
            }

            throw new InvalidOperationException(String.Format(
                "Received close for a client '{0}' that wasn't in the socket dictionary", socket.RemoteEndPoint));
        }
        public ServerInstruction ClientDataCallback(Socket socket, byte[] bytes, int bytesRead)
        {
            Socket socketPair;
            if (!socketPairs.TryGetValue(socket, out socketPair))
                throw new InvalidOperationException(String.Format("Missing Socket pair of socket '{0}'", socket.RemoteEndPoint));

            if (!socketPair.Connected)
            {
                Console.WriteLine("[Warning] Got {0} bytes from '{1}' but socket pair is closed", bytesRead);
                return ServerInstruction.CloseClient;
            }

            Console.WriteLine("{0,5} Bytes {1,21} > {2,-21}", bytesRead, socket.RemoteEndPoint, socketPair.RemoteEndPoint);
            socketPair.Send(bytes, bytesRead, SocketFlags.None);
            return ServerInstruction.NoInstruction;
        }
    }

    class SocketAndLastAccessTime
    {
        public Socket socket;
        public Int64 expireTime;
        public SocketAndLastAccessTime(Socket socket, Int64 expireTime)
        {
            this.socket = socket;
            this.expireTime = expireTime;
        }
    }
    class DirectUdpCallback : DatagramSelectServerCallback
    {
        // TODO: keep an ordered list (ordered by how recent the last communication was)
        //       and also get a maximum connection count, then clean up oldest sockets
        //       when new connections arrive

        readonly EndPoint remoteServerEndPoint;

        readonly Dictionary<EndPoint, Socket> clientEndPointToServerSockets;
        readonly Dictionary<Socket, EndPoint> remoteServerSocketsToClientEndPoints;

        Socket clientListenSocket;

        public DirectUdpCallback(EndPoint remoteServerEndPoint)
        {
            this.remoteServerEndPoint = remoteServerEndPoint;
            this.clientEndPointToServerSockets = new Dictionary<EndPoint, Socket>();
            this.remoteServerSocketsToClientEndPoints = new Dictionary<Socket, EndPoint>();
        }
        public void ServerStopped()
        {
        }
        public ServerInstruction ListenSocketClosed(int clientCount)
        {
            return ServerInstruction.NoInstruction;
        }
        public ServerInstruction DatagramPacket(EndPoint endPoint, Socket socket, byte[] bytes, int bytesRead)
        {
            //
            // Check if this socket is from RemoteServer
            //
            EndPoint clientEndPoint;
            if (remoteServerSocketsToClientEndPoints.TryGetValue(socket, out clientEndPoint))
            {
                Console.WriteLine("[UDP] {0} Byte Datagram '{0}' to '{1}'", bytesRead, endPoint, clientEndPoint);
                clientListenSocket.SendTo(bytes, bytesRead, SocketFlags.None, clientEndPoint);
                return ServerInstruction.NoInstruction;
            }

            //
            // The datagram is from a client
            //
            if (clientListenSocket == null)
            {
                clientListenSocket = socket; // This should never change
            }
            else
            {
                if (clientListenSocket != socket)
                {
                    String clientListenLocal = "?", clientListenRemote = "?",
                        socketLocal = "?", socketRemote = "?";

                    try { clientListenLocal = clientListenSocket.LocalEndPoint.ToString(); } catch(Exception) { }
                    try { clientListenRemote = clientListenSocket.RemoteEndPoint.ToString(); } catch(Exception) { }
                    try { socketLocal = socket.LocalEndPoint.ToString(); } catch(Exception) { }
                    try { socketRemote = socket.RemoteEndPoint.ToString(); } catch(Exception) { }

                    throw new InvalidOperationException(String.Format(
                    "Code Bug: These sockets should always be equal '{0}' != '{1}'",
                    clientListenLocal + "/" + clientListenRemote, socketLocal + "/" + socketRemote));
                }
            }

            Socket socketToRemoteServer;
            if (!clientEndPointToServerSockets.TryGetValue(endPoint, out socketToRemoteServer))
            {
                socketToRemoteServer = new Socket(remoteServerEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                socketToRemoteServer.Connect(remoteServerEndPoint);

                clientEndPointToServerSockets.Add(endPoint, socketToRemoteServer);
                remoteServerSocketsToClientEndPoints.Add(socketToRemoteServer, endPoint);

                TunnelProgram.selectServer.AddUdpSocket(socketToRemoteServer, this);
                Console.WriteLine("[UDP] New Connection From '{0}' to '{1}' ({2} Connections To This Remote Server/Port)", endPoint, remoteServerEndPoint, clientEndPointToServerSockets.Count);
            }
            Console.WriteLine("[UDP] {0} Byte Datagram '{0}' to '{1}'", bytesRead, endPoint, remoteServerEndPoint);
            socketToRemoteServer.Send(bytes, bytesRead, SocketFlags.None);
            return ServerInstruction.NoInstruction;
        }
    }
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
        public ServerInstruction ListenSocketClosed(int clientCount)
        {
            throw new NotImplementedException();
        }
        public ServerInstruction DatagramPacket(EndPoint endPoint, Socket socket, byte[] bytes, int bytesRead)
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
            remoteSocketAndAccessTime.socket.Send(bytes, bytesRead, SocketFlags.None);

            
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

}