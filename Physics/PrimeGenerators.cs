using System;
using System.Collections;
using System.Collections.Generic;

namespace More.Physics
{
    public static class PrimeCount
    {
        static readonly UInt32[] LowerBoundValuesUpTo17 = new UInt32[] {
            0, 0,
            1,
            2, 2,
            3, 3,
            4, 4, 4, 4,
            5, 5,
            6, 6, 6, 6,
            7,
        };
        public static UInt32 UpperBound(UInt32 n)
        {
            return (UInt32)(1.25506 * (double)n / Math.Log(n));
        }
        public static UInt32 LowerBound(UInt32 n)
        {
            if (n <= 17) return LowerBoundValuesUpTo17[n];
            return (UInt32)((double)n / Math.Log(n));
        }
    }
    public static class PrimeApproximations
    {
        // prime at primeIndex of 0 = 2
        // prime at primeIndex of 1 = 3
        // ...
        public static UInt32 UpperBoundOfPrimeAtIndex(UInt32 primeIndex)
        {
            if (primeIndex < PrimeTable.Length) return PrimeTable.Values[primeIndex];
            UInt32 n = primeIndex + 1;
            return (UInt32)Math.Ceiling((Double)n * Math.Log(n));
        }
    }

    public class BruteForcePrimeEnumerator
    {
        public static UInt32[] GeneratePrimes(UInt32 primeCount)
        {
            if (primeCount < 2) throw new ArgumentOutOfRangeException("primeCount");

            UInt32[] primes = new UInt32[primeCount];

            primes[0] = 2;
            primes[1] = 3;

            UInt32 primeIndex = 2;
            UInt32 nextPotentialPrime = 5;
            while (true)
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
        }


        readonly List<UInt32> knownPrimes = new List<UInt32>();
        UInt32 currentPrime;

