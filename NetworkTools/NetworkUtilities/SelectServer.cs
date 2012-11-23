using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace Marler.NetworkTools
{
    [Flags]
    public enum ServerInstruction
    {
        NoInstruction                 = 0x00,
        CloseClient          = 0x01,
        StopServer           = 0x02,
        StopShouldBeGraceful = 0x04,
    }

    public interface SelectServerCallback
    {
        void ServerStopped(); // Always called if the server is stopped
        ServerInstruction ListenSocketClosed(int clientCount);
    }
    public interface StreamSelectServerCallback : SelectServerCallback {
        ServerInstruction ClientOpenCallback(int clientCount, Socket socket);
        ServerInstruction ClientCloseCallback(int clientCount, Socket socket);
        ServerInstruction ClientDataCallback(Socket socket, byte[] bytes, int bytesRead);
    }

    public interface DatagramSelectServerCallback : SelectServerCallback 
    {
        ServerInstruction DatagramPacket(EndPoint endPoint, Socket socket, byte[] bytes, int bytesRead);
    }
    
    public class TcpSelectServer {
    	
        readonly List<Socket> connectedSockets;
	    private Boolean keepRunning;
    	
	    public TcpSelectServer() {
            this.connectedSockets = new List<Socket>();
		    this.keepRunning = false;
	    }    	
	    public void Stop() {
		    this.keepRunning = false;

            lock (connectedSockets)
            {
                foreach (Socket socket in connectedSockets)
                {
                    if(socket.Connected)
                    {
                        try
                        {
                            socket.Shutdown(SocketShutdown.Both);
                        }
                        catch(Exception) {}
                    }
                    socket.Close();
                }
                connectedSockets.Clear();
            }
	    }
        public void AddReadSocket(Socket socket)
        {
            connectedSockets.Add(socket);
        }
        public void PrepareToRun()
        {
		    this.keepRunning = true;
	    }
        private ServerInstruction CloseAndRemoveClient(Socket socket, StreamSelectServerCallback callback)
        {
            if (!connectedSockets.Remove(socket))
            {
                Console.WriteLine("[TcpSelectServerWarning] failed to remove socket from list");
            }

            if (socket.Connected)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception) { }
                socket.Close();
            }

            return callback.ClientCloseCallback(connectedSockets.Count, socket);
        }
        public void Run(TextWriter log, IPEndPoint ipEndPoint, Int32 backlog, byte[] readBytes, StreamSelectServerCallback callback)
        {    		
            List<Socket> selectSockets = new List<Socket>();

            try
            {
                Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(ipEndPoint);
                listenSocket.Listen(backlog);

                while (keepRunning)
                {
                    if (log != null) log.WriteLine("[TcpSelectServer] select...");

                    selectSockets.Clear();
                    selectSockets.Add(listenSocket);
                    selectSockets.AddRange(connectedSockets);

                    // debug
                    Socket.Select(selectSockets, null, null, Int32.MaxValue);

                    foreach (Socket readSocket in selectSockets)
                    {
                        if (readSocket == listenSocket)
                        {
                            // Accept and store the new client's socket
                            Socket newSocket = readSocket.Accept();
                            connectedSockets.Add(newSocket);

                            ServerInstruction instruction = callback.ClientOpenCallback(connectedSockets.Count, newSocket);

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
                                ServerInstruction instruction = callback.ClientDataCallback(readSocket, readBytes, bytesRead);
                                if ((instruction & ServerInstruction.CloseClient) != 0)
                                {
                                    instruction |= CloseAndRemoveClient(readSocket, callback);
                                }

                                if ((instruction & ServerInstruction.StopServer) != 0) return;
                            }
                        }
                    }
                }

		    } finally {
			    Stop();
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
        readonly Dictionary<Socket, TcpSelectListener> connectedTcpDictionary;


        private Boolean keepRunning;

        public UInt64 totalSelectBlockTimeMicroseconds;

        public MultipleListenersSelectServer()
        {
            this.connectedTcpSockets = new List<Socket>();
            this.connectedTcpDictionary = new Dictionary<Socket, TcpSelectListener>();
            this.keepRunning = false;
        }
        public void Stop()
        {
            this.keepRunning = false;

            connectedTcpDictionary.Clear();

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
            connectedTcpDictionary.Clear();
        }
        private ServerInstruction CloseAndRemoveClient(Socket socket, TcpSelectListener tcpListener)
        {
            connectedTcpSockets.Remove(socket);

            if (socket.Connected)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception) { }
                socket.Close();
            }

            connectedTcpDictionary.Remove(socket);

            return tcpListener.callback.ClientCloseCallback(connectedTcpSockets.Count, socket);
        }
        public void Run(TextWriter eventLog, Byte[] readBytes, TcpSelectListener[] tcpListeners, UdpSelectListener[] udpListeners)
        {
            List<Socket> selectSockets = new List<Socket>();

            Socket[] tcpListenSockets = null;
            Dictionary<Socket, TcpSelectListener> tcpListenerDictionary = null;

            Socket[] udpSockets = null;
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
                        tcpListenSockets[i] = new TcpSocket(AddressFamily.InterNetwork);
                        tcpListenSockets[i].Bind(tcpListener.endPoint);
                        tcpListenSockets[i].Listen(tcpListener.backlog);

                        tcpListenerDictionary.Add(tcpListenSockets[i], tcpListener);
                    }

                }

                //
                // Setup UDP Sockets
                //
                if (udpListeners != null && udpListeners.Length > 0)
                {
                    udpSockets = new Socket[udpListeners.Length];
                    udpListenerDictionary = new Dictionary<Socket, UdpSelectListener>();
                    for (int i = 0; i < udpListeners.Length; i++)
                    {
                        UdpSelectListener udpListener = udpListeners[i];

                        udpSockets[i] = new UdpSocket(AddressFamily.InterNetwork);
                        udpSockets[i].Bind(udpListener.endPoint);

                        udpListenerDictionary.Add(udpSockets[i], udpListener);
                    }
                }

                totalSelectBlockTimeMicroseconds = 0;

                while (keepRunning)
                {
                    if (eventLog != null) eventLog.WriteLine("[SelectServer] Select...");

                    selectSockets.Clear();
                    if (tcpListenSockets != null) selectSockets.AddRange(tcpListenSockets);
                    if (udpSockets != null) selectSockets.AddRange(udpSockets);
                    selectSockets.AddRange(connectedTcpSockets);


                    Int64 beforeSelect = Stopwatch.GetTimestamp();

                    //Socket.Select(selectSockets, null, null, -1);
                    Socket.Select(selectSockets, null, null, Int32.MaxValue);

                    Int64 selectTicks = Stopwatch.GetTimestamp() - beforeSelect;

                    Int64 selectBlockMicroseconds = selectTicks.StopwatchTicksAsMicroseconds();
                    totalSelectBlockTimeMicroseconds += (UInt64)selectBlockMicroseconds;
                    if (eventLog != null) eventLog.WriteLine("[SelectServer] Blocked for {0} microseconds, {1} Sockets Ready",
                        selectBlockMicroseconds, selectSockets.Count);

                    
                    if(selectSockets.Count <= 0)
                    {
                        // Allow the socket to pop every minute without any sockets ready
                        if (selectBlockMicroseconds < (1000000 * 60))
                        {
                            throw new InvalidOperationException(String.Format("Select popped after only {0} microseconds without any sockets ready",
                                selectBlockMicroseconds));
                        }
                    }


                    foreach (Socket readSocket in selectSockets)
                    {
                        //
                        // Check if it is a stream socket
                        //
                        {
                            TcpSelectListener tcpListener;

                            if (connectedTcpDictionary.TryGetValue(readSocket, out tcpListener))
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
                                    ServerInstruction instruction = CloseAndRemoveClient(readSocket, tcpListener);
                                    if ((instruction & ServerInstruction.StopServer) != 0) return;
                                }
                                else
                                {
                                    ServerInstruction instruction = tcpListener.callback.ClientDataCallback(readSocket, readBytes, bytesRead);
                                    if ((instruction & ServerInstruction.CloseClient) != 0)
                                    {
                                        instruction |= CloseAndRemoveClient(readSocket, tcpListener);
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
                                    ServerInstruction instruction = udpListener.callback.DatagramPacket(udpListenerEndPoint, readSocket, readBytes, bytesRead);
                                    if ((instruction & ServerInstruction.StopServer) != 0) return;
                                }
                                continue;
                            }
                        }

                        //
                        // It must be a tcp socket
                        //
                        if (tcpListenerDictionary != null)
                        {
                            TcpSelectListener tcpListener;
                            if (tcpListenerDictionary.TryGetValue(readSocket, out tcpListener))
                            {
                                // Accept and store the new client's socket
                                Socket newSocket = readSocket.Accept();
                                connectedTcpSockets.Add(newSocket);
                                connectedTcpDictionary.Add(newSocket, tcpListener);

                                ServerInstruction instruction = tcpListener.callback.ClientOpenCallback(connectedTcpSockets.Count, newSocket);

                                if ((instruction & ServerInstruction.CloseClient) != 0)
                                {
                                    instruction |= CloseAndRemoveClient(newSocket, tcpListener);
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
}
