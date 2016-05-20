using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace More.Net
{
    public class ClientServer
    {
        static InternetHost server; // can't be readonly because it's a struct with non-readonly fields
        static PcapLogger pcapLogger;
        static TextWriter log;

        public static Int32 Run(Options options, List<String> nonOptionArgs)
        {
            log = Console.Out;

            //
            // Options
            //
            if (nonOptionArgs.Count < 2) return options.ErrorAndUsage("Not enough arguments");

            String clientSideConnector = nonOptionArgs[0];
            String listenPortsString = nonOptionArgs[1];

            {
                Proxy proxy;
                String ipOrHostAndPort = Proxy.StripAndParseProxies(clientSideConnector, DnsPriority.IPv4ThenIPv6, out proxy);
                UInt16 port;
                String ipOrHost = EndPoints.SplitIPOrHostAndPort(ipOrHostAndPort, out port);
                server = new InternetHost(ipOrHost, port, DnsPriority.IPv4ThenIPv6, proxy);
            }
            SortedNumberSet listenPortSet = PortSet.ParsePortSet(listenPortsString);

            SelectServer selectServer = new SelectServer(false, new Buf(options.readBufferSize.ArgValue));
            IPAddress listenIP = IPAddress.Any;

            foreach (var port in listenPortSet)
            {
                IPEndPoint listenEndPoint = new IPEndPoint(listenIP, port);
                Socket socket = new Socket(listenEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(listenEndPoint);
                socket.Listen(options.socketBackLog.ArgValue);
                selectServer.control.AddListenSocket(socket, AcceptCallback);
            }

            selectServer.Run();
            return 0;
        }

        static void AcceptCallback(ref SelectControl control, Socket listenSock, Buf safeBuffer)
        {
            Socket newSock = listenSock.Accept();
            if (log != null)
            {
                log.WriteLine("Accepted new client {0}", newSock.SafeRemoteEndPointString());
            }

            Socket clientSideSocket = new Socket(server.GetAddressFamilyForTcp(), SocketType.Stream, ProtocolType.Tcp);

            BufStruct leftOver = new BufStruct(safeBuffer.array);
            clientSideSocket.Connect(server, DnsPriority.IPv4ThenIPv6, ProxyConnectOptions.None, ref leftOver);
            if (leftOver.contentLength > 0)
            {
                newSock.Send(leftOver.buf, 0, (int)leftOver.contentLength, SocketFlags.None);
                if (pcapLogger != null)
                {
                    //pcapLogger.LogTcpData(leftOver.buf, 0, leftOver.contentLength);
                }
            }

            SelectSocketTunnel tunnel = new SelectSocketTunnel(newSock, clientSideSocket);
            control.AddReceiveSocket(newSock, tunnel.ReceiveCallback);
            control.AddReceiveSocket(clientSideSocket, tunnel.ReceiveCallback);
        }
        public class SelectSocketTunnel
        {
            public readonly Socket a;
            public readonly Socket b;
            public SelectSocketTunnel(Socket a, Socket b)
            {
                this.a = a;
                this.b = b;
            }
            public void ReceiveCallback(ref SelectControl control, Socket sock, Buf safeBuffer)
            {
                Socket other = (sock == a) ? b : a;

                int bytesReceived;
                try
                {
                    bytesReceived = sock.Receive(safeBuffer.array);
                }
                catch (SocketException)
                {
                    bytesReceived = -1;
                }

                if (bytesReceived <= 0)
                {
                    other.ShutdownSafe();
                    control.DisposeAndRemoveReceiveSocket(sock);
                }
                else
                {
                    try
                    {
                        other.Send(safeBuffer.array, bytesReceived, SocketFlags.None);
                        if (pcapLogger != null)
                        {
                            //pcapLogger.Log(safeBuffer.array, 0, (uint)bytesReceived);
                        }
                    }
                    catch (SocketException)
                    {
                        sock.ShutdownSafe();
                        control.DisposeAndRemoveReceiveSocket(sock);
                    }
                }
            }
        }
    }
}
