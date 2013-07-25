using System;
using System.Threading;

using More;

namespace DataVis
{
    //
    // Where to get the data for the graph
    //   1. From STDIN (default)
    //   2. 
    public class DataVisCLOptions : CLParser
    {
        public DataVisCLOptions()
        {
        }
        public override void PrintUsageHeader()
        {
            Console.WriteLine("DataVis.exe [options]");
        }
        public override void PrintUsageFooter()
        {
            Console.WriteLine("Where to get the visualization data");
            Console.WriteLine("    1. STDIN (default)");
            Console.WriteLine("    2. File (use --follow to follow the file)");
            Console.WriteLine("    3. Socket (use --listen <port> to wait for input or use --connect <host-specifier>)");
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            DataVisCLOptions options = new DataVisCLOptions();
            options.PrintUsage();



            AutoResetEvent updateEvent = new AutoResetEvent(false);

            //
            // Negotiate reader
            //
            DataFollower follower = new FileFollower(updateEvent, @"C:\temp\follow.data", 1024);

            new Thread(follower.Run).Start();

            DataVisWindow dataVisWindow = new DataVisWindow(follower);
            dataVisWindow.Run();
        }
    }
}
