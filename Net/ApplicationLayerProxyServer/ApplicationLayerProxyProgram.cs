using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;

namespace Marler.Net
{
    class ApplicationLayerProxyProgram
    {
        static void Main(string[] args)
        {
            ApplicationLayerProxyLogger.logger = Console.Out;
            ApplicationLayerProxyLogger.errorLogger = Console.Out;

            //
            // Options
            //
            Int32 backlog = 32;

            Int32 listenPort = 8080;
            IPAddress listenIP = null;

            Byte[] readBytes = new Byte[4096];

            HttpSettings.proxySelector = new SingleConnectorSelector(
                new GatewayProxy(new DnsEndPoint("proxy.houston.hp.com", 8080, true)));

            IPEndPoint listenEndPoint = (listenIP == null) ?
                new IPEndPoint(IPAddress.Parse("0.0.0.0"), listenPort) :
                new IPEndPoint(listenIP, listenPort);

            TcpSelectServer selectServer = new TcpSelectServer();

            ApplicationLayerSelectHandler selectServerHandler =
                new ApplicationLayerSelectHandler(selectServer);

            selectServer.PrepareToRun();
            selectServer.Run(null, listenEndPoint, backlog, readBytes, selectServerHandler);
        }
    }
    internal static class HttpSettings
    {
        public static readonly Byte[] ConnectAsBytes = Encoding.UTF8.GetBytes("CONNECT");
        public static readonly Byte[] ConnectionEstablishedAsBytes = Encoding.UTF8.GetBytes(
            "HTTP/1.1 200 Connection established\r\n\r\n");

        public static IProxySelector proxySelector = null;
    }
    public static class ApplicationLayerProxyLogger
    {
        public static TextWriter logger;
        public static TextWriter errorLogger;
    }
}
