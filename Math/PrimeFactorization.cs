using System;
using System.Collections.Generic;

namespace More
{
    public delegate PoweredPrime[] PrimeFactorizer(UInt32 value);

    //
    // Not Thread Safe
    //
    public class BruteForcePrimeFactorizer
    {
        readonly List<PoweredPrime> primeBuffer;
        public BruteForcePrimeFactorizer()
        {
            primeBuffer = new List<PoweredPrime>();
        }
        public PoweredPrime[] PrimeFactorize(UInt32 value)
        {
            if (value < 2) return null;

            PoweredPrime[] primeFactors;
            lock (primeBuffer)
            {
                PrimeFactorization.BruteFoce(value, primeBuffer);
                primeFactors = primeBuffer.ToArray();
                primeBuffer.Clear();
            }
            return primeFactors;
        }
    }

    public static class PrimeFactorization
    {
        public static void BruteFoce(UInt32 value, List<PoweredPrime> sortedPrimeFactors)
        {
            if (value < 2) return;

            PrimeTableEnumerator primeEnumerator = new PrimeTableEnumerator(1);
            UInt32 currentPrime = 2;
            SByte currentPrimePower = 0;
            while (true)
            {
                if (value % currentPrime == 0)
                {
                    if (currentPrimePower >= SByte.MaxValue) throw new OverflowException(String.Format("Value had a prime factor of {0} with a power greate than 127", currentPrime));
                    currentPrimePower++;

                    //
                    // Divide the value and check if it is done being factored
                    //
                    value = value / currentPrime;
                    if (value == 1)
                    {
                        PoweredPrime poweredPrime;
                        poweredPrime.value = currentPrime;
                        poweredPrime.power = currentPrimePower;
                        sortedPrimeFactors.Add(poweredPrime);
                        return;
                    }
                }
                else
                {
                    if (currentPrimePower > 0)
                    {
                        PoweredPrime poweredPrime;
                        poweredPrime.value = currentPrime;
                        poweredPrime.power = currentPrimePower;
                        sortedPrimeFactors.Add(poweredPrime);
                    }

                    //
                    // Get the next prime
                    //
                    currentPrime = primeEnumerator.Next();
                    currentPrimePower = 0;
                }
            }
        }
    }
    public static class Pollard
    {
        /*
        public static void PrimeFactorization(UInt32 value)
        {
            UInt32 x = 2, y = 2, d = 1;
            while (d == 1)
            {

            }
        }
        */
    }


    /*def gcd(a,b):
        """ the euclidean algorithm """
        while a:
                a, b = b%a, a
        return b
 
def pollard(n):
    x = 2
    y = 2
    d = 1
    while d == 1:
        x = function(x)%n
        y = (function(function(y)%n)%n)
        d = gcd(modulus(x - y), n)
    if d == n:
        return False
    else:
        return d
 
 
def function(x):
    return x*x + 5
 
def modulus(x):
    if x < 0:
        return (-1)*(x)
    else:
        return x*/
}