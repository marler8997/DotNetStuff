using System;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More.Test
{
    [TestClass]
    public class TestEulersToitentFunction
    {
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
            BruteForcePrimeEnumerator bruteForce = new BruteForcePrimeEnumerator();

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
