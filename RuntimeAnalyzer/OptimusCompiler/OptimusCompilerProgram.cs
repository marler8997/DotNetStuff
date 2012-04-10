﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Marler.RuntimeAnalyzer
{
    class OptimusCompilerProgram
    {
        static Int32 Main(string[] args)
        {
            OptimusCompilerProgramOptions optionsParser = new OptimusCompilerProgramOptions();
            List<String> nonOptionArgs = optionsParser.Parse(args);

            if (nonOptionArgs.Count < 1)
            {
                Console.WriteLine("Missing the Optimus Program File");
                optionsParser.PrintUsage();
                return -1;
            }
            else if (nonOptionArgs.Count > 1)
            {
                Console.WriteLine("Expected 1 non-option argument, you gave {0}", nonOptionArgs.Count);
                optionsParser.PrintUsage();
                return -1;
            }

            String optimusFile = nonOptionArgs[0];
            AssemblyBuilder assembly = null;
            using (FileStream assemblyStream = new FileStream(assemblyFile, FileMode.Open))
            {
                AssemblyTokenizer tokenizer = new AssemblyTokenizer();
                tokenizer.SetStream(new StreamReader(assemblyStream), 1);

                AssemblyParser parser = new AssemblyParser(Console.Out, tokenizer);
                assembly = parser.Parse();
            }

            if (assembly != null)
            {
                Console.WriteLine();
                Console.WriteLine("ASSEMBLY:");
                assembly.Print();


                using (FileStream raCodeStream = new FileStream(Path.GetFileNameWithoutExtension(assemblyFile) + ".ra", FileMode.Create))
                {
                    assembly.Output(raCodeStream);
                }

            }


            return 0;
        }
    }
}