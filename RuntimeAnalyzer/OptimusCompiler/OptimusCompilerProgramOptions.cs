using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marler.OptionsParser;

namespace Marler.RuntimeAnalyzer
{
    public class OptimusCompilerProgramOptions : Options
    {
        public OptimusCompilerProgramOptions()
        {
        }


        public override void PrintHeader()
        {
            Console.WriteLine("Assembler [options] <assembly-file>");
        }
    }
}
