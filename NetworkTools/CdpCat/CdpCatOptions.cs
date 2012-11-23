using System;

using Marler.OptionsParser;

namespace Marler.NetworkTools
{
    public class CdpCatOptions : Options
    {
        public readonly OptionGenericArg<UInt16> listenPort;
        public readonly OptionGenericArg<UInt16> localPort;
        public readonly OptionGenericArg<UInt16> maxPayload;
        public readonly OptionNoArg verbose;

        public CdpCatOptions()
            : base()
        {
            listenPort = new OptionGenericArg<UInt16>(UInt16.Parse, 'l', "listen mode", "Specifies that NetCat should wait for a tcp connection on the given port");
            AddOption(listenPort);

            localPort = new OptionGenericArg<UInt16>(UInt16.Parse, 'p', "local port (the listen port in listen mode)");
            AddOption(localPort);

            maxPayload = new OptionGenericArg<UInt16>(UInt16.Parse, 'm', "maximum cdp packet size");
            maxPayload.SetDefault(4096);
            AddOption(maxPayload);

            verbose = new OptionNoArg('v', "Verbose");
            AddOption(verbose);
        }

        public override void PrintHeader()
        {
            Console.WriteLine("Outbound Connection: CdpCat.exe [options] host port");
            Console.WriteLine("InBound Connection : CdpCat.exe -l listen-port [options] [host]");
        }




    }
}
