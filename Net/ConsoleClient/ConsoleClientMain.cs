using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using More;

namespace More.Net
{
    public class ConsoleClientOptions : CLParser
    {
    }

    public class ConsoleClientProgram
    {
        static Int32 Main(string[] args)
        {
            ConsoleClientOptions optionsParser = new ConsoleClientOptions();
            List<String> nonOptionArgs = optionsParser.Parse(args);

            InternetHost host;

            if (nonOptionArgs.Count > 0)
            {
                if (nonOptionArgs.Count != 2)
                {
                    Console.WriteLine("Expected either 0 or 2 command line aruments but got {0}", nonOptionArgs.Count);
                    optionsParser.PrintUsage();
                    return -1;
                }

                Proxy proxy;
                String ipOrHost = Proxy.StripAndParseProxies(args[0], DnsPriority.IPv4ThenIPv6, out proxy);
                host = new InternetHost(ipOrHost, UInt16.Parse(args[1]), DnsPriority.IPv4ThenIPv6, proxy);
            }
            else
            {
                host = default(InternetHost);
            }

            ConsoleClient commandClient = new ConsoleClient(1024, 1024, host, new ConsoleMessageLogger("Connection"),
                new ConnectionDataLoggerSingleLog(ConsoleDataLogger.Instance, "[SendData]", "[RecvData]"));
            commandClient.RunPrepare();
            commandClient.Run();

            return 0;
        }
    }
}
