using System;

namespace Marler.Hmd
{
    public static class CountProperty
    {
        /// <summary>
        /// Parses a count property string and returns a class that can validate the number of times an ID appears.
        //	Scenarios: (let n >= 0)
        //  String   Summary
        //  0-*      "no restriction on count" (this is the default)
        //  n        "exactly n"
        //  n-*      "at least n"
        //  0-n      "up to n"
        //  n-m      "at least n, up to m" (note that m > n)
        //  Consider implementing same thing with <= and >= instead?
        /// </summary>
        /// <param name="countString">The count property as a string.</param>
        /// <returns>A CountProperty class that can validate the number of times an ID appears.</returns>
        /// 
        public static ICountProperty Parse(String countString) // TODO: Maybe pass in an offset/length to this function instead of using the whole string:)
        {
            Int32 indexOfDash = countString.IndexOf('-');
            if (indexOfDash < 0)
            {
                return new StaticCount(UInt32.Parse(countString));
            }

            UInt32 min = UInt32.Parse(countString.Substring(0, indexOfDash));

            if (countString[indexOfDash + 1] == '*')
            {
                if (min == 0)
                {
                    return UnrestrictedCount.Instance;
                }
                else
                {
                    return new CountWithMin(min);
                }
            }
            else
            {
                UInt32 max = UInt32.Parse(countString.Substring(indexOfDash + 1));
                if (min == 0)
                {
                    return new CountWithMax(max);
                }
                else
                {
                    return new CountWithMinAndMax(min, max);
                }
            }
        }
    }

    public interface ICountProperty
    {
        UInt32 MinCount { get; }
        Boolean Multiple { get; }
        Boolean IsValidCount(UInt32 count);

        Boolean Equals(ICountProperty countProperty);
    }

    public class UnrestrictedCount : ICountProperty
    {
        private static UnrestrictedCount instance;
        public static UnrestrictedCount Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UnrestrictedCount();
                }
                return instance;
            }
        }

        private UnrestrictedCount() { }

        public UInt32 MinCount { get { return 0; } }
        public Boolean Multiple { get { return true; } }
        public Boolean IsValidCount(UInt32 count) { return true; }
        public Boolean Equals(ICountProperty countProperty)
        {
            //
            // Since UnrestrictedCount is a singleton, we only need to 
            // check that both CountProperty classes point to the UnrestrictedCount singleton
            //
            return countProperty == instance;
        }
        public override string ToString()
        {
            return "0-*";
        }
    }

    public class StaticCount : ICountProperty
    {
        public readonly UInt32 count;

        public StaticCount(UInt32 count)
        {
            this.count = count;
        }

        public UInt32 MinCount { get { return count; } }
        public Boolean Multiple { get { return count > 1; } }
        public Boolean IsValidCount(UInt32 count) { return this.count == count; }
        public Boolean Equals(ICountProperty countProperty)
        {
            StaticCount staticCount = countProperty as StaticCount;
            if (staticCount == null)
            {
                return false;
            }
            return this.count == staticCount.count;
        }
        public override string ToString()
        {
            return count.ToString();
        }
    }

    public class CountWithMin : ICountProperty
    {
        public readonly UInt32 min;

        public CountWithMin(UInt32 min)
        {
            #if DEBUG
            if (min == 0)
            {
                throw new ArgumentOutOfRangeException("min", "min cannot be 0, if the min is supposed to be 0, then you are using the wrong CountProperty subclass");
            }
            #endif

            this.min = min;
        }

        public UInt32 MinCount { get { return min; } }
        public Boolean Multiple { get { return true; } }
        public Boolean IsValidCount(UInt32 count) { return count >= min; }
        public Boolean Equals(ICountProperty countProperty)
        {
            CountWithMin countWithMin = countProperty as CountWithMin;
            if (countWithMin == null)
            {
                return false;
            }
            return this.min == countWithMin.min;
        }
        public override string ToString()
        {
            return String.Format("{0}-*",min);
        }
    }

    public class CountWithMax : ICountProperty
    {
        public readonly UInt32 max;

        public CountWithMax(UInt32 max)
        {
            #if DEBUG
            if (max == 0)
            {
                throw new ArgumentOutOfRangeException("max", "max cannot be 0, if the max is supposed to be 0, then you are using the wrong CountProperty subclass");
            }
            #endif

            this.max = max;
        }

        public UInt32 MinCount { get { return 0; } }
        public Boolean Multiple { get { return max > 1; } }
        public Boolean IsValidCount(UInt32 count) { return count <= max; }
        public Boolean Equals(ICountProperty countProperty)
        {
            CountWithMax countWithMax = countProperty as CountWithMax;
            if (countWithMax == null)
            {
                return false;
            }
            return this.max == countWithMax.max;
        }
        public override string ToString()
        {
            return String.Format("0-{0}", max);
        }
    }

    public class CountWithMinAndMax : ICountProperty
    {
        public readonly UInt32 min;
        public readonly UInt32 max;

        public CountWithMinAndMax(UInt32 min, UInt32 max)
        {
            #if DEBUG
            if (min == 0)
            {
                throw new ArgumentOutOfRangeException("min", "min cannot be 0, if the min is supposed to be 0, then you are using the wrong CountProperty subclass");
            }
            #endif

            this.min = min;
            if (min >= max)
            {
                throw new ArgumentException(String.Format("'min' ({0}) be less than 'max ({1})", min, max));
            }
            this.max = max;
        }

        public UInt32 MinCount { get { return min; } }
        public Boolean Multiple { get { return true; /*max will alway be > 1 in this class*/} }
        public Boolean IsValidCount(UInt32 count)
        {
            return (count >= min) && (count <= max);
        }
        public Boolean Equals(ICountProperty countProperty)
        {
            CountWithMinAndMax countWithMinAndMax = countProperty as CountWithMinAndMax;
            if (countWithMinAndMax == null)
            {
                return false;
            }
            return (this.min == countWithMinAndMax.min) && (this.max == countWithMinAndMax.max);
        }
        public override string ToString()
        {
            return String.Format("{0}-{1}", min, max);
        }
    }
}
