using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using More;

namespace More.Net
{
    public class TcpSocket : Socket
    {
        public TcpSocket(AddressFamily addressFamily)
            : base(addressFamily, SocketType.Stream, ProtocolType.Tcp)
        {
        }
    }
    public class UdpSocket : Socket
    {
        public UdpSocket(AddressFamily addressFamily)
            : base(addressFamily, SocketType.Dgram, ProtocolType.Udp)
        {
        }
    }

    [Flags]
    public enum ServerInstruction
    {
        NoInstruction = 0x00,
        CloseClient = 0x01,
        StopServer = 0x02,
        StopShouldBeGraceful = 0x04,
    }
    public interface SelectServerCallback
    {
        void ServerStopped(); // Always called if the server is stopped
        ServerInstruction ListenSocketClosed(UInt32 clientCount);
    }
    public interface StreamSelectServerCallback : SelectServerCallback
    {
        void ServerListening(Socket listenSocket);
        ServerInstruction ClientOpenCallback(UInt32 clientCount, Socket socket);
        ServerInstruction ClientCloseCallback(UInt32 clientCount, Socket socket);
        ServerInstruction ClientDataCallback(Socket socket, Byte[] bytes, UInt32 bytesRead);
    }
    public interface DatagramSelectServerCallback : SelectServerCallback
    {
        ServerInstruction DatagramPacket(EndPoint endPoint, Socket socket, Byte[] bytes, UInt32 bytesRead);
    }
    public class TcpSelectServer : IDisposable
    {
        readonly List<Socket> connectedSockets;

        Boolean keepRunning;
        Socket listenSocket;

        public TcpSelectServer()
        {
            this.connectedSockets = new List<Socket>();
            this.keepRunning = false;
            this.listenSocket = null;
        }
        public void AddReadSocket(Socket socket)
        {
            lock (connectedSockets)
            {
                connectedSockets.Add(socket);
            }
        }
        public void PrepareToRun()
        {
            this.keepRunning = true;
        }
        public void Dispose()
        {
            this.keepRunning = false;

            //
            // Close listen socket
            //
            Socket listenSocket = this.listenSocket;
            this.listenSocket = null;
            if (listenSocket != null)
            {
                listenSocket.Close();
            }

            //
            // Close connections
            //
            lock (connectedSockets)
            {
                for (int i = 0; i < connectedSockets.Count; i++)
                {
                    Socket socket = connectedSockets[i];
                    if (socket.Connected)
                    {
                        try
                        {
                            socket.Shutdown(SocketShutdown.Both);
                        }
                        catch (Exception) { }
                    }
                    socket.Close();
                }
                connectedSockets.Clear();
            }
        }
        ServerInstruction CloseAndRemoveClient(Socket socket, StreamSelectServerCallback callback)
        {
            lock (connectedSockets)
            {
                connectedSockets.Remove(socket);
            }

            if (socket.Connected)
            {
                try { socket.Shutdown(SocketShutdown.Both); }
                catch (SystemException) { }
            }

            ServerInstruction instruction = callback.ClientCloseCallback((UInt32)connectedSockets.Count, socket);

            socket.Close();

            return instruction;
        }
        public void Run(TextWriter log, IPEndPoint ipEndPoint, Int32 backlog, byte[] readBytes, StreamSelectServerCallback callback)
        {
            List<Socket> selectSockets = new List<Socket>();

            try
            {
                if (listenSocket != null) throw new InvalidOperationException(String.Format("This server class is already running"));

                this.listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(ipEndPoint);
                listenSocket.Listen(backlog);

                callback.ServerListening(listenSocket);

                while (keepRunning)
                {
                    if (log != null) log.WriteLine("[TcpSelectServer] select...");

                    selectSockets.Clear();
                    selectSockets.Add(listenSocket);
                    lock (connectedSockets)
                    {
                        selectSockets.AddRange(connectedSockets);
                    }

                    // debug
                    Socket.Select(selectSockets, null, null, Int32.MaxValue);

                    for(int i = 0; i < selectSockets.Count; i++)
                    {
                        Socket readSocket = selectSockets[i];

                        if (readSocket == listenSocket)
                        {
                            // Accept and store the new client's socket
                            Socket newSocket = readSocket.Accept();
                            lock (connectedSockets)
                            {
                                connectedSockets.Add(newSocket);
                            }

                            ServerInstruction instruction = callback.ClientOpenCallback((UInt32)connectedSockets.Count, newSocket);

                            if ((instruction & ServerInstruction.CloseClient) != 0)
                            {
                                instruction |= CloseAndRemoveClient(newSocket, callback);
                            }

                            if ((instruction & ServerInstruction.StopServer) != 0) return;
                        }
                        else
                        {
                            // Read and process the data as appropriate
                            int bytesRead;
                            try
                            {
                                bytesRead = readSocket.Receive(readBytes);
                            }
                            catch (SocketException)
                            {
                                bytesRead = -1;
                            }
                            catch (ObjectDisposedException)
                            {
                                bytesRead = -1;
                            }
                            if (bytesRead <= 0)
                            {
                                ServerInstruction instruction = CloseAndRemoveClient(readSocket, callback);
                                if ((instruction & ServerInstruction.StopServer) != 0) return;
                            }
                            else
                            {
                                ServerInstruction instruction = callback.ClientDataCallback(readSocket, readBytes, (UInt32)bytesRead);
                                if ((instruction & ServerInstruction.CloseClient) != 0)
                                {
                                    instruction |= CloseAndRemoveClient(readSocket, callback);
                                }

                                if ((instruction & ServerInstruction.StopServer) != 0) return;
                            }
                        }
                    }
                }

            }
            finally
            {
                Dispose();
                callback.ServerStopped();
            }
        }
    }
    public class TcpSelectListener
    {
        public readonly IPEndPoint endPoint;
        public readonly Int32 backlog;
        public readonly StreamSelectServerCallback callback;

