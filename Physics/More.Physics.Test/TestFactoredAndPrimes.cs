using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More.Physics.Test
{
    [TestClass]
    public class TestFactoredAndPrimes
    {
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
