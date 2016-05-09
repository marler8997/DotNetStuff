using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace More.Net
{
    public class NetworkAdapterProgram
    {
        static Int32 Main(string[] args)
        {
            NetworkAdapterOptions optionsParser = new NetworkAdapterOptions();

            List<String> nonOptionArgs = optionsParser.Parse(args);

            if (nonOptionArgs.Count <= 0)
                return optionsParser.ErrorAndUsage("Please specify the adapter type 'client-server' ('cs'), 'client-client' ('cc') or 'server-server' ('ss')");

            AdapterType adapterType = AdapterTypeMethods.ParseAdapterType(nonOptionArgs[0]);
            nonOptionArgs.RemoveAt(0);
            optionsParser.SetAdapterTypeMode(adapterType);

            if (adapterType == AdapterType.ClientServer)
            {
                //
                // ClientConnectWaitMode.OtherEndReceiveConnectRequest:
                //       Means that the server will be accepting connections from 
                //       another instance of NetworkAdapter that has been configured to send connection
                //       requests to the side that is connected to this server.
                //
                //       Other Adapter    |    This Adapter
                //                        |
                //          <----->       |
                //            OR          |      >----->
                //          >----->       |
                //
                //

                //
                // Options
                //
                if (nonOptionArgs.Count < 3) return optionsParser.ErrorAndUsage("Not enough arguments");

                String clientSideConnectorString = nonOptionArgs[0];
                String clientSideWaitModeString = nonOptionArgs[1];
                String listenPortsString = nonOptionArgs[2];

                InternetHost clientSideServerHost;
                {
                    Proxy proxy;
                    String ipOrHostAndPort = Proxy.StripAndParseProxies(clientSideConnectorString, DnsPriority.IPv4ThenIPv6, out proxy);
                    UInt16 port;
                    String ipOrHost = EndPoints.SplitIPOrHostAndPort(ipOrHostAndPort, out port);
                    clientSideServerHost = new InternetHost(ipOrHost, port, DnsPriority.IPv4ThenIPv6, proxy);
                }
                ClientConnectWaitMode clientConnectWaitMode = ClientConnectWaitModeMethods.Parse(clientSideWaitModeString);

                PortSet listenPortSet = ParseUtilities.ParsePortSet(listenPortsString);
                //
                // End of Options
                //

                //
                // Run
                //
                ClientServer clientServer = new ClientServer(clientSideServerHost,
                    clientConnectWaitMode, listenPortSet, optionsParser.socketBackLog.ArgValue,
                    optionsParser.readBufferSize.ArgValue, optionsParser.logData.set);
                clientServer.Start();

                Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                Console.ReadLine();
            }
            else if (adapterType == AdapterType.ClientClient)
            {
                throw new NotImplementedException();

            }
            else if (adapterType == AdapterType.ServerServer)
            {
                //
                // Get Options
                //
                if (nonOptionArgs.Count < 1)
                {
                    return optionsParser.ErrorAndUsage("Please supply at least one tunnel");
                }

                IPortTunnel[] tunnelArray = ParseUtilities.ParseTunnels(nonOptionArgs);

                
                UInt16[] specialPorts = null;
                if (optionsParser.specialPortList.set)
                {
                    specialPorts = ParseUtilities.ParsePorts(optionsParser.specialPortList.ArgValue);
                }

                TunnelList tunnelList = new TunnelList(tunnelArray, specialPorts);
                

                //
                // Run
                //
                ServerServer serverServer = new ServerServer(tunnelList, optionsParser.socketBackLog.ArgValue, 
                    optionsParser.readBufferSize.ArgValue, optionsParser.logData.set, !optionsParser.noTransferMessages.set);
                serverServer.Start();
                Console.ReadLine();
            }
            else
            {
                throw new FormatException(String.Format("Invalid Enum '{0}' ({1})", adapterType, (Int32)adapterType));
            }


            /*
            NetworkAdapter inputAdapter = NetworkAdapter.ParseNetworkAdapter(nonOptionArgs, ref offset);
            NetworkAdapter outputAdapter = NetworkAdapter.ParseNetworkAdapter(nonOptionArgs, ref offset);

            if (offset < nonOptionArgs.Count)
            {
                Console.WriteLine("Only expected {0} arguments but you gave {1}", offset, nonOptionArgs.Count);
                return -1;
            }

            FileStream communicationLogFile = null;
            try
            {
                if (optionsParser.communicationLogFile.isSet)
                {
                    communicationLogFile = new FileStream(optionsParser.communicationLogFile.ArgValue, FileMode.Create);
                }

                if (inputAdapter.isClient)
                {
                    Console.WriteLine("Input Adapter Connecting to '{0}' on port {1}...",
                        inputAdapter.host, inputAdapter.port);
                    INetworkConnector inputConnector = new NetworkConnectorFromHost(inputAdapter.host);
                    NetworkStream inputStream = inputConnector.Connect(inputAdapter.port);

                    if (outputAdapter.isClient)
                    {
                        Console.WriteLine("Output Adapter Connecting to '{0}' on port {1}...",
                            outputAdapter.host, outputAdapter.port);
                        INetworkConnector outputConnector = new NetworkConnectorFromHost(outputAdapter.host);
                        NetworkStream outputStream = outputConnector.Connect(inputAdapter.port);

                        StreamThread inputStreamThread = new StreamThread(communicationLogFile,
                            new Logger("Input"), inputStream, outputStream);
                        StreamThread outputStreamThread = new StreamThread(communicationLogFile,
                            new Logger("Output"), outputStream, inputStream);

                        new Thread(inputStreamThread.Run).Start();
                        new Thread(outputStreamThread.Run).Start();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    if (outputAdapter.isClient)
                    {
                        ClientAcceptor clientAcceptor = new ClientAcceptor(inputAdapter.port, optionsParser.inputSocketBackLog.ArgValue);
                        while (true)
                        {
                            Logger logger;
                            NetworkStream inputStream = clientAcceptor.Accept(out logger);
                            logger.Log("Output Adapter Connecting to '{0}' on port {1}",
                                outputAdapter.host, outputAdapter.port);
                            INetworkConnector outputConnector = new NetworkConnectorFromHost(outputAdapter.host);
                            NetworkStream outputStream = outputConnector.Connect(outputAdapter.port);

                            StreamThread inputStreamThread = new StreamThread(communicationLogFile,
                                new Logger(String.Format("Input {0}", logger.name)), inputStream, outputStream);
                            StreamThread outputStreamThread = new StreamThread(communicationLogFile,
                                new Logger(String.Format("Output {0}", logger.name)), outputStream, inputStream);

                            new Thread(inputStreamThread.Run).Start();
                            new Thread(outputStreamThread.Run).Start();
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                }

            }
            finally
            {
                if (communicationLogFile != null) communicationLogFile.Close();
            }
            */

            return 0;
        }


    }
}