        public TcpSelectListener(IPEndPoint endPoint, Int32 backlog, StreamSelectServerCallback callback)
        {
            this.endPoint = endPoint;
            this.backlog = backlog;
            this.callback = callback;
        }
    }
    public class UdpSelectListener
    {
        public readonly IPEndPoint endPoint;
        public readonly DatagramSelectServerCallback callback;
        public UdpSelectListener(IPEndPoint endPoint, DatagramSelectServerCallback callback)
        {
            this.endPoint = endPoint;
            this.callback = callback;
        }
    }
    public class MultipleListenersSelectServer
    {
        readonly List<Socket> connectedTcpSockets;
        readonly Dictionary<Socket, StreamSelectServerCallback> connectedTcpCallbackDictionary;

        readonly List<Socket> connectedUdpSockets;
        readonly Dictionary<Socket, DatagramSelectServerCallback> connectedUdpCallbackDictionary;

        private Boolean keepRunning;

        public UInt64 totalSelectBlockTimeMicroseconds;

        public MultipleListenersSelectServer()
        {
            this.connectedTcpSockets = new List<Socket>();
            this.connectedTcpCallbackDictionary = new Dictionary<Socket, StreamSelectServerCallback>();

            this.connectedUdpSockets = new List<Socket>();
            this.connectedUdpCallbackDictionary = new Dictionary<Socket, DatagramSelectServerCallback>();

            this.keepRunning = false;
        }
        public void AddReadTcpSocket(Socket socket, StreamSelectServerCallback callback)
        {
            connectedTcpSockets.Add(socket);
            connectedTcpCallbackDictionary.Add(socket, callback);
        }
        public void AddUdpSocket(Socket socket, DatagramSelectServerCallback callback)
        {
            connectedUdpSockets.Add(socket);
            connectedUdpCallbackDictionary.Add(socket, callback);
        }
        public void Stop()
        {
            this.keepRunning = false;

            connectedTcpCallbackDictionary.Clear();

            lock (connectedTcpSockets)
            {
                foreach (Socket socket in connectedTcpSockets)
                {
                    if (socket.Connected)
                    {
                        try
                        {
                            socket.Shutdown(SocketShutdown.Both);
                        }
                        catch (Exception) { }
                    }
                    socket.Close();
                }
                connectedTcpSockets.Clear();
            }
        }
        public void PrepareToRun()
        {
            this.keepRunning = true;

            connectedTcpSockets.Clear();
            connectedTcpCallbackDictionary.Clear();

            connectedUdpSockets.Clear();
            connectedUdpCallbackDictionary.Clear();
        }
        private ServerInstruction CloseAndRemoveClient(Socket socket, StreamSelectServerCallback tcpCallback)
        {
            connectedTcpSockets.Remove(socket);
            connectedTcpCallbackDictionary.Remove(socket);
            if (socket.Connected)
            {
                try { socket.Shutdown(SocketShutdown.Both); }
                catch (SystemException) { }
            }

            ServerInstruction instruction = tcpCallback.ClientCloseCallback((UInt32)connectedTcpSockets.Count, socket);

            socket.Close();

            return instruction;
        }
        private void CloseAndRemoveUdpConnectedSocket(Socket udpSocket)
        {
            connectedUdpSockets.Remove(udpSocket);
            connectedUdpCallbackDictionary.Remove(udpSocket);
        }
        public void Run(TextWriter eventLog, Byte[] readBytes, TcpSelectListener[] tcpListeners, UdpSelectListener[] udpListeners)
        {
            List<Socket> selectSockets = new List<Socket>();

            Socket[] tcpListenSockets = null;
            Dictionary<Socket, TcpSelectListener> tcpListenerDictionary = null;

            Socket[] udpListenSockets = null;
            Dictionary<Socket, UdpSelectListener> udpListenerDictionary = null;

            try
            {
                //
                // Setup Tcp Sockets
                //
                if (tcpListeners != null && tcpListeners.Length > 0)
                {
                    tcpListenSockets = new Socket[tcpListeners.Length];
                    tcpListenerDictionary = new Dictionary<Socket, TcpSelectListener>();
                    for (int i = 0; i < tcpListeners.Length; i++)
                    {
                        TcpSelectListener tcpListener = tcpListeners[i];

                        TcpSocket tcpSocket = new TcpSocket(AddressFamily.InterNetwork);
                        tcpSocket.Bind(tcpListener.endPoint);
                        tcpSocket.Listen(tcpListener.backlog);
                        tcpListener.callback.ServerListening(tcpSocket);

                        tcpListenSockets[i] = tcpSocket;
                        tcpListenerDictionary.Add(tcpListenSockets[i], tcpListener);
                    }
                }

                //
                // Setup UDP Sockets
                //
                if (udpListeners != null && udpListeners.Length > 0)
                {
                    udpListenSockets = new Socket[udpListeners.Length];
                    udpListenerDictionary = new Dictionary<Socket, UdpSelectListener>();
                    for (int i = 0; i < udpListeners.Length; i++)
                    {
                        UdpSelectListener udpListener = udpListeners[i];

                        UdpSocket udpSocket = new UdpSocket(AddressFamily.InterNetwork);
                        udpSocket.Bind(udpListener.endPoint);

                        udpListenSockets[i] = udpSocket;
                        udpListenerDictionary.Add(udpSocket, udpListener);
                    }
                }

                totalSelectBlockTimeMicroseconds = 0;

                while (keepRunning)
                {
                    if (eventLog != null) eventLog.WriteLine("[SelectServer] Select...");

                    selectSockets.Clear();
                    if (tcpListenSockets != null) selectSockets.AddRange(tcpListenSockets);
                    if (udpListenSockets != null) selectSockets.AddRange(udpListenSockets);

                    selectSockets.AddRange(connectedTcpSockets);
                    selectSockets.AddRange(connectedUdpSockets);


                    Int64 beforeSelect = Stopwatch.GetTimestamp();

                    //Socket.Select(selectSockets, null, null, -1);
                    Socket.Select(selectSockets, null, null, Int32.MaxValue);

                    Int64 selectTicks = Stopwatch.GetTimestamp() - beforeSelect;

                    Int64 selectBlockMicroseconds = selectTicks.StopwatchTicksAsMicroseconds();
                    totalSelectBlockTimeMicroseconds += (UInt64)selectBlockMicroseconds;
                    if (eventLog != null) eventLog.WriteLine("[SelectServer] Blocked for {0} microseconds, {1} Sockets Ready",
                        selectBlockMicroseconds, selectSockets.Count);

                    if (selectSockets.Count <= 0)
                    {
                        // Allow the socket to pop every minute without any sockets ready
                        if (selectBlockMicroseconds < (1000000 * 60))
                        {
                            throw new InvalidOperationException(String.Format("Select popped after only {0} microseconds without any sockets ready",
                                selectBlockMicroseconds));
                        }
                    }

                    for (int i = 0; i < selectSockets.Count; i++)
                    {
                        Socket readSocket = selectSockets[i];

                        //
                        // Check if it is a stream socket
                        //
                        {
                            StreamSelectServerCallback tcpCallback;

                            if (connectedTcpCallbackDictionary.TryGetValue(readSocket, out tcpCallback))
                            {
                                // Read and process the data as appropriate
                                int bytesRead;
                                try
                                {
                                    bytesRead = readSocket.Receive(readBytes);
                                }
                                catch (SocketException)
                                {
                                    bytesRead = -1;
                                }
                                if (bytesRead <= 0)
                                {
                                    ServerInstruction instruction = CloseAndRemoveClient(readSocket, tcpCallback);
                                    if ((instruction & ServerInstruction.StopServer) != 0) return;
                                }
                                else
                                {
                                    ServerInstruction instruction = tcpCallback.ClientDataCallback(readSocket, readBytes, (UInt32)bytesRead);
                                    if ((instruction & ServerInstruction.CloseClient) != 0)
                                    {
                                        instruction |= CloseAndRemoveClient(readSocket, tcpCallback);
                                    }
                                    if ((instruction & ServerInstruction.StopServer) != 0) return;
                                }
                                continue;
                            }
                        }

                        {
                            DatagramSelectServerCallback udpCallback;

                            if (connectedUdpCallbackDictionary.TryGetValue(readSocket, out udpCallback))
                            {
                                // Read and process the data as appropriate
                                int bytesRead;
                                try
                                {
                                    bytesRead = readSocket.Receive(readBytes);
                                }
                                catch (SocketException)
                                {
                                    bytesRead = -1;
                                }
                                if (bytesRead <= 0)
                                {
                                    CloseAndRemoveUdpConnectedSocket(readSocket);
                                }
                                else
                                {
                                    ServerInstruction instruction = udpCallback.DatagramPacket(readSocket.RemoteEndPoint, readSocket, readBytes, (UInt32)bytesRead);
                                    if ((instruction & ServerInstruction.CloseClient) != 0)
                                    {
                                        CloseAndRemoveUdpConnectedSocket(readSocket);
                                    }
                                    if ((instruction & ServerInstruction.StopServer) != 0) return;
                                }
                                continue;
                            }
                        }


                        //
                        // Check if it is a datagram socket
                        //
                        if (udpListenerDictionary != null)
                        {
                            UdpSelectListener udpListener;
                            if (udpListenerDictionary.TryGetValue(readSocket, out udpListener))
                            {
                                // Read and process the data as appropriate
                                EndPoint udpListenerEndPoint = udpListener.endPoint;
                                int bytesRead;
                                try
                                {
                                    bytesRead = readSocket.ReceiveFrom(readBytes, ref udpListenerEndPoint);
                                }
                                catch (SocketException e)
                                {
                                    if (eventLog != null) eventLog.WriteLine("[SelectServer] {0}: {1}", e.GetType().Name, e.Message);
                                    continue;
                                }

                                if (bytesRead <= 0)
                                {
                                    if (eventLog != null) eventLog.WriteLine("[SelectServer] [Warning] readSocket return {0} bytes", bytesRead);
                                }
                                else
                                {
                                    ServerInstruction instruction = udpListener.callback.DatagramPacket(udpListenerEndPoint, readSocket, readBytes, (UInt32)bytesRead);
                                    if ((instruction & ServerInstruction.StopServer) != 0) return;
                                }
                                continue;
                            }
                        }

                        //
                        // It must be a tcp listen socket
                        //
                        if (tcpListenerDictionary != null)
                        {
                            TcpSelectListener tcpListener;
                            if (tcpListenerDictionary.TryGetValue(readSocket, out tcpListener))
                            {
                                // Accept and store the new client's socket
                                Socket newSocket = readSocket.Accept();
                                connectedTcpSockets.Add(newSocket);
                                connectedTcpCallbackDictionary.Add(newSocket, tcpListener.callback);

                                ServerInstruction instruction = tcpListener.callback.ClientOpenCallback((UInt32)connectedTcpSockets.Count, newSocket);

                                if ((instruction & ServerInstruction.CloseClient) != 0)
                                {
                                    instruction |= CloseAndRemoveClient(newSocket, tcpListener.callback);
                                }

                                if ((instruction & ServerInstruction.StopServer) != 0) return;
                                continue;
                            }
                        }

                        throw new InvalidOperationException(String.Format("Socket '{0}' was not a connected socket or a listen socket?", readSocket));
                    }
                }
            }
            finally
            {
                Stop();
                if (tcpListeners != null)
                {
                    for (int i = 0; i < tcpListeners.Length; i++)
                    {
                        tcpListeners[i].callback.ServerStopped();
                    }
                }
                if (udpListeners != null)
                {
                    for (int i = 0; i < udpListeners.Length; i++)
                    {
                        udpListeners[i].callback.ServerStopped();
                    }
                }
            }
        }
    }



