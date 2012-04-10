using System;
using System.Collections.Generic;

namespace Marler.RuntimeAnalyzer
{
    public class RuntimeProgram
    {
        static Int32 Main(string[] args)
        {
            RuntimeOptions optionsParser = new RuntimeOptions();
            List<String> nonOptionArgs = optionsParser.Parse(args);

            if (nonOptionArgs.Count < 1)
            {
                Console.WriteLine("Missing the RA Program");
                optionsParser.PrintUsage();
                return -1;
            }
            else if (nonOptionArgs.Count > 1)
            {
                Console.WriteLine("Expected 1 non-option argument, you gave {0}", nonOptionArgs.Count);
                optionsParser.PrintUsage();
                return -1;
            }

            String raProgram = nonOptionArgs[0];

            Memory processStack = new Memory(2048,
                new ReadCallback[] { IOHandlers.ConsoleRead },
                new WriteCallback[] { IOHandlers.ConsoleWrite }
                );

            Runtime runtime = new Runtime(raProgram, processStack);
            runtime.Run();

            return 0;
        }
    }
}
