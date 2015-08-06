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
    public struct TcpSocket
    {
        public readonly Socket sock;
        public TcpSocket(AddressFamily addressFamily)
        {
            this.sock = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
        }
    }
    public struct UdpSocket
    {
        public readonly Socket sock;
        public UdpSocket(AddressFamily addressFamily)
        {
            this.sock = new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp);
        }
    }

    public delegate void SimpleSelectHandler(ref SelectControl readSocket, Socket socket, Buf safeBuffer);

    public struct SelectControl
    {
        // Both listenSockets and receiveSockets are read sockets
        readonly List<Socket> listenSockets;
        readonly List<Socket> receiveSockets;

        readonly List<Socket> connectList;

        readonly IDictionary<Socket, SimpleSelectHandler> map; // contains mapping from socket to handlers

        public UInt32 TotalSocketCount { get { return (uint)map.Count; } }

        /// <summary>
        /// Should only be set by the select server, used to indicate to connect callbacks whether the
        /// connect socket had an error.
        /// </summary>
        Boolean connectionError;
        public Boolean ConnectionError { get { return connectionError; } set { connectionError = value; } }

        public Boolean SupportsConnect { get { return connectList != null; } }

        // NOTE: Only call this if you have already checked that SupportsConnect is true
        public Boolean HasConnectSockets
        {
            get
            {
                return connectList.Count > 0;
            }
        }

        public SelectControl(Boolean supportConnectSockets)
        {
            this.listenSockets = new List<Socket>();
            this.receiveSockets = new List<Socket>();
            this.map = new Dictionary<Socket, SimpleSelectHandler>();
            if(supportConnectSockets)
            {
                this.connectList = new List<Socket>();
            }
            else
            {
                this.connectList = null;
            }
            this.connectionError = false;
        }

        /// <summary>
        /// Helper method for listen sockets.  Automatically adds the new socket
        /// to this class with the given handler.
        /// </summary>
        /// <param name="listenSocket"></param>
        /// <param name="handler"></param>
        public void PerformAccept(Socket listenSocket, SimpleSelectHandler handler)
        {
            Socket newReceiveSocket = listenSocket.Accept();
            if (newReceiveSocket.Connected)
            {
                AddReceiveSocket(newReceiveSocket, handler);
            }
            else
            {
                newReceiveSocket.Close();
            }
        }

        /// <summary>
        /// Tries to retrieve the handler for the given listner, receiver, or connector socket.
        /// </summary>
        /// <param name="socket">A controlled socket</param>
        /// <returns>The handler if the socket is conrolled, otherwise returns null</returns>
        public SimpleSelectHandler TryGetHandler(Socket socket)
        {
            SimpleSelectHandler ret;
            return map.TryGetValue(socket, out ret) ? ret : null;
        }
        /// <summary>
        /// Copies all listen and receiver sockets to the given list
        /// </summary>
        public void CopyReadSocketsTo(List<Socket> sockets)
        {
            sockets.AddRange(listenSockets);
            sockets.AddRange(receiveSockets);
        }
        
        /// <summary>
        /// Copies all connect sockets to the given list.  Note: this function assumes
        /// that SupportsConnect is true.
        /// </summary>
        public void CopyConnectSocketsTo(List<Socket> sockets)
        {
            sockets.AddRange(connectList);
        }
        public void UpdateHandler(Socket socket, SimpleSelectHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");
            map[socket] = handler;
        }
        public void UpdateConnectorToReceiver(Socket socket, SimpleSelectHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");
            connectList.Remove(socket);
            receiveSockets.Add(socket);
            map[socket] = handler;
        }
        public void AddListenSocket(Socket socket, SimpleSelectHandler handler)
        {
            if (socket == null || handler == null)
                throw new ArgumentNullException();
            listenSockets.Add(socket);
            map.Add(socket, handler);
        }
        public void AddReceiveSocket(Socket socket, SimpleSelectHandler handler)
        {
            if (socket == null || handler == null)
                throw new ArgumentNullException();
            receiveSockets.Add(socket);
            map.Add(socket, handler);
        }
        public void RemoveReceiveSocket(Socket socket)
        {
            receiveSockets.Remove(socket);
            map.Remove(socket);
        }
        public void AddConnectSocket(Socket socket, SimpleSelectHandler handler)
        {
            if (socket == null || handler == null)
                throw new ArgumentNullException();

            connectList.Add(socket);
            map.Add(socket, handler);
        }
        // Note: does not call shutdown
        public void DisposeAndRemoveReceiveSocket(Socket socket)
        {
            socket.Close();
            receiveSockets.Remove(socket);
            map.Remove(socket);
        }
        public void DisposeAndRemoveConnectSocket(Socket socket)
        {
            socket.Close();
            connectList.Remove(socket);
            map.Remove(socket);
        }

        public void ShutdownIfConnectedDisposeAndRemoveReceiveSocket(Socket socket)
        {
            try { if(socket.Connected) socket.Shutdown(SocketShutdown.Both); }
            catch (Exception) { }
            socket.Close();
            receiveSockets.Remove(socket);
            map.Remove(socket);
        }
        public void ShutdownDisposeAndRemoveReceiveSocket(Socket socket)
        {
            try { socket.Shutdown(SocketShutdown.Both); }
            catch (Exception) { }
            socket.Close();
            receiveSockets.Remove(socket);
            map.Remove(socket);
        }
        public void Dispose()
        {
            for (int i = 0; i < listenSockets.Count; i++)
            {
                listenSockets[i].Close();
            }
            listenSockets.Clear();
            for (int i = 0; i < receiveSockets.Count; i++)
            {
                receiveSockets[i].ShutdownAndDispose();
            }
            receiveSockets.Clear();
            if (connectList != null)
            {
                for (int i = 0; i < connectList.Count; i++)
                {
                    connectList[i].ShutdownAndDispose();
                }
                connectList.Clear();
            }
            map.Clear();
        }
        // Calls the socket shutdown/close methods
        // but does not modify the lists
        public void StopServerFromAnotherThread()
        {
            for (int i = 0; i < listenSockets.Count; i++)
            {
                listenSockets[i].Close();
            }
            for (int i = 0; i < receiveSockets.Count; i++)
            {
                receiveSockets[i].ShutdownAndDispose();
            }
            if (connectList != null)
            {
                for (int i = 0; i < connectList.Count; i++)
                {
                    connectList[i].ShutdownAndDispose();
                }
                connectList.Clear();
            }
        }
    }

    // TODO: Create a LockingSelectServer that allows other threads to interact with it.
    /// <summary>
    /// This server can only be modifed within the SimpleSelectHandler callbacks.
    /// Note: the only code that interacts with this class should be the AccepHandlers and ReceiveHandlers
    /// </summary>
    public class SelectServer
    {
        SelectControl selectControl;
        readonly Buf safeBuffer;

        public SelectServer(SelectControl selectControl, Buf safeBuffer)
        {
            if (selectControl.TotalSocketCount == 0)
                throw new InvalidOperationException("You must add at least one socket to selectControl");
            this.selectControl = selectControl;
            this.safeBuffer = safeBuffer;
        }
        public void StopServerFromTheRunThread()
        {
            selectControl.Dispose();
        }
        public void StopServerFromAnotherThread()
        {
            selectControl.StopServerFromAnotherThread();
        }
        public void Run()
        {
            List<Socket> selectReadSockets = new List<Socket>();
            List<Socket> connectSockets, errorSockets;
            if (selectControl.SupportsConnect)
            {
                connectSockets = new List<Socket>();
                errorSockets = new List<Socket>();
            }
            else
            {
                connectSockets = null;
                errorSockets = null;
            }

            while(true)
            {
                //
                // Perform the select
                //
                selectReadSockets.Clear();
                selectControl.CopyReadSocketsTo(selectReadSockets);

                if (connectSockets == null || !selectControl.HasConnectSockets)
                {
                    if (selectReadSockets.Count == 0)
                        return;

                    //Console.WriteLine("[DEBUG] Selecting on 0 connect sockets");
                    Socket.Select(selectReadSockets, null, null, Int32.MaxValue);
                }
                else
                {
                    connectSockets.Clear();
                    errorSockets.Clear();
                    selectControl.CopyConnectSocketsTo(connectSockets);
                    selectControl.CopyConnectSocketsTo(errorSockets);

                    if (selectReadSockets.Count == 0 && connectSockets.Count == 0)
                        return;

                    //Console.WriteLine("[DEBUG] Selecting on {0} connect sockets", connectSockets.Count);
                    Socket.Select(selectReadSockets, connectSockets, errorSockets, Int32.MaxValue);

                    selectControl.ConnectionError = true;
                    for (int i = 0; i < errorSockets.Count; i++)
                    {
                        Socket socket = errorSockets[i];
                        SimpleSelectHandler handler = selectControl.TryGetHandler(socket);
                        if (handler != null)
                        {
                            handler(ref selectControl, socket, safeBuffer);
                        }
                    }
                    selectControl.ConnectionError = false;
                    for (int i = 0; i < connectSockets.Count; i++)
                    {
                        Socket socket = connectSockets[i];
                        // Make sure you don't call the handler twice
                        if (!errorSockets.Contains(socket))
                        {
                            SimpleSelectHandler handler = selectControl.TryGetHandler(socket);
                            if (handler != null)
                            {
                                handler(ref selectControl, socket, safeBuffer);
                            }
                        }
                    }
                }

                for (int i = 0; i < selectReadSockets.Count; i++)
                {
                    Socket socket = selectReadSockets[i];

                    SimpleSelectHandler handler = selectControl.TryGetHandler(socket);
                    if (handler != null)
                    {
                        handler(ref selectControl, socket, safeBuffer);
                    }
                }
            }
        }
    }

}
