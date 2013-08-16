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
            BruteForcePrimeEnumerator bruteForce = new BruteForcePrimeEnumerator();

            long before;

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            UInt32 maxPrime;

            before = Stopwatch.GetTimestamp();

            UInt32[] bruteForcePrimes = BruteForcePrimeEnumerator.GeneratePrimes(primeCount);

            maxPrime = bruteForcePrimes[primeCount - 1];
            Console.WriteLine("PrimeCount {0} MaxPrime {1}", primeCount, maxPrime);
            //Console.WriteLine(bruteForcePrimes.SerializeObject());

            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            before = Stopwatch.GetTimestamp();

            UInt32 calculatedPrimeCount;

            //
            // Eratosthenes Using Prime Value
            //
            UInt32[] eratosthenesPrimesUsingPrimeValue = EratosthenesSeive.GeneratePrimes(maxPrime, out calculatedPrimeCount);
            Assert.AreEqual(primeCount, calculatedPrimeCount);

            //Console.WriteLine(primes.SerializeObject());

            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            //
            // Atkins
            //
            /*
            UInt32[] atkinsPrimes = AtkinSeive.GeneratePrimes(maxPrime, out calculatedPrimeCount);

            Console.WriteLine(atkinsPrimes.SerializeObject());
            Assert.AreEqual(primeCount, calculatedPrimeCount);
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
            */

            //
            // Test that the primes are the same
            //
            for (int i = 0; i < primeCount; i++)
            {
                Assert.AreEqual(eratosthenesPrimesUsingPrimeValue[i], bruteForcePrimes[i]);
                //Assert.AreEqual(atkinsPrimes[i], bruteForcePrimes[i]);
            }
        }

        [TestMethod]
        public void PerformanceTestEratosthenesSeivePrimeGenerators()
        {
            //PerformanceTestEratosthenesSeivePrimeGenerators(0xC0000000); // OutOfMemoryException
            //PerformanceTestEratosthenesSeivePrimeGenerators(0xB7000000);

            //PerformanceTestEratosthenesSeivePrimeGenerators(20000);
            //PerformanceTestEratosthenesSeivePrimeGenerators(200000);
            //PerformanceTestEratosthenesSeivePrimeGenerators(2000000);
            //PerformanceTestEratosthenesSeivePrimeGenerators(20000000);
            PerformanceTestEratosthenesSeivePrimeGenerators(1000000000);
            //PerformanceTestEratosthenesSeivePrimeGenerators(2000000000);
        }
        void PerformanceTestEratosthenesSeivePrimeGenerators(UInt32 maxPrime)
        {
            BruteForcePrimeEnumerator bruteForce = new BruteForcePrimeEnumerator();

            long before;

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            before = Stopwatch.GetTimestamp();

            UInt32 calculatedPrimeCount;
            UInt32[] eratosthenesPrimes = EratosthenesSeive.GeneratePrimes(maxPrime, out calculatedPrimeCount);

            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            Console.WriteLine("GC({0},{1},{2})", GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));

            Console.WriteLine("PrimeCount {0} ArrayLength {1} ({2:00.0}% of Array Used)", calculatedPrimeCount,
                eratosthenesPrimes.Length, 100f*(float)calculatedPrimeCount / (float)eratosthenesPrimes.Length);
        }

        [TestMethod]
        public void PerformanceTestPrimeEnumerators()
        {
            PerformanceTestPrimeEnumerators(PrimeTable.Length);
        }
        void PerformanceTestPrimeEnumerators(UInt32 primeCount)
        {
            PrimeTableEnumerator tablePrimeEnumerator = new PrimeTableEnumerator(0);

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
