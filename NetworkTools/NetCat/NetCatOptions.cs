using System;

using Marler.OptionsParser;

namespace Marler.NetworkTools
{
    public class NetCatOptions : Options
    {
        public readonly OptionStringArg listenProxy;
        public readonly OptionGenericArg<UInt16> listenPort;
        public readonly OptionGenericArg<UInt16> localPort;
        public readonly OptionNoArg verbose;

        public NetCatOptions()
            : base()
        {
            listenProxy = new OptionStringArg('x', "Listen Proxy Server", "Use this to open a port on a proxy server");
            AddOption(listenProxy);

            listenPort = new OptionGenericArg<UInt16>(UInt16.Parse, 'l', "listen mode", "Specifies that NetCat should wait for a tcp connection on the given port");
            AddOption(listenPort);

            localPort = new OptionGenericArg<UInt16>(UInt16.Parse, 'p', "local port (the listen port in listen mode)");
            AddOption(localPort);

            verbose = new OptionNoArg('v', "Verbose");
            AddOption(verbose);
        }

        public override void PrintHeader()
        {
            Console.WriteLine("Outbound Connection: NetCat.exe [-options] host port");
            Console.WriteLine("InBound Connection : NetCat.exe -l listen-port [-options] [host]");      
        }




    }
}
