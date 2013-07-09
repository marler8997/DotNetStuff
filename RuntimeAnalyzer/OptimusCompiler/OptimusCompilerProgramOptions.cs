using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using More;

namespace Marler.RuntimeAnalyzer
{
    public class OptimusCompilerProgramOptions : CLParser
    {
        public OptimusCompilerProgramOptions()
        {
        }
        public override void PrintUsageHeader()
        {
            Console.WriteLine("Assembler [options] <assembly-file>");
        }
    }
}
