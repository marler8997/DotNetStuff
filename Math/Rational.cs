using System;
using System.Collections.Generic;

namespace More
{
    public struct PoweredPrime
    {
        public UInt32 value;
        public SByte power;
        public PoweredPrime(UInt32 value, SByte power)
        {
            this.value = value;
            this.power = power;
        }
    }
    public struct SignedPowerPrimeMultiplier
    {
        public UInt32 value;
        public SByte power;
    }

    public struct Rational
    {
        public static readonly Rational Nan      = new Rational(0, 0);
        public static readonly Rational Zero     = new Rational(0, 1);
        public static readonly Rational Infinity = new Rational(1, 0);        

        public Int32 numerator;
        public UInt32 denominator;
        public Rational(Int32 numerator, UInt32 denominator)
        {
            this.numerator = numerator;
            this.denominator = denominator;

            if (numerator == 0)
            {
                if (denominator != 0)
                {
                    denominator = 1; // Normalize Zero
                }
            }
            else if (denominator == 0)
            {
                numerator = 1;       // Normalize Infinity
            }            
        }
        public Boolean Equals(Rational r)
        {
            return this.numerator == r.numerator && this.denominator == r.denominator;
        }
    }

    public static class Factored
    {
        public static readonly Dictionary<UInt32, FactoredInteger> PositiveIntegerFactorizations = new Dictionary<UInt32, FactoredInteger>();
        public static readonly Dictionary<Rational, FactoredRational> PositiveRationalFactorizations = new Dictionary<Rational, FactoredRational>();
    }

    public struct FactoredInteger
    {
        public static readonly FactoredInteger Zero     = new FactoredInteger(0, (PoweredPrime[])null);
        public static readonly FactoredInteger One      = new FactoredInteger(1, new PoweredPrime[0]);

        public static FactoredInteger Create(Int32 value, PrimeFactorizer factorizer)
        {
            if (value == 0) return Zero;

            UInt32 absoluteValue = (UInt32)((value < 0) ? value * -1 : value);

            //
            // Check if it's prime factorization was already calculated
            //
            FactoredInteger factoredInteger;
            if (Factored.PositiveIntegerFactorizations.TryGetValue(absoluteValue, out factoredInteger))
            {
                if(value < 0) factoredInteger.value *= -1;
                return factoredInteger;
            }

            factoredInteger = new FactoredInteger((Int32)absoluteValue, factorizer(absoluteValue));
            Factored.PositiveIntegerFactorizations.Add(absoluteValue, factoredInteger);

            if (value < 0) factoredInteger.value *= -1;
            return factoredInteger;
        }

        public Int32 value;
        public PoweredPrime[] primeFactors;
        private FactoredInteger(Int32 value, PoweredPrime[] primeFactors)
        {
            this.value = value;
            this.primeFactors = primeFactors;
        }
    }

    public struct FactoredRational
    {
        public static readonly FactoredRational Nan      = new FactoredRational(Rational.Nan     , (PoweredPrime[])null);
        public static readonly FactoredRational Zero     = new FactoredRational(Rational.Zero    , (PoweredPrime[])null);
        public static readonly FactoredRational Infinity = new FactoredRational(Rational.Infinity, (PoweredPrime[])null);
        public static readonly FactoredRational One      = new FactoredRational(new Rational(1, 1), new PoweredPrime[0]);

        public static FactoredRational Create(Rational rational, PrimeFactorizer factorizer)
        {
            if (rational.numerator == 0) return (rational.denominator == 0) ? Nan : Zero;
            if (rational.denominator == 0) return Infinity;
            if (rational.denominator == 1)
            {
                if (rational.numerator == 1) return One;

            }

            //
            // Take the absolute value of the rational
            //
            Boolean negative;
            if (rational.numerator < 0)
            {
                negative = true;
                rational.numerator *= -1;
            }
            else
            {
                negative = false;
            }

            //
            // Check if it's prime factorization was already calculated
            //
            FactoredRational factoredRational;
            if (Factored.PositiveRationalFactorizations.TryGetValue(rational, out factoredRational))
            {
                if (negative) factoredRational.value.numerator *= -1;
                return factoredRational;
            }

            //factoredRational = new FactoredRational(rational);
            Factored.PositiveRationalFactorizations.Add(rational, factoredRational);

            if (negative) factoredRational.value.numerator *= -1;
            return factoredRational;
        }

        public Rational value;
        public PoweredPrime[] primeFactors;
        private FactoredRational(Rational value, PoweredPrime[] primeFactors)
        {
            this.value = value;
            this.primeFactors = primeFactors;
        }
    }

}