using System;
using System.Collections.Generic;
using System.Linq;

namespace More.Physics
{
    public class PrimeTableEnumerator
    {
        UInt32 nextPrimeIndex;
        public PrimeTableEnumerator(UInt32 startPrimeIndex)
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
}
