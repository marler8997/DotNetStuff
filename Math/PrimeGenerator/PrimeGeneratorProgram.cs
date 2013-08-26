using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace More
{
    class Options : CLParser
    {
        public override void PrintUsageHeader()
        {
            Console.WriteLine("Usage: PrimeGenerator.exe M (M is the maximum number to go to)");
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

            UInt32 MaxNumber = UInt32.Parse(nonOptionArgs[0]);
            if (MaxNumber == 0) return options.ErrorAndUsage("MaxNumber cannot be 0");

            UInt32 C1Length = (MaxNumber + (((MaxNumber % 2) == 1) ? (UInt32)1 : 0)) / 2;

            //
            // Generate the primes
            //
            Console.WriteLine("Allocating two arrays of length {0}...", C1Length);
            UInt32[] coprimes1 = new UInt32[C1Length];
            UInt32[] coprimes2 = new UInt32[C1Length];
            Console.WriteLine("Done allocating memory");


            Int64 sequenceGenerationStartTime = Stopwatch.GetTimestamp();

            //
            // InitializeCoprimes1
            //
            UInt32 sum = 1;
            for (UInt32 i = 0; i < C1Length; i++)
            {
                coprimes1[i] = sum;
                sum += 2;
            }

            //
            // Generate sequence
            //
            UInt32 coprimes1Length = C1Length, coprimes2Length;
            UInt32 n = 1;
            UInt32 Pn;

            while(true)
            {
                Coprimes.CnMinusOneToCn(coprimes1, coprimes1Length, coprimes2, out coprimes2Length);
                n++;

                Pn = coprimes2[2];
                if (Pn * Pn > coprimes1[coprimes1Length - 1])
                {
                    Console.WriteLine("Pn {0}, coprimes.length = {1}, coprimes[last] = {2}, n = {3}",
                        Pn, coprimes1Length, coprimes1[coprimes1Length - 1], n);
                    n += coprimes1Length;
                    Pn = coprimes1[coprimes1Length - 1];
                    break;
                }

                Coprimes.CnMinusOneToCn(coprimes2, coprimes2Length, coprimes1, out coprimes1Length);
                n++;

                Pn = coprimes1[2];
                if (Pn * Pn > coprimes2[coprimes2Length - 1])
                {
                    Console.WriteLine("Pn {0}, coprimes.length = {1}, coprimes[last] = {2}, n = {3}",
                        Pn, coprimes2Length, coprimes2[coprimes2Length - 1], n);
                    n += coprimes2Length;
                    Pn = coprimes2[coprimes2Length - 1];
                    break;
                }
            }

            //
            // Print sequence generation time
            //
            Int64 sequenceGenerationEndTime = Stopwatch.GetTimestamp();
            Console.WriteLine("Sequence Generation Time: {0} milliseconds",
                (sequenceGenerationEndTime - sequenceGenerationStartTime).StopwatchTicksAsUInt32Milliseconds());

            //
            // Print the nth prime number
            //
            Console.WriteLine("Prime {0} is equal to {1}", n, Pn);

            return 0;
        }
    }
}
