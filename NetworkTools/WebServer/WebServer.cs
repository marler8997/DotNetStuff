using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Marler.NetworkTools
{
    public class WebServer
    {
        public const String Name = "MarlerHTTPServer/1.0.*";

        public readonly IResourceHandler resourceHandler;
        public readonly UInt16 listenPort;
        public readonly Int32 socketBackLog;

        public WebServer(IResourceHandler resourceHandler, UInt16 listenPort, Int32 socketBackLog)
        {
            if (resourceHandler == null) throw new ArgumentNullException("resourceHandler");

            this.resourceHandler = resourceHandler;
            this.listenPort = listenPort;
            this.socketBackLog = socketBackLog;
        }

        public void Run()
        {
            Console.WriteLine("Starting HTTP Server {0}", this);

            UInt32 acceptCount = 0;

            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Any, listenPort));
            listenSocket.Listen(socketBackLog);

            while (true)
            {
                MessageLogger nextMessageLogger = new ConsoleMessageLogger(String.Format("Handler {0}", acceptCount.ToString()));

                nextMessageLogger.Log("Listening");
                Socket newClientSocket = listenSocket.Accept();

                nextMessageLogger.Log("Accepted {0}", newClientSocket.RemoteEndPoint);

                HttpRequestHandler requestHandler = new HttpRequestHandler(
                    resourceHandler,
                    new NetworkStream(newClientSocket),
                    nextMessageLogger,
                    new ConnectionDataLoggerPrettyLog(acceptCount, ConsoleDataLogger.Instance,
                        newClientSocket.RemoteEndPoint.ToString(), "localhost"));

                Thread handlerThread = new Thread(new ThreadStart(requestHandler.Run));
                handlerThread.IsBackground = true;
                handlerThread.Name = String.Format("{0} Thread", nextMessageLogger.name);
                handlerThread.Start();

                acceptCount++;
            }
        }

        public override string ToString()
        {
            return String.Format("[{0}] ListenPort: {1} SocketBackLog: {2}",
                resourceHandler, listenPort, socketBackLog);
        }
    }
}
