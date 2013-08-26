using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More.Test
{
    public struct CompositAndPercentageChecked
    {
        public readonly UInt32 composite;
        public readonly UInt32 lowestDivisor;
        public readonly Single percentageChecked;
        public CompositAndPercentageChecked(UInt32 composite, UInt32 lowestDivisor, Single percentageChecked)
        {
            this.composite = composite;
            this.lowestDivisor = lowestDivisor;
            this.percentageChecked = percentageChecked;
        }
    }
    public class BruteForceMemoryIntensivePrimeEnumeratorForTest
    {

        readonly List<UInt32> knownPrimes = new List<UInt32>();
        UInt32 currentPrime;

        public readonly List<CompositAndPercentageChecked> compositePercentageChecked = new List<CompositAndPercentageChecked>();
        public readonly UInt32[] percentageCheckedHistogram = new UInt32[101];

        public BruteForceMemoryIntensivePrimeEnumeratorForTest()
        {
            currentPrime = 3;
            knownPrimes.Add(3);
        }
        public UInt32 NextNoOverflowCheck()
        {
            while (true)
            {
                UInt32 nextPotentialPrime = this.currentPrime += 2;

                Int32 squareRoot = (Int32)Math.Sqrt(currentPrime);
                Int32 i;
                for (i = 0; true; i++)
                {
                    UInt32 knownPrime = knownPrimes[i];
                    if (knownPrime > squareRoot)
                    {
                        knownPrimes.Add(nextPotentialPrime);
                        this.currentPrime = nextPotentialPrime;
                        return nextPotentialPrime;
                    }
                    if ((nextPotentialPrime % knownPrime) == 0) break;
                }

                // Number is composite, save statistics
                Single percentageChecked = (float)(100 * i) / (float)knownPrimes.Count;
                compositePercentageChecked.Add(new CompositAndPercentageChecked(currentPrime, knownPrimes[i], percentageChecked));
                percentageCheckedHistogram[(UInt32)Math.Ceiling(percentageChecked)]++;
            }
        }
    }

    [TestClass]
    public class TestFactoredAndPrimes
    {
        [TestMethod]
        public void TestPercentageChecked()
        {
            BruteForceMemoryIntensivePrimeEnumeratorForTest primeEnumerator =
                new BruteForceMemoryIntensivePrimeEnumeratorForTest();
            for (int i = 0; i < 10000; i++)
            {
                primeEnumerator.NextNoOverflowCheck();
            }

            for (int i = 0; i < primeEnumerator.percentageCheckedHistogram.Length; i++)
            {
                Console.WriteLine("{0,3}%: {1}", i, primeEnumerator.percentageCheckedHistogram[i]);
            }

            /*
            //
            // Write to file
            //
            using(StreamWriter writer = new StreamWriter(new FileStream(@"C:\temp\composites.csv", FileMode.Create, FileAccess.Write)))
            {
                writer.WriteLine("Composite,LowestDivisor,LowestDivisorOverPrimeCount");
                for(int i = 0; i < primeEnumerator.compositePercentageChecked.Count; i++)
                {
                    CompositAndPercentageChecked c = primeEnumerator.compositePercentageChecked[i];
                    writer.WriteLine("{0},{1},{2:00.0}", c.composite, c.lowestDivisor, c.percentageChecked);
                }
            }
            */
        }

        readonly StringBuilder builder = new StringBuilder();

        [TestMethod]
        public void TestFactorizers()
        {
            BruteForcePrimeFactorizer bruteForceFactorizer = new BruteForcePrimeFactorizer();

            TestFactorizer(bruteForceFactorizer.PrimeFactorize);
            //GetMaxPrime(bruteForceFactorizer.PrimeFactorize, 1024);

        }

        void TestFactorizer(PrimeFactorizer factorizer)
        {
            Assert.IsNull(factorizer(0));
            Assert.IsNull(factorizer(1));
            TestFactorize(factorizer,  2, new PoweredPrime(2, 1));
            TestFactorize(factorizer,  3, new PoweredPrime(3, 1));
            TestFactorize(factorizer,  4, new PoweredPrime(2, 2));
            TestFactorize(factorizer,  5, new PoweredPrime(5, 1));
            TestFactorize(factorizer,  6, new PoweredPrime(2, 1), new PoweredPrime(3, 1));
            TestFactorize(factorizer,  7, new PoweredPrime(7, 1));
            TestFactorize(factorizer,  8, new PoweredPrime(2, 3));
            TestFactorize(factorizer,  9, new PoweredPrime(3, 2));
            TestFactorize(factorizer, 10, new PoweredPrime(2, 1), new PoweredPrime(5, 1));
            TestFactorize(factorizer, 11, new PoweredPrime(11, 1));
            TestFactorize(factorizer, 12, new PoweredPrime(2, 2), new PoweredPrime(3, 1));
            TestFactorize(factorizer, 13, new PoweredPrime(13, 1));
            TestFactorize(factorizer, 14, new PoweredPrime(2, 1), new PoweredPrime(7, 1));
            TestFactorize(factorizer, 15, new PoweredPrime(3, 1), new PoweredPrime(5, 1));
            TestFactorize(factorizer, 16, new PoweredPrime(2, 4));
            TestFactorize(factorizer, 17, new PoweredPrime(17, 1));
            TestFactorize(factorizer, 18, new PoweredPrime(2, 1), new PoweredPrime(3, 2));
            TestFactorize(factorizer, 19, new PoweredPrime(19, 1));
        }

        void TestFactorize(PrimeFactorizer factorizer, UInt32 value, params PoweredPrime[] expectedPrimeFactors)
        {
            PoweredPrime[] calculatedPrimeFactors = factorizer(value);

            builder.Length = 0;
            calculatedPrimeFactors.SerializeArray(builder);
            Console.WriteLine("Value '{0}' PrimeFactors: {1}", value, builder.ToString());

            String sosDiff = expectedPrimeFactors.Diff(calculatedPrimeFactors);

            if (sosDiff != null)
            {
                Assert.Fail("Diff {0}", sosDiff);
            }
        }

        void GetMaxPrime(PrimeFactorizer factorizer, UInt32 printEvery)
        {
            UInt32 i = 0;
            while (true)
            {
                try
                {
                    PoweredPrime[] primeFactors = factorizer(i);
                    if (i % printEvery == 0)
                    {
                        builder.Length = 0;
                        primeFactors.SerializeArray(builder);
                        Console.WriteLine("Value '{0}' PrimeFactors: {1}", i, builder.ToString());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("At {0}, got exception {1}", i, e);
                    throw;
                }
                i++;
            }
        }

        [TestMethod]
        public void TestFactoredRationals1()
        {


        }
    }
}
