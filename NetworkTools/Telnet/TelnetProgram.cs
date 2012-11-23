using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace Marler.NetworkTools
{
    public class TelnetProgram
    {
        static Int32 Main(string[] args)
        {
            TelnetOptions optionsParser = new TelnetOptions();
            List<String> nonOptionArgs = optionsParser.Parse(args);

            if (nonOptionArgs.Count != 1)
            {
                Console.WriteLine("Expected 1 argument but got {0}", nonOptionArgs.Count);
                optionsParser.PrintUsage();
                return -1;
            }

            ISocketConnector connector = null;
            if (nonOptionArgs.Count == 1)
            {
                connector = ParseUtilities.ParseConnectionSpecifier(nonOptionArgs[0]);
            }

            TelnetClient client = new TelnetClient(optionsParser.wantServerEcho.isSet);
            Socket socket = connector.Connect();
            client.Run(socket);

            return 0;
        }
    }
}
