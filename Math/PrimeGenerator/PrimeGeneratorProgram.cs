using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace More
{
    class Options : CLParser
    {
        public override void PrintUsageHeader()
        {
            Console.WriteLine("Usage: PrimeGenerator.exe n (n is the nth prime number starting from 1)");
        }

    }


    class PrimeGeneratorProgram
    {
        static Int32 Main(string[] args)
        {
            //
            // Process command line arguments
            //
            Options options = new Options();
            List<String> nonOptionArgs = options.Parse(args);

            if (nonOptionArgs.Count <= 0)
            {
                options.PrintUsage();
                return 1;
            }
            if(nonOptionArgs.Count != 1)
            {
                return options.ErrorAndUsage("Expected {0} non-option arguments but got {1}",
                    1, nonOptionArgs.Count);
            }

            UInt32 n = UInt32.Parse(nonOptionArgs[0]);
            if (n == 0) return options.ErrorAndUsage("n cannot be 0");
            if (n == 1)
            {
                Console.WriteLine("Prime number 1 is {0}", 2);
                return 0;
            }

            //
            // Generate the primes
            //
            Console.WriteLine("Allocating array of length {0}...", n - 1);
            UInt32[] intervals = new UInt32[n-1];
            Console.WriteLine("Done allocating memory");


            Int64 sequenceGenerationStartTime = Stopwatch.GetTimestamp();

            //
            // Fill with 2s
            //
            for (UInt32 i = 0; i < intervals.Length; i++)
            {
                intervals[i] = 2;
            }

            //
            // Generate sequence
            //
            for (UInt32 i = 0; i < intervals.Length; i++)
            {
                UInt32 currentPrime = 1 + intervals[i];

                UInt32 dstIndex = i;
                for (UInt32 j = i; j < intervals.Length; j++)
                {

                }
            }


            //
            // Print sequence generation time
            //
            Int64 sequenceGenerationEndTime = Stopwatch.GetTimestamp();
            Console.WriteLine("Sequence Generation Time: {0} milliseconds",
                (sequenceGenerationEndTime - sequenceGenerationStartTime).StopwatchTicksAsUInt32Milliseconds());



            //
            // Calculate and print the nth prime
            //
            UInt32 prime = 1;
            for (UInt32 i = 0; i < intervals.Length; i++)
            {
                prime += intervals[i];
            }
            Console.WriteLine("Prime number {0} is {1}", n, prime);

            return 0;
        }
    }
}
