using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace More
{
    class Options : CLParser
    {
        public readonly CLSwitch DontPrintSequences;
        public readonly CLGenericArgument<UInt32> MaxCnCount;


        public Options()
        {
            DontPrintSequences = new CLSwitch('n', "noprint", "Do not print sequences");
            Add(DontPrintSequences);

            MaxCnCount = new CLGenericArgument<UInt32>(UInt32.Parse, 'm', "max-cn-count", "The maximum number of characters in the sequence");
            Add(MaxCnCount);
        }
        public override void PrintUsageHeader()
        {
            Console.WriteLine("Usage: CoprimeGenerator.exe n");
        }
    }
    class CoprimeGeneratorProgram
    {
        static void PrintFilterMap(UInt32 PnPlusOne, UInt32 Qn, UInt64[] bprimes, UInt32 smallMultiplierIndex)
        {
            UInt64 maxBPrime = bprimes[bprimes.Length - 1];

            UInt32[] bprimeFilterMap = new UInt32[bprimes.Length];

            UInt64 smallMultiplier = bprimes[smallMultiplierIndex];
            UInt32 bigMultiplierIndex;
            for (bigMultiplierIndex = smallMultiplierIndex; true; bigMultiplierIndex++)
            {
                UInt64 bigMultiplier = bprimes[bigMultiplierIndex];
                UInt64 filterNumber = smallMultiplier * bigMultiplier;
                if (filterNumber > maxBPrime) break;

                // Mark filter number
                for (UInt32 check = 0; true; check++)
                {
                    UInt64 checkBprime = bprimes[check];
                    if (checkBprime == filterNumber)
                    {
                        if (bprimeFilterMap[check] == 0)
                        {
                            bprimeFilterMap[check] = smallMultiplierIndex;
                        }
                        break;
                    }
                }
            }

            UInt64 maxFilterDecimalDigits = DecimalDigitCount(bprimeFilterMap[bprimeFilterMap.Length - 1]);
            String filterNumberFormat = String.Format("{{0,{0}}}", maxFilterDecimalDigits);
            Console.WriteLine();
            Console.WriteLine("FilterMap {0}", smallMultiplierIndex);
            Console.WriteLine("-----------------------------------");
            {
                UInt32 filterIndex = 0;
                for (UInt32 i = 1; i <= PnPlusOne; i++)
                {
                    Console.Write("[{0,4}]", i);

                    for (UInt32 j = 0; j < Qn; j++)
                    {
                        Console.Write(' ');
                        Console.Write(filterNumberFormat, bprimeFilterMap[filterIndex]);
                        filterIndex++;
                    }

                    Console.WriteLine();
                }
            }
        }

        static void FilterMapToImage(Stream stream, UInt32[] filterMap, UInt32 PnPlusOne, UInt32 Qn)
        {
            UInt32 min = filterMap[0], max = filterMap[0];
            for (int i = 1; i < filterMap.Length; i++)
            {
                UInt32 filterIndex = filterMap[i];
                if (filterIndex < min)
                {
                    min = filterIndex;
                }
                else if (filterIndex > max)
                {
                    max = filterIndex;
                }
            }

            UInt32 range = max - min;

            Byte[] color = new Byte[3];
            for (int i = 0; i < filterMap.Length; i++)
            {
                UInt32 filterIndex = filterMap[i];

                if (filterIndex == 0)
                {
                    color[0] = 255;
                    color[1] = 255;
                    color[2] = 255;
                }
                else
                {
                    /*
                    if (filterIndex == 1)
                    {
                        color[0] = 0;
                        color[1] = 0;
                        color[2] = 0;
                    }
                    else
                    {
                        color[0] = 255;
                        color[1] = 255;
                        color[2] = 255;
                    }
                    */

                    //Byte shade = (Byte)((float)filterIndex / (float)range * 255.0);
                    //Console.WriteLine("filterIndex {0} range {1} Shade {2}", filterIndex, range, shade);
                    //color[0] = shade;
                    //color[1] = shade;
                    //color[2] = shade;

                    UInt32 shade = (UInt32)((float)filterIndex / (float)range * (float)0xFFFFFF);
                    color[0] = (Byte)(shade >> 16);
                    color[1] = (Byte)(shade >>  8);
                    color[2] = (Byte)(shade      );
                }

                


                stream.Write(color, 0, color.Length);
            }
        }


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
            if (nonOptionArgs.Count != 1)
            {
                return options.ErrorAndUsage("Expected {0} non-option arguments but got {1}",
                    1, nonOptionArgs.Count);
            }

            UInt32 n = UInt32.Parse(nonOptionArgs[0]);
            if (n == 0) return options.ErrorAndUsage("n cannot be 0");

            UInt32 Pn = PrimeTable.Values[n - 1];
            UInt32 PnMinusOne = PrimeTable.Values[n - 2];
            UInt32 PnPlusOne = PrimeTable.Values[n];


            //
            // Find Qn
            //
            UInt32 cnCount;
            UInt64 QnIfSmallEnough = 0; // 0 means a UInt64 is not small enough to hold Qn
            if (options.MaxCnCount.set)
            {
                cnCount = options.MaxCnCount.ArgValue;

                try
                {
                    QnIfSmallEnough = Coprimes.CalculateQn(n);
                    if (QnIfSmallEnough < cnCount)
                    {
                        cnCount = (UInt32)QnIfSmallEnough;
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Console.WriteLine(e.Message);
                    QnIfSmallEnough = 0;
                }
            }
            else
            {
                QnIfSmallEnough = Coprimes.CalculateQn(n);
                if (QnIfSmallEnough > UInt32.MaxValue)
                {
                    Console.WriteLine("Qn for n={0} is too large for a UInt64, use -m option to only generate the first part of the sequence", n);
                    return 1;
                }
                cnCount = (UInt32)QnIfSmallEnough;
            }

            //
            // Create the GnBuffer
            //
            ISimpleList<UInt32> GnList;
            if (cnCount == QnIfSmallEnough)
            {
                GnList = new FixedSimpleList<UInt32>((UInt32)Coprimes.CalculateQn(n - 1));
            }
            else
            {
                GnList = new DynamicSimpleList<UInt32>();
            }

            //
            // Find Primorial
            //
            UInt64 PrimorialIfSmallEnough = 0; // 0 means a UInt64 is not small enough to hold Primorial
            try
            {
                PrimorialIfSmallEnough = Coprimes.CalculatePrimorial(n);
            }
            catch (ArgumentOutOfRangeException e)
            {
            }


            Console.WriteLine("n = {0}, Pn = {1}, Qn = {2}, Primorial = {3}", n, Pn,
                (QnIfSmallEnough == 0) ? "TooLargeForUInt64" : QnIfSmallEnough.ToString(),
                (PrimorialIfSmallEnough == 0) ? "TooLargeForUInt64" : PrimorialIfSmallEnough.ToString());


            if (cnCount != QnIfSmallEnough)
            {
                Console.WriteLine();
                Console.WriteLine("NOTE: You have limited the results to {0}", cnCount);
            }



            //
            // For now just use the slow Cn creator
            //
            UInt32[] Cn = Coprimes.BruteForceCreateCn(n, cnCount + 1, GnList); // Add 1 to the CnCount in order to have Cn intervals


            //
            // Print
            //
            UInt64 maxCoprimeDecimalDigits = DecimalDigitCount(Cn[cnCount - 1]);
            String coprimeNumberFormat = String.Format("{{0,{0}}}", maxCoprimeDecimalDigits);

            Console.WriteLine();
            if (options.DontPrintSequences.set)
            {
                Console.WriteLine("[Sequences Omitted]");
            }
            else
            {
                Console.Write("Gn:");
                for (UInt32 i = 0; i < GnList.Count; i++)
                {
                    Console.Write(' ');
                    Console.Write(coprimeNumberFormat, GnList[i]);
                }
                Console.WriteLine();
                Console.WriteLine();

                //
                // Print Indices
                //
                Console.Write("i :");
                for (UInt32 i = 1; i <= cnCount; i++)
                {
                    Console.Write(' ');
                    Console.Write(coprimeNumberFormat, i);
                }
                Console.WriteLine();

                //
                // Print Cn
                //
                Console.Write("Cn:");
                UInt32 currentGnIndex = 0;
                for (UInt32 i = 0; i < cnCount; i++)
                {
                    UInt32 coprime = Cn[i];

                    if (currentGnIndex < GnList.Count && coprime > GnList[currentGnIndex])
                    {
                        currentGnIndex++;
                        Console.Write('|');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                    Console.Write(coprimeNumberFormat, coprime);
                }
                // Print the first coprime of the next sequence
                Console.WriteLine(" ({0})", Cn[cnCount]);
                Console.WriteLine();

                Console.Write("In:");
                // Offset the interval numbers
                for (UInt32 i = 0; i < maxCoprimeDecimalDigits / 2; i++)
                {
                    Console.Write(' ');
                }
                for (UInt32 i = 0; i < cnCount; i++)
                {
                    Console.Write(' ');
                    Console.Write(coprimeNumberFormat, Cn[i + 1] - Cn[i]);
                }
                Console.WriteLine();
            }


            //
            // Generate BPrimes
            //
            UInt64[] bprimes = new UInt64[QnIfSmallEnough * PnPlusOne];
            {
                UInt64 bprimeIndex = 0;
                UInt64 bprimeValueOffset = 0;
                for (UInt64 i = 0; i < PnPlusOne; i++)
                {
                    for (UInt64 j = 0; j < QnIfSmallEnough; j++)
                    {
                        bprimes[bprimeIndex++] = Cn[j] + bprimeValueOffset;
                    }
                    bprimeValueOffset += PrimorialIfSmallEnough;
                }
            }




            //
            // Analysis
            //
            UInt64 maxBprime = bprimes[bprimes.Length - 1];
            UInt64 maxBprimeDecimalDigits = DecimalDigitCount(maxBprime);
            String bprimeNumberFormat = String.Format("{{0,{0}}}", maxBprimeDecimalDigits);

            Console.WriteLine();
            Console.WriteLine("Analyzing B_n+1 (B_{0})", n + 1);
            Console.WriteLine("-----------------------------------");

            /*
            Console.WriteLine();
            Console.WriteLine("B_{0}", n + 1);
            Console.WriteLine("-----------------------------------");
            {
                UInt32 bprimeIndex = 0;
                for (UInt32 i = 1; i <= PnPlusOne; i++)
                {
                    Console.Write("[{0}]", i);

                    for (UInt32 j = 0; j < QnIfSmallEnough; j++)
                    {
                        Console.Write(' ');
                        Console.Write(bprimeNumberFormat, bprimes[bprimeIndex]);
                        bprimeIndex++;
                    }

                    Console.WriteLine();
                }
            }
            */
			
            /*
            UInt32 maxFilterIndex;
            {
                UInt32 filterIndex = 1;
                while (true)
                {
                    //PrintFilterMap(PnPlusOne, (UInt32)QnIfSmallEnough, bprimes, filterIndex);
                    filterIndex++;
                    UInt64 bprimeChecker = bprimes[filterIndex];
                    if (bprimeChecker * bprimeChecker > maxBprime) break;
                }
                maxFilterIndex = filterIndex - 1;
                Console.WriteLine("MaxFilterIndex: {0}, PnPlusOne: {1}", maxFilterIndex, PnPlusOne);
            }
            */

            //
            // Print Combined Filter Map
            //
            /*
            UInt32[] bprimeFilterColumnCount = new UInt32[QnIfSmallEnough];
            UInt32[] bprimeFilterMap = new UInt32[bprimes.Length];
            UInt32 maxFilter;
            {
                UInt32 bprimeIndex;
                for(bprimeIndex = 1; true; bprimeIndex++)
                {
                    UInt64 bprime = bprimes[bprimeIndex];
                    UInt32 multiplierIndex;
                    for (multiplierIndex = bprimeIndex; true; multiplierIndex++)
                    {
                        UInt64 multipler = bprimes[multiplierIndex];
                        UInt64 filterNumber = bprime * multipler;
                        if (filterNumber > maxBprime)
                        {
                            break;
                        }

                        //
                        // Mark filter number
                        //
                        UInt32 filterIndex = (UInt32)((float)filterNumber / (float)maxBprime * (float)bprimes.Length);
                        if (filterIndex >= bprimes.Length) filterIndex = (UInt32)bprimes.Length - 1U;

                        UInt64 guess = bprimes[filterIndex];
                        if (guess == filterNumber)
                        UInt32 modQn = 0;
                        for (UInt32 check = 0; true; check++)
                        {
                            //Console.WriteLine("FilterNumber {0} Guess {1} Correct", filterNumber, guess);
                        }
                        else if(guess > filterNumber)
                        {
                            do
                            {
                                filterIndex--;
                            } while (bprimes[filterIndex] != filterNumber);
                            //Console.WriteLine("FilterNumber {0} Guess {1} Over", filterNumber, guess);
                        }
                        else
                        {
                            do
                            {
                                filterIndex++;
                            } while (bprimes[filterIndex] != filterNumber);
                            //Console.WriteLine("FilterNumber {0} Guess {1} Under", filterNumber, guess);
                        }
                        
                        if (bprimeFilterMap[filterIndex] == 0)
                        {
                            bprimeFilterMap[filterIndex] = bprimeIndex;
                            //Console.WriteLine("filter[{0}] = {1}", filterNumber, bprimeIndex);
                            if (bprimeFilterMap[check] == 0)
                            {
                                bprimeFilterMap[check] = bprimeIndex;
                                bprimeFilterColumnCount[modQn]++;
                                //Console.WriteLine("filter[{0}] = {1}", filterNumber, bprimeIndex);
                            }
                            break;
                            modQn++;
                            if (modQn == QnIfSmallEnough) modQn = 0;
                        }

                    }
                    if (multiplierIndex == bprimeIndex) break;
                }
                maxFilter = bprimeIndex;
            }


            //
            // Search for Horizontal Prime Path
            //
            for (UInt32 i = 0; i < (UInt32)QnIfSmallEnough - 1; i++)
            {
                UInt32 clearPathIndex = i;
                while (true)
                {
                    if (bprimeFilterMap[clearPathIndex] == 0 && bprimeFilterMap[clearPathIndex + 1] == 0)
                    {
                        if (clearPathIndex != i)
                        {
                            //Console.WriteLine("Column {0} had to use lower row", i);
                        }
                        break; // Found clear path
                    }
                    clearPathIndex += (UInt32)QnIfSmallEnough;
                    if (clearPathIndex >= bprimeFilterMap.Length)
                    {
                        Console.WriteLine("Column {0} had no prime path", i);
                        break;
                    }
                }
            }

            
            */




            /*
            UInt64 maxFilterDecimalDigits = DecimalDigitCount(maxFilter);
            String filterNumberFormat = String.Format("{{0,{0}}}", maxFilterDecimalDigits);
            Console.WriteLine();
            Console.WriteLine("FilterMap");
            Console.WriteLine("-----------------------------------");
            {
                UInt32 filterIndex = 0;
                for (UInt32 i = 1; i <= PnPlusOne; i++)
                {
                    Console.Write("[{0,4}]", i);

                    for (UInt32 j = 0; j < QnIfSmallEnough; j++)
                    {
                        Console.Write(' ');
                        Console.Write(filterNumberFormat, bprimeFilterMap[filterIndex]);
                        filterIndex++;
                    }

                    Console.WriteLine();
                }
            }
            */

            //
            // Write Image File
            //
            /*
            using(FileStream stream = new FileStream(String.Format(@"C:\temp\CoprimImage{0}.data", n), FileMode.Create, FileAccess.Write))
            {
                FilterMapToImage(stream, bprimeFilterMap, PnPlusOne, (UInt32)QnIfSmallEnough);
            }
            */




            /*
            //
            // Count the number of primes
            //
            UInt32 currentCoprimeIndex = 1;
            UInt32 nextPrimeIndex = n + 1;
            UInt32 primeCount = 1;
            //Console.WriteLine("Cn,2 = P{0} = {1}", nextPrimeIndex + 1, Cn[1]);
            while (true)
            {
                currentCoprimeIndex++;
                if (currentCoprimeIndex >= cnCount) break;
                UInt32 coprime = Cn[currentCoprimeIndex];

                UInt32 nextPrime = PrimeTable.Values[nextPrimeIndex];
                while (true)
                {
                    if (coprime == nextPrime)
                    {
                        //Console.WriteLine("Cn,{0} = P{1} = {2}", currentCoprimeIndex + 1, nextPrimeIndex + 1, nextPrime);
                        primeCount++;
                        nextPrimeIndex++;
                        break;
                    }
                    currentCoprimeIndex++;
                    if (currentCoprimeIndex >= cnCount) break;
                    coprime = Cn[currentCoprimeIndex];
                }
            }
            Console.WriteLine("There are {0} primes in this coprime set", primeCount);


            //
            // Count Intervals and get max interval
            //
            List<UInt32> intervalCounts = new List<UInt32>();
            for (int i = 0; i < cnCount; i++)
            {
                UInt32 interval = Cn[i + 1] - Cn[i];
                UInt32 intervalIndex = interval / 2 - 1;

                //
                // Add to the interval index
                //
                while (intervalIndex >= intervalCounts.Count)
                {
                    intervalCounts.Add(0);
                }

                intervalCounts[(Int32)intervalIndex]++;
            }

            UInt32 maxInterval = (UInt32)intervalCounts.Count * 2;
            }*/

            /*
            UInt32 max = 0;
            for (int i = 0; i < bprimeFilterColumnCount.Length; i++)
            {
                if (Cn[i + 1] - Cn[i] == 2)
                {
                    UInt32 count = bprimeFilterColumnCount[i];
                    if (count > max) max = count;
                    Console.Write(count);
                }
                else if(i > 0 && Cn[i] - Cn[i-1] == 2)
                {
                    UInt32 count = bprimeFilterColumnCount[i];
                    if (count > max) max = count;
                    Console.Write('-');
                    Console.Write(count);
                    Console.Write("  ");
                }
            }
            Console.WriteLine();
            Console.WriteLine("Max: {0}", max);
            */

            return 0;
        }

        static UInt64 DecimalDigitCount(UInt64 number)
        {
            UInt64 digitCount = 1;
            while (number > 9)
            {
                number /= 10;
                digitCount++;
            }
            return digitCount;
        }

    }
}
