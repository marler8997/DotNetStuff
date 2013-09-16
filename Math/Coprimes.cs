using System;
using System.Collections.Generic;

namespace More
{






    public static class Coprimes
    {
        public static UInt64 CalculateQn(UInt32 n)
        {
            if (n == 0) return 0; // Modify the definition of Qn for this
            UInt64 Qn = 1;
            for (UInt32 i = 0; i < n; i++)
            {
                UInt32 PkMinusOne = PrimeTable.Values[i] - 1;

                if (Qn > UInt64.MaxValue / PkMinusOne) throw new ArgumentOutOfRangeException(
                    "n", n, String.Format("A UInt64 is not big enough to hold Qn for n = {0} (Stopped at n = {1})", n, i + 1));

                Qn *= (UInt64)PkMinusOne;
            }
            return Qn;
        }

        public static UInt64 CalculatePrimorial(UInt32 n)
        {
            UInt64 primorial = 1;
            for (UInt32 i = 0; i < n; i++)
            {
                UInt32 Pk = PrimeTable.Values[i];

                if (primorial > UInt64.MaxValue / Pk) throw new ArgumentOutOfRangeException(
                    "n", n, String.Format("A UInt64 is not big enough to hold Primorial for n = {0} (Stopped at n = {1})", n, i + 1));
                primorial *= (UInt64)Pk;
            }
            return primorial;
        }


        public static UInt32[] BruteForceCreateCn(UInt32 n, UInt32 count, ISimpleList<UInt32> GnBuffer)
        {
            if (n == 0) throw new ArgumentOutOfRangeException("n", n, "n cannot be 0");

            UInt32[] Cn = new UInt32[count];
            Cn[0] = 1;
            UInt32 nextCnIndex = 1;

            for (UInt32 potentialCoprime = 2; nextCnIndex < count; potentialCoprime++)
            {
                Boolean isCoprime = true;
                for (UInt32 primeIndex = 0; primeIndex < n; primeIndex++)
                {
                    UInt32 prime = PrimeTable.Values[primeIndex];
                    if ((potentialCoprime % prime) == 0)
                    {
                        isCoprime = false;

                        if (primeIndex == n - 1)
                        {
                            GnBuffer.Add(potentialCoprime);
                        }

                        break;
                    }
                }

                if (isCoprime)
                {
                    Cn[nextCnIndex++] = potentialCoprime;
                }
            }

            return Cn;
        }


        public static void BruteForceCreateCnWithLimit(UInt32 n, UInt32 limit, ISimpleList<UInt32> Cn, ISimpleList<UInt32> GnBuffer)
        {
            if (n == 0) throw new ArgumentOutOfRangeException("n", n, "n cannot be 0");

            Cn.Add(1);

            for (UInt32 potentialCoprime = 2; potentialCoprime <= limit; potentialCoprime++)
            {
                Boolean isCoprime = true;
                for (UInt32 primeIndex = 0; primeIndex < n; primeIndex++)
                {
                    UInt32 prime = PrimeTable.Values[primeIndex];
                    if ((potentialCoprime % prime) == 0)
                    {
                        isCoprime = false;

                        if (primeIndex == n - 1)
                        {
                            GnBuffer.Add(potentialCoprime);
                        }
                        break;
                    }
                }

                if (isCoprime)
                {
                    Cn.Add(potentialCoprime);
                }
            }
        }


        //
        // TODO: Implement this function
        //
        public static void CnMinusOneToCn(UInt32[] CnMinusOne, UInt32 CnMinusOneLength, UInt32[] Cn, out UInt32 CnLength)
        {
            UInt32 nextWriteIndex = 0, readIndex = 0;

            UInt32 Pn = CnMinusOne[1];
            UInt32 nextCnMinusOneFactorIndex = 1;
            UInt32 nextFilterNumber = Pn;

            while (readIndex < CnMinusOneLength)
            {
                UInt32 currentNumber = CnMinusOne[readIndex];
                readIndex++;

                if (currentNumber == nextFilterNumber)
                {
                    nextFilterNumber = Pn * CnMinusOne[nextCnMinusOneFactorIndex];
                    nextCnMinusOneFactorIndex++;
                }
                else
                {
                    Cn[nextWriteIndex] = currentNumber;
                    nextWriteIndex++;
                }
            }

            CnLength = nextWriteIndex;
        }

    }
}
