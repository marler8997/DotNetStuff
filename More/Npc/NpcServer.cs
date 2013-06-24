using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using More.Net;

namespace More
{
    public class NpcServerMultiThreaded : IDisposable
    {
        public static Int32 Backlog = 32;
        public static Int32 SingleThreadedReceiveBufferLength = 1024;

        readonly INpcServerCallback callback;
        private readonly NpcExecutor npcExecutor;
        public readonly INpcHtmlGenerator htmlGenerator;
        public readonly UInt16 port;

        Socket listenSocket;
        Boolean keepRunning;

        public NpcServerMultiThreaded(INpcServerCallback callback, NpcExecutor npcExecutor, String htmlPageTitle, UInt16 port)
            : this(callback, npcExecutor, new DefaultNpcHtmlGenerator(htmlPageTitle, npcExecutor), port)
        {
        }
        public NpcServerMultiThreaded(INpcServerCallback callback, NpcExecutor npcExecutor, INpcHtmlGenerator htmlGenerator, UInt16 port)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            if (npcExecutor == null) throw new ArgumentNullException("npcReflector");
            if (htmlGenerator == null) throw new ArgumentNullException("htmlGenerator");

            this.callback = callback;
            this.npcExecutor = npcExecutor;
            this.htmlGenerator = htmlGenerator;
            this.port = port;
            this.keepRunning = false;
        }
        public void Dispose()
        {
            this.keepRunning = false;

            Socket socket = this.listenSocket;
            this.listenSocket = null;
            if(socket != null) try { socket.Close(); } catch(Exception) { }
        }
        public void RunPrepare()
        {
            this.keepRunning = true;
        }
        public void Run()
        {
            try
            {
                listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                listenSocket.Listen(Backlog);

                while (keepRunning)
                {
                    Socket clientSocket = listenSocket.Accept();

                    String clientString = clientSocket.RemoteEndPoint.ToString();

                    NpcBlockingThreadHander npcHandler = new NpcBlockingThreadHander(callback, clientSocket, npcExecutor, htmlGenerator);
                    Thread handlerThread = new Thread(npcHandler.Run);
                    handlerThread.Start();
                }
            }
            catch (Exception e)
            {
                if (keepRunning) throw e;
            }
            finally
            {
                keepRunning = false;
                if(listenSocket != null) try { listenSocket.Close(); } catch(Exception) { }
            }
        }
    }
    public class NpcServerSingleThreaded : IDisposable
    {
        public static Int32 Backlog = 32;
        public static Int32 SingleThreadedReceiveBufferLength = 1024;

        internal readonly INpcServerCallback callback;
        internal readonly NpcExecutor npcExecutor;
        internal readonly INpcHtmlGenerator htmlGenerator;
        public readonly UInt16 port;

        TcpSelectServer selectServer;

        public NpcServerSingleThreaded(INpcServerCallback callback, NpcExecutor npcExecutor, String htmlPageTitle,
            UInt16 port)
            : this(callback, npcExecutor, new DefaultNpcHtmlGenerator(htmlPageTitle, npcExecutor), port)
        {
        }
        public NpcServerSingleThreaded(INpcServerCallback callback, NpcExecutor npcExecutor, INpcHtmlGenerator htmlGenerator,
            UInt16 port)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            if (npcExecutor == null) throw new ArgumentNullException("npcExecutor");
            if (htmlGenerator == null) throw new ArgumentNullException("htmlGenerator");

            this.callback = callback;
            this.npcExecutor = npcExecutor;
            this.htmlGenerator = htmlGenerator;
            this.port = port;
        }
        public void Dispose()
        {
            TcpSelectServer selectServer = this.selectServer;
            this.selectServer = null;
            if (selectServer != null)
            {
                selectServer.Dispose();
            }
        }
        public void Run()
        {
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            selectServer = new TcpSelectServer();
            selectServer.PrepareToRun();
            selectServer.Run(null, new IPEndPoint(IPAddress.Any, port), Backlog,
                new Byte[SingleThreadedReceiveBufferLength], new NpcStreamSelectServerCallback(this));
        }
    }
    public class NpcStreamSelectServerCallback : StreamSelectServerCallback
    {
        readonly Dictionary<Socket, NpcDataHandler> clientMap;

        readonly INpcServerCallback callback;
        readonly NpcExecutor npcExecutor;
        readonly INpcHtmlGenerator htmlGenerator;

        public NpcStreamSelectServerCallback(NpcServerSingleThreaded server)
            : this(server.callback, server.npcExecutor, server.htmlGenerator)
        {
        }
        public NpcStreamSelectServerCallback(INpcServerCallback callback, NpcExecutor npcExecutor, INpcHtmlGenerator htmlGenerator)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            if (npcExecutor == null) throw new ArgumentNullException("npcExecutor");
            if (htmlGenerator == null) throw new ArgumentNullException("htmlGenerator");

            this.clientMap = new Dictionary<Socket, NpcDataHandler>();

            this.callback = callback;
            this.npcExecutor = npcExecutor;
            this.htmlGenerator = htmlGenerator;
        }
        public void ServerStopped()
        {
            clientMap.Clear();
        }
        public void ServerListening(Socket listenSocket)
        {
            callback.ServerListening(listenSocket);
        }
        public ServerInstruction ListenSocketClosed(int clientCount)
        {
            throw new InvalidOperationException("The listen socket has closed");
        }
        public ServerInstruction ClientOpenCallback(int clientCount, System.Net.Sockets.Socket socket)
        {
            clientMap[socket] = new NpcDataHandler(callback, socket, npcExecutor, htmlGenerator);
            return ServerInstruction.NoInstruction;
        }
        public ServerInstruction ClientCloseCallback(int clientCount, System.Net.Sockets.Socket socket)
        {
            clientMap[socket] = null;
            return ServerInstruction.NoInstruction;
        }
        public ServerInstruction ClientDataCallback(System.Net.Sockets.Socket socket, byte[] bytes, int bytesRead)
        {
            NpcDataHandler dataHandler;
            if (!clientMap.TryGetValue(socket, out dataHandler))
            {
                Console.WriteLine("Error: socket '{0}' was not found in the client map dictionary", socket.RemoteEndPoint.ToString());
                return ServerInstruction.CloseClient;
            }
            dataHandler.Handle(bytes, bytesRead);
            if (dataHandler.Done || !socket.Connected) return ServerInstruction.CloseClient;
            return ServerInstruction.NoInstruction;
        }
    }
    public class NpcServerLoggerCallback : INpcServerCallback
    {
        private readonly TextWriter log;

        public NpcServerLoggerCallback(TextWriter log)
        {
            this.log = log;
        }
        public void ServerListening(Socket listenSocket)
        {
        }
        public void  FunctionCall(string clientString, String methodName)
        {
            log.WriteLine("[{0}] Called '{1}'", clientString, methodName);
        }
        public void  FunctionCallThrewException(string clientString, string methodName, Exception e)
        {
            log.WriteLine("[{0}] Called '{1}' threw exception '{2}'", clientString, methodName, e.GetType().Name + ": " + e.Message);
        }
        public void  GotInvalidData(string clientString, string message)
        {
            log.WriteLine("[{0}] Invalid Data: {1}", clientString, message);
        }
        public void  ExceptionDuringExecution(string clientString, string methodName, Exception e)
        {
            log.WriteLine("[{0}] Exception During Execution of '{1}': {2}" + Environment.NewLine + e.StackTrace,
                clientString, methodName, e.GetType().Name + ": " + e.Message);
        }
        public void  ExceptionWhileGeneratingHtml(string clientString, Exception e)
        {
            log.WriteLine("[{0}] Exception During Html generation: {1}",
                clientString, e.GetType().Name + ": " + e.Message);
        }
        public void UnhandledException(string clientString, Exception e)
        {
            log.WriteLine("[{0}] Unhandled Exception: {1}" + Environment.NewLine + e.StackTrace,
                clientString, e.GetType().Name + ": " + e.Message);            
        }
    }
    public class NpcServerConsoleLoggerCallback : INpcServerCallback
    {
        public static readonly NpcServerConsoleLoggerCallback Instance = new NpcServerConsoleLoggerCallback();
        private NpcServerConsoleLoggerCallback()
        {
        }
        public void ServerListening(Socket listenSocket)
        {
        }
        public void FunctionCall(string clientString, String methodName)
        {
            Console.Out.WriteLine("[{0}] Called '{1}'", clientString, methodName);
        }
        public void FunctionCallThrewException(string clientString, string methodName, Exception e)
        {
            Console.Out.WriteLine("[{0}] Called '{1}' threw exception '{2}'", clientString, methodName, e.GetType().Name + ": " + e.Message);
        }
        public void GotInvalidData(string clientString, string message)
        {
            Console.Out.WriteLine("[{0}] Invalid Data: {1}", clientString, message);
        }
        public void ExceptionDuringExecution(string clientString, string methodName, Exception e)
        {
            Console.Out.WriteLine("[{0}] Exception During Execution of '{1}': {2}" + Environment.NewLine + e.StackTrace,
                clientString, methodName, e.GetType().Name + ": " + e.Message);
        }
        public void ExceptionWhileGeneratingHtml(string clientString, Exception e)
        {
            Console.Out.WriteLine("[{0}] Exception During Html generation: {1}",
                clientString, e.GetType().Name + ": " + e.Message);
        }
        public void UnhandledException(string clientString, Exception e)
        {
            Console.Out.WriteLine("[{0}] Unhandled Exception: {1}" + Environment.NewLine + e.StackTrace,
                clientString, e.GetType().Name + ": " + e.Message);
        }
    }
}
