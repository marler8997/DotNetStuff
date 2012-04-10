using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marler.OptionsParser;

namespace Marler.NetworkTools
{
    public class ConsoleClientOptions : Options
    {
        public override void PrintHeader()
        {
            Console.WriteLine("CommandClient [options] [host]");
        }
    }
}
