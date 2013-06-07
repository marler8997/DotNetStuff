using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace More.Net
{
    public interface Handler
    {
        void Run();
    }
    public delegate Handler HandlerConstructor(MessageLogger logger, NetworkStream stream);

    public class ServerHandler
    {
        public readonly UInt16 listenPort;
        public readonly Int32 socketBackLog;
        public readonly HandlerConstructor handlerConstructor;

        private Boolean keepRunning;
        private Socket listenSocket;
        private Thread listenThread;

        public ServerHandler(UInt16 listenPort, Int32 socketBackLog, HandlerConstructor handlerConstructor)
        {
            this.listenPort = listenPort;
            this.socketBackLog = socketBackLog;
            this.handlerConstructor = handlerConstructor;

            this.keepRunning = false;
            this.listenSocket = null;
            this.listenThread = null;
        }

        public void Start()
        {
            if (listenThread != null) throw new InvalidOperationException("Listen Thread Already Started");

            this.listenThread = new Thread(Run);
            this.listenThread.Name = "Server Handler Listen Thread";
            this.keepRunning = true;
            this.listenThread.Start();
        }

        public void Interrupt()
        {
            Socket listenSocketCache = this.listenSocket;
            Thread listenThreadCache = this.listenThread;
            this.keepRunning = false;

            if (listenSocketCache != null) listenSocketCache.Close();
            if (listenThreadCache != null) listenThreadCache.Join();
        }

        private void Run()
        {
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

                    nextLogger.Log("Accepted {0}", newClientSocket.RemoteEndPoint);

                    Handler handler = handlerConstructor(nextLogger, new NetworkStream(newClientSocket));

                    Thread handlerThread = new Thread(new ThreadStart(handler.Run));
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
            return String.Format("Server Handler ListenPort: {0} SocketBackLog: {1}",
                listenPort, socketBackLog);
        }
    }
}
