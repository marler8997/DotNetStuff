using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More.Physics.Test
{
    [TestClass]
    public class PatternResearch
    {
        //
        // A PrimeIntervalPattern of maxPrimeIndex n is an array of intervals
        // sucn that adding them in sequence will generate a new sequence where every
        // number cannot be divided by the first n+1 prime numbers.
        //   i.e. 
        //     PrimeIntervalPattern(0) =   [2]; 1 (+2) 3 (+2) 5 (+2) 7...
        //     PrimeIntervalPattern(1) = [4,2]; 1 (+4) 5 (+2) 7 (+4) 7...
        //
        // Let P_i be the prime at index i.
        //   P_0 = 2
        //   P_1 = 3
        //   P-2 = 5 ....
        // The length of PrimeIntervalPattern(n) is calculated by:
        //         n
        //      Product[ P_i - 1 ]
        //       i = 1

        public static UInt64 CalculatePrimeIntervalLengthAndPreCheckForOverflow(UInt32 maxPrimeIndex)
        {
            if (maxPrimeIndex >= PrimeTable.Length) throw new NotSupportedException(String.Format(
                "The given prime index {0} is too large for the current prime table {1}", maxPrimeIndex, PrimeTable.Length));
            
            UInt64 length = 1;

            // Start at i=1 because when i=0, primeMinusOne = 1
            for (UInt32 i = 1; i <= maxPrimeIndex; i++)
            {
                UInt64 primeMinusOne = PrimeTable.Values[i] - 1;

                // Check for overflow
                if (length > UInt64.MaxValue / primeMinusOne) throw new OverflowException(String.Format(
                    "The Length calculated by the given prime index {0} is too large for a UInt32", maxPrimeIndex));

                length *= primeMinusOne;
            }

            return length;
        }

        static UInt32 CheckAndDownCast(UInt64 value)
        {
            if(value > UInt32.MaxValue) throw new InvalidOperationException(String.Format("UInt64 value '{0}' is too large to cast to a UInt32", value));
            return (UInt32)value;
        }


        public static UInt32[] CreatePrimeIntervalPattern(UInt32 maxPrimeIndex)
        {
            UInt32 patternLength = CheckAndDownCast(CalculatePrimeIntervalLengthAndPreCheckForOverflow(maxPrimeIndex));
            UInt32[] pattern = new UInt32[patternLength];

            UInt32 patternOffset = 0;

            UInt32 lastValue = 1;
            UInt32 currentValue = 3;

            while (patternOffset < patternLength)
            {
                Boolean canBeDivided = false;
                for (int i = 1; i <= maxPrimeIndex; i++)
                {
                    if (currentValue % PrimeTable.Values[i] == 0)
                    {
                        canBeDivided = true;
                        break;
                    }
                }

                if (!canBeDivided)
                {
                    pattern[patternOffset++] = currentValue - lastValue;
                    lastValue = currentValue;
                }

                currentValue += 2;
            }

            return pattern;
        }

        public UInt32[] CreatePrimeIntervalPatternCompressed(UInt32 maxPrimeIndex)
        {
            if (maxPrimeIndex <= 0) throw new ArgumentOutOfRangeException();

            UInt32 halfPatternLength = CheckAndDownCast(CalculatePrimeIntervalLengthAndPreCheckForOverflow(maxPrimeIndex) / 2 + 1);

            UInt32[] pattern = new UInt32[halfPatternLength];
            pattern[0] = 0;
            UInt32 patternOffset = 1;

            UInt32 lastValue = 1;
            UInt32 currentValue = 3;

            while (patternOffset < halfPatternLength)
            {
                Boolean canBeDivided = false;
                for (int i = 1; i <= maxPrimeIndex; i++)
                {
                    if (currentValue % PrimeTable.Values[i] == 0)
                    {
                        canBeDivided = true;
                        break;
                    }
                }

                if (!canBeDivided)
                {
                    pattern[patternOffset++] = (currentValue - lastValue) / 2 - 1;
                    lastValue = currentValue;
                }

                currentValue += 2;
            }

            return pattern;
        }


        [TestMethod]
        public void PrintPrimeIntervalPatternLengths()
        {
            //
            // The max PrimeIntervalLength that a UInt64 can hold is 15
            //
            for (UInt32 i = 0; i <= 15; i++)
            {
                UInt64 primeIntervalPatternLength = CalculatePrimeIntervalLengthAndPreCheckForOverflow(i);
                Console.WriteLine("PrimeIntervalPattern({0}).Length = {1}", i, primeIntervalPatternLength);
            }
        }
        [TestMethod]
        public void PrintPrimeIntervals()
        {
            StringBuilder builder = new StringBuilder();

            for (UInt32 i = 2; i < 5; i++)
            {
                //UInt32[] primeIntervalPattern = CreatePrimeIntervalPattern(i);
                UInt32[] primeIntervalPattern = CreatePrimeIntervalPatternCompressed(i);
                
                UInt32 patternSum = 0;
                for (UInt32 j = 0; j < primeIntervalPattern.Length; j++)
                {
                    patternSum += primeIntervalPattern[j];

                }

                // Create Primes String
                builder.Length = 0;
                builder.Append('[');
                for (int j = 0; j <= i; j++)
                {
                    if(j > 0) builder.Append(", ");
                    builder.Append(PrimeTable.Values[j]);
                }
                builder.Append(']');
                String primeString = builder.ToString();


                Console.WriteLine("Primes={0}, PatternLength={1}, PatternSum={2}: {3}", primeString,
                    primeIntervalPattern.Length, patternSum, primeIntervalPattern.SerializeObject());

                AnalyzePatterns(primeIntervalPattern);
                //AnalyzeReversedPatterns(primeIntervalPattern);
                //FixedLengthPatternAnalyzer(primeIntervalPattern, 2);
            }
        }




        class PatternsAtFixedLength
        {
            class PatternAndCount
            {
                public readonly UInt32[] pattern;
                public UInt32 extraCount;
                public PatternAndCount(UInt32[] pattern)
                {
                    this.pattern = pattern;
                }
            }

            public readonly UInt32 length;
            UInt32 totalPatternCount;

            readonly List<PatternAndCount> uniquePatterns = new List<PatternAndCount>();

            public PatternsAtFixedLength(UInt32 length)
            {
                this.length = length;
                this.totalPatternCount = 0;
            }
            public void AddPattern(UInt32[] pattern)
            {
                if (pattern.Length != length) throw new InvalidOperationException();

                totalPatternCount++;

                // Check if this pattern already exists
                for (int i = 0; i < uniquePatterns.Count; i++)
                {
                    PatternAndCount existingPattern = uniquePatterns[i];
                    Boolean allElementsMatch = true;
                    for (UInt32 compareIndex = 0; compareIndex < length; compareIndex++)
                    {
                        if (existingPattern.pattern[compareIndex] != pattern[compareIndex])
                        {
                            allElementsMatch = false;
                            break;
                        }
                    }
                    if (allElementsMatch)
                    {
                        existingPattern.extraCount++;
                        return;
                    }
                }

                //
                // Did not match any existing pattern
                //
                uniquePatterns.Add(new PatternAndCount(pattern));
            }
            public void Print(Boolean printPatterns)
            {
                if (totalPatternCount == 0) return;

                Console.WriteLine("{0} patterns of length {1}", totalPatternCount, length);
                if (printPatterns)
                {
                    for (int i = 0; i < uniquePatterns.Count; i++)
                    {
                        PatternAndCount pattern = uniquePatterns[i];
                        Console.WriteLine("  {0}: {1}", pattern.extraCount + 1, pattern.pattern.SerializeObject());
                    }
                }
            }
        }
        public void AnalyzePatterns(UInt32[] pattern)
        {
            UInt32[] patternLengthHistogram = new UInt32[pattern.Length];
            PatternsAtFixedLength[] savedPatterns = new PatternsAtFixedLength[pattern.Length];
            for (UInt32 i = 2; i < savedPatterns.Length; i++)
            {
                savedPatterns[i] = new PatternsAtFixedLength(i);
            }


            for (UInt32 patternStart = 0; patternStart < pattern.Length - 1; patternStart++)
            {
                RollingArray<UInt32> patternEnumerable = new RollingArray<UInt32>(pattern, patternStart);
                IEnumerator<UInt32> patternEnumerator = patternEnumerable.GetEnumerator();

                for(UInt32 compareStart = patternStart + 1; compareStart < pattern.Length; compareStart++)
                {
                    UInt32 matchLength = 0;
                    patternEnumerator.Reset();

                    IEnumerator<UInt32> compareEnumerator = new RollingArray<UInt32>(pattern, compareStart).GetEnumerator();
                    while (patternEnumerator.MoveNext() && compareEnumerator.MoveNext())
                    {
                        if (patternEnumerator.Current == compareEnumerator.Current)
                        {
                            matchLength++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    patternLengthHistogram[matchLength]++;

                    if (matchLength >= 2)
                    {
                        savedPatterns[matchLength].AddPattern(patternEnumerable.CreateNewArray(matchLength));
                    }
                }
            }

            /*
            //
            // Print PatternLength Histogram
            //
            for (UInt32 i = 0; i < patternLengthHistogram.Length; i++)
            {
                UInt32 histogramCount = patternLengthHistogram[i];
                if (histogramCount != 0) Console.WriteLine("PatternLength {0,3}: {1}", i, histogramCount);
            }
            */

            //
            // Print saved patters
            //
            for (UInt32 i = 2; i < savedPatterns.Length; i++)
            {
                PatternsAtFixedLength patterns = savedPatterns[i];
                patterns.Print(true);
            }
        }

        public void AnalyzeReversedPatterns(UInt32[] pattern)
        {
            UInt32[] patternLengthHistogram = new UInt32[pattern.Length + 1];
            PatternsAtFixedLength[] savedPatterns = new PatternsAtFixedLength[pattern.Length + 1];
            for (UInt32 i = 2; i < savedPatterns.Length; i++)
            {
                savedPatterns[i] = new PatternsAtFixedLength(i);
            }


            for (UInt32 patternStart = 0; patternStart < pattern.Length - 1; patternStart++)
            {
                RollingArray<UInt32> patternEnumerable = new RollingArray<UInt32>(pattern, patternStart);
                IEnumerator<UInt32> patternEnumerator = patternEnumerable.GetEnumerator();

                for (UInt32 compareStart = 0; compareStart < pattern.Length; compareStart++)
                {
                    UInt32 matchLength = 0;
                    patternEnumerator.Reset();

                    IEnumerator<UInt32> compareEnumerator = new RollingArrayReversed<UInt32>(pattern, compareStart).GetEnumerator();
                    while (patternEnumerator.MoveNext() && compareEnumerator.MoveNext())
                    {
                        if (patternEnumerator.Current == compareEnumerator.Current)
                        {
                            matchLength++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    patternLengthHistogram[matchLength]++;

                    if (matchLength >= 2)
                    {
                        savedPatterns[matchLength].AddPattern(patternEnumerable.CreateNewArray(matchLength));
                    }
                }
            }

            /*
            //
            // Print PatternLength Histogram
            //
            for (UInt32 i = 0; i < patternLengthHistogram.Length; i++)
            {
                UInt32 histogramCount = patternLengthHistogram[i];
                if (histogramCount != 0) Console.WriteLine("PatternLength {0,3}: {1}", i, histogramCount);
            }
            */

            //
            // Print saved patters
            //
            for (UInt32 i = 2; i < savedPatterns.Length; i++)
            {
                PatternsAtFixedLength patterns = savedPatterns[i];
                patterns.Print(true);
            }
        }
        /*
        public void FixedLengthPatternAnalyzer(UInt32[] pattern, UInt32 subPatternLength)
        {
            if (subPatternLength >= pattern.Length) return;

            Console.WriteLine("SubPatternLength {0}", subPatternLength);

            UInt32[] subPattern = new UInt32[subPatternLength];

            for (UInt32 i = 0; i < pattern.Length - subPatternLength; i++)
            {
                for(UInt32 j = 0; j < subPatternLength; j++)
                {
                    subPattern[j] = pattern[i+j];
                }


                Console.WriteLine("SubPattern {0}", subPattern.SerializeObject());
            }
        }
        */







        public UInt32[] CreateHalfPrimeIntervalPattern()
        {
            return null;
        }



    }

}
