using System;
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
            Console.WriteLine("Outbound Connection: NetCat.exe [-options] connector");
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
                Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(new IPEndPoint(EndPoints.ParseIPOrResolveHost(optionsParser.localHost.ArgValue),
                    optionsParser.localPort.ArgValue));

                listenSocket.Listen(1);
                connectedSocket = listenSocket.Accept();
            }
            else if (nonOptionArgs.Count != 1)
            {
                Console.WriteLine("In client/connect mode there should be 1 non-option command line argument but got {0}", nonOptionArgs.Count);
                optionsParser.PrintUsage();
                return -1;
            }
            else
            {
                String serverConnectorString = nonOptionArgs[0];
                ISocketConnector connector;
                EndPoint serverEndPoint = ConnectorParser.Parse(serverConnectorString, -1, out connector);

                connectedSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if(optionsParser.localPort.set)
                {
                    connectedSocket.Bind(new IPEndPoint(IPAddress.Any, optionsParser.localPort.ArgValue));
                }

                if (connector == null)
                {
                    connectedSocket.Connect(serverEndPoint);
                }
                else
                {
                    connector.Connect(connectedSocket, serverEndPoint);
                }
            }

            TextReaderToSocket consoleReader = new TextReaderToSocket(null, Console.In, connectedSocket, optionsParser.bufferSizes.ArgValue);
            SocketToTextWriter socketReader = new SocketToTextWriter(null, connectedSocket, Console.Out, optionsParser.bufferSizes.ArgValue);

            //
            // The Console Reader Thread is set to a background thread because it will never close
            // so it should not keep the program from exiting...the socket reader will determine that
            //
            Thread consoleReaderThread = new Thread(consoleReader.Run);
            consoleReaderThread.IsBackground = true;
            consoleReaderThread.Start();

            socketReader.Run();

            return 0;
        }
    }
}
