using System;
using System.Collections.Generic;

namespace More
{
    public struct Rational
    {
        public static readonly Rational Nan      = new Rational(0, 0);
        public static readonly Rational Zero     = new Rational(0, 1);
        public static readonly Rational Infinity = new Rational(1, 0);        

        public Int32 numerator;
        public UInt32 denominator;
        public Rational(Int32 numerator)
        {
            this.numerator = numerator;
            this.denominator = 1;
        }
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
        public Single ConvertToSingle()
        {
            return (Single)numerator / (Single)denominator;
        }
        public Double ConvertToDouble()
        {
            return (Double)numerator / (Double)denominator;
        }
        public Boolean Equals(Rational r)
        {
            return this.numerator == r.numerator && this.denominator == r.denominator;
        }
        public override String ToString()
        {
            if(numerator == 0) return (denominator == 0) ? "Nan" : "0";
            if(denominator == 0) return "Infinity";
            if(denominator == 1) return numerator.ToString();
            return String.Format("{0}/{1}", numerator, denominator);
        }
    }
}