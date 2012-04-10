using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Marler.NetworkTools
{
    class PortScannerProgram
    {
        public static Int32 Main(String[] args)
        {
            PortScannerOptions optionsParser = new PortScannerOptions();
            List<String> nonOptionArgs = optionsParser.Parse(args);

            if (nonOptionArgs.Count < 3)
            {
                Console.WriteLine("Please give the remote-host, minPort and maxPort");
                optionsParser.PrintUsage();
                return -1;
            }
            else if (nonOptionArgs.Count > 3)
            {
                Console.WriteLine("Expected 3 non-option arguments, you gave {0}", nonOptionArgs.Count);
                optionsParser.PrintUsage();
                return -1;
            }

            String remoteHost = nonOptionArgs[0];
            String minPortString = nonOptionArgs[1];
            String maxPortString = nonOptionArgs[2];

            UInt16 minPort = UInt16.Parse(minPortString);
            UInt16 maxPort = UInt16.Parse(maxPortString);

            TimeSpan timeout = optionsParser.timeoutMilliseconds.isSet ? 
                new TimeSpan(0,0,0,0,(Int32)optionsParser.timeoutMilliseconds.ArgValue) :
                TimeSpan.Zero;

            IPAddress remoteHostIP = GenericUtilities.ResolveHost(remoteHost);

            PortScanner portScanner = new PortScanner(remoteHostIP, minPort, maxPort,
                optionsParser.maxThreadCount.ArgValue, timeout, optionsParser.sleepTimeMilliseconds.ArgValue);
            portScanner.Scan();

            return 0;
        }
    }
}
