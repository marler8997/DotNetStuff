using System;
using System.Collections;
using System.Collections.Generic;

namespace More.Physics
{
    public static class PrimeCount
    {
        public static UInt32 UpperBound(UInt32 n)
        {
            return (UInt32)(1.25506 * (double)n / Math.Log(n));
        }
    }


    public class BruteForceMemoryIntensivePrimeEnumerator
    {
        readonly List<UInt32> knownPrimes = new List<UInt32>();
        UInt32 currentPrime;

        public BruteForceMemoryIntensivePrimeEnumerator()
        {
            currentPrime = 3;
            knownPrimes.Add(3);
        }
        public UInt32 NextNoOverflowCheck()
        {
            while (true)
            {
                UInt32 nextPotentialPrime = this.currentPrime += 2;

                Int32 squareRoot = (Int32)Math.Sqrt(nextPotentialPrime);
                for (int i = 0; true; i++)
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
            }
        }
        public static UInt32[] GeneratePrimes(UInt32 primeCount)
        {
            UInt32[] primes = new UInt32[primeCount];

            primes[0] = 2;
            primes[1] = 3;

            UInt32 primeIndex = 2;
            UInt32 nextPotentialPrime = 5;
            while(true)
            {
                Int32 squareRoot = (Int32)Math.Sqrt(nextPotentialPrime);
                for (UInt32 i = 0; true; i++)
                {
                    UInt32 knownPrime = primes[i];
                    if (knownPrime > squareRoot)
                    {
                        primes[primeIndex++] = nextPotentialPrime;
                        if (primeIndex >= primes.Length) return primes;
                        break;
                    }
                    if ((nextPotentialPrime % knownPrime) == 0) break;
                }
                nextPotentialPrime += 2;
            }
            return primes;
        }


        // Starts at 
        /*
        public static UInt32[] GeneratePrimeCountingTable(UInt32 count)
        {
            UInt32[] primeCounts = new UInt32[count];

            primeCounts[0] = 0;
            primeCounts[1] = 0;
            primeCounts[2] = 1;

            UInt32 primeCountIndex = 0;

            for (UInt32 i = 0; i < count; i++)
            {

            }
        }
        */
    }
    public class EratosthenesSeive
    {
        public static UInt32[] GeneratePrimes(UInt32 max, out UInt32 outPrimeCount)
        {
            UInt32[] primes = new UInt32[PrimeCount.UpperBound(max)];
            UInt32 primeCount;

            UInt32 maxSquareRoot = (UInt32)Math.Sqrt(max);
            BitArray isComposite = new BitArray((Int32)max + 1);

            primes[0] = 2;
            primeCount = 1;

            for (Int32 i = 3; i <= max; i += 2)
            {
                if (!isComposite[i])
                {
                    //
                    // Mark all multiples of this prime as composites
                    //
                    if (i <= maxSquareRoot)
                    {
                        for (int j = i * i; j <= max; j += 2 * i)
                        {
                            isComposite[j] = true;
                        }
                    }

                    //
                    // Add to the primes
                    //
                    primes[primeCount++] = (UInt32)i;
                }
            }

            outPrimeCount = primeCount;
            return primes;
        }
    }
}
