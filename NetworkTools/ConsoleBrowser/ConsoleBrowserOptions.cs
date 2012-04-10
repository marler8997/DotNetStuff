using System;
using System.Collections.Generic;
using System.Text;
using Marler.OptionsParser;

namespace Marler.NetworkTools
{
    public class ConsoleBrowserOptions : Options
    {


        public override void PrintHeader()
        {
            Console.WriteLine("ConsoleBrowser [options]");
        }
    }
}
