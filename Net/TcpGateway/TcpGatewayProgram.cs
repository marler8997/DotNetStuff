using System;
using System.Collections.Generic;

using Marler.Common;
using Marler.Net;

namespace TcpGateway
{
    class TcpGatewayOptions : CLParser
    {
        public readonly CLInt32Argument socketBackLog;

        public TcpGatewayOptions()
            : base()
        {
            socketBackLog = new CLInt32Argument('b', "Socket Back Log", "The maximum length of the pending connections queue");
            socketBackLog.SetDefault(32);
            Add(socketBackLog);
        }

        public override void PrintUsageHeader()
        {
            Console.WriteLine("WebServer [options] root-path");
        }
    }
    class TcpGatewayProgram
    {
        static Int32 Main(string[] args)
        {
            TcpGatewayOptions optionsParser = new TcpGatewayOptions();
            List<String> nonOptionArgs = optionsParser.Parse(args);

            // Options
            //
            if (nonOptionArgs.Count < 1)
            {
                return optionsParser.ErrorAndUsage("Please supply at least one port.");
            }
            
            PortSet listenPortSet = ParseUtilities.ParsePortSet(nonOptionArgs, 0);

            TcpGateway tcpGateway = new TcpGateway(listenPortSet, optionsParser.socketBackLog.ArgValue);

            return 0;
        }
    }
}
