using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace Marler.NetworkTools
{
    public delegate void PortSetListenerCallback(UInt32 socketID, UInt16 port, IncomingConnection incomingConnection);

    public class PortSetListener
    {
        private readonly PortSet portSet;
        private readonly PortSetListenerCallback callback;
        public readonly Int32 socketBackLog;

        private Thread[] listenThreads;

        public PortSetListener(PortSet portSet, PortSetListenerCallback callback, Int32 socketBackLog)
        {
            if (portSet == null) throw new ArgumentNullException("portSet");
            if (callback == null) throw new ArgumentNullException("callback");

            this.portSet = portSet;
            this.callback = callback;
            this.socketBackLog = socketBackLog;

            this.listenThreads = null;
        }
        public void Start()
        {
            if (listenThreads != null) throw new InvalidOperationException("Already Started");

            listenThreads = new Thread[portSet.Length];
            for (Int32 i = 0; i < portSet.Length; i++)
            {
                UInt16 listenPort = portSet[i];

                PortSetListenerThread listenThread = new PortSetListenerThread(
                    new ConsoleMessageLogger(String.Format("Port {0}", listenPort)),
                    callback, listenPort, socketBackLog);

                Thread newThread = new Thread(listenThread.Run);
                listenThreads[i] = newThread;

                newThread.IsBackground = true;

                newThread.Start();
            }
        }

        private class PortSetListenerThread
        {
            private readonly MessageLogger messageLogger;
            private readonly PortSetListenerCallback callback;
            public readonly UInt16 listenPort;
            private readonly Socket listenSocket;
            public readonly Int32 socketBackLog;

            private Boolean keepRunning;

            public PortSetListenerThread(MessageLogger messageLogger,
                PortSetListenerCallback callback, UInt16 listenPort, Int32 socketBackLog)
            {
                this.messageLogger = messageLogger;
                this.callback = callback;
                this.listenPort = listenPort;
                this.socketBackLog = socketBackLog;
                this.listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.listenSocket.Bind(new IPEndPoint(IPAddress.Any, listenPort));
                this.keepRunning = true;
            }

            public void ExpectToStop()
            {
                this.keepRunning = false;
            }

            public void Run()
            {
                try
                {
                    UInt32 socketID = 0;
                    listenSocket.Listen(socketBackLog);

                    while (keepRunning)
                    {
                        if (messageLogger != null) messageLogger.Log("Socket {0}: Listening", socketID);
                        Socket newClient = listenSocket.Accept();

                        if (messageLogger != null) messageLogger.Log("Socket {0}: Accepted {1}", socketID, newClient.RemoteEndPoint);

                        IncomingConnection incomingConnection = new IncomingConnection(newClient, listenPort);
                        callback(socketID, listenPort, incomingConnection);
                        socketID++;
                    }
                }
                finally
                {
                    if (messageLogger != null) messageLogger.Log("Closing Listen Socket");
                    listenSocket.Close();
                }
            }
        }
    }
}