    public class SocketHandlerMethods
    {
        public Boolean swallowExceptions;
        public SocketReceiveHandler receiveHandler;
        public SocketClosedHandler socketClosedHandler;
        public SocketHandlerMethods(Boolean swallowExceptions, SocketReceiveHandler receiveHandler, SocketClosedHandler socketClosedHandler)
        {
            this.swallowExceptions = swallowExceptions;
            this.receiveHandler = receiveHandler;
            this.socketClosedHandler = socketClosedHandler;
        }
    }

    public class SocketReceiveHandlerToDataHandler
    {
        readonly DataHandler dataHandler;
        public SocketReceiveHandlerToDataHandler(DataHandler dataHandler)
        {
            this.dataHandler = dataHandler;
        }
        public Boolean ReceiveHandler(Socket socket, ByteBuffer safeBuffer, ref SocketReceiveHandler receiveHandler)
        {
            Int32 bytesRead = socket.Receive(safeBuffer.array);
            if (bytesRead <= 0) return true;

            dataHandler(safeBuffer.array, 0, (UInt32)bytesRead);
            return false;
        }
    }

    // return null to close the client
    public delegate SocketHandlerMethods AcceptHandler(Socket listenSocket, Socket socket, ByteBuffer safeBuffer);

    // return true to close client
    public delegate void SocketReceiveHandler(Socket socket, ByteBuffer safeBuffer, ref SocketHandlerMethods receiveHandler);
    public delegate void SocketClosedHandler(Socket socket);

