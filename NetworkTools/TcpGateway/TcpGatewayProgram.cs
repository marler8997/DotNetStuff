using System;
using System.Collections.Generic;

using Marler.NetworkTools;

namespace TcpGateway
{
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
                return optionsParser.ErrorMessage("Please supply at least one port.");
            }
            
            PortSet listenPortSet = ParseUtilities.ParsePortSet(nonOptionArgs, 0);

            TcpGateway tcpGateway = new TcpGateway(listenPortSet, optionsParser.socketBackLog.ArgValue);

            return 0;
        }
    }
}
