using System;
using Marler.OptionsParser;

namespace Marler.NetworkTools
{
    class PortScannerOptions : Options
    {
        public readonly OptionGenericArg<UInt16> maxThreadCount;
        public readonly OptionGenericArg<UInt32> timeoutMilliseconds;
        public readonly OptionGenericArg<UInt32> sleepTimeMilliseconds;

        public PortScannerOptions()
            : base()
        {
            this.maxThreadCount = new OptionGenericArg<UInt16>(UInt16.Parse, 'm', "Max Thread Count");
            this.maxThreadCount.SetDefault(500);

            this.timeoutMilliseconds = new OptionGenericArg<UInt32>(UInt32.Parse, 't', "Connection Timeout (milliseconds)");

            this.sleepTimeMilliseconds = new OptionGenericArg<UInt32>(UInt32.Parse, 's', "Sleep Time (milliseconds)",
                "The amount of time to sleep if the maximum number of threads is reached");
            this.sleepTimeMilliseconds.SetDefault(250);

            AddOption(maxThreadCount);
            AddOption(timeoutMilliseconds);
            AddOption(sleepTimeMilliseconds);
        }

        public override void PrintHeader()
        {
            Console.WriteLine("PortScanner <remote-host> <min-port> <max-port> [options]");
        }
    }
}
