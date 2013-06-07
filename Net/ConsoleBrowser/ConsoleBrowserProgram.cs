using System;
using System.Collections.Generic;

using More;

namespace More.Net
{
    public class ConsoleBrowserOptions : CLParser
    {
        public override void PrintUsageHeader()
        {
            Console.WriteLine("ConsoleBrowser [options]");
        }
    }
    public class CommandClientProgram
    {
        static Int32 Main(string[] args)
        {
            ConsoleBrowserOptions optionsParser = new ConsoleBrowserOptions();
            List<String> nonOptionArgs = optionsParser.Parse(args);

            if (nonOptionArgs.Count > 1)
            {
                Console.WriteLine("Expected up to 1 non-option argument, you gave {0}", nonOptionArgs.Count);
                optionsParser.PrintUsage();
                return -1;
            }

            /*
            ISocketConnector connector = null;
            if (nonOptionArgs.Count == 1)
            {
                connector = ParseUtilities.ParseConnectionSpecifier(nonOptionArgs[0]);
            }
            */

            //CommandClient commandClient = new CommandClient(1024);
            //commandClient.Shell(new ConsoleDataLoggerWithLabels("[Received Data]", "[End of Received Data]"), 1024, connector);

            return 0;
        }
    }
}
