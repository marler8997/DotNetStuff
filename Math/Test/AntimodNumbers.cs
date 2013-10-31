using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More.Test
{
    [TestClass]
    public class AntimodNumbers
    {

        // An Antimod 'X' number is a number that is not X mod n for all primes less than or
        // equal to the square root of itself.  Why are these interesting? Because I think
        // (though have not yet proven) that the Antimod0 numbers are equal to the prime numbers
        // 
        [TestMethod]
        public void CreateAntimodOneNumbers()
        {
            CreateAntimodXNumbers(100, 1);
        }

        public void CreateAntimodXNumbers(UInt32 max, UInt32 x)
        {
            for (UInt32 i = 1; i <= max; i++)
            {
                UInt32 squareRootOfMax = (UInt32)System.Math.Sqrt(i);

                Boolean isUnModOne = true;

                // Check all primes
                for (UInt32 primeIndex = 0; PrimeTable.Values[primeIndex] <= squareRootOfMax; primeIndex++)
                {
                    if (i % PrimeTable.Values[primeIndex] == x)
                    {
                        Console.WriteLine("{0} is not unmodone because it is 1 mod {1}", i, PrimeTable.Values[primeIndex]);
                        isUnModOne = false;
                        break;
                    }
                }

                if (isUnModOne)
                {
                    Console.WriteLine(i);
                }
            }
        }
    }
}
