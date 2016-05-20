using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace More.Net
{
    public enum AdapterType { ClientServer, ClientClient, ServerServer };
    public static class AdapterTypeMethods
    {
        public static AdapterType ParseAdapterType(String adapterType)
        {
            if (adapterType.Equals("cs", StringComparison.CurrentCultureIgnoreCase) ||
                adapterType.Equals("client-server", StringComparison.CurrentCultureIgnoreCase))
            {
                return AdapterType.ClientServer;
            }
            if (adapterType.Equals("cc", StringComparison.CurrentCultureIgnoreCase) ||
                adapterType.Equals("client-client", StringComparison.CurrentCultureIgnoreCase))
            {
                return AdapterType.ClientClient;
            }
            if (adapterType.Equals("ss", StringComparison.CurrentCultureIgnoreCase) ||
                adapterType.Equals("server-server", StringComparison.CurrentCultureIgnoreCase))
            {
                return AdapterType.ServerServer;
            }
            throw new FormatException(String.Format("Unrecognized Adapter Type '{0}', expected 'client-server', 'client-client' or 'server-server'", adapterType));
        }
    }
    public enum ClientConnectWaitMode { Immediate, OtherEndToConnect, OtherEndReceiveConnectRequest };
    public static class ClientConnectWaitModeMethods
    {
        public static ClientConnectWaitMode Parse(String waitMode)
        {
            if (waitMode.Equals("i", StringComparison.CurrentCultureIgnoreCase) ||
                waitMode.Equals("immediate", StringComparison.CurrentCultureIgnoreCase))
            {
                return ClientConnectWaitMode.Immediate;
            }
            if (waitMode.Equals("c", StringComparison.CurrentCultureIgnoreCase) ||
                waitMode.Equals("connect", StringComparison.CurrentCultureIgnoreCase))
            {
                return ClientConnectWaitMode.OtherEndToConnect;
            }
            if (waitMode.Equals("w", StringComparison.CurrentCultureIgnoreCase) ||
                waitMode.Equals("wait", StringComparison.CurrentCultureIgnoreCase))
            {
                return ClientConnectWaitMode.OtherEndReceiveConnectRequest;
            }
            throw new FormatException(String.Format("Unrecognized ClientConnectWaitMode '{0}', expected 'immediate', 'connect' or 'wait'", waitMode));
        }
    }
    public class Options : CLParser
    {
        public readonly CLStringArgument pcapLogFile;
        //public readonly CLStringArgument communicationLogFile;
        public readonly CLStringArgument specialPortList;
        public readonly CLUInt32Argument readBufferSize;
        public readonly CLInt32Argument socketBackLog;
        public readonly CLSwitch logData;
        public readonly CLSwitch noTransferMessages;

        private Boolean adapterTypeModeIsSet;
        private AdapterType adapterTypeMode;

        public Options()
            : base()
        {
            pcapLogFile = new CLStringArgument("pcap-log", "Log data to this pcap file");
            Add(pcapLogFile);

            //communicationLogFile = new CLStringArgument('l', "CommunicationLogFile");
            //Add(communicationLogFile);

            specialPortList = new CLStringArgument('c', "Connect Request Port List", "Special list of ports that when connected, must establish a connection request. A connection request must be established with incoming connections if the connection is to another instance of NetworkAdapter where it's opposite end is a client that is waiting for a connection request.");
            Add(specialPortList);

            readBufferSize = new CLUInt32Argument('b', "Read Buffer Size", "The size of the buffer used to hold the bytes being read from a socket");
            readBufferSize.SetDefault(4096);
            Add(readBufferSize);

            socketBackLog = new CLInt32Argument('s', "Server Socket BackLog", "The maximum length of the pending connections queue");
            socketBackLog.SetDefault(32);
            Add(socketBackLog);

            logData = new CLSwitch('d', "log-data", "Log the socket data");
            Add(logData);

            noTransferMessages = new CLSwitch('t', "no-transfer-messages", "Do not log transfer messages");
            Add(noTransferMessages);

            this.adapterTypeModeIsSet = false;
        }

        public void SetAdapterTypeMode(AdapterType adapterType)
        {
            this.adapterTypeModeIsSet = true;
            this.adapterTypeMode = adapterType;
        }

        public override void PrintUsageHeader()
        {
            if (!adapterTypeModeIsSet)
            {
                Console.WriteLine("NetworkAdapter <adapter-type> [options]");
                Console.WriteLine();
            }
            if (!adapterTypeModeIsSet || (adapterTypeMode == AdapterType.ClientServer))
            {
                Console.WriteLine("NetworkAdapter cs|client-server [options] <server-connector> <listen-port-list>");
            }
            if (!adapterTypeModeIsSet || (adapterTypeMode == AdapterType.ClientClient))
            {
                Console.WriteLine("NetworkAdapter cc|client-client [options] <not-implemented-yet>");
            }

            if (!adapterTypeModeIsSet || (adapterTypeMode == AdapterType.ServerServer))
            {
                Console.WriteLine();
                Console.WriteLine("NetworkAdapter ss|server-server [options] <tunnel-list>");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Example 1: NetworkAdapter ss 80 80-81 80,81,82-90");
                Console.WriteLine("This means listen on ports 80, 81, 82 and 90");
                Console.WriteLine("Tunnel '80'          : if 2 clients come in on port 80, connect them");
                Console.WriteLine("Tunnel '80-81'       : if a client comes in on port 80 and another on port 81, connect them");
                Console.WriteLine("Tunnel '80,81,82-90' : if a client comes in on either port 80, 81, or 82, and another on port 90, connect them");
                Console.WriteLine();
                Console.WriteLine("NetworkAdapter server-server 80 81$");
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine("Note: I need to provide a way to specify that a client should wait for a connect request from the other end. The conditions for this are that the other adapter are:");
            Console.WriteLine("   1. Has a client on the other end");
            Console.WriteLine("   2. When it was run, they specified that the client should wait for a connection request");
            Console.WriteLine();
            Console.WriteLine("Type Grammar:");
            Console.WriteLine("   <tunnel-list>  = <tunnel> | <tunnel> <tunnel-list>");
            Console.WriteLine("   <tunnel>       = <port-list> | <port-list> '-' <port-list>");
            Console.WriteLine("   <port-list>    = <port> | <port> ',' <port-list>");
            Console.WriteLine("   <port>         = 1-65535");
            Console.WriteLine();
            Console.WriteLine("Connection Requester Sequence: {0,3} {1,3} {2,3} {3,3}", NetworkAdapter.ConnectionRequesterSequence[0], NetworkAdapter.ConnectionRequesterSequence[1], NetworkAdapter.ConnectionRequesterSequence[2], NetworkAdapter.ConnectionRequesterSequence[3]);
            Console.WriteLine("                             : {0,3} {1,3} {2,3} {3,3}", NetworkAdapter.ConnectionRequesterSequence[4], NetworkAdapter.ConnectionRequesterSequence[5], NetworkAdapter.ConnectionRequesterSequence[6], NetworkAdapter.ConnectionRequesterSequence[7]);
            Console.WriteLine("                             : {0,3} {1,3} {2,3} {3,3}", NetworkAdapter.ConnectionRequesterSequence[8], NetworkAdapter.ConnectionRequesterSequence[9], NetworkAdapter.ConnectionRequesterSequence[10], NetworkAdapter.ConnectionRequesterSequence[11]);
            Console.WriteLine("                             : {0,3} {1,3} {2,3} {3,3}", NetworkAdapter.ConnectionRequesterSequence[12], NetworkAdapter.ConnectionRequesterSequence[13], NetworkAdapter.ConnectionRequesterSequence[14], NetworkAdapter.ConnectionRequesterSequence[15]);
            Console.WriteLine("Connection Receiver Sequence : {0,3} {1,3} {2,3} {3,3}", NetworkAdapter.ConnectionReceiverSequence[0], NetworkAdapter.ConnectionReceiverSequence[1], NetworkAdapter.ConnectionReceiverSequence[2], NetworkAdapter.ConnectionReceiverSequence[3]);
            Console.WriteLine("                             : {0,3} {1,3} {2,3} {3,3}", NetworkAdapter.ConnectionReceiverSequence[4], NetworkAdapter.ConnectionReceiverSequence[5], NetworkAdapter.ConnectionReceiverSequence[6], NetworkAdapter.ConnectionReceiverSequence[7]);
            Console.WriteLine("                             : {0,3} {1,3} {2,3} {3,3}", NetworkAdapter.ConnectionReceiverSequence[8], NetworkAdapter.ConnectionReceiverSequence[9], NetworkAdapter.ConnectionReceiverSequence[10], NetworkAdapter.ConnectionReceiverSequence[11]);
            Console.WriteLine("                             : {0,3} {1,3} {2,3} {3,3}", NetworkAdapter.ConnectionReceiverSequence[12], NetworkAdapter.ConnectionReceiverSequence[13], NetworkAdapter.ConnectionReceiverSequence[14], NetworkAdapter.ConnectionReceiverSequence[15]);
        }
    }
    public class NetworkAdapterProgram
    {
        static Int32 Main(string[] args)
        {
            Options options = new Options();
            List<String> nonOptionArgs = options.Parse(args);

            if (nonOptionArgs.Count <= 0)
            {
                return options.ErrorAndUsage("Please specify the adapter type 'client-server' ('cs'), 'client-client' ('cc') or 'server-server' ('ss')");
            }

            AdapterType adapterType = AdapterTypeMethods.ParseAdapterType(nonOptionArgs[0]);
            nonOptionArgs.RemoveAt(0);
            options.SetAdapterTypeMode(adapterType);

            if (adapterType == AdapterType.ClientServer)
            {
                return ClientServer.Run(options, nonOptionArgs);
            }
            else if (adapterType == AdapterType.ClientClient)
            {
                throw new NotImplementedException();

            }
            else if (adapterType == AdapterType.ServerServer)
            {
                throw new NotImplementedException();
                /*
                //
                // Get Options
                //
                if (nonOptionArgs.Count < 1)
                {
                    return options.ErrorAndUsage("Please supply at least one tunnel");
                }

                IPortTunnel[] tunnelArray = ParseUtilities.ParseTunnels(nonOptionArgs);

                
                UInt16[] specialPorts = null;
                if (options.specialPortList.set)
                {
                    specialPorts = ParseUtilities.ParsePorts(options.specialPortList.ArgValue);
                }

                TunnelList tunnelList = new TunnelList(tunnelArray, specialPorts);
                

                //
                // Run
                //
                ServerServer serverServer = new ServerServer(tunnelList, options.socketBackLog.ArgValue, 
                    options.readBufferSize.ArgValue, options.logData.set, !options.noTransferMessages.set);
                serverServer.Start();
                Console.ReadLine();
                */
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