        public BruteForcePrimeEnumerator()
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
    }
    public class EratosthenesSeive
    {
        //
        // On an HP Z600, It takes about 38 seconds to generate
        // the first 150 million primes (about 4 million primes per second)
        //
        public static UInt32[] GeneratePrimes(UInt32 max, out UInt32 outPrimeCount)
        {
            if (max == UInt32.MaxValue) throw new ArgumentOutOfRangeException("max", "max cannot be UInt32.MaxValue");

            UInt32[] primes = new UInt32[PrimeCount.UpperBound(max)];
            UInt32 primeCount;

            UInt32 maxSquareRoot = (UInt32)Math.Sqrt(max);
            BetterBitArray isComposite = new BetterBitArray((max + 1) >> 1);

            primes[0] = 2;
            primes[1] = 3;
            primeCount = 2;

            UInt32 candidatePrime;
            UInt32 incrementAmount = 0;
            for (candidatePrime = 5; candidatePrime <= maxSquareRoot; candidatePrime += incrementAmount)
            {
                if (!isComposite.Get(candidatePrime >> 1))
                {
                    //
                    // Mark all multiples of this prime as composites
                    //
                    UInt32 candidatePrimeDoubled = 2 * candidatePrime;
                    for (UInt32 j = candidatePrime * candidatePrime; j <= max; j += candidatePrimeDoubled)
                    {
                        isComposite.Assert(j >> 1);
                    }

                    //
                    // Add to the primes
                    //
                    primes[primeCount++] = candidatePrime;
                }

                incrementAmount = (incrementAmount == 2) ? 4U : 2U;
            }

            // Add the rest of the primes
            for (; candidatePrime <= max; candidatePrime += incrementAmount)
            {
                if (!isComposite.Get(candidatePrime >> 1)) primes[primeCount++] = candidatePrime;
                incrementAmount = (incrementAmount == 2) ? 4U : 2U;
            }

            outPrimeCount = primeCount;
            return primes;
        }
        /*
        public static UInt32[] GeneratePrimes(UInt32 primeCount)
        {
            UInt32[] primes = new UInt32[primeCount];

            UInt32 primeUpperBound = PrimeApproximations.UpperBoundOfPrimeAtIndex(primeCount - 1);

            UInt32 primeUpperBoundSquareRoot = (UInt32)Math.Sqrt(primeUpperBound);
            BetterBitArray isComposite = new BetterBitArray((primeUpperBound + 1) >> 1);

            primes[0] = 2;
            primes[1] = 3;
            UInt32 currentPrimeCount = 2;

            UInt32 candidatePrime;
            UInt32 incrementAmount = 0;
            for (candidatePrime = 5; candidatePrime <= primeUpperBoundSquareRoot; candidatePrime += incrementAmount)
            {
                if (!isComposite.Get(candidatePrime >> 1))
                {
                    //
                    // Mark all multiples of this prime as composites
                    //
                    UInt32 candidatePrimeDoubled = 2 * candidatePrime;
                    for (UInt32 j = candidatePrime * candidatePrime; j <= primeUpperBound; j += candidatePrimeDoubled)
                    {
                        isComposite.Assert(j >> 1);
                    }

                    //
                    // Add to the primes
                    //
                    primes[currentPrimeCount++] = candidatePrime;
                }

                incrementAmount = (incrementAmount == 2) ? 4U : 2U;
            }

            if (currentPrimeCount >= primeCount) return primes;

            // Add the rest of the primes
            while(true)
            {
                if (!isComposite.Get(candidatePrime >> 1))
                {
                    primes[currentPrimeCount++] = candidatePrime;
                    if (currentPrimeCount >= primeCount) return primes;
                }

                incrementAmount = (incrementAmount == 2) ? 4U : 2U;
                candidatePrime += incrementAmount;

                if (candidatePrime > primeUpperBound) throw new InvalidOperationException("CodeBug");
            }
        }
        */
    }
    public class AtkinSeive
    {
        public static UInt32[] GeneratePrimes(UInt32 max, out UInt32 outPrimeCount)
        {
            if (max < 5) throw new ArgumentOutOfRangeException("max", "max can't be too small");
            if (max == UInt32.MaxValue) throw new ArgumentOutOfRangeException("max", "max cannot be UInt32.MaxValue");

            UInt32[] primes = new UInt32[PrimeCount.UpperBound(max)];
            UInt32 primeCount;

            UInt32 maxSquareRoot = (UInt32)Math.Sqrt(max);
            BetterBitArray isPrime = new BetterBitArray(max + 1);

            // put in candidate primes
            for (UInt32 i = 1; i <= maxSquareRoot; i++)
            {
                for (UInt32 j = 1; j <= maxSquareRoot; j++)
                {
                    UInt32 n;

                    n = 4 * i + j;
                    //if (n > max) throw new InvalidOperationException();
                    if (n <= max) // Should always be true for large enough max (maybe take this out0
                    {
                        UInt32 nMod12 = n % 12;
                        if (nMod12 == 1 || nMod12 == 5)
                        {
                            isPrime.Flip(n);
                        }
                    }

                    n = 3 * i + j;
                    //if (n > max) throw new InvalidOperationException();
                    if (n <= max) // Should always be true for large enough max (maybe take this out0
                    {
                        UInt32 nMod12 = n % 12;
                        if (nMod12 == 7)
                        {
                            isPrime.Flip(n);
                        }
                    }

                    if (i > j)
                    {
                        n = 3 * i - j;
                        //if (n > max) throw new InvalidOperationException();
                        if (n <= max) // Should always be true for large enough max (maybe take this out0
                        {
                            UInt32 nMod12 = n % 12;
                            if (nMod12 == 11)
                            {
                                isPrime.Flip(n);
                            }
                        }
                    }
                }
            }

            primes[0] = 2;
            primes[1] = 3;
            primeCount = 2;

            UInt32 candidatePrime;
            for (candidatePrime = 5; candidatePrime <= maxSquareRoot; candidatePrime += 2)
            {
                if (isPrime.Get(candidatePrime))
                {
                    //
                    // Mark all multiples of its square as composite
                    //
                    //
                    UInt32 candidatePrimeSquared = candidatePrime * candidatePrime;
                    //UInt32 candidatePrimeSquaredThenDoubled = iSquared * 2;
                    for (UInt32 j = candidatePrimeSquared; j <= max; j += candidatePrimeSquared)
                    {
                        isPrime.Deassert(j);
                    }

                    //
                    // Add to the primes
                    //
                    primes[primeCount++] = candidatePrime;
                }
            }

            // Add the rest of the primes
            for (; candidatePrime <= max; candidatePrime += 2)
            {
                if (isPrime.Get(candidatePrime))
                {
                    primes[primeCount++] = candidatePrime;
                }
            }

            outPrimeCount = primeCount;
            return primes;
        }
    }
}
