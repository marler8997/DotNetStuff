using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marler.OptionsParser;

namespace Marler.RuntimeAnalyzer
{
    public class AssemblerOptions : Options
    {
        public AssemblerOptions()
        {

        }


        public override void PrintHeader()
        {
            Console.WriteLine("Assembler [options] <assembly-file>");
        }
    }
}
