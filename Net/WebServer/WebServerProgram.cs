using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

using More;

namespace More.Net
{
    public class WebServerOptions : CLParser
    {
        public readonly CLGenericArgument<UInt16> port;
        public readonly CLStringArgument defaultIndexFile;
        public readonly CLInt32Argument socketBackLog;
        public readonly CLStringArgument logFile;
        public readonly CLSwitch logData;

        public WebServerOptions()
            : base()
        {
            port = new CLGenericArgument<UInt16>(UInt16.Parse, 'p', "Listen Port", "The port number to listen on");
            port.SetDefault(80);
            Add(port);

            defaultIndexFile = new CLStringArgument('i', "Default Index File", "Filename of the default file to send when the client requests a directory");
            defaultIndexFile.SetDefault("index.html");
            Add(defaultIndexFile);

            socketBackLog = new CLInt32Argument('b', "Socket Back Log", "The maximum length of the pending connections queue");
            socketBackLog.SetDefault(32);
            Add(socketBackLog);

            logFile = new CLStringArgument("log", "File for message log");
            Add(logFile);

            logData = new CLSwitch('d', "log-data", "Use to turn on data logging");
            Add(logData);
        }

        public override void PrintUsageHeader()
        {
            Console.WriteLine("WebServer [options] root-path");
        }
    }
    public class WebServer
    {
        public const String ServerName = "MoreHTTPServer/1.0.*";

        public static String RootDirectory;
        //public static DefaultUrlToFileTranslator UrlTranslator;
        //static DefaultFileResourceHandler fileHandler;

        public static TextWriter Logger = Console.Out;

        public static void Main(String[] args)
        {
            WebServerOptions optionsParser = new WebServerOptions();
            List<String> nonOptionArgs = optionsParser.Parse(args);

            if (nonOptionArgs.Count == 0)
            {
                Console.WriteLine("Error: missing root-path");
                return;
            }

            if (nonOptionArgs.Count > 1)
            {
                Console.WriteLine("Error: too many arguments");
                return;
            }

            RootDirectory = nonOptionArgs[0];
            if (!Directory.Exists(RootDirectory))
            {
                Console.WriteLine("Error: root directory '{0}' does not exist", RootDirectory);
                return;
            }

            //fileHandler = new DefaultFileResourceHandler(
            //    UrlTranslator, optionsParser.defaultIndexFile.ArgValue);

            /*
            ExtensionFilteredResourceHandler extensionHandler =
                new ExtensionFilteredResourceHandler(defaultFileHandler);

            extensionHandler.AddExtensionHandler("bat", new BatchResourceHandler(
                urlTranslator, TimeSpan.FromSeconds(20)));
            */

            SelectServer server = new SelectServer(false, new Buf(1024, 1024));

            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Any, optionsParser.port.ArgValue));
            listenSocket.Listen(optionsParser.socketBackLog.ArgValue);
            server.control.AddListenSocket(listenSocket, AcceptCallback);

