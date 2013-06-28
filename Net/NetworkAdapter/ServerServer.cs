using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

using More;

namespace More.Net
{
    public class ServerServer
    {
        readonly SelectTunnelsThread tunnelsThread;

        private readonly TunnelList tunnelList;
        public readonly Int32 socketBackLog;
        public readonly Int32 readBufferSize;
        readonly Boolean logData;
        readonly Boolean logTransferMessages;

        private ServerServerListenThread[] threads;
        private readonly List<IncomingConnection> connectionList;

        public ServerServer(TunnelList tunnelSet, Int32 socketBackLog, Int32 readBufferSize, Boolean logData, 
            Boolean logTransferMessages)
        {
            if (tunnelSet == null) throw new ArgumentNullException("tunnelSet");

            this.tunnelsThread = new SelectTunnelsThread(2048);

            this.tunnelList = tunnelSet;
            this.socketBackLog = socketBackLog;
            this.readBufferSize = readBufferSize;
            this.logData = logData;
            this.logTransferMessages = logTransferMessages;

            this.threads = null;
            this.connectionList = new List<IncomingConnection>();
        }

        public void Start()
        {
            if (threads != null) throw new InvalidOperationException("There are already threads running");

            PortSet fullPortSet = tunnelList.fullPortSet;

            threads = new ServerServerListenThread[fullPortSet.Length];
            for (Int32 i = 0; i < threads.Length; i++)
            {
                ServerServerListenThread thread = new ServerServerListenThread(this, fullPortSet[i]);
                threads[i] = thread;
                thread.Start();
            }
        }

        public void Interrupt()
        {
            if (threads != null)
            {
                for (Int32 i = 0; i < threads.Length; i++)
                {
                    threads[i].Interrupt();
                }
                threads = null;
            }
        }

        private void NewClient(IncomingConnection newConnection)
        {
            Console.WriteLine("[New Client {0}, Finding a connection match]", newConnection.endPointName);

            IPortTunnel tunnel = null;
            IncomingConnection matchedConnection = null;

            lock (connectionList)
            {
                if (connectionList.Count > 0)
                {
                    for (int i = 0; i < connectionList.Count; i++)
                    {
                        IncomingConnection queuedConnection = connectionList[i];
                        tunnel = tunnelList.IsATunnel(newConnection.localPort, queuedConnection.localPort);
                        if (tunnel != null)
                        {
                            matchedConnection = queuedConnection;
                            connectionList.RemoveAt(i);
                            break;
                        }
                    }
                }

                if (matchedConnection == null)
                {
                    Console.WriteLine("[Client {0} has no matching connection, adding to queue]", newConnection.endPointName);
                    connectionList.Add(newConnection);
                    Console.WriteLine("[Queue ({0}): {1}]", connectionList.Count, connectionList.ToDataString());
                    return;
                }
            }            

            Console.WriteLine("[Found Tunnel '{0}' that matches New Client {1} and Queued Client {2}]",
                tunnel, newConnection, matchedConnection);
            
            /*
            ConnectionMessageLogger messageLogger = new ConnectionMessageLoggerSingleLog(
                new ConsoleMessageLogger("Tunnel"), String.Format("{0} to {1}", newConnection.endPointName, matchedConnection.endPointName),
                String.Format("{0} to {1}", matchedConnection.endPointName, newConnection.endPointName));

            IConnectionDataLogger dataLogger = (logData) ? new ConnectionDataLoggerSingleLog(ConsoleDataLogger.Instance,
                newConnection.endPointName, matchedConnection.endPointName) :
                ConnectionDataLogger.Null;

            TwoWaySocketTunnel socketTunnel = new TwoWaySocketTunnel(
                newConnection.socket, matchedConnection.socket, readBufferSize, messageLogger, dataLogger);
            new Thread(socketTunnel.StartOneAndRunOne).Start();
            */
            tunnelsThread.Add(newConnection.socket, matchedConnection.socket);

        }

        private class ServerServerListenThread
        {
            private readonly ServerServer server;
            private readonly UInt16 listenPort;

            private Boolean keepRunning;
            private Thread listenThread;
            private Socket listenSocket;

            public ServerServerListenThread(ServerServer server, UInt16 listenPort)
            {
                this.server = server;
                this.listenPort = listenPort;

                this.keepRunning = false;
                this.listenThread = null;
                this.listenThread = null;
            }

            public void Start()
            {
                if (listenThread == null)
                {
                    listenThread = new Thread(Run);
                    listenThread.IsBackground = false;
                    listenThread.Name = String.Format("Port {0} Listener", listenPort);
                    keepRunning = true;
                    listenThread.Start();
                }
                else
                {
                    throw new InvalidOperationException("Listen Thread Already Started");
                }
            }

            public void Interrupt()
            {
                this.keepRunning = false;
                Socket listenSocketCache = this.listenSocket;
                Thread listenThreadCache = this.listenThread;

                if (listenSocketCache != null)
                {
                    listenSocketCache.Close();
                    this.listenSocket = null;
                }
                if (listenThreadCache != null)
                {
                    listenThreadCache.Join();
                    this.listenThread = null;
                }
            }

            private void Run()
            {
                UInt32 acceptCount = 0;

                this.listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(new IPEndPoint(IPAddress.Any, listenPort));
                listenSocket.Listen(server.socketBackLog);
                
                try
                {
                    while (keepRunning)
                    {
                        MessageLogger nextLogger = new ConsoleMessageLogger(String.Format("Port {0} Handler {1}", listenPort, acceptCount.ToString()));

                        nextLogger.Log("Listening");
                        Socket newClientSocket = listenSocket.Accept();

                        nextLogger.Log("Accepted {0}", newClientSocket.RemoteEndPoint);
                        server.NewClient(new IncomingConnection(newClientSocket, listenPort));

                        acceptCount++;
                    }
                }
                catch (SocketException se)
                {
                    if (keepRunning)
                    {
                        throw se;
                    }
                }
            }
        }

    }

}
