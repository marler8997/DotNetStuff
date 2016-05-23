using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Net;
using System.Text;

using More;

#if WindowsCE
using IPParser = System.Net.MissingInCEIPParser;
#else
using IPParser = System.Net.IPAddress;
#endif

namespace More.Net
{
    public static class NonBlockingSocket
    {
        // Note: in order to perform a non-blocking connect without
        // having to start an unnecessary thread, you have to set the
        // socket to nonblocking and then call connect and swallow the exception.
        // I hate using exceptions for control flow but it is better then the alternative
        // which is to spin up a new thread (and potentially block multiple connections)
        /// <summary>
        /// Note: Assumes that the socket.Blocking is already set to false
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="endpoint"></param>
        /// <returns>true if connection is in progress, false if it connnect synchronously</returns>
        public static Boolean ConnectNonBlocking(Socket socket, IPEndPoint endPoint)
        {
            //Console.WriteLine("[DEBUG] Connect {0} {1}", socket.Handle, endPoint);
            try
            {
                socket.Connect(endPoint);
                return false;
            }
            catch (Win32Exception e)
            {
                // Note: this may not be platform independent
                if (e.ErrorCode == 10035) // Only rethrow if the exception is not associated with the connect
                {
                    return true;
                }
                throw;
            }
        }
    }
    public class TcpBridge
    {
        readonly String clientLogString;
        readonly String serverLogString;
        readonly Socket client, server;
        public TcpBridge(String clientLogString, Socket client, String serverLogString, Socket server)
        {
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
                        if (AppLayerProxy.Logger != null)
                            AppLayerProxy.Logger.WriteLine("{0} > {1} Client Disconnected",
                                clientLogString, serverLogString);
                        selectControl.DisposeAndRemoveReceiveSocket(client);
                        selectControl.ShutdownIfConnectedDisposeAndRemoveReceiveSocket(server);
                    }
                    else
                    {
                        if (AppLayerProxy.Logger != null)
                            AppLayerProxy.Logger.WriteLine("{0} > {1} Server Disconnected",
                                clientLogString, serverLogString);
                        selectControl.DisposeAndRemoveReceiveSocket(server);
                        selectControl.ShutdownIfConnectedDisposeAndRemoveReceiveSocket(client);
                    }
                }
                else
                {
                    if (socket == client)
                    {
                        if (AppLayerProxy.Logger != null)
                            AppLayerProxy.Logger.WriteLine("{0} > {1} {2} bytes",
                                clientLogString, serverLogString, bytesReceived);
                        server.Send(safeBuffer.array, bytesReceived, SocketFlags.None);
                    }
                    else
                    {
                        if (AppLayerProxy.Logger != null)
                            AppLayerProxy.Logger.WriteLine("{0} < {1} {2} bytes",
                                clientLogString, serverLogString, bytesReceived);
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
    public class ConnectionInitiator
    {
        const UInt32 InitialClientBufferLength = 512;

        readonly Socket clientSocket;
        readonly String clientLogString;

        // Used to buffer data sent from the client that may need to be sent to the server later
        ByteBuilder clientBuffer;
        UInt32 headersLength;

        Boolean isConnect;
        String serverLogString;
        Socket serverSocket;
        StringEndPoint serverEndPointForProxy;

        public ConnectionInitiator(Socket clientSocket, String clientLogString)
        {
            this.clientSocket = clientSocket;
            this.clientLogString = clientLogString;
            this.serverLogString = "?";
        }

        public void InitialReceiveHandler(ref SelectControl selectControl, Socket clientSocket, Buf safeBuffer)
        {
            int bytesReceived = clientSocket.Receive(safeBuffer.array);
            if (bytesReceived <= 0)
            {
                if (AppLayerProxy.Logger != null)
                    AppLayerProxy.Logger.WriteLine("{0} Closed (no data received)", clientLogString);
                selectControl.DisposeAndRemoveReceiveSocket(clientSocket);
                return;
            }

            if (!CheckForEndOfHeadersAndHandle(ref selectControl, clientSocket, safeBuffer.array, 0, (uint)bytesReceived))
            {
                if (AppLayerProxy.Logger != null)
                    AppLayerProxy.Logger.WriteLine("{0} Initial socket receive did not contain all headers. Need to copy partial header to a new buffer.",
                        clientLogString);

                clientBuffer = new ByteBuilder(InitialClientBufferLength +
                    ((bytesReceived > InitialClientBufferLength) ? (uint)bytesReceived : 0));
                Array.Copy(safeBuffer.array, clientBuffer.bytes, bytesReceived);
                clientBuffer.contentLength = (uint)bytesReceived;

                selectControl.UpdateHandler(clientSocket, HeaderBuilderHandler);
            }
        }
        // Handler that receives data if all the headers were not in the initial receive
        void HeaderBuilderHandler(ref SelectControl selectControl, Socket clientSocket, Buf safeBuffer)
        {
            int bytesReceived = clientSocket.Receive(safeBuffer.array);
            if (bytesReceived <= 0)
            {
                if (AppLayerProxy.Logger != null)
                    AppLayerProxy.Logger.WriteLine("{0} Closed ({1} bytes received but did not finish HTTP headers)", clientLogString, clientBuffer.contentLength);
                selectControl.DisposeAndRemoveReceiveSocket(clientSocket);
                return;
            }

            clientBuffer.Append(safeBuffer.array, 0, (uint)bytesReceived);
            CheckForEndOfHeadersAndHandle(ref selectControl, clientSocket, clientBuffer.bytes, 0, clientBuffer.contentLength);
        }

        // Complete the forward proxy connection if a proxy is configured and also
        // send any buffered data from the client to the server.
        void FinishConnection(Byte[] extraData, UInt32 offset, UInt32 length)
        {
            if (AppLayerProxy.ForwardProxy != null)
            {
                serverSocket.Blocking = true;
                BufStruct buf = default(BufStruct);
                AppLayerProxy.ForwardProxy.ProxyConnectTcp(serverSocket, ref serverEndPointForProxy,
                    isConnect ? 0 : ProxyConnectOptions.ContentIsRawHttp, ref buf);
                if (buf.contentLength > 0)
                {
                    throw new NotImplementedException("The proxy connection left over some data from the server, this is not currently being handled");
                }
            }
            if (isConnect)
            {
                clientSocket.Send(HttpVars.ConnectionEstablishedAsBytes);
            }
            if (length > 0)
            {
                if (AppLayerProxy.Logger != null)
                    AppLayerProxy.Logger.WriteLine("{0} > {1} {2} bytes{3}",
                        clientLogString, serverLogString, length,
                        (clientBuffer != null && extraData == clientBuffer.bytes) ? " (buffered)" : "");

                // The clientBuffer should have already been setup then
                serverSocket.Send(extraData, (int)clientBuffer.contentLength, SocketFlags.None);
            }
        }

        void ServerSocketConnected(ref SelectControl selectControl, Socket serverSocket, Buf safeBuffer)
        {
            try
            {
                if (!clientSocket.Connected)
                {
                    clientSocket.Close(); // Should already removed from selectControl
                    if (!selectControl.ConnectionError && serverSocket.Connected)
                    {
                        if (AppLayerProxy.Logger != null)
                            AppLayerProxy.Logger.WriteLine("{0} > {1} Server Connected but Client Disconnected...Closing Server", clientLogString, serverLogString);
                        try { serverSocket.Shutdown(SocketShutdown.Both); }
                        catch (Exception) { }
                    }
                    else
                    {
                        if (AppLayerProxy.Logger != null)
                            AppLayerProxy.Logger.WriteLine("{0} > {1} Client disconnected before server could connect", clientLogString, serverLogString);
                    }
                    selectControl.DisposeAndRemoveConnectSocket(serverSocket);
                }
                else if (selectControl.ConnectionError)
                {
                    if (AppLayerProxy.Logger != null)
                    {
                        if (AppLayerProxy.ForwardProxy == null)
                        {
                            AppLayerProxy.Logger.WriteLine("{0} > {1} Failed to connect to server..Closing Client", clientLogString, serverLogString);
                        }
                        else
                        {
                            AppLayerProxy.Logger.WriteLine("{0} > {1} Failed to connect to proxy server '{2}'..Closing Client",
                                clientLogString, serverLogString, AppLayerProxy.ForwardProxy.host.CreateTargetString());
                        }
                    }
                    clientSocket.ShutdownAndDispose(); // Should already removed from selectControl
                    selectControl.DisposeAndRemoveConnectSocket(serverSocket);
                }
                else if (!serverSocket.Connected)
                {
                    // Ignore.  Only do something if we are connected or get a ConnectionError
                }
                else
                {
                    if (AppLayerProxy.Logger != null)
                        AppLayerProxy.Logger.WriteLine("{0} > {1} Connected to Server", clientLogString, serverLogString);

                    if (isConnect)
                    {
                        if (clientBuffer == null)
                        {
                            FinishConnection(null, 0, 0);
                        }
                        else
                        {
                            uint extraChars = clientBuffer.contentLength - headersLength;
                            FinishConnection(clientBuffer.bytes, headersLength, extraChars);
                        }
                    }
                    else
                    {
                        // A NonConnect will always have buffered data to send
                        FinishConnection(clientBuffer.bytes, 0, clientBuffer.contentLength);
                    }

                    TcpBridge bridge = new TcpBridge(clientLogString, clientSocket, serverLogString, serverSocket);
                    selectControl.AddReceiveSocket(clientSocket, bridge.ReceiveHandler);
                    selectControl.UpdateConnectorToReceiver(serverSocket, bridge.ReceiveHandler);
                }
            }
            catch (Exception e)
            {
                if (AppLayerProxy.ErrorLogger != null)
                    AppLayerProxy.ErrorLogger.WriteLine("{0} > {1} Failed to finish connection: {2}", clientLogString, serverLogString, e.Message);
                selectControl.ShutdownIfConnectedDisposeAndRemoveReceiveSocket(clientSocket);
                selectControl.ShutdownIfConnectedDisposeAndRemoveReceiveSocket(serverSocket);
                selectControl.DisposeAndRemoveConnectSocket(serverSocket);
            }
        }

        // Returns true if all headers were found and handled, false otherwise
        Boolean CheckForEndOfHeadersAndHandle(ref SelectControl selectControl, Socket clientSocket,
            Byte[] headers, UInt32 alreadyCheckedOffset, UInt32 length)
        {
            if (alreadyCheckedOffset >= 7)
            {
                alreadyCheckedOffset -= 3; // Check the last 3 chars as well
            }

            UInt32 totalLength = length;

            while (length > alreadyCheckedOffset)
            {
                if (
                    headers[length - 1] == '\n' &&
                    (headers[length - 2] == '\n' ||
                      (headers[length - 2] == '\r' &&
                       headers[length - 3] == '\n' &&
                       headers[length - 4] == '\r')
                    ))
                {
                    this.headersLength = length;
                    GotHeaders(ref selectControl, clientSocket, headers, totalLength);
                    return true;
                }
                length--;
            }
            return false;
        }
        void GotHeaders(ref SelectControl selectControl, Socket clientSocket, Byte[] headers, UInt32 totalLength)
        {
            String serverIPOrHost = null;
            UInt16 serverPort = 0;
            try
            {
                serverIPOrHost = GetServerFromHeaders(out serverPort, out isConnect,
                    headers, ref headersLength, ref totalLength, clientLogString);
            }
            catch (Exception e)
            {
                if (AppLayerProxy.ErrorLogger != null)
                    AppLayerProxy.ErrorLogger.WriteLine("{0} Failed to get server from HTTP Headers: {1}", clientLogString, e);
            }

            if (serverIPOrHost == null)
            {
                selectControl.ShutdownDisposeAndRemoveReceiveSocket(clientSocket);
                return;
            }

            this.serverLogString = serverIPOrHost + ":" + serverPort.ToString();

            Boolean needToConnect;
            if (AppLayerProxy.ForwardProxy == null)
            {
                // TODO: Fix so this does not block during DNS resolution
                IPAddress serverIP;
                try
                {
                    serverIP = EndPoints.ParseIPOrResolveHost(serverIPOrHost, DnsPriority.IPv4ThenIPv6);
                }
                catch (SocketException)
                {
                    if (AppLayerProxy.ErrorLogger != null)
                        AppLayerProxy.ErrorLogger.WriteLine("{0} Failed to resolve server hostname '{1}'",
                            clientLogString, serverIPOrHost);
                    selectControl.ShutdownDisposeAndRemoveReceiveSocket(clientSocket);
                    return;
                }
                IPEndPoint serverEndPoint = new IPEndPoint(serverIP, serverPort);

                serverSocket = new Socket(serverEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Blocking = false;
                needToConnect = NonBlockingSocket.ConnectNonBlocking(serverSocket, serverEndPoint);
            }
            else
            {
                this.serverEndPointForProxy = new StringEndPoint(serverIPOrHost, serverPort);

                Console.WriteLine("[DEBUG] Connecting to proxy '{0}'", AppLayerProxy.ForwardProxy.host.CreateTargetString());
                serverSocket = new Socket(AppLayerProxy.ForwardProxy.host.GetAddressFamilyForTcp(), SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Blocking = false;
                throw new NotImplementedException();
                //needToConnect = NonBlockingSocket.ConnectNonBlocking(serverSocket, AppLayerProxy.ForwardProxy.endPoint);
            }

            //Console.WriteLine("[DEBUG] {0} > {1} Connecting...", clientLogString, serverLogString);
            if (needToConnect)
            {
                selectControl.RemoveReceiveSocket(clientSocket); // Remove the clientSocket from the read list
                                                                 // until the connection gets established or lost
                selectControl.AddConnectSocket(serverSocket, ServerSocketConnected);

                // Save Data
                if (isConnect)
                {
                    if (totalLength == headersLength)
                    {
                        clientBuffer = null; // Clean up the client buffer (if there is one)
                    }
                }
                else
                {
                    if (clientBuffer == null)
                    {
                        clientBuffer = new ByteBuilder(totalLength);
                        Array.Copy(headers, clientBuffer.bytes, totalLength);
                        clientBuffer.contentLength = totalLength;
                    }
                    else
                    {
                        // Data already in the client buffer
                    }
                }
            }
            else
            {
                if(AppLayerProxy.Logger != null)
                    AppLayerProxy.Logger.WriteLine("{0} > {1} Connection Completed Synchronously (CornerCase)", clientLogString, serverLogString);

                if (!clientSocket.Connected)
                {
                    selectControl.DisposeAndRemoveReceiveSocket(clientSocket);
                    if (serverSocket.Connected)
                    {
                        if (AppLayerProxy.Logger != null)
                            AppLayerProxy.Logger.WriteLine("{0} > {1} Server Connected but Client Disconnected...Closing Server", clientLogString, serverLogString);
                        try { serverSocket.Shutdown(SocketShutdown.Both); }
                        catch (Exception) { }
                    }
                    else
                    {
                        if (AppLayerProxy.Logger != null)
                            AppLayerProxy.Logger.WriteLine("{0} > {1} Client disconnected before server could connect", clientLogString, serverLogString);
                    }
                    serverSocket.Close();
                }
                else if (!serverSocket.Connected)
                {
                    if (AppLayerProxy.Logger != null)
                        AppLayerProxy.Logger.WriteLine("{0} > {1} Failed to connect to server..Closing Client", clientLogString, serverLogString);
                    selectControl.ShutdownDisposeAndRemoveReceiveSocket(clientSocket);
                    serverSocket.Close();
                }
                else
                {
                    if (AppLayerProxy.Logger != null)
                        AppLayerProxy.Logger.WriteLine("{0} > {1} Connected to Server", clientLogString, serverLogString);

                    if (isConnect)
                    {
                        uint extraChars = totalLength - headersLength;
                        FinishConnection(headers, headersLength, extraChars);
                    }
                    else
                    {
                        FinishConnection(headers, 0, totalLength);
                    }

                    TcpBridge bridge = new TcpBridge(clientLogString, clientSocket, serverLogString, serverSocket);
                    selectControl.UpdateHandler(clientSocket, bridge.ReceiveHandler);
                    selectControl.AddReceiveSocket(serverSocket, bridge.ReceiveHandler);
                }
            }
        }

        // Returns the host name
        static String GetServerFromHeaders(out UInt16 serverPort, out Boolean isConnect, Byte[] headers,
            ref UInt32 headersLength, ref UInt32 totalLength, String clientLogString)
        {
            // If it starts with "CONNECT"
            if (headers.EqualsAt(0, HttpVars.ConnectUtf8, 0, (uint)HttpVars.ConnectUtf8.Length) &&
                headers[HttpVars.ConnectUtf8.Length] == ' ')
            {
                isConnect = true;
                uint offset = (uint)HttpVars.ConnectUtf8.Length + 1;

                //
                // Parse the Host Name and Port
                //
                int uriEndIndex = headers.IndexOf(offset, headersLength, (Byte)' ');
                if (uriEndIndex <= 0)
                {
                    if (AppLayerProxy.ErrorLogger != null)
                        AppLayerProxy.ErrorLogger.WriteLine("{0} Invalid first line of CONNECT '{1}': Missing space after uri",
                            clientLogString, ConnectLineToStringForLog(headers, headersLength));
                    serverPort = 0;
                    return null;
                }

                String serverString = ParseHost(headers, offset, (uint)uriEndIndex, true, clientLogString, out serverPort);
                if (serverString == null)
                    return null;

                //
                // Make the forward connection
                //
                if (AppLayerProxy.Logger != null)
                    AppLayerProxy.Logger.WriteLine("{0} CONNECT {1}:{2} (connecting...)", clientLogString, serverString, serverPort);

                return serverString;
            }
            else
            {
                isConnect = false;
                //
                // Find the host header
                //
                Int32 hostIndex = headers.IndexOf(0, headersLength, HttpVars.HostHeaderUtf8);
                if (hostIndex < 0)
                {
                    if (AppLayerProxy.ErrorLogger != null)
                        AppLayerProxy.ErrorLogger.WriteLine("{0} Missing Host header '{1}'", clientLogString, headers);
                    serverPort = 0;
                    return null;
                }
                hostIndex += HttpVars.HostHeaderUtf8.Length;

                // Get this before the headers are shifted from removing the Proxy-Connection header
                serverPort = 80;
                String serverString;
                {
                    uint newlineIndex = GetNewlineIndex(headers, (uint)hostIndex, headersLength);
                    serverString = ParseHost(headers, (uint)hostIndex, newlineIndex, false, clientLogString, out serverPort);
                }

                //
                // Remove the Proxy-Connection header
                //
                Int32 proxyConnectionIndex = headers.IndexOf(0, headersLength, HttpVars.ProxyConnectionHeaderUtf8);
                if (proxyConnectionIndex > 0)
                {
                    int newlineIndex = headers.IndexOf((uint)(proxyConnectionIndex + HttpVars.ProxyConnectionHeaderUtf8.Length), headersLength, (Byte)'\n');
                    if (newlineIndex < 0)
                        throw new InvalidOperationException("CodeBug: this should never happen, a newline should have already been found");
                    uint removeLength = (uint)(newlineIndex - proxyConnectionIndex);
                    for(uint i = (uint)newlineIndex + 1; i < totalLength; i++)
                    {
                        headers[i - removeLength] = headers[i];
                    }
                    headersLength -= removeLength;
                    totalLength -= removeLength;
                }

                //
                // Make the forward connection
                //
                if (AppLayerProxy.Logger != null)
                    AppLayerProxy.Logger.WriteLine("{0} Host: {1}:{2} Proxy", clientLogString, serverString, serverPort);

                return serverString;
            }
        }

        // Returns null on error (logs it's own errors)
        static String ParseHost(Byte[] text, UInt32 offset, UInt32 limit,  Boolean isConnect, String clientLogString, out UInt16 port)
        {
            UInt32 originalOffset = offset;

            // GET HOST NAME
            if (text.EqualsAt(offset, HttpVars.HttpsPrefix, 0, (uint)HttpVars.HttpsPrefix.Length))
            {
                port = 443;
                offset += (uint)HttpVars.HttpsPrefix.Length;
            }
            else if (text.EqualsAt(offset, HttpVars.HttpPrefix, 0, (uint)HttpVars.HttpPrefix.Length))
            {
                port = 80;
                offset += (uint)HttpVars.HttpPrefix.Length;
            }
            else
            {
                port = 80; // Default Port
            }

            if (offset + 1 >= limit)
            {
                if (AppLayerProxy.ErrorLogger != null)
                    AppLayerProxy.ErrorLogger.WriteLine("{0} Invalid Host (not long enough) '{1}'",
                        clientLogString, Encoding.UTF8.GetString(text, (int)originalOffset, (int)(limit - originalOffset)));
                return null;
            }

            // Parse Port
            UInt32 hostLength;
            for (UInt32 i = limit - 1; true; i--)
            {
                Byte c = text[i];
                if (c == ':')
                {
                    Int32 parsedPort;
                    UInt32 indexAfterPort = text.TryParseInt32(i + 1, limit, out parsedPort);
                    if (indexAfterPort != limit || parsedPort < 0 || parsedPort > UInt16.MaxValue)
                    {
                        if (AppLayerProxy.ErrorLogger != null)
                            AppLayerProxy.ErrorLogger.WriteLine("[{0}] Invalid port number '{1}'",
                                clientLogString, Encoding.UTF8.GetString(text, (int)i + 1, (int)(limit - (i + 1))));
                        return null;
                    }
                    port = (UInt16)parsedPort;
                    hostLength = i - offset;
                    break;
                }
                if (c < '0' || c > '9' || limit <= offset)
                {
                    if (isConnect)
                    {
                        if (AppLayerProxy.ErrorLogger != null)
                            AppLayerProxy.ErrorLogger.WriteLine("{0} Invalid first line of CONNECT '{1}': Missing colon in host name",
                                clientLogString, ConnectLineToStringForLog(text, limit));
                        return null;
                    }
                    port = 80;
                    hostLength = limit - offset;
                    break;
                }
            }

            return Encoding.UTF8.GetString(text, (int)offset, (int)hostLength);
        }


        static UInt32 GetNewlineIndex(Byte[] text, UInt32 offset, UInt32 limit)
        {
            // Note: there should always be a newline so it should be impossible to reach the limit
            int newlineIndex = text.IndexOf(offset, limit, (Byte)'\n');
            if (newlineIndex < 0)
            {
                throw new InvalidOperationException("CodeBug: this should never happen, a newline should have already been found");
            }
            return (newlineIndex > 0 && text[newlineIndex - 1] == '\r') ? (uint)newlineIndex - 1 : (uint)newlineIndex;
        }
        static String ConnectLineToStringForLog(Byte[] headers, UInt32 limit)
        {
            return Encoding.UTF8.GetString(headers, 0, (int)GetNewlineIndex(headers, 0, limit));
        }
    }
}
