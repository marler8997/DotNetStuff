using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.Net
{
    class FtpServerProgram
    {
        static void Main(string[] args)
        {
            FtpServerOptions optionsParser = new FtpServerOptions();
            List<String> nonOptionArgs = optionsParser.Parse(args);

            if (nonOptionArgs.Count < 1)
            {
                Console.WriteLine("Please give the path");
                optionsParser.PrintUsage();
            }
            else if (nonOptionArgs.Count > 1)
            {
                Console.WriteLine("Expected 1 non-option argument, you gave {0}", nonOptionArgs.Count);
                optionsParser.PrintUsage();
            }

            String path = nonOptionArgs[0];

            FtpServer ftpServer = new FtpServer(
                path,
                optionsParser.port.ArgValue,
                optionsParser.socketBackLog.ArgValue);

            ftpServer.Run();
        }
    }
}
