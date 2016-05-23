using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;

using More;

namespace More.Net
{
    public class HttpToCdpClient
    {
        public const String hostHeaderSearchString = "\nHost: ";
        public const String proxyConnectionHeaderSearchString = "\nProxy-Connection: ";
        public readonly Char[] newlineSet = new Char[] { '\r', '\n' };

        readonly String clientEndPointString;
        readonly Socket clientSocket;
        readonly HttpToCtpSelectServerHandler serverHandler;

        public Socket serverSocket;

        private StringBuilder httpHeaderBuilders;

        public HttpToCdpClient(String clientEndPointString, Socket clientSocket, HttpToCtpSelectServerHandler serverHandler)
        {
            this.clientEndPointString = clientEndPointString;
            this.clientSocket = clientSocket;
            this.serverHandler = serverHandler;
            this.httpHeaderBuilders = new StringBuilder();
        }


        public void DataRecvHandler(ref SelectControl control, Socket sock, Buf safeBuffer)
        {
            Socket otherSocket = (sock == serverSocket) ? clientSocket : serverSocket;

            int bytesReceived = sock.ReceiveNoThrow(safeBuffer.array, 0, safeBuffer.array.Length, SocketFlags.None);
            if (bytesReceived <= 0)
            {
                if (otherSocket != null)
                {
                    otherSocket.ShutdownSafe();
                }
                control.RemoveReceiveSocket(sock);
                return;
            }

            if (otherSocket != null)
            {
                try
                {
                    otherSocket.Send(safeBuffer.array, bytesReceived, SocketFlags.None);
                    return;
                }
                catch (SocketException)
                {
                    sock.ShutdownSafe();
                    control.DisposeAndRemoveReceiveSocket(sock);
                }
            }
        }


        public void TcpRecvHandler(ref SelectControl control, Socket sock, Buf safeBuffer)
        {
            int bytesReceived = sock.ReceiveNoThrow(safeBuffer.array, 0, safeBuffer.array.Length, SocketFlags.None);
            if (bytesReceived <= 0)
            {
                if (serverSocket != null)
                {
                    serverSocket.ShutdownSafe();
                }
                control.RemoveReceiveSocket(sock);
                return;
            }

            if (serverSocket != null)
            {
                try
                {
                    serverSocket.Send(safeBuffer.array, bytesReceived, SocketFlags.None);
                    return;
                }
                catch (SocketException)
                {
                    sock.ShutdownSafe();
                    control.DisposeAndRemoveReceiveSocket(sock);
                    return;
                }
            }

            httpHeaderBuilders.Append(Encoding.ASCII.GetString(safeBuffer.array, 0, bytesReceived));
            //
            // Check if you have received all the headers
            //
            Int32 totalLength = httpHeaderBuilders.Length;                
            if(totalLength > 3)
            {
                if(
                    httpHeaderBuilders[totalLength - 1] == '\n' &&
                    (
                      (httpHeaderBuilders[totalLength - 2] == '\n') ||
                      (httpHeaderBuilders[totalLength - 2] == '\r' &&
                       httpHeaderBuilders[totalLength - 3] == '\n' &&
                       httpHeaderBuilders[totalLength - 4] == '\r'))
                    )
                {
                    String headers = httpHeaderBuilders.ToString();
                    httpHeaderBuilders = null;
                    HandleHeaders(ref control, sock, headers);
                }
            }
        }
        void HandleHeaders(ref SelectControl control, Socket sock, String headers)
        {
            if (headers.StartsWith("CONNECT"))
            {
                UInt16 serverPort;
                String serverString = ParseConnectLine(headers.Remove(headers.IndexOfAny(newlineSet)), out serverPort);
                if (serverString == null)
                {
                    return;
                }

                //
                // Make the forward connection
                //
                if (HttpToCtpLogger.logger != null)
                {
                    HttpToCtpLogger.logger.WriteLine("[{0}] CONNECT {1}:{2}", clientEndPointString, serverString, serverPort);
                }

                //
                // Create and connect the socket
                //
                IPEndPoint serverEndPoint = new IPEndPoint(EndPoints.ParseIPOrResolveHost(serverString, DnsPriority.IPv4ThenIPv6), serverPort);

                serverSocket = new Socket(serverEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                //if (HttpToCtp.proxySelector == null)
                //{
                //    serverSocket.Connect(serverEndPoint);
                //}
                //else
                //{
                //    HttpToCtp.proxySelector.Connect(serverSocket, serverEndPoint);
                //}
                serverSocket.Connect(serverEndPoint);

                //
                // Send positive connect response
                //
                sock.Send(HttpToCtp.ConnectionEstablishedAsBytes);
            }
            else
            {
                //
                // Find the host header
                //
                //Console.Write("Original Headers '{0}'", headers);
                Int32 hostIndex = headers.IndexOf(hostHeaderSearchString);
                if (hostIndex < 0)
                {
                    if (HttpToCtpLogger.errorLogger != null)
                    {
                        HttpToCtpLogger.errorLogger.WriteLine("[{0}] Missing Host header '{1}'", clientEndPointString, headers);
                    }
                    sock.ShutdownSafe();
                    control.DisposeAndRemoveReceiveSocket(sock);
                    return;
                }
                hostIndex += hostHeaderSearchString.Length;

                Console.WriteLine("[Debug] hostIndex: {0}, endIndex: {1}", hostIndex, headers.IndexOfAny(newlineSet, hostIndex));
                String hostHeaderValue = headers.Substring(hostIndex, headers.IndexOfAny(newlineSet, hostIndex) - hostIndex);

                //
                // Remove the Proxy-Connection header
                //
                Int32 proxyConnectionIndex = headers.IndexOf(proxyConnectionHeaderSearchString);
                if (proxyConnectionIndex > 0)
                {
                    headers = headers.Remove(proxyConnectionIndex + 1,
                        headers.IndexOf('\n', proxyConnectionIndex + proxyConnectionHeaderSearchString.Length) -
                        proxyConnectionIndex);
                }

                String serverString = hostHeaderValue;
                UInt16 serverPort = 80;

                if (serverString.StartsWith("http://")) serverString = serverString.Substring("http://".Length);
                else if (serverString.StartsWith("https://")) { serverString = serverString.Substring("https://".Length); serverPort = 443; }

                //
                // Parse Optional Port
                //
                int lastColonIndex = serverString.IndexOf(':');
                if (lastColonIndex >= 0)
                {
                    String portString = serverString.Substring(lastColonIndex + 1);
                    serverString = serverString.Substring(0, lastColonIndex);
                    if (!UInt16.TryParse(portString, out serverPort))
                    {
                        if (HttpToCtpLogger.errorLogger != null)
                        {
                            HttpToCtpLogger.errorLogger.WriteLine(
                                "[{0}] The HTTP Host header value '{1}' contained an invalid string after the colon in the host value. Expected a port number but got '{2}'",
                            clientEndPointString, hostHeaderValue, portString);
                        }
                        sock.ShutdownSafe();
                        control.DisposeAndRemoveReceiveSocket(sock);
                        return;
                    }
                }

                //
                // Make the forward connection
                //	
                if (HttpToCtpLogger.logger != null)
                    HttpToCtpLogger.logger.WriteLine("[{0}] Proxy to '{1}' on port {2}", clientEndPointString, serverString, serverPort);

                //
                // Create and connect the socket
                //
                IPEndPoint serverEndPoint = new IPEndPoint(EndPoints.ParseIPOrResolveHost(serverString, DnsPriority.IPv4ThenIPv6), serverPort);

                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //if (HttpToCtp.proxySelector == null)
                //{
                //    serverSocket.Connect(serverEndPoint);
                //}
                //else
                //{
                //    HttpToCtp.proxySelector.Connect(serverSocket, serverEndPoint);
                //}
                serverSocket.Connect(serverEndPoint);

                //Console.Write("New Headers '{0}'", headers);
                serverSocket.Send(Encoding.UTF8.GetBytes(headers));
            }


            if (HttpToCtpLogger.logger != null)
            {
                HttpToCtpLogger.logger.WriteLine("[{0}] Adding Server Socket '{1}'", clientEndPointString, serverSocket.RemoteEndPoint);
            }
            control.UpdateHandler(sock, DataRecvHandler);
            control.AddReceiveSocket(serverSocket, DataRecvHandler);
        }

        // returns the host name to connect to and sets the forward port
        private String ParseConnectLine(String connectLine, out UInt16 forwardPort)
        {
            //
            // Parse the Host Name and Port
            //
            String uri = connectLine.Substring(HttpToCtp.ConnectAsBytes.Length + 1); // REMOVE 'CONNECT '

            int uriEndIndex = uri.IndexOf(' ', HttpToCtp.ConnectAsBytes.Length + 1);
            if (uriEndIndex <= 0)
            {
                if (HttpToCtpLogger.errorLogger != null)
                    HttpToCtpLogger.errorLogger.WriteLine(
                        "[{0}] Invalid first line of CONNECT '{1}': Missing space after uri",
                        clientEndPointString, connectLine);

                forwardPort = 0;
                return null;
            }

            uri = uri.Substring(0, uriEndIndex);

            int lastColonIndex = uri.LastIndexOf(':');
            if (lastColonIndex <= 0)
            {
                if (HttpToCtpLogger.errorLogger != null)
                    HttpToCtpLogger.errorLogger.WriteLine(
                        "[{0}] Invalid first line of CONNECT '{1}': Missing colon in host name",
                        clientEndPointString, connectLine);
                forwardPort = 0;
                return null;
            }

            String portString = uri.Substring(lastColonIndex + 1);
            uri = uri.Substring(0, lastColonIndex);

            // GET PORT
            if (!UInt16.TryParse(portString, out forwardPort))
            {
                if (HttpToCtpLogger.errorLogger != null)
                    HttpToCtpLogger.errorLogger.WriteLine(
                        "[{0}] Invalid first line of CONNECT '{1}': Failed to parse port number after colon",
                        clientEndPointString, connectLine);
                forwardPort = 0;
                return null;
            }

            // GET HOST NAME
            String hostName = uri;
            if (hostName.StartsWith("https://")) hostName = hostName.Substring("https://".Length);
            else if (hostName.StartsWith("http://")) hostName = hostName.Substring("http://".Length);
            return hostName;
        }
    }
}
