using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace More
{
    //
    // Contains static extension methods for PoweredPrime[]
    //
    // Restrictions on PoweredPrime
    // -------------------------------------------------------
    //    1. poweredPrime.prime must be a prime number (note this is not checked by the constructor and must be checked by the user)
    //    2. poweredPrime.power should never be 0
    //
    // Restrictions on PoweredPrime[]
    // -------------------------------------------------------
    //    1. The elements of PoweredPrime[] must be in ascending order according to PoweredPrime.value
    //    2. No instances of PoweredPrime[] should ever be null (PoweredPrime.None represents an array of no prime factors)
    //
    //
    public struct PoweredPrime
    {
        public static readonly PoweredPrime[] None = new PoweredPrime[0];

        public readonly UInt32 prime;
        public readonly SByte power;
        public readonly UInt32 value;
        public PoweredPrime(UInt32 prime, SByte power)
        {
            this.prime = prime;
            this.power = power;
            
            UInt64 valueAsUInt64 = 1;
            if (power > 0)
            {
                for (Int32 i = 0; i < power; i++)
                {
                    valueAsUInt64 *= prime;
                    if (valueAsUInt64 > UInt32.MaxValue) throw new OverflowException(String.Format(
                        "A PoweredPrime (prime={0}, power={1}) is too large to be held by a UInt32", prime, power));
                }
            }
            else if (power < 0)
            {
                for (Int32 i = 0; i > power; i--)
                {
                    valueAsUInt64 *= prime;
                    if (valueAsUInt64 > UInt32.MaxValue) throw new OverflowException(String.Format(
                        "A PoweredPrime (prime={0}, power={1}) is too large to be held by a UInt32", prime, power));
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot create a powered prime with power 0");
            }
            this.value = (UInt32)valueAsUInt64;
        }
        public override String ToString()
        {
            return String.Format("{0}^{1}", prime, power);
        }
    }
    public static class PoweredPrimeFactors
    {
        public static void InvertTo(this PoweredPrime[] factors, List<PoweredPrime> inverted)
        {
            for (int i = 0; i < factors.Length; i++)
            {
                PoweredPrime factor = factors[i];
                inverted.Add(new PoweredPrime(factor.prime, (SByte)(-factor.power)));
            }
        }
        public static void DivideInto(this PoweredPrime[] divideFactors, List<PoweredPrime> factors)
        {
            if (divideFactors.Length <= 0) return;

            int factorIndex = 0;

            int divideIndex = 0;
            PoweredPrime currentDivideFactor = divideFactors[0];

            while (factorIndex < factors.Count)
            {
                PoweredPrime currentFactor = factors[factorIndex];
                if (currentDivideFactor.prime < currentFactor.prime)
                {
                    factors.Insert(factorIndex, new PoweredPrime(currentDivideFactor.prime, (SByte)(-currentDivideFactor.power)));

                    divideIndex++;
                    if (divideIndex >= divideFactors.Length) return;
                    currentDivideFactor = divideFactors[divideIndex];
                }
                else if (currentDivideFactor.prime == currentFactor.prime)
                {
                    Int32 powerDifference = (Int32)currentFactor.power - (Int32)currentDivideFactor.power;
                    if (powerDifference == 0)
                    {
                        factors.RemoveAt(factorIndex);
                    }
                    else
                    {
                        if (powerDifference < SByte.MinValue || powerDifference > SByte.MaxValue)
                            throw new InvalidOperationException(String.Format(
                                "Cannot perform division because the difference of powers for factor {0} is out of range ({1} - {2} = {3})",
                                currentFactor.prime, currentFactor.power, currentDivideFactor.power, powerDifference));

                        factors[factorIndex] = new PoweredPrime(currentFactor.prime, (SByte)powerDifference);
                        factorIndex++;
                    }

                    divideIndex++;
                    if (divideIndex >= divideFactors.Length) return;
                    currentDivideFactor = divideFactors[divideIndex];
                }
                else
                {
                    factorIndex++;
                }
            }

            // Add the rest of the divide factors
            while (true)
            {
                factors.Add(new PoweredPrime(currentDivideFactor.prime, (SByte)(-currentDivideFactor.power)));
                divideIndex++;
                if (divideIndex >= divideFactors.Length) return;
                currentDivideFactor = divideFactors[divideIndex];
            }
        }
    }
}