    public class TcpListener
    {
        public readonly Socket boundSocket;
        public readonly Int32 socketBackLog;
        public readonly AcceptHandler acceptHandler;
        public TcpListener(Int32 listenPort, Int32 socketBackLog, AcceptHandler acceptHandler)
        {
            this.boundSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.boundSocket.Bind(new IPEndPoint(IPAddress.Any, listenPort));
            this.socketBackLog = socketBackLog;
            this.acceptHandler = acceptHandler;
        }
        public TcpListener(EndPoint listenEndPoint, Int32 socketBackLog, AcceptHandler acceptHandler)
        {
            this.boundSocket = new Socket(listenEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.boundSocket.Bind(listenEndPoint);
            this.socketBackLog = socketBackLog;
            this.acceptHandler = acceptHandler;
        }
        public TcpListener(Socket boundSocket, Int32 socketBackLog, AcceptHandler acceptHandler)
        {
#if WindowsCE
            if (boundSocket.LocalEndPoint == null) 
#else
            if (!boundSocket.IsBound) 
#endif
                throw new InvalidOperationException("You must bind the socket before using this constructor");

            this.boundSocket = boundSocket;
            this.socketBackLog = socketBackLog;
            this.acceptHandler = acceptHandler;
        }
    }
    public class SelectServerStaticTcpListeners : IDisposable
    {
        //
        // Listen Sockets
        //
        readonly Socket[] tcpListenSockets;
        readonly IDictionary<Socket, AcceptHandler> tcpListenSocketMap;