            server.Run();
        }
        static void AcceptCallback(ref SelectControl selectControl, Socket listenSocket, Buf safeBuffer)
        {
            Socket clientSocket = listenSocket.Accept();
            if (clientSocket.Connected)
            {
                String clientLogString = clientSocket.SafeRemoteEndPointString();
                Console.WriteLine("[{0}] NewClient", clientLogString);
                selectControl.AddReceiveSocket(clientSocket,
                    new OpenHttpSocket(clientLogString).HeaderRecvHandler);
            }
            else
            {
                clientSocket.Close();
            }
        }

        public static String HttpResourceToFile(String resource)
        {
            if (!String.IsNullOrEmpty(resource))
            {
                if (resource[0] == '/') resource = resource.Substring(1);
                if (Path.DirectorySeparatorChar != '/')
                {
                    resource = resource.Replace('/', Path.DirectorySeparatorChar);
                }
            }

            return Path.Combine(RootDirectory, resource);
        }
    }
    struct Slice
    {
        public UInt32 offset, limit;
        public String Decode(Byte[] bytes)
        {
            return System.Text.Encoding.ASCII.GetString(bytes, (int)offset, (int)(limit - offset));
        }
    }
    class OpenHttpSocket
    {
        public readonly String clientLogString;
        readonly ByteBuilder builder;
        UInt32 headerContentLength;

        String method, resource;

        public OpenHttpSocket(String clientLogString)
        {
            this.clientLogString = clientLogString;
            this.builder = new ByteBuilder(1024);
        }

        static readonly Byte[] NotFound404 = System.Text.Encoding.ASCII.GetBytes("HTTP/1.1 404 Not Found\r\nConnection: close\r\n\r\n");

        public void HeaderRecvHandler(ref SelectControl selectControl, Socket clientSocket, Buf ignore)
        {
            builder.EnsureTotalCapacity(builder.contentLength + 128);

            UInt32 dataOffset;
            try
            {
                dataOffset = Http.ReadHttpHeaders(clientSocket, builder);
            }
            catch (Exception e)
            {
                if(WebServer.Logger != null)
                    WebServer.Logger.WriteLine("[{0}] Closed: {1}", clientLogString, e.Message);
                selectControl.RemoveReceiveSocket(clientSocket);
                return;
            }

            //
            // Parse the request
            //
            try
            {
                UInt32 parseOffset = 0;

                Slice httpMethod;
                httpMethod.offset = parseOffset;
                httpMethod.limit = builder.bytes.IndexOfUInt32(parseOffset, dataOffset, (Byte)' ');
                if (httpMethod.limit == UInt32.MaxValue)
                {
                    throw new FormatException("Invalid request: no space after HTTP method");
                }
                parseOffset = (uint)httpMethod.limit + 1;

                Slice httpResource;
                httpResource.offset = parseOffset;
                httpResource.limit = builder.bytes.IndexOfUInt32(parseOffset, dataOffset, (Byte)' ');
                if (httpResource.limit == UInt32.MaxValue)
                {
                    throw new FormatException("Invalid request: no space after HTTP resource");
                }
                parseOffset = (uint)httpResource.limit + 1;

                this.method = httpMethod.Decode(builder.bytes);
                this.resource = httpResource.Decode(builder.bytes);
                
                headerContentLength = Http.GetContentLength(builder.bytes, 0, dataOffset);

                if (headerContentLength != UInt32.MaxValue)
                {
                    throw new NotImplementedException(String.Format("Content-Length {0} is not implemented", headerContentLength));
                }
            }
            catch(Exception e)
            {
                if(WebServer.Logger != null)
                    WebServer.Logger.WriteLine("[{0}] InvalidRequest: {1}", clientLogString, e.Message);
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
                selectControl.RemoveReceiveSocket(clientSocket);
                return;
            }

            if(WebServer.Logger != null)
                WebServer.Logger.WriteLine("[{0}] {1} {2}", clientLogString, method, resource);

            if(!method.Equals("GET"))
            {
                if(WebServer.Logger != null)
                    WebServer.Logger.WriteLine("[{0}] Unsupported HTTP Method: {1}", clientLogString, method);
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
                selectControl.RemoveReceiveSocket(clientSocket);
                return;
            }
            String filename = WebServer.HttpResourceToFile(resource);
            if (!File.Exists(filename))
            {
                clientSocket.Send(NotFound404);
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
                selectControl.RemoveReceiveSocket(clientSocket);
                return;
            }


            builder.Clear();
            FileInfo fileInfo = new FileInfo(filename);
            Int64 fileLength = fileInfo.Length;
            builder.AppendAscii("HTTP/1.1 200 OK\r\nContent-Length: ");
            builder.AppendAscii(fileLength.ToString());
            builder.AppendAscii("\r\n\r\n");
            using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                UInt32 bufferLeft = (uint)builder.bytes.Length - builder.contentLength;

                if (fileLength <= bufferLeft)
                {
                    fileStream.ReadFullSize(builder.bytes, (int)builder.contentLength, (int)fileLength);
                    clientSocket.Send(builder.bytes, 0, (int)(builder.contentLength + fileLength), 0);
                }
                else
                {
                    Int64 fileLeft = fileLength;

                    fileStream.ReadFullSize(builder.bytes, (int)builder.contentLength, (int)bufferLeft);
                    clientSocket.Send(builder.bytes);
                    fileLeft -= bufferLeft;

                    while (fileLeft > builder.bytes.Length)
                    {
                        fileStream.ReadFullSize(builder.bytes, 0, builder.bytes.Length);
                        clientSocket.Send(builder.bytes);
                        fileLeft -= builder.bytes.Length;
                    }

                    if (fileLeft > 0)
                    {
                        fileStream.ReadFullSize(builder.bytes, 0, (int)fileLeft);
                        clientSocket.Send(builder.bytes, 0, (int)fileLeft, 0);
                    }
                }

                clientSocket.Close();
                selectControl.RemoveReceiveSocket(clientSocket);
            }
        }


    }
    public class WebServerBleh
    {

        public void Run()
        {

            /*
            UInt32 acceptCount = 0;

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
            */
        }

        /*
        public override string ToString()
        {
            return String.Format("[{0}] ListenPort: {1} SocketBackLog: {2}",
                resourceHandler, listenPort, socketBackLog);
        }
        */
    }
}
