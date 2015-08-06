using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

using More;

namespace More.Net
{
    class AppLayerProxyOptions : CLParser
    {
        public readonly CLGenericArgument<IPAddress> listenIP;
        public readonly CLStringArgument forwardProxy;
        public readonly CLGenericArgument<UInt16> backlog;
        public readonly CLSwitch help;
        public AppLayerProxyOptions()
        {
            listenIP = new CLGenericArgument<IPAddress>(IPAddress.Parse, 'l', "listenip", "IP address of the interface to listen on");
            listenIP.SetDefault(IPAddress.Any);
            Add(listenIP);

            forwardProxy = new CLStringArgument('p', "proxy", "Set a proxy to forward all connections to");
            Add(forwardProxy);

            backlog = new CLGenericArgument<UInt16>(UInt16.Parse, 'b', "backlog", "Listen socket backlog");
            backlog.SetDefault(32);
            Add(backlog);

            help = new CLSwitch('h', "help", "Show the usage");
            Add(help);
        }
        public override void PrintUsageHeader()
        {
            Console.WriteLine("ApplicationLayerProxyServer <port>");
        }
    }
    class ApplicationLayerProxyProgram
    {
        static UInt16 ListenPort;
        static void Main(string[] args)
        {
            AppLayerProxy.Logger = Console.Out;
            AppLayerProxy.ErrorLogger = Console.Out;

            var options = new AppLayerProxyOptions();
            var nonOptionArgs = options.Parse(args);
            if (nonOptionArgs.Count == 0 || options.help.set)
            {
                options.PrintUsage();
                return;
            }
            if (nonOptionArgs.Count > 1)
            {
                options.ErrorAndUsage("Expected 1 argument but got {0}", args.Length);
                return;
            }

            if (options.forwardProxy.set)
            {
                AppLayerProxy.ForwardProxy = ConnectorParser.ParseProxy(AddressFamily.InterNetwork,
                    options.forwardProxy.ArgValue);
            }

            ListenPort = UInt16.Parse(nonOptionArgs[0]);
            var listenIP = options.listenIP.ArgValue;

            Socket listenSocket = new Socket(listenIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(listenIP, ListenPort));
            listenSocket.Listen(options.backlog.ArgValue);

            SelectControl selectControl = new SelectControl(true);
            selectControl.AddListenSocket(listenSocket, AcceptProxyClient);

            SelectServer selectServer = new SelectServer(selectControl, new Buf(4096));
            selectServer.Run();
        }
        public static void AcceptProxyClient(ref SelectControl selectControl, Socket listenSocket, Buf safeBuffer)
        {
            Socket clientSocket = listenSocket.Accept();
            if (clientSocket.Connected)
            {
                String clientLogString = clientSocket.SafeRemoteEndPointString();

                if (AppLayerProxy.Logger != null)
                    AppLayerProxy.Logger.WriteLine("Listener:{0} new client {1}", ListenPort, clientLogString);

                ConnectionInitiator connectionInitiator = new ConnectionInitiator(clientSocket, clientLogString);
                selectControl.AddReceiveSocket(clientSocket, connectionInitiator.InitialReceiveHandler);
            }
            else
            {
                if (AppLayerProxy.Logger != null)
                    AppLayerProxy.Logger.WriteLine("Listener:{0} new client was accepted but was not connected", ListenPort);

                clientSocket.Close();
            }
        }
    }
    public static class HttpVars
    {
        public static readonly Byte[] ConnectionEstablishedAsBytes = Encoding.UTF8.GetBytes(
            "HTTP/1.1 200 Connection established\r\n\r\n");

        public static readonly Byte[] ConnectUtf8 = new Byte[] { (Byte)'C', (Byte)'O', (Byte)'N', (Byte)'N', (Byte)'E', (Byte)'C', (Byte)'T' };

        public static readonly Byte[] HttpPrefix = new Byte[] { (Byte)'h', (Byte)'t', (Byte)'t', (Byte)'p', (Byte)':', (Byte)'/', (Byte)'/' };
        public static readonly Byte[] HttpsPrefix = new Byte[] { (Byte)'h', (Byte)'t', (Byte)'t', (Byte)'p', (Byte)'s', (Byte)':', (Byte)'/', (Byte)'/' };

        public static readonly Byte[] HostHeaderUtf8 = new Byte[] { (Byte)'\n', (Byte)'H', (Byte)'o', (Byte)'s', (Byte)'t', (Byte)':', (Byte)' ' };
        public static readonly Byte[] ProxyConnectionHeaderUtf8 = new Byte[] {
            (Byte)'\n', (Byte)'P', (Byte)'r', (Byte)'o', (Byte)'x', (Byte)'y', (Byte)'-',
            (Byte)'C', (Byte)'o', (Byte)'n', (Byte)'n', (Byte)'e', (Byte)'c', (Byte)'t', (Byte)'i', (Byte)'o', (Byte)'n', (Byte)':', (Byte)' ' };
    }
    public static class AppLayerProxy
    {
        public static TextWriter Logger;
        public static TextWriter ErrorLogger;

        // Forward all connections to another proxy
        public static Proxy ForwardProxy = default(Proxy);
    }
}
