using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using More;
using More.Net;

namespace UdpCat
{
    //
    // Note: Maybe in the future it would be nice to have an option for single threaded using select or multi threaded
    //
    public class UdpCatOptions : CLParser
    {
        public readonly CLSwitch listenMode;

        public readonly CLGenericArgument<UInt16> localPort;
        public readonly CLStringArgument localHost;

        public readonly CLInt32Argument bufferSizes;

        public readonly CLEnumArgument<ThreadModel> threadModel;

        public UdpCatOptions()
            : base()
        {
            listenMode = new CLSwitch('l', "listen", "Specifies that UdpCat should listen for UDP packets");
            Add(listenMode);

            localPort = new CLGenericArgument<UInt16>(UInt16.Parse, 'p', "the local port to bind to");
            localPort.SetDefault(0);
            Add(localPort);

            localHost = new CLStringArgument('i', "the local host or ip address to bind to");
            localHost.SetDefault("0.0.0.0");
            Add(localHost);
        }

        public override void PrintUsageHeader()
        {
            Console.WriteLine("UdpCat.exe [-options] [<host-connector> <port>]");
        }
    }
    public class UdpCatProgram
    {
        static Socket udpSocket;

        static void ListenLoop()
        {
            Byte[] buffer = new Byte[1024];
            EndPoint from = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                Int32 bytesRead;
                try
                {
                    bytesRead = udpSocket.ReceiveFrom(buffer, ref from);
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        Console.WriteLine("[DEBUG] Got ConnectionReset (so the last UDP packet sent was likely not accepted)");
                        continue;
                    }
                    throw;
                }
                Console.WriteLine("[{0}] '{1}'", from.ToString(), Encoding.ASCII.GetString(buffer, 0, bytesRead));
            }
        }

        static Int32 Main(String[] args)
        {
            UdpCatOptions options = new UdpCatOptions();

            List<String> nonOptionArgs = options.Parse(args);

            //
            // Create the UDP Socket
            //
            IPEndPoint listenEndPoint = new IPEndPoint(EndPoints.ParseIPOrResolveHost(options.localHost.ArgValue),
                options.localPort.ArgValue);
             udpSocket = new Socket(listenEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.Bind(listenEndPoint);


            Boolean noConsoleLoop;
            if (nonOptionArgs.Count == 0)
            {
                noConsoleLoop = true;
            }
            else if (nonOptionArgs.Count == 2)
            {
                noConsoleLoop = false;
            }
            else
            {
                return options.ErrorAndUsage("Expected 0 or 2 arguments but got {0}", nonOptionArgs.Count);
            }

            if (options.listenMode.set)
            {
                if (!options.localPort.set)
                {
                    Console.WriteLine("Cannot use option '-l' without setting the local port with '-p <port>'");
                    return 1;
                }
                if (noConsoleLoop) 
                {
                    ListenLoop();
                    return 0;
                }
                else
                {
                    new Thread(ListenLoop).Start();
                }
            }
            
            String hostConnectorString = nonOptionArgs[0];

            String portString = nonOptionArgs[1];
            UInt16 port = UInt16.Parse(portString);

            ISocketConnector connector;
            String hostString = ConnectorParser.ParseConnector(hostConnectorString, out connector);
            EndPoint serverEndPoint = EndPoints.EndPointFromIPOrHost(hostString, port);

            if (connector != null)
            {
                connector.Connect(udpSocket, serverEndPoint);
            }

            //
            // Get Packets
            //
            while (true)
            {
                String line = Console.ReadLine();
                if (line == null) break;

                line = '"' + line.Replace("\"", "\\\"") + '"';

                Object deserialized;
                Sos.Deserialize(out deserialized, typeof(String), line, 0, line.Length);

                Byte[] packet = Encoding.ASCII.GetBytes((String)deserialized);

                if (connector == null)
                {
                    udpSocket.SendTo(packet, serverEndPoint);
                }
                else
                {
                    udpSocket.Send(packet);
                }
            }


            return 0;
        }
    }
}
