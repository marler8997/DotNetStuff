using System;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More.Physics.Test
{
    [TestClass]
    public class TestEulersToitentFunction
    {
        [TestMethod]
        public void GenerateResarchNumbers()
        {
            Int32 last = 1;
            Int32 printCount = 0;
            for (int i = 2; i <= 680000; i++)
            {
                if (
                    (i % 2 != 0) &&
                    (i % 3 != 0) &&
                    (i % 5 != 0) &&
                    (i % 7 != 0) &&
                    (i % 11 != 0) &&
                    (i % 13 != 0) &&
                    (i % 17 != 0) 
                    )
                {
                    Console.Write("{0,4},", i - last);
                    printCount++;
                    last = i;
                    if (printCount % 16 == 0) Console.WriteLine();
                }
            }
        }

        [TestMethod]
        public void TestEratosthenesSeivePrimeGenerators()
        {
            TestEratosthenesSeivePrimeGenerators(55000);
            //TestEratosthenesSeivePrimeGenerators(2147483647); (Int32.MaxValue)
            //TestEratosthenesSeivePrimeGenerators(2147483616); (Get Arithmetic Operation Exception)
            //TestEratosthenesSeivePrimeGenerators(2147483615);
        }
        void TestEratosthenesSeivePrimeGenerators(UInt32 maxPrime)
        {
            BruteForceMemoryIntensivePrimeEnumerator bruteForce = new BruteForceMemoryIntensivePrimeEnumerator();

            long before;

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            for (UInt32 i = 3; i <= maxPrime; i++)
            {
                before = Stopwatch.GetTimestamp();

                UInt32 calculatedPrimeCount;

                try
                {
                    UInt32[] eratosthenesPrimes = EratosthenesSeive.GeneratePrimes(i, out calculatedPrimeCount);
                }
                catch (IndexOutOfRangeException)
                {
                    Console.WriteLine("Failed at maxprime={0}", i);
                    throw;
                }

                //Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
                //Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
                //Console.WriteLine("PrimeCount {0}", calculatedPrimeCount);
            }
        }
    }
}
