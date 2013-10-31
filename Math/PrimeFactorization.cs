using System;
using System.Collections.Generic;

namespace More
{
    public interface IPrimeFactorizer
    {
        PoweredPrime[] PrimeFactorize(UInt32 value);
        PoweredPrime[] Divide(UInt32 value, PoweredPrime[] denominatorFactors);        
    }


    //public delegate PoweredPrime[] PrimeFactorizer(UInt32 value);

    //
    // Not Thread Safe
    //
    public class BruteForcePrimeFactorizer : IPrimeFactorizer
    {
        readonly List<PoweredPrime> sharedPrimeList;
        public BruteForcePrimeFactorizer()
        {
            sharedPrimeList = new List<PoweredPrime>();
        }
        public PoweredPrime[] PrimeFactorize(UInt32 value)
        {
            if (value < 2) return PoweredPrime.None;

            PoweredPrime[] primeFactors;
            lock (sharedPrimeList)
            {
                PrimeFactorization.BruteFoce(value, sharedPrimeList);
                primeFactors = sharedPrimeList.ToArray();
                sharedPrimeList.Clear();
            }
            return primeFactors;
        }
        public PoweredPrime[] Divide(UInt32 value, PoweredPrime[] denominatorFactors)
        {
            PoweredPrime[] resultingFactors;
            lock (sharedPrimeList)
            {
                PrimeFactorization.BruteFoce(value, sharedPrimeList);

                denominatorFactors.DivideInto(sharedPrimeList);
                resultingFactors = sharedPrimeList.ToArray();
                sharedPrimeList.Clear();
            }
            return resultingFactors;
        }
    }

    public class CachingFactorizer : IPrimeFactorizer
    {
        readonly IPrimeFactorizer underlyingFactorizer;

        readonly Dictionary<UInt32, PoweredPrime[]> cachedUInt32Factorizations = new Dictionary<UInt32, PoweredPrime[]>();
        readonly Dictionary<Rational, FactoredRational> PositiveRationalFactorizations = new Dictionary<Rational, FactoredRational>();

        public CachingFactorizer(IPrimeFactorizer underlyingFactorizer)
        {
            if (underlyingFactorizer == null) throw new ArgumentNullException("underlyingFactorizer");
            this.underlyingFactorizer = underlyingFactorizer;
        }
        public PoweredPrime[] PrimeFactorize(UInt32 value)
        {
            PoweredPrime[] factors;
            if (cachedUInt32Factorizations.TryGetValue(value, out factors)) return factors;

            factors = underlyingFactorizer.PrimeFactorize(value);

            if (factors == null) return null;
            
            cachedUInt32Factorizations.Add(value, factors);
            return factors;
        }
        public PoweredPrime[] Divide(UInt32 value, PoweredPrime[] denominatorFactors)
        {
            return underlyingFactorizer.Divide(value, denominatorFactors);
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
                        PoweredPrime poweredPrime = new PoweredPrime(currentPrime, currentPrimePower);
                        sortedPrimeFactors.Add(poweredPrime);
                        return;
                    }
                }
                else
                {
                    if (currentPrimePower > 0)
                    {
                        PoweredPrime poweredPrime = new PoweredPrime(currentPrime, currentPrimePower);
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