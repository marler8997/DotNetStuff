using System;
using System.Collections.Generic;
using System.Linq;

namespace More.Physics
{
    public class LimitedTablePrimeEnumerator
    {
        UInt32 nextPrimeIndex;
        public LimitedTablePrimeEnumerator(UInt32 startPrimeIndex)
        {
            this.nextPrimeIndex = startPrimeIndex;
        }
        public void SetNextPrimeIndex(UInt32 primeIndex)
        {
            this.nextPrimeIndex = primeIndex;
        }
        public UInt32 Next()
        {
            if (nextPrimeIndex >= PrimeTable.Length)
                throw new NotImplementedException(String.Format("Prime index {0} is to high for the current implementation", nextPrimeIndex));

            UInt32 prime = PrimeTable.Values[nextPrimeIndex];

            nextPrimeIndex++;
            return prime;
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
