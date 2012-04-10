using System;

using Marler.OptionsParser;

namespace Marler.NetworkTools
{
    class DnsClientOptions : Options
    {
        public override void PrintHeader()
        {
            Console.WriteLine("DnsClient [options] [host]");
        }
    }
}
