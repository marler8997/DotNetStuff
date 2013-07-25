using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More.Physics.Test
{
    [TestClass]
    public class PerformanceChecks
    {
        [TestMethod]
        public void PerformanceTestPrimeGenerators()
        {
            PerformanceTestPrimeGenerators(1000000);
        }
        void PerformanceTestPrimeGenerators(UInt32 primeCount)
        {
            BruteForceMemoryIntensivePrimeEnumerator bruteForce = new BruteForceMemoryIntensivePrimeEnumerator();

            long before;

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            UInt32 maxPrime = 2;

            before = Stopwatch.GetTimestamp();

            UInt32[] bruteForcePrimes = BruteForceMemoryIntensivePrimeEnumerator.GeneratePrimes(primeCount);
            maxPrime = bruteForcePrimes[primeCount - 1];
            //Console.WriteLine(bruteForcePrimes.SerializeObject());

            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            before = Stopwatch.GetTimestamp();

            UInt32 calculatedPrimeCount;

            UInt32[] eratosthenesPrimes = EratosthenesSeive.GeneratePrimes(maxPrime, out calculatedPrimeCount);

            //Console.WriteLine(primes.SerializeObject());
            Assert.AreEqual(primeCount, calculatedPrimeCount);

            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            //
            // Test that the primes are the same
            //
            /*
            for (int i = 0; i < primeCount; i++)
            {
                Assert.AreEqual(eratosthenesPrimes[i], bruteForcePrimes[i]);
            }
            */
        }


        [TestMethod]
        public void PerformanceTestEratosthenesSeivePrimeGenerators()
        {
          //PerformanceTestEratosthenesSeivePrimeGenerators(2147483647); //(Int32.MaxValue)
          //PerformanceTestEratosthenesSeivePrimeGenerators(2147483615); (Problem with BitArray)
            // 39115
            // 39107
            PerformanceTestEratosthenesSeivePrimeGenerators(2147391110);
        }
        void PerformanceTestEratosthenesSeivePrimeGenerators(UInt32 maxPrime)
        {
            BruteForceMemoryIntensivePrimeEnumerator bruteForce = new BruteForceMemoryIntensivePrimeEnumerator();

            long before;

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            before = Stopwatch.GetTimestamp();

            UInt32 calculatedPrimeCount;
            UInt32[] eratosthenesPrimes = EratosthenesSeive.GeneratePrimes(maxPrime, out calculatedPrimeCount);

            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            Console.WriteLine("PrimeCount {0}", calculatedPrimeCount);
        }






        [TestMethod]
        public void PerformanceTestPrimeEnumerators()
        {
            PerformanceTestPrimeEnumerators(LimitedTablePrimeEnumerator.PrimeCount);
        }
        void PerformanceTestPrimeEnumerators(UInt32 primeCount)
        {
            LimitedTablePrimeEnumerator tablePrimeEnumerator = new LimitedTablePrimeEnumerator(0);

            long before;

            UInt32 prime;

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1; i++)
            {
                tablePrimeEnumerator.SetNextPrimeIndex(0);
                for (UInt32 j = 0; j < primeCount; j++)
                {
                    prime = tablePrimeEnumerator.Next();
                }
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
        }
    }
}