        //
        // Receive Sockets
        //
        readonly List<Socket> tcpReceiveSocketList;
        readonly IDictionary<Socket, SocketHandlerMethods> tcpReceiveSocketMap;

        readonly ByteBuffer safeBuffer;

        Boolean keepRunning;

        public SelectServerStaticTcpListeners(IList<TcpListener> tcpListeners,
            UInt32 safeBufferInitialCapacity, UInt32 safeBufferExpandLength)
        {
            if(tcpListeners.Count <= 0) throw new InvalidOperationException("You have provided 0 tcp listeners");

            this.tcpListenSockets = new Socket[tcpListeners.Count];
            this.tcpListenSocketMap = new Dictionary<Socket, AcceptHandler>(tcpListeners.Count);
            for(int i = 0; i < tcpListeners.Count; i++)
            {
                TcpListener tcpListener = tcpListeners[i];
                this.tcpListenSockets[i] = tcpListener.boundSocket;
                this.tcpListenSocketMap.Add(tcpListener.boundSocket, tcpListener.acceptHandler);
                tcpListener.boundSocket.Listen(tcpListener.socketBackLog);
            }

            this.tcpReceiveSocketList = new List<Socket>();
            this.tcpReceiveSocketMap = new Dictionary<Socket, SocketHandlerMethods>();

            this.safeBuffer = new ByteBuffer(safeBufferInitialCapacity, safeBufferExpandLength);

            this.keepRunning = true;
        }
        public void Dispose()
        {
            this.keepRunning = false;

            lock (tcpListenSockets)
            {
                //
                // Close all listen sockets
                //
                for (int i = 0; i < tcpListenSockets.Length; i++)
                {
                    tcpListenSockets[i].Close();
                }

                //
                // Close all receive sockets
                //
                for (int i = 0; i < tcpReceiveSocketList.Count; i++)
                {
                    tcpReceiveSocketList[i].ShutdownAndDispose();
                }
                tcpReceiveSocketList.Clear();
                tcpReceiveSocketMap.Clear();
            }
        }
        public void Run()
        {
            List<Socket> selectSockets = new List<Socket>();

            try
            {
                while (keepRunning)
                {
                    //
                    // Perform the select
                    //
                    selectSockets.Clear();
                    selectSockets.AddRange(tcpListenSockets);
                    selectSockets.AddRange(tcpReceiveSocketList);
                    
                    Socket.Select(selectSockets, null, null, Int32.MaxValue);

                    if (selectSockets.Count <= 0) continue;

                    lock(tcpListenSockets)
                    {
                        for (int i = 0; i < selectSockets.Count; i++)
                        {
                            Socket socket = selectSockets[i];

                            SocketHandlerMethods handlerMethods;
                            AcceptHandler acceptHandler;

                            if (tcpReceiveSocketMap.TryGetValue(socket, out handlerMethods))
                            {
                                try
                                {
                                    handlerMethods.receiveHandler(
                                        socket, safeBuffer, ref handlerMethods);
                                }
                                catch (SocketException)
                                {
                                    if (handlerMethods != null)
                                    {
                                        handlerMethods.receiveHandler = null;
                                    }
                                }
                                catch (Exception)
                                {
                                    if (handlerMethods != null)
                                    {
                                        handlerMethods.receiveHandler = null;
                                        if (!handlerMethods.swallowExceptions) throw;
                                    }
                                }

                                if (!socket.Connected || handlerMethods == null || handlerMethods.receiveHandler == null)
                                {
                                    socket.ShutdownAndDispose();

                                    tcpReceiveSocketList.Remove(socket);
                                    tcpReceiveSocketMap.Remove(socket);

                                    if (handlerMethods.socketClosedHandler != null)
                                        handlerMethods.socketClosedHandler(socket);
                                }
                            }
                            else if (tcpListenSocketMap.TryGetValue(socket, out acceptHandler))
                            {
                                Socket newReceiveSocket = socket.Accept();

                                handlerMethods = acceptHandler(socket, newReceiveSocket, safeBuffer);

                                if (handlerMethods == null || handlerMethods.receiveHandler == null)
                                {
                                    socket.ShutdownAndDispose();
                                    if (handlerMethods.socketClosedHandler != null) handlerMethods.socketClosedHandler(newReceiveSocket);
                                }
                                else
                                {
                                    tcpReceiveSocketList.Add(newReceiveSocket);
                                    tcpReceiveSocketMap.Add(newReceiveSocket, handlerMethods);
                                }
                            }
                            else
                            {
                                throw new InvalidOperationException(String.Format(
                                    "CodeBug: The SelectSocket list had a socket that was not a recognized listen or receive socket (LocalEndPoint='{0}', RemoteEndPoint='{1}')",
                                    socket.SafeLocalEndPointString(), socket.SafeRemoteEndPointString()));
                            }
                        }
                    }
                }
            }
            finally
            {
                Dispose();
            }
        }
    }
    public class SelectServerDynamicTcpListeners : IDisposable
    {
        //
        // Pop Socket
        //
        readonly Socket popSocket;
        readonly EndPoint popSocketEndPoint;

