using System;
using System.Collections.Generic;

namespace Marler.NetworkTools
{
    public class ConsoleClientProgram
    {
        static Int32 Main(string[] args)
        {
            ConsoleClientOptions optionsParser = new ConsoleClientOptions();
            List<String> nonOptionArgs = optionsParser.Parse(args);
            
            if (nonOptionArgs.Count > 1)
            {
                Console.WriteLine("Expected up to 1 non-option argument, you gave {0}", nonOptionArgs.Count);
                optionsParser.PrintUsage();
                return -1;
            }
            
            ISocketConnector connector = null;
            if(nonOptionArgs.Count == 1)
            {
                connector = ParseUtilities.ParseConnectionSpecifier(nonOptionArgs[0]);
            }

            ConsoleClient commandClient = new ConsoleClient(1024);
            commandClient.Shell(new ConsoleDataLoggerWithLabels("[Received Data]", "[End of Received Data]"), 1024, connector);

            return 0;
        }
    }
}
