using System;
using System.Collections.Generic;
using System.IO;

namespace More.Pdl
{
    class PdlCodeGeneratorOptions : CLParser
    {
        public override void PrintUsageHeader()
        {
            Console.WriteLine("PdlCodeGenerator.exe <namespace> <input-file>");
        }
    }
    class Program
    {
        static Int32 Main(string[] args)
        {
            PdlCodeGeneratorOptions options = new PdlCodeGeneratorOptions();

            List<String> nonOptionArgs = options.Parse(args);

            if(nonOptionArgs.Count != 2)
            {
                return options.ErrorAndUsage("Expected 2 non-option arguments but got {0}", nonOptionArgs.Count);
            }

            String @namespace = nonOptionArgs[0];
            String inputFileName = nonOptionArgs[1];

            PdlFile pdlFile;
            using (StreamReader reader = new StreamReader(new FileStream(inputFileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                pdlFile = PdlFileParser.ParsePdlFile(reader);
            }

            PdlCodeGenerator.GenerateCode(Console.Out, pdlFile, @namespace, false);

            return 0;
        }
    }

    
}
