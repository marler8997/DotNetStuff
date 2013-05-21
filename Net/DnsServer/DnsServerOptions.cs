using System;

using Marler.Common;

namespace Marler.Net
{
    class DnsServerOptions : CLParser
    {
        public readonly CLStringArgument notificationHost;
        public readonly CLGenericArgument<UInt16> port;
        public readonly CLInt32Argument socketBackLog;

        public DnsServerOptions()
            : base()
        {
            notificationHost = new CLStringArgument('n', "Notification Host", "Host [and port] to send notifications when an address is handed out");
            Add(notificationHost);

            port = new CLGenericArgument<UInt16>(UInt16.Parse, 'p', "Listen Port");
            port.SetDefault(53);
            Add(port);

            socketBackLog = new CLInt32Argument('b', "Socket Back Log", "The maximum length of the pending connections queue");
            socketBackLog.SetDefault(32);
            Add(socketBackLog);
        }

        public override void PrintUsageHeader()
        {
            Console.WriteLine("DnsServer [options]");
        }
    }
}
