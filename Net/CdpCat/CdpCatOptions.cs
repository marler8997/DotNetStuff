using System;

using Marler.Common;

namespace Marler.Net
{
    public class CdpCatOptions : CLParser
    {
        public readonly CLGenericArgument<UInt16> listenPort;
        public readonly CLGenericArgument<UInt16> localPort;
        public readonly CLGenericArgument<UInt16> maxPayload;
        public readonly CLSwitch verbose;

        public CdpCatOptions()
            : base()
        {
            listenPort = new CLGenericArgument<UInt16>(UInt16.Parse, 'l', "listen mode", "Specifies that NetCat should wait for a tcp connection on the given port");
            Add(listenPort);

            localPort = new CLGenericArgument<UInt16>(UInt16.Parse, 'p', "local port (the listen port in listen mode)");
            Add(localPort);

            maxPayload = new CLGenericArgument<UInt16>(UInt16.Parse, 'm', "maximum cdp packet size");
            maxPayload.SetDefault(4096);
            Add(maxPayload);

            verbose = new CLSwitch('v', "Verbose");
            Add(verbose);
        }

        public override void PrintUsageHeader()
        {
            Console.WriteLine("Outbound Connection: CdpCat.exe [options] host port");
            Console.WriteLine("InBound Connection : CdpCat.exe -l listen-port [options] [host]");
        }




    }
}
