using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Marler.NetworkTools
{
    public class FtpServer
    {
        public readonly String rootPath;
        public readonly UInt16 listenPort;
        public readonly Int32 socketBackLog;

        public FtpServer(String rootPath, UInt16 listenPort, Int32 socketBackLog)
        {
            if (String.IsNullOrEmpty(rootPath)) throw new ArgumentNullException("rootPath");
            if (!Directory.Exists(rootPath)) throw new DirectoryNotFoundException(String.Format("Directory \"{0}\" does not exist", rootPath));

            this.rootPath = rootPath;
            this.listenPort = listenPort;
            this.socketBackLog = socketBackLog;
        }

        public void Run()
        {
            Console.WriteLine("Starting FTP Server {0}", this);

            UInt32 acceptCount = 0;

            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Any, listenPort));
            listenSocket.Listen(socketBackLog);

            while (true)
            {
                MessageLogger nextMessageLogger = new ConsoleMessageLogger(String.Format("Handler {0}", acceptCount.ToString()));

                nextMessageLogger.Log("Listening");
                Socket newClientSocket = listenSocket.Accept();

                nextMessageLogger.Log("Accepted {0}", newClientSocket.RemoteEndPoint.GetString());


                FtpHandler ftpHandler = new FtpHandler(
                    new DictionaryCommandHandler(),
                    new NetworkStream(newClientSocket),
                    nextMessageLogger, 
                    new ConsoleDataLoggerWithLabels(String.Format("[{0} Data]:", nextMessageLogger.name),
                        String.Format("[{0} End of Data]", nextMessageLogger.name)));

                Thread handlerThread = new Thread(new ThreadStart(ftpHandler.Run));
                handlerThread.IsBackground = true;
                handlerThread.Name = String.Format("{0} Thread", nextMessageLogger.name);
                handlerThread.Start();

                acceptCount++;
            }
        }

        public override string ToString()
        {
            return String.Format("ListenPort: {0} SocketBackLog: {1} RootPath: '{2}'",
                listenPort, socketBackLog, rootPath);
        }
    }
}
