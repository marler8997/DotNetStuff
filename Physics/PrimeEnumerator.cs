using System;

namespace More.Physics
{
    public class LimitedFastPrimeEnumerator
    {
        UInt16 nextFirstPrimeIndex;
        public LimitedFastPrimeEnumerator(UInt16 startPrimeIndex)
        {
            if (startPrimeIndex >= Primes.FirstPrimes.Length)
                throw new NotImplementedException(String.Format("Prime index {0} is to high for the current implementation", startPrimeIndex));

            this.nextFirstPrimeIndex = startPrimeIndex;
        }
        public UInt32 Next()
        {
            if (nextFirstPrimeIndex >= Primes.FirstPrimes.Length)
                throw new NotImplementedException(String.Format("Prime index {0} is to high for the current implementation", nextFirstPrimeIndex));

            return Primes.FirstPrimes[nextFirstPrimeIndex++];
        }
    }
}
