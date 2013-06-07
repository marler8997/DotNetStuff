using System;
using System.Collections.Generic;
using System.Net;

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
            
            EndPoint serverEndPoint = null;

            if (nonOptionArgs.Count > 0)
            {
                if (nonOptionArgs.Count != 2)
                {
                    Console.WriteLine("Expected either 0 or 2 command line aruments but got {0}", nonOptionArgs.Count);
                    optionsParser.PrintUsage();
                    return -1;
                }
                serverEndPoint = EndPoints.EndPointFromIPOrHost(args[0], UInt16.Parse(args[1]));
            }

            //Proxy proxy = optionsParser.proxy.ArgValue;

            ConsoleClient commandClient = new ConsoleClient(1024, 1024, null, serverEndPoint, new ConsoleMessageLogger("Connection"),
                new ConnectionDataLoggerSingleLog(ConsoleDataLogger.Instance, "[SendData]", "[RecvData]"));
            commandClient.RunPrepare();
            commandClient.Run();

            return 0;
        }
    }
}
