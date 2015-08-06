using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

using More;

namespace More.Net
{
    //
    // Note: Maybe in the future it would be nice to have an option for single threaded using select or multi threaded
    //
    public class NetCatOptions : CLParser
    {
        public readonly CLSwitch listenMode;

        public readonly CLGenericArgument<UInt16> localPort;
        public readonly CLStringArgument localHost;

        public readonly CLInt32Argument bufferSizes;

        public readonly CLEnumArgument<ThreadModel> threadModel;

        public NetCatOptions()
            : base()
        {
            listenMode = new CLSwitch('l', "listen mode", "Specifies that NetCat will listen for a tcp connection");
            Add(listenMode);

            localPort = new CLGenericArgument<UInt16>(UInt16.Parse, 'p', "the local port to bind to");
            localPort.SetDefault(0);
            Add(localPort);

            localHost = new CLStringArgument('i', "the local host or ip address to bind to");
            localHost.SetDefault("0.0.0.0");
            Add(localHost);

            bufferSizes = new CLInt32Argument('s', "tunnel buffer sizes");
            bufferSizes.SetDefault(2048);
            Add(bufferSizes);

            threadModel = new CLEnumArgument<ThreadModel>('m', "model", "Thread Model");
            threadModel.SetDefault(ThreadModel.SingleThreaded);
            Add(threadModel);
        }

        public override void PrintUsageHeader()
        {
            Console.WriteLine("Outbound Connection: NetCat.exe [-options] <host-connector> <port>");
            Console.WriteLine("InBound Connection : NetCat.exe [-options]");
        }
    }

    public class NetCatProgram
    {
        static Int32 Main(string[] args)
        {
            NetCatOptions optionsParser = new NetCatOptions();

            List<String> nonOptionArgs = optionsParser.Parse(args);

            Socket connectedSocket;

            if (optionsParser.listenMode.set)
            {
                IPAddress localAddress = EndPoints.ParseIPOrResolveHost(
                    AddressFamily.Unspecified, optionsParser.localHost.ArgValue);

                Socket listenSocket = new Socket(localAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(new IPEndPoint(localAddress, optionsParser.localPort.ArgValue));

                listenSocket.Listen(1);
                connectedSocket = listenSocket.Accept();
            }
            else if (nonOptionArgs.Count != 2)
            {
                return optionsParser.ErrorAndUsage("In client/connect mode there should be 2 non-option command line arguments but got {0}", nonOptionArgs.Count);
            }
            else
            {
                String hostConnectorString = nonOptionArgs[0];
                
                String portString = nonOptionArgs[1];
                UInt16 port = UInt16.Parse(portString);

                HostWithOptionalProxy host = ConnectorParser.ParseConnectorWithNoPortAndOptionalProxy(
                    AddressFamily.Unspecified, hostConnectorString, port);

                if (host.proxy == null)
                {
                    var endPoint = host.endPoint;
                    endPoint.ForceIPResolution(AddressFamily.Unspecified);
                    connectedSocket = new Socket(endPoint.parsedOrResolvedIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    if (optionsParser.localPort.set)
                    {
                        connectedSocket.Bind(new IPEndPoint(IPAddress.Any, optionsParser.localPort.ArgValue));
                    }
                    connectedSocket.Connect(endPoint);
                }
                else
                {
                    connectedSocket = new Socket(host.proxy.endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    if (optionsParser.localPort.set)
                    {
                        connectedSocket.Bind(new IPEndPoint(IPAddress.Any, optionsParser.localPort.ArgValue));
                    }
                    connectedSocket.Connect(host.proxy.endPoint);
                    BufStruct bufStruct = new BufStruct();
                    host.proxy.ProxyConnectTcp(connectedSocket, host.endPoint, ProxyConnectOptions.None, ref bufStruct);
                    if (bufStruct.contentLength > 0)
                    {
                        Console.WriteLine(Encoding.UTF8.GetString(bufStruct.buf, 0, (int)bufStruct.contentLength));
                    }
                }
            }

            new NetcatLoop(connectedSocket, optionsParser.bufferSizes.ArgValue);

            return 0;
        }
    }
    public class NetcatLoop : ITunnelCallback
    {
        readonly Socket socket;

        public NetcatLoop(Socket connectedSocket, Int32 bufferSizes)
        {
            this.socket = connectedSocket;

            //
            // Open Console as raw streams
            //
            Stream consoleInputStream = Console.OpenStandardInput();
            Stream consoleOutputStream = Console.OpenStandardOutput();

            StreamToSocket consoleToSocket = new StreamToSocket(this, consoleInputStream, connectedSocket, bufferSizes);
            SocketToStream socketToConsole = new SocketToStream(this, connectedSocket, consoleOutputStream, bufferSizes);

            //
            // The Console Reader Thread is set to a background thread because it will never close
            // so it should not keep the program from exiting...the socket reader will determine that
            //
            Thread consoleReaderThread = new Thread(consoleToSocket.Run);
            consoleReaderThread.IsBackground = true;
            consoleReaderThread.Start();

            socketToConsole.Run();
        }
        public void TunnelClosed(ITunnel tunnel)
        {
            socket.ShutdownAndDispose();
        }
    }
}
