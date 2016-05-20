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

                    String clientString = clientSocket.SafeRemoteEndPointString();

                    NpcBlockingThreadHander npcHandler = new NpcBlockingThreadHander(clientString, callback, clientSocket, npcExecutor, htmlGenerator);
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
        public static UInt32 SingleThreadedReceiveBufferLength = 1024;

        public readonly INpcServerCallback callback;
        public readonly NpcExecutor npcExecutor;
        public readonly INpcHtmlGenerator htmlGenerator;

        readonly SelectServer selectServer;

        public NpcServerSingleThreaded(INpcServerCallback callback, NpcExecutor npcExecutor, String htmlPageTitle, UInt16 port)
            : this(callback, npcExecutor, new DefaultNpcHtmlGenerator(htmlPageTitle, npcExecutor), port)
        {
        }
        public NpcServerSingleThreaded(INpcServerCallback callback, NpcExecutor npcExecutor, INpcHtmlGenerator htmlGenerator, UInt16 port)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            if (npcExecutor == null) throw new ArgumentNullException("npcExecutor");
            if (htmlGenerator == null) throw new ArgumentNullException("htmlGenerator");

            this.callback = callback;
            this.npcExecutor = npcExecutor;
            this.htmlGenerator = htmlGenerator;

            Socket listenSocket = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            listenSocket.Listen(Backlog);
            callback.ServerListening(listenSocket);

            selectServer = new SelectServer(false, new Buf(SingleThreadedReceiveBufferLength));
            selectServer.control.AddListenSocket(listenSocket, AcceptCallback);
        }
        public NpcServerSingleThreaded(INpcServerCallback callback, NpcExecutor npcExecutor, INpcHtmlGenerator htmlGenerator, Socket listenSocket)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            if (npcExecutor == null) throw new ArgumentNullException("npcExecutor");
            if (htmlGenerator == null) throw new ArgumentNullException("htmlGenerator");

            this.callback = callback;
            this.npcExecutor = npcExecutor;
            this.htmlGenerator = htmlGenerator;

            selectServer = new SelectServer(false, new Buf(SingleThreadedReceiveBufferLength));
            selectServer.control.AddListenSocket(listenSocket, AcceptCallback);
        }
        public void Dispose()
        {
            selectServer.control.Dispose();
        }
        public ThreadStart Run { get { return selectServer.Run; } }

        public void AcceptCallback(ref SelectControl selectControl, Socket listenSocket, Buf safeBuffer)
        {
            Socket clientSocket = listenSocket.Accept();
            if (clientSocket.Connected)
            {
                String clientLogString = clientSocket.SafeRemoteEndPointString();
                
                var dataHandler = new NpcSocketHandler(clientLogString, callback, npcExecutor, htmlGenerator);
                selectControl.AddReceiveSocket(clientSocket, dataHandler.InitialRecvHandler);
            }
            else
            {
                clientSocket.Close();
            }
        }
    }
    public class NpcSelectServerHandler
    {
        public readonly INpcServerCallback callback;
        public readonly NpcExecutor npcExecutor;
        public readonly INpcHtmlGenerator htmlGenerator;

        public NpcSelectServerHandler(INpcServerCallback callback, NpcExecutor npcExecutor, INpcHtmlGenerator htmlGenerator)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            if (npcExecutor == null) throw new ArgumentNullException("npcExecutor");
            if (htmlGenerator == null) throw new ArgumentNullException("htmlGenerator");

            this.callback = callback;
            this.npcExecutor = npcExecutor;
            this.htmlGenerator = htmlGenerator;
        }
        public void AcceptCallback(ref SelectControl selectControl, Socket listenSocket, Buf safeBuffer)
        {
            Socket clientSocket = listenSocket.Accept();
            if (clientSocket.Connected)
            {
                String clientLogString = clientSocket.SafeRemoteEndPointString();

                var dataHandler = new NpcSocketHandler(clientLogString, callback, npcExecutor, htmlGenerator);
                selectControl.AddReceiveSocket(clientSocket, dataHandler.InitialRecvHandler);
            }
            else
            {
                clientSocket.Close();
            }
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