        //
        // Listen Sockets
        //
        readonly List<Socket> tcpListenSockets;
        readonly IDictionary<Socket, AcceptHandler> tcpListenSocketMap;

        //
        // Receive Sockets
        //
        readonly List<Socket> tcpReceiveSocketList;
        readonly IDictionary<Socket, SocketHandlerMethods> tcpReceiveSocketMap;

        readonly ByteBuffer safeBuffer;

        Boolean keepRunning;

        public SelectServerDynamicTcpListeners(UInt32 safeBufferInitialCapacity, UInt32 safeBufferExpandLength)
        {
            this.popSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.popSocket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            this.popSocketEndPoint = popSocket.LocalEndPoint;

            this.tcpListenSockets = new List<Socket>();
            this.tcpListenSocketMap = new Dictionary<Socket, AcceptHandler>();

            this.tcpReceiveSocketList = new List<Socket>();
            this.tcpReceiveSocketMap = new Dictionary<Socket, SocketHandlerMethods>();

            this.safeBuffer = new ByteBuffer(safeBufferInitialCapacity, safeBufferExpandLength);

            this.keepRunning = true;
        }
        public void Dispose()
        {
            this.keepRunning = false;

            lock (tcpListenSockets)
            {
                //
                // Close pop socket
                //
                popSocket.Close();

                //
                // Close all listen sockets
                //
                for (int i = 0; i < tcpListenSockets.Count; i++)
                {
                    tcpListenSockets[i].Close();
                }
                tcpListenSockets.Clear();
                tcpListenSocketMap.Clear();

                //
                // Close all receive sockets
                //
                for (int i = 0; i < tcpReceiveSocketList.Count; i++)
                {
                    tcpReceiveSocketList[i].ShutdownAndDispose();
                }
                tcpReceiveSocketList.Clear();
                tcpReceiveSocketMap.Clear();
            }
        }
        public void AddListener(UInt16 port, Int32 backlog, AcceptHandler acceptHandler)
        {
            AddListener(new IPEndPoint(IPAddress.Any, port), backlog, acceptHandler);
        }
        public void AddListener(IPEndPoint listenEndPoint, Int32 backlog, AcceptHandler acceptHandler)
        {
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(listenEndPoint);
            AddListener(listenSocket, backlog, acceptHandler);
        }
        public void AddListener(Socket boundSocket, Int32 backlog, AcceptHandler acceptHandler)
        {
#if WindowsCE
            if (boundSocket.LocalEndPoint == null)
#else
            if (!boundSocket.IsBound) 
#endif
                throw new InvalidOperationException("This method requires that the socket is bound");

            boundSocket.Listen(backlog);

            lock (tcpListenSockets)
            {
                tcpListenSockets.Add(boundSocket);
                tcpListenSocketMap.Add(boundSocket, acceptHandler);
            }

            popSocket.SendTo(StaticData.ZeroByteArray, popSocketEndPoint);
        }
        public void Run()
        {
            List<Socket> selectSockets = new List<Socket>();

            try
            {
                while (keepRunning)
                {
                    //
                    // Perform the select
                    //
                    selectSockets.Clear();
                    selectSockets.Add(popSocket);

                    lock (tcpListenSockets)
                    {
                        selectSockets.AddRange(tcpListenSockets);
                        selectSockets.AddRange(tcpReceiveSocketList);
                    }
                    
                    Socket.Select(selectSockets, null, null, Int32.MaxValue);

                    if (selectSockets.Count <= 0) continue;

                    lock (tcpListenSockets)
                    {
                        for (int i = 0; i < selectSockets.Count; i++)
                        {
                            Socket socket = selectSockets[i];

                            SocketHandlerMethods handlerMethods;
                            AcceptHandler acceptHandler;

                            if (tcpReceiveSocketMap.TryGetValue(socket, out handlerMethods))
                            {
                                try
                                {
                                    handlerMethods.receiveHandler(
                                        socket, safeBuffer, ref handlerMethods);
                                }
                                catch (SocketException)
                                {
                                    if (handlerMethods != null)
                                    {
                                        handlerMethods.receiveHandler = null;
                                    }
                                }
                                catch (Exception)
                                {
                                    if (handlerMethods != null)
                                    {
                                        handlerMethods.receiveHandler = null;
                                        if (!handlerMethods.swallowExceptions) throw;
                                    }
                                }

                                if (!socket.Connected || handlerMethods == null || handlerMethods.receiveHandler == null)
                                {
                                    socket.ShutdownAndDispose();

                                    tcpReceiveSocketList.Remove(socket);
                                    tcpReceiveSocketMap.Remove(socket);

                                    if (handlerMethods.socketClosedHandler != null)
                                        handlerMethods.socketClosedHandler(socket);
                                }
                            }
                            else if (tcpListenSocketMap.TryGetValue(socket, out acceptHandler))
                            {
                                Socket newReceiveSocket = socket.Accept();

                                handlerMethods = acceptHandler(socket, newReceiveSocket, safeBuffer);

                                if (handlerMethods == null || handlerMethods.receiveHandler == null)
                                {
                                    socket.ShutdownAndDispose();
                                    if (handlerMethods.socketClosedHandler != null) handlerMethods.socketClosedHandler(newReceiveSocket);
                                }
                                else
                                {
                                    tcpReceiveSocketList.Add(newReceiveSocket);
                                    tcpReceiveSocketMap.Add(newReceiveSocket, handlerMethods);
                                }
                            }
                            else if (socket == popSocket)
                            {
                                popSocket.Receive(safeBuffer.array);
                            }
                            else
                            {
                                throw new InvalidOperationException(String.Format(
                                     "CodeBug: The SelectSocket list had a socket that was not a recognized listen or receive socket (RemoteEndPoint={0})",
                                     socket.SafeRemoteEndPointString()));
                            }
                        }
                    }
                }
            }
            finally
            {
                Dispose();
            }
        }
    }
}
