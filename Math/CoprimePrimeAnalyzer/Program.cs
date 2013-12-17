using System;
using System.Collections.Generic;
using System.Diagnostics;

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

            {
                UInt32 filterIndex = 1;
                while (true)
                {
                    PrintFilterMap(PnPlusOne, (UInt32)QnIfSmallEnough, bprimes, filterIndex);
                    filterIndex++;
                    UInt64 bprimeChecker = bprimes[filterIndex];
                    if (bprimeChecker * bprimeChecker > maxBprime) break;
                }
            }


            //
            // Print Combined Filter Map
            //
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

                        // Mark filter number
                        for (UInt32 check = 0; true; check++)
                        {
                            UInt64 checkBprime = bprimes[check];
                            if (checkBprime == filterNumber)
                            {
                                if (bprimeFilterMap[check] == 0)
                                {
                                    bprimeFilterMap[check] = bprimeIndex;
                                    //Console.WriteLine("filter[{0}] = {1}", filterNumber, bprimeIndex);
                                }
                                break;
                            }
                        }


                    }
                    if (multiplierIndex == bprimeIndex) break;
                }
                maxFilter = bprimeIndex;
            }

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

            Console.WriteLine();
            Console.WriteLine("Maximum interval is {0}", maxInterval);
            Console.WriteLine();

            UInt32 maxIntervalDecimalDigits = DecimalDigitCount(maxInterval);
            String intervalFormat = String.Format("{{0,{0}}}", maxIntervalDecimalDigits);

            Console.WriteLine("Interval Counts");
            Console.WriteLine("-------------------------");
            UInt32 maxIntervalCount = 0;
            for (UInt32 i = 0; i < intervalCounts.Count; i++)
            {
                UInt32 interval = (i + 1) * 2;
                UInt32 intervalCount = intervalCounts[(Int32)i];

                if (intervalCount > maxIntervalCount) maxIntervalCount = intervalCount;

                Console.WriteLine("{0} : {1}", String.Format(intervalFormat, interval), intervalCount);
            }


            UInt32 maxIntervalCountDecimalDigits = DecimalDigitCount(maxIntervalCount);
            String intervalCountFormat = String.Format("{{0,{0}}}", maxIntervalCountDecimalDigits);



            //
            // Count Intervals Pairs
            //
            List<List<UInt32>> intervalPairCountTable = new List<List<UInt32>>();
            UInt32 maxIntervalRowIndex = 0;
            for (int i = 0; i < cnCount - 1; i++)
            {
                UInt32 firstInterval = Cn[i + 1] - Cn[i];
                UInt32 secondInterval = Cn[i + 2] - Cn[i + 1];
                UInt32 firstIntervalIndex = firstInterval / 2 - 1;
                UInt32 secondIntervalIndex = secondInterval / 2 - 1;

                if (secondIntervalIndex > maxIntervalRowIndex)
                {
                    maxIntervalRowIndex = secondIntervalIndex;
                }

                //
                // Add to the interval pair to the table
                //
                while (firstIntervalIndex >= intervalPairCountTable.Count)
                {
                    intervalPairCountTable.Add(new List<UInt32>());
                }

                List<UInt32> intervalRow = intervalPairCountTable[(Int32)firstIntervalIndex];
                while (secondIntervalIndex >= intervalRow.Count)
                {
                    intervalRow.Add(0);
                }
                intervalRow[(Int32)secondIntervalIndex]++;
            }
            Console.WriteLine();
            Console.WriteLine("Interval Pair Map:");
            Console.Write("{0} :", String.Format(intervalFormat, ""));
            for (UInt32 intervalIndex = 0; intervalIndex <= maxIntervalRowIndex; intervalIndex++)
            {
                UInt32 interval = (intervalIndex + 1) * 2;
                Console.Write(" {0}", String.Format(intervalCountFormat, interval));
            }
            Console.WriteLine();
            for (UInt32 i = 0; i < intervalPairCountTable.Count; i++)
            {
                UInt32 firstInterval = (i + 1) * 2;
                Console.Write("{0} :", String.Format(intervalFormat, firstInterval));
                List<UInt32> intervalRow = intervalPairCountTable[(Int32)i];
                UInt32 j;
                for (j = 0; j < intervalRow.Count; j++)
                {
                    UInt32 secondInterval = (j + 1) * 2;
                    Console.Write(" {0}", String.Format(intervalCountFormat, intervalRow[(Int32)j]));
                }
                for (; j <= maxIntervalRowIndex; j++)
                {
                    Console.Write(" {0}", String.Format(intervalCountFormat, 0));
                }
                Console.WriteLine();
            }



            //
            // Check distances between 2s
            //
            //Console.Write("Distances between 2s:");
            Int32 last2Index = 0;
            Int32 maxDistance = 0;
            for (int i = 0; i < cnCount; i++)
            {
                if (Cn[i + 1] - Cn[i] == 2)
                {
                    Int32 distance = i - last2Index;
                    //Console.Write(" {0}", distance);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                    }
                    last2Index = i;
                }
            }
            Console.WriteLine();
            Console.WriteLine("Max distance between 2s: {0}", maxDistance);

            //
            // Find intervals between Twin Coprimes
            //
            Int32 lastTwinCoprime = -1;
            UInt32 maxTwinCoprimeDistance = 0;
            for (UInt32 i = 0; i < cnCount - 1; i++)
            {
                UInt32 coprime = Cn[i];
                if (Cn[i + 1] - coprime == 2)
                {
                    UInt32 distance = (UInt32)((Int32)coprime - lastTwinCoprime);
                    //Console.WriteLine("Cn[{0}] = {1} is a twin coprime (distance = {2})", i, coprime, distance);
                    if (distance > maxTwinCoprimeDistance)
                    {
                        maxTwinCoprimeDistance = distance;
                    }
                    lastTwinCoprime = (Int32)coprime;
                }
            }
            Console.WriteLine("Max Twin Coprime Distance: {0}", maxTwinCoprimeDistance);

            //
            // Checks twin coprimes between filter numbers
            //
            Console.WriteLine();
            Console.WriteLine("Twin Coprimes Between Filter Numbers:");
            UInt32 currentFilterNumberIndex = 1;
            UInt32 previousFilterNumber = GnList[0];
            UInt32 currentFilterNumber = GnList[1];
            UInt32 currentTwinCoprimeCount = 0;
            for (UInt32 i = 1; i < cnCount; i++)
            {
                UInt32 coprime = Cn[i];
                if (coprime > currentFilterNumber)
                {
                    //Console.WriteLine("There are {0} twin coprimes between {1} and {2}",
                    //    currentTwinCoprimeCount, previousFilterNumber, currentFilterNumber);
                    currentFilterNumberIndex++;
                    if (currentFilterNumberIndex >= GnList.Count) break;
                    previousFilterNumber = currentFilterNumber;
                    currentFilterNumber = GnList[currentFilterNumberIndex];
                    currentTwinCoprimeCount = 0;
                }

                if (Cn[i + 1] - coprime == 2)
                {
                    currentTwinCoprimeCount++;
                }
            }

            //
            // Check palindromic
            //
            Console.WriteLine();
            if (cnCount == QnIfSmallEnough)
            {
                Int32 pandoromeMismatches = 0;
                for (int i = 0; i < cnCount - 1; i++)
                {
                    if (Cn[i + 1] - Cn[i] != Cn[cnCount - i - 1] - Cn[cnCount - i - 2])
                    {
                        Console.WriteLine("Palindrome mismatch at {0}", i + 1);
                        pandoromeMismatches++;
                    }
                }
                if (pandoromeMismatches == 0)
                {
                    Console.WriteLine("In is a palindrome");
                }
                else
                {
                    Console.WriteLine("There were {0} palindrome mismatches", pandoromeMismatches);
                }
            }

            //
            // Count filter numbers between 1 and Pn-1#
            //
            UInt32 limitedFilterNumberCount = 0;
            UInt32 filterNumberLimit = (UInt32)((double)PrimorialIfSmallEnough / (double)PnPlusOne);
            for (UInt32 i = 0; i < QnIfSmallEnough; i++)
            {
                if (Cn[i] > filterNumberLimit) break;
                limitedFilterNumberCount++;

            }
            Console.WriteLine();
            Console.WriteLine("There are {0} coprimes between 1 and {1}",
                limitedFilterNumberCount, filterNumberLimit);




            //
            // Check mods
            //
            for (UInt32 i = 0; i < cnCount; i++)
            {
                //Console.WriteLine("Cn[{0}] mod {1} = {2}", i + 1, Pn, Cn[i] % Pn);
            }

            Console.WriteLine("Pn = {0}", Pn);
            for (UInt32 i = 0; i < GnList.Count; i++)
            {
                //Console.WriteLine("Gn[{0}] = {1} mod {2} = {3}", i + 1, GnBuffer[i], Pn, GnBuffer[i] % Pn);
            }


            //
            // Generate CnMinusOne
            //

            ISimpleList<UInt32> GnMinusOneList = new DynamicSimpleList<UInt32>();
            UInt32[] CnMinusOne = Coprimes.BruteForceCreateCn((UInt32)(n - 1), (UInt32)(Coprimes.CalculateQn(n - 1) * Pn), GnMinusOneList);


            Console.WriteLine();
            UInt32 QnMinusOne = (UInt32)Coprimes.CalculateQn(n - 1);
            Console.WriteLine("Qn-1 = {0}, Pn = {1}", QnMinusOne, Pn);
            Console.WriteLine();
            Console.WriteLine("The first {0} values", QnMinusOne);
            for (UInt32 i = 0; i < QnMinusOne; i++)
            {
                //Console.WriteLine("Cn-1[{0,3}] = {1,3} mod {2} = {3} (k mod {4} = {5})",
                //    i + 1, CnMinusOne[i], Pn, CnMinusOne[i] % Pn, QnMinusOne, (i % QnMinusOne) + 1);
            }
            Console.WriteLine();
            UInt32 modZeroCount = 0;

            UInt32 lastImportantMod = 0;
            UInt32 lastImportantModCount = 0;

            List<UInt32> importantMods = new List<UInt32>();
            List<UInt32> importantModCounts = new List<UInt32>();

            for (UInt32 i = 0; i < CnMinusOne.Length; i++)
            {
                if (CnMinusOne[i] % Pn == 0)
                {
                    modZeroCount++;

                    UInt32 importantMod = CnMinusOne[(i % QnMinusOne)] % Pn;
                    if (importantMod == lastImportantMod)
                    {
                        lastImportantModCount++;
                    }
                    else
                    {
                        Console.WriteLine("{0} Entries with mod {1}", lastImportantModCount, lastImportantMod);
                        importantMods.Add(lastImportantMod);
                        importantModCounts.Add(lastImportantModCount);
                        lastImportantMod = importantMod;
                        lastImportantModCount = 0;
                    }

                    //Console.WriteLine("Fn[{0,3}] = Cn-1[{1,3}] = {2,4} mod {3} = {4} (k mod {5} = {6,3}) (Cn-1[{6,3}] = {7,3} mod {3,3} = {8})",
                    //    modZeroCount,i + 1, CnMinusOne[i], Pn, CnMinusOne[i] % Pn, QnMinusOne, (i % QnMinusOne) + 1, CnMinusOne[(i % QnMinusOne)], CnMinusOne[(i % QnMinusOne)] % Pn);

                }
            }

            for (int i = 0; i < importantMods.Count; i++)
            {
                Console.WriteLine("Count {0,4} Mod {1}", importantModCounts[i], importantMods[i]);
            }


            //
            //
            //
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
