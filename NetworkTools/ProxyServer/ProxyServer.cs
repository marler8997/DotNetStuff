using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace Marler.NetworkTools
{
    public class ProxyServer
    {
        public const String Name = "MarlerProxyServer/1.0.*";

        public readonly UInt16 listenPort;
        public readonly Int32 socketBackLog;

        private Boolean keepRunning;
        private Socket listenSocket;
        private Thread listenThread;

        public ProxyServer(UInt16 listenPort, Int32 socketBackLog)
        {
            this.listenPort = listenPort;
            this.socketBackLog = socketBackLog;

            this.keepRunning = false;
            this.listenSocket = null;
            this.listenThread = null;
        }

        public void Start()
        {
            if (listenThread == null)
            {
                this.listenThread = new Thread(Run);
                this.listenThread.Name = String.Format("{0} Listen Thread", Name);
                this.keepRunning = true;
                this.listenThread.Start();
            }
            else
            {
                throw new InvalidOperationException("Listen Thread Already Started");
            }
        }

        public void Interrupt()
        {
            Socket listenSocketCache = this.listenSocket;
            Thread listenThreadCache = this.listenThread;
            this.keepRunning = false;

            if (listenSocket != null) listenSocket.Close();
            if (listenThreadCache != null) listenThreadCache.Join();
        }

        private void Run()
        {
            Console.WriteLine("Starting Proxy Server {0}", this);

            UInt32 acceptCount = 0;
            
            this.listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Any, listenPort));      
            listenSocket.Listen(socketBackLog);

            while (keepRunning)
            {
                try
                {
                    MessageLogger nextLogger = new ConsoleMessageLogger(String.Format("Handler {0}", acceptCount.ToString()));

                    nextLogger.Log("Listening");
                    Socket newClientSocket = listenSocket.Accept();

                    nextLogger.Log("Accepted {0}", newClientSocket.RemoteEndPoint.GetString());

                    ProxyHandler proxyHandler = new ProxyHandler(nextLogger, new NetworkStream(newClientSocket));

                    Thread handlerThread = new Thread(new ThreadStart(proxyHandler.Run));
                    handlerThread.Name = String.Format("{0} Thread", nextLogger.name);
                    handlerThread.Start();

                    acceptCount++;
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

        public override string ToString()
        {
            return String.Format("ListenPort: {0} SocketBackLog: {1}", listenPort, socketBackLog);
        }

    }
}
