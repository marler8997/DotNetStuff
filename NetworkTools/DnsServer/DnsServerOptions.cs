using System;

using Marler.OptionsParser;

namespace Marler.NetworkTools
{
    class DnsServerOptions : Options
    {
        public readonly OptionStringArg notificationHost;
        public readonly OptionGenericArg<UInt16> port;
        public readonly OptionIntArg socketBackLog;

        public DnsServerOptions()
            : base()
        {
            notificationHost = new OptionStringArg('n', "Notification Host", "Host [and port] to send notifications when an address is handed out");
            AddOption(notificationHost);

            port = new OptionGenericArg<UInt16>(UInt16.Parse, 'p', "Listen Port");
            port.SetDefault(53);
            AddOption(port);

            socketBackLog = new OptionIntArg('b', "Socket Back Log", "The maximum length of the pending connections queue");
            socketBackLog.SetDefault(32);
            AddOption(socketBackLog);
        }

        public override void PrintHeader()
        {
            Console.WriteLine("DnsServer [options]");
        }
    }
}
