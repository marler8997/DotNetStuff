using System;

using More;

namespace More.Net
{
    class DnsClientOptions : CLParser
    {
        public override void PrintUsageHeader()
        {
            Console.WriteLine("DnsClient [options] [host]");
        }
    }
}
