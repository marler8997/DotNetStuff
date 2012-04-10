using System;
using Marler.OptionsParser;

namespace Marler.RuntimeAnalyzer
{
    public class RuntimeOptions : Options
    {
        public RuntimeOptions()
        {

        }
        

        public override void PrintHeader()
        {
            Console.WriteLine("Runtime [options] <ra-program>");
        }
    }
}
