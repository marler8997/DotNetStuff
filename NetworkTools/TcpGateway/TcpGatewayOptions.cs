using System;
using System.Collections.Generic;
using System.Text;
using Marler.OptionsParser;

namespace TcpGateway
{
    class TcpGatewayOptions : Options
    {
        public readonly OptionIntArg socketBackLog;

        public TcpGatewayOptions()
            : base()
        {
            socketBackLog = new OptionIntArg('b', "Socket Back Log", "The maximum length of the pending connections queue");
            socketBackLog.SetDefault(32);

            AddOption(socketBackLog);
        }

        public override void PrintHeader()
        {
            Console.WriteLine("WebServer [options] root-path");
        }
    }
}
