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
    /// <summary>
    /// Callback for sockets controlled by a select server.
    /// </summary>
    /// <param name="control">A reference to the select server control data structure.</param>
    /// <param name="socket">The socket that the event occurred on.</param>
    /// <param name="safeBuffer">A safe buffer that can be used by the handler.  However, data is not guaranteed to be kept intact between handler calls.</param>
    public delegate void SelectHandler(ref SelectControl control, Socket socket, Buf safeBuffer);

    /// <summary>
    /// A data structure used to control the select server.
    /// </summary>
    public struct SelectControl
    {
        /// <summary>
        /// Can use to check if your current thread is the same as the select server thread.
        /// Note: DO NOT SET, should only be set by the SelectServer.
        /// </summary>
        public Thread RunThread;

        // Both listenSockets and receiveSockets are read sockets
        readonly List<Socket> listenSockets;
        readonly List<Socket> receiveSockets;

        readonly List<Socket> connectList;

        readonly IDictionary<Socket, SelectHandler> map; // contains mapping from socket to handlers

        public UInt32 TotalSocketCount { get { return (uint)map.Count; } }

        /// <summary>
        /// A handler can use this to check whether or not a connect failed.
        /// NOTE: DO NOT SET, should only be set by the SelectServer.
        /// </summary>
        Boolean connectionError;
        public Boolean ConnectionError { get { return connectionError; } set { connectionError = value; } }

        /// <summary>Indicates that the control accepts connecting sockets.</summary>
        public Boolean SupportsConnect { get { return connectList != null; } }

        /// <summary>DO NOT MODIFY! Only use to access information.</summary>
        public ICollection<Socket> ListenSockets { get { return listenSockets; } }
        /// <summary>DO NOT MODIFY! Only use to access information.</summary>
        public ICollection<Socket> ReceiveSockets { get { return receiveSockets; } }
        /// <summary>DO NOT MODIFY! Only use to access information.</summary>
        public ICollection<Socket> ConnectSockets { get { return connectList; } }

        /// <summary>Returns true if there are currently any connect sockets.
        /// This should only be called if SupportsConnect is true.</summary>
        public Boolean HasConnectSockets
        {
            get
            {
                return connectList.Count > 0;
            }
        }

        public SelectControl(Boolean supportConnectSockets)
        {
            this.RunThread = null;
            this.listenSockets = new List<Socket>();
            this.receiveSockets = new List<Socket>();
            this.map = new Dictionary<Socket, SelectHandler>();
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
        public void PerformAccept(Socket listenSocket, SelectHandler handler)
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
        public SelectHandler TryGetHandler(Socket socket)
        {
            SelectHandler ret;
            return map.TryGetValue(socket, out ret) ? ret : null;
        }
        /// <summary>
        /// Copies all listen and receiver sockets to the given list.
        /// Typically used by the SelectServer.
        /// </summary>
        public void CopyReadSocketsTo(List<Socket> sockets)
        {
            sockets.AddRange(listenSockets);
            sockets.AddRange(receiveSockets);
        }
        
        /// <summary>
        /// Copies all connect sockets to the given list.  Note: this function assumes
        /// that SupportsConnect is true.
        /// Typically used by the SelectServer.
        /// </summary>
        public void CopyConnectSocketsTo(List<Socket> sockets)
        {
            sockets.AddRange(connectList);
        }
        public void UpdateHandler(Socket socket, SelectHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");
            map[socket] = handler;
        }
        public void UpdateConnectorToReceiver(Socket socket, SelectHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");
            connectList.Remove(socket);
            receiveSockets.Add(socket);
            map[socket] = handler;
        }
        public void AddListenSocket(Socket socket, SelectHandler handler)
        {
            if (socket == null || handler == null)
                throw new ArgumentNullException();
            listenSockets.Add(socket);
            map.Add(socket, handler);
        }
        public void AddReceiveSocket(Socket socket, SelectHandler handler)
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
        public void AddConnectSocket(Socket socket, SelectHandler handler)
        {
            if (socket == null || handler == null)
                throw new ArgumentNullException();

            connectList.Add(socket);
            map.Add(socket, handler);
        }
        // Note: does not call shutdown
        public void DisposeAndRemoveReceiveSocket(Socket socket)
        {
            try { socket.Close(); } catch { }
            receiveSockets.Remove(socket);
            map.Remove(socket);
        }
        public void DisposeAndRemoveConnectSocket(Socket socket)
        {
            try { socket.Close(); } catch { }
            connectList.Remove(socket);
            map.Remove(socket);
        }

        public void ShutdownIfConnectedDisposeAndRemoveReceiveSocket(Socket socket)
        {
            try { if(socket.Connected) socket.Shutdown(SocketShutdown.Both); } catch { }
            try { socket.Close(); } catch { }
            receiveSockets.Remove(socket);
            map.Remove(socket);
        }
        public void ShutdownDisposeAndRemoveReceiveSocket(Socket socket)
        {
            try { socket.Shutdown(SocketShutdown.Both); } catch { }
            try { socket.Close(); } catch { }
            receiveSockets.Remove(socket);
            map.Remove(socket);
        }


        /// <summary>
        /// Shutdown and close all sockets.
        /// If this is called from the RunThread of the SelectServer, then
        /// it will also remove all the sockets from the control.
        /// </summary>
        public void Dispose()
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
            if (Thread.CurrentThread == RunThread)
            {
                listenSockets.Clear();
                receiveSockets.Clear();
                map.Clear();
                RunThread = null;
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
        public SelectControl control;
        readonly Buf safeBuffer;

        public SelectServer(Boolean supportConnectSockets, Buf safeBuffer)
        {
            this.control = new SelectControl(supportConnectSockets);
            this.safeBuffer = safeBuffer;
        }
        public void Run()
        {
            control.RunThread = Thread.CurrentThread;

            List<Socket> selectReadSockets = new List<Socket>();
            List<Socket> connectSockets, errorSockets;
            if (control.SupportsConnect)
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
                control.CopyReadSocketsTo(selectReadSockets);

                if (connectSockets == null || !control.HasConnectSockets)
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
                    control.CopyConnectSocketsTo(connectSockets);
                    control.CopyConnectSocketsTo(errorSockets);

                    if (selectReadSockets.Count == 0 && connectSockets.Count == 0)
                        return;

                    //Console.WriteLine("[DEBUG] Selecting on {0} connect sockets", connectSockets.Count);
                    Socket.Select(selectReadSockets, connectSockets, errorSockets, Int32.MaxValue);

                    control.ConnectionError = true;
                    for (int i = 0; i < errorSockets.Count; i++)
                    {
                        Socket socket = errorSockets[i];
                        SelectHandler handler = control.TryGetHandler(socket);
                        if (handler != null)
                        {
                            handler(ref control, socket, safeBuffer);
                        }
                    }
                    control.ConnectionError = false;
                    for (int i = 0; i < connectSockets.Count; i++)
                    {
                        Socket socket = connectSockets[i];
                        // Make sure you don't call the handler twice
                        if (!errorSockets.Contains(socket))
                        {
                            SelectHandler handler = control.TryGetHandler(socket);
                            if (handler != null)
                            {
                                handler(ref control, socket, safeBuffer);
                            }
                        }
                    }
                }

                for (int i = 0; i < selectReadSockets.Count; i++)
                {
                    Socket socket = selectReadSockets[i];

                    SelectHandler handler = control.TryGetHandler(socket);
                    if (handler != null)
                    {
                        handler(ref control, socket, safeBuffer);
                    }
                }
            }
        }
    }

}
