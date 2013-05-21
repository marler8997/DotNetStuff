using System;

using Marler.Common;

namespace Marler.Net
{
    class DnsClientOptions : CLParser
    {
        public override void PrintUsageHeader()
        {
            Console.WriteLine("DnsClient [options] [host]");
        }
    }
}
