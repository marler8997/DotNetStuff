using System;
using More;

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

    public class NetworkAdapterOptions : CLParser
    {
        public readonly CLStringArgument communicationLogFile;
        public readonly CLStringArgument specialPortList;
        public readonly CLInt32Argument readBufferSize;
        public readonly CLInt32Argument socketBackLog;

        private Boolean adapterTypeModeIsSet;
        private AdapterType adapterTypeMode;

        public NetworkAdapterOptions()
            : base()
        {
            communicationLogFile = new CLStringArgument('l', "CommunicationLogFile");
            Add(communicationLogFile);

            specialPortList = new CLStringArgument('c', "Connect Request Port List", "Special list of ports that when connected, must establish a connection request. A connection request must be established with incoming connections if the connection is to another instance of NetworkAdapter where it's opposite end is a client that is waiting for a connection request.");
            Add(specialPortList);

            readBufferSize = new CLInt32Argument('b', "Read Buffer Size", "The size of the buffer used to hold the bytes being read from a socket");
            readBufferSize.SetDefault(4096);
            Add(readBufferSize);

            socketBackLog = new CLInt32Argument('s', "Server Socket BackLog", "The maximum length of the pending connections queue");
            socketBackLog.SetDefault(32);
            Add(socketBackLog);

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
                Console.WriteLine("NetworkAdapter cs|client-server [options] <server-host> <server-port> <listen-port-list>");
            }
            if(!adapterTypeModeIsSet || (adapterTypeMode == AdapterType.ClientClient))
            {
                Console.WriteLine("NetworkAdapter cc|client-client [options] <not-implemented-yet>");
            }

            if(!adapterTypeModeIsSet || (adapterTypeMode == AdapterType.ServerServer))
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
}
