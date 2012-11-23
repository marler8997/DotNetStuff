using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace Marler.NetworkTools
{
    public class NetCatProgram
    {
        static Int32 Main(string[] args)
        {
            NetCatOptions optionsParser = new NetCatOptions();

            if (args.Length <= 0)
            {
                optionsParser.PrintUsage();
                return -1;
            }           
            
            List<String> nonOptionArgs = optionsParser.Parse(args);



            Socket client;

            if (optionsParser.listenPort.isSet)
            {
                if (optionsParser.listenProxy.isSet)
                {
                    IDirectSocketConnector proxyConnector = ParseUtilities.ParseDirectHost(optionsParser.listenProxy.ArgValue);
                    Proxy4ConnectSocket proxySocket = new Proxy4ConnectSocket(null, proxyConnector);
                    if (optionsParser.verbose.isSet) Console.WriteLine("INFO: Waiting For Connection...");
                    client = proxySocket.ListenAndAccept(optionsParser.listenPort.ArgValue);
                    if (optionsParser.verbose.isSet) Console.WriteLine("INFO: Accepted {0}", client.LocalEndPoint);
                }
                else
                {
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Bind(new IPEndPoint(IPAddress.Any, optionsParser.listenPort.ArgValue));
                    socket.Listen(1);

                    if (optionsParser.verbose.isSet) Console.WriteLine("INFO: Waiting For Connection...");
                    client = socket.Accept();
                    if (optionsParser.verbose.isSet) Console.WriteLine("INFO: Accepted {0}", client.LocalEndPoint);
                }
            }
            else
            {
                if (nonOptionArgs.Count < 1)
                {
                    Console.WriteLine("Missing command line arguments: host");
                    optionsParser.PrintUsage();
                    return -1;
                }

                ISocketConnector connector = ParseUtilities.ParseConnectionSpecifier(nonOptionArgs[0]);
                client = connector.Connect();
            }



            StreamSocketTunnelCallback callback = new StreamSocketTunnelCallback(client, Console.In, Console.Out);
            SocketToTextWriter a = new SocketToTextWriter(callback, client, Console.Out, 1024);
            TextReaderToSocket b = new TextReaderToSocket(callback, Console.In, client, 1024);

            Thread threadA = new Thread(a.Run);
            Thread threadB = new Thread(b.Run);
            threadA.IsBackground = true;
            threadB.IsBackground = true;
            threadA.Start();
            threadB.Start();

            callback.BlockTillClosed();



            return 0;
        }
    }

}
