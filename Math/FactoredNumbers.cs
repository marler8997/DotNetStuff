using System;
using System.Collections.Generic;

namespace More
{
    public static class Factorizer
    {
        public static InvalidOperationException UnableToFactorize(this IPrimeFactorizer factorizer, UInt32 value)
        {
            return new InvalidOperationException(String.Format("{0} was not able to factorize {1}", factorizer.GetType().Name, value));
        }
        public static InvalidOperationException UnableToDivide(this IPrimeFactorizer factorizer, UInt32 value, PoweredPrime[] factors)
        {
            return new InvalidOperationException(String.Format("{0} was not able to divide {1} byte factors {2}",
                factorizer.GetType().Name, value, factors.SerializeObject()));
        }
    }
    public struct FactoredInt32
    {
        public static readonly FactoredInt32 Zero = new FactoredInt32(0, PoweredPrime.None);
        public static readonly FactoredInt32 One = new FactoredInt32(1, PoweredPrime.None);
        public static readonly FactoredInt32 Two = new FactoredInt32(2, new PoweredPrime[] { new PoweredPrime(2, 1) });
        public static readonly FactoredInt32 Three = new FactoredInt32(3, new PoweredPrime[] { new PoweredPrime(3, 1) });
        static readonly FactoredInt32[] FirstFour = new FactoredInt32[] {
            Zero, One, Two, Three
        };
        public static FactoredInt32 Create(Int32 value, IPrimeFactorizer factorizer)
        {
            if (value >= 0 && value <= 3) return FirstFour[value];

            UInt32 absoluteValue = (UInt32)((value >= 0) ? value : -value);

            PoweredPrime[] factors = factorizer.PrimeFactorize(absoluteValue);
            if (factors == null) throw factorizer.UnableToFactorize(absoluteValue);

            return new FactoredInt32(value, factors);
        }

        public readonly Int32 value;
        public readonly PoweredPrime[] primeFactors;
        private FactoredInt32(Int32 value, PoweredPrime[] primeFactors)
        {
            this.value = value;
            this.primeFactors = primeFactors;
        }
    }
    public struct FactoredUInt32
    {
        public static readonly FactoredUInt32 Zero  = new FactoredUInt32(0, PoweredPrime.None);
        public static readonly FactoredUInt32 One   = new FactoredUInt32(1, PoweredPrime.None);
        public static readonly FactoredUInt32 Two   = new FactoredUInt32(2, new PoweredPrime[] { new PoweredPrime(2, 1) });
        public static readonly FactoredUInt32 Three = new FactoredUInt32(3, new PoweredPrime[] { new PoweredPrime(3, 1) });
        static readonly FactoredUInt32[] FirstFour = new FactoredUInt32[] {
            Zero, One, Two, Three
        };
        public static FactoredUInt32 Create(UInt32 value, IPrimeFactorizer factorizer)
        {
            if (value <= 3) return FirstFour[value];

            PoweredPrime[] factors = factorizer.PrimeFactorize(value);
            if (factors == null) throw factorizer.UnableToFactorize(value);

            return new FactoredUInt32(value, factors);
        }
        public readonly UInt32 value;
        public readonly PoweredPrime[] primeFactors;
        private FactoredUInt32(UInt32 value, PoweredPrime[] primeFactors)
        {
            this.value = value;
            this.primeFactors = primeFactors;
        }
    }

    public struct FactoredRational
    {
        public static readonly FactoredRational Nan = new FactoredRational(0, 0, PoweredPrime.None);
        public static readonly FactoredRational Zero = new FactoredRational(0, 1, PoweredPrime.None);
        public static readonly FactoredRational Infinity = new FactoredRational(1, 0, PoweredPrime.None);
        public static readonly FactoredRational One = new FactoredRational(1, 1, PoweredPrime.None);

        public Int32 numerator;
        public UInt32 denominator;
        public PoweredPrime[] primeFactors;
        private FactoredRational(Int32 numerator, UInt32 denominator, PoweredPrime[] primeFactors)
        {
            this.numerator = numerator;
            this.denominator = denominator;
            this.primeFactors = primeFactors;
        }
        public FactoredRational(Int32 value, IPrimeFactorizer factorizer)
        {
            FactoredInt32 factoredInt32 = FactoredInt32.Create(value, factorizer);
            this.numerator = value;
            this.denominator = 1;
            this.primeFactors = factoredInt32.primeFactors;
        }
        public FactoredRational(Rational rational, IPrimeFactorizer factorizer)
        {
            if (rational.numerator == 0 || rational.denominator == 0)
            {
                this.numerator = rational.numerator;
                this.denominator = rational.denominator;
                this.primeFactors = PoweredPrime.None;
            }
            else
            {
                //
                // Take the absolute value of the rational
                //
                PoweredPrime[] denominatorFactors = factorizer.PrimeFactorize(rational.denominator);
                if (denominatorFactors == null) throw factorizer.UnableToFactorize(rational.denominator);

                UInt32 numeratorAbsoluteValue = (UInt32)((rational.numerator >= 0) ? rational.numerator : -rational.numerator);

                this.primeFactors = factorizer.Divide(numeratorAbsoluteValue, denominatorFactors);
                if (this.primeFactors == null) throw factorizer.UnableToDivide(numeratorAbsoluteValue, denominatorFactors);

                // Get new numerator and denominator
                this.numerator = 1;
                this.denominator = 1;
                for (int i = 0; i < this.primeFactors.Length; i++)
                {
                    PoweredPrime factor = this.primeFactors[i];
                    if (factor.power > 0)
                    {
                        numerator *= (Int32)factor.value;
                        // I don't need to check overflow here because these factors came from an Int32
                    }
                    else
                    {
                        denominator *= factor.value;
                        // I don't need to check overflow here because these factors came from a UInt32
                    }
                }
                if (rational.numerator < 0) numerator = -numerator;
            }
        }

        public Single ConvertToSingle()
        {
            return (Single)numerator / (Single)denominator;
        }
        public Double ConvertToDouble()
        {
            return (Double)numerator / (Double)denominator;
        }
        /*
        public Rational Add(Rational other, PrimeFactorizer factorizer)
        {

        }
        */
    }

}