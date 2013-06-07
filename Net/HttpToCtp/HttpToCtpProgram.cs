using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

using More;

namespace More.Net
{
    //
    // This program will serve like an HTTP proxy, but (when the server supports it)
    // it will actually be using CTP to make requests to web servers.
    //
    // The program will not only be listening for HTTP proxy connections, but
    // will also be listening on a control port for configuration.
    //
    // There, people can connect to it via a browser to add hosts who
    // have CTP servers and view sites that support CTP.
    //
    // However, users will not need to manually add hosts that support CTP because
    // whenever a connection is made to a host that is not added to the CTP list, this program
    // will start requesting http packets but will also start a thread to send a CTP request and listen
    // for a response.  If it gets one, it knows that the server supports CTP and then the host is added
    // to the supports CTP list.
    //
    //
    class HttpToCtpProgram
    {
        static void Main(string[] args)
        {
            HttpToCtpLogger.logger = Console.Out;
            HttpToCtpLogger.errorLogger = Console.Out;

            //
            // Options
            //
            Int32 backlog = 32;

            Int32 listenPort = 8080;
            IPAddress listenIP = null;

            Byte[] readBytes = new Byte[4096];

            HttpToCtp.proxySelector = new SingleConnectorSelector(
                new GatewayProxy(new DnsEndPoint("proxy.houston.hp.com", 8080, true)));


            IPEndPoint listenEndPoint = (listenIP == null) ?
                new IPEndPoint(IPAddress.Parse("0.0.0.0"), listenPort) :
                new IPEndPoint(listenIP, listenPort);


            TcpSelectServer selectServer = new TcpSelectServer();

            HttpToCtpSelectServerHandler selectServerHandler =
                new HttpToCtpSelectServerHandler(selectServer);

            selectServer.PrepareToRun();
            selectServer.Run(null, listenEndPoint, backlog, readBytes, selectServerHandler);
        }
    }

    internal static class HttpToCtp
    {
        public static readonly Byte[] ConnectAsBytes = Encoding.UTF8.GetBytes("CONNECT");
        public static readonly Byte[] ConnectionEstablishedAsBytes = Encoding.UTF8.GetBytes(
            "HTTP/1.1 200 Connection established\r\n\r\n");

        public static IProxySelector proxySelector = null;
    }
    public static class HttpToCtpLogger
    {
        public static TextWriter logger;
        public static TextWriter errorLogger;
    }

}
