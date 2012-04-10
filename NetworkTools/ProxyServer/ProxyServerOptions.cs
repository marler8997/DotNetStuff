using System;
using System.Collections.Generic;
using System.Text;
using Marler.OptionsParser;

namespace Marler.NetworkTools
{
    public class ProxyServerOptions : Options
    {
        public readonly OptionGenericArg<UInt16> port;
        public readonly OptionIntArg socketBackLog;

        public ProxyServerOptions()
            : base()
        {
            this.port = new OptionGenericArg<UInt16>(UInt16.Parse, 'p', "Listening Port");
            this.port.SetDefault(1080);

            this.socketBackLog = new OptionIntArg('b', "Socket Back Log", "The maximum length of the pending connections queue");
            this.socketBackLog.SetDefault(32);

            AddOption(port);
            AddOption(socketBackLog);
        }

        public override void PrintHeader()
        {
            Console.WriteLine("ProxyServer [options]");
        }
    }
}
