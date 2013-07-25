﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace More.Physics
{
    public class LimitedTablePrimeEnumerator
    {
        public static UInt32 PrimeCount { get { return (UInt32)Primes.FirstPrimes.Length; } }

        UInt32 nextFirstPrimeIndex;
        public LimitedTablePrimeEnumerator(UInt32 startPrimeIndex)
        {
            this.nextFirstPrimeIndex = startPrimeIndex;
        }
        public void SetNextPrimeIndex(UInt32 primeIndex)
        {
            this.nextFirstPrimeIndex = primeIndex;
        }
        public UInt32 Next()
        {
            if (nextFirstPrimeIndex >= Primes.FirstPrimes.Length)
                throw new NotImplementedException(String.Format("Prime index {0} is to high for the current implementation", nextFirstPrimeIndex));

            return Primes.FirstPrimes[nextFirstPrimeIndex++];
        }
    }
    /*
    public class DynamicPrimeEnumerator
    {
        readonly List<UInt32> knownPrimes = new List<UInt32>();

        public IEnumerable<UInt32> Primes()
        {
            UInt32 sqrt = 1;
            var primes = PotentialPrimes().Where(x =>
            {
                sqrt = GetSqrtCeiling(x, sqrt);
                return !knownPrimes
                            .TakeWhile(y => y <= sqrt)
                            .Any(y => x % y == 0);
            });
            foreach (UInt32 prime in primes)
            {
                yield return prime;
                knownPrimes.Add(prime);
            }
        }
        public IEnumerable<UInt32> PotentialPrimes()
        {
            yield return 3;
            UInt32 k = 1;
            while (k > 0)
            {
                yield return 6 * k - 1;
                yield return 6 * k + 1;
                k++;
            }
        }
        private UInt32 GetSqrtCeiling(UInt32 value, UInt32 start)
        {
            while (start * start < value)
            {
                start++;
            }
            return start;
        }
    }
    */
}
