using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More.Test
{
    [TestClass]
    public class TwimPrimeResearch
    {
        [TestMethod]
        public void PrettyFormatPrintCoprimeIntervalPatterns()
        {
            for (UInt32 i = 1; i <= 3; i++)
            {
                PrettyFormatPrintCoprimeIntervalPatterns(i);
            }
        }
        void PrettyFormatPrintCoprimeIntervalPatterns(UInt32 maxPrimeIndex)
        {
            UInt32[] pattern = PatternResearch.CreatePrimeIntervalPattern(maxPrimeIndex);
            Console.Write("MaxPrime {0,6}: ", PrimeTable.Values[maxPrimeIndex]);
            for (Int32 i = 0; i < pattern.Length; i++)
            {
                Console.Write("{0,5}", pattern[i]);
            }
            Console.WriteLine();

        }

        [TestMethod]
        public void CalculateCoprimeIntervalPatternUpToNextPrimeSquaredLengths()
        {
            Console.WriteLine("PrimeIndex,Prime,Length");
            for (UInt32 i = 1; i < 100; i++)
            {
                List<UInt32> sequence = CreatePrimeIntervalPatternUpToNextPrimeSquared(i);
                //Console.WriteLine("PrimeIndex {0,3} Prime {1,5} SequenceLength {2}",
                //    i, PrimeTable.Values[i], sequence.Count);
                Console.WriteLine("{0,3},{1,5},{2}",
                    i, PrimeTable.Values[i], sequence.Count);
            }
        }

        [TestMethod]
        public void VerifyPatternElevationLemma()
        {
            UInt32 max = 200;

            List<UInt32> previousPattern = CreatePrimeIntervalPatternUpToNextPrimeSquared(1);
            
            for(UInt32 i = 2; i <= max; i++)
            {
                Console.WriteLine("At Prime Index {0}", i);

                List<UInt32> currentPattern = CreatePrimeIntervalPatternUpToNextPrimeSquared(i);

                // Verify first
                Assert.AreEqual(currentPattern[0], previousPattern[0] + previousPattern[1]);
                //Console.WriteLine(currentPattern[0]);

                for (int j = 2; j < previousPattern.Count; j++)
                {
                    Assert.AreEqual(currentPattern[j - 1], previousPattern[j], String.Format("Failed at sequence index {0}", j-1));
                    //Console.WriteLine(currentPattern[j-1]);
                }

                previousPattern = currentPattern;
            }
        }
        [TestMethod]
        public void TestLimitOfPatternElevationLemma()
        {
            UInt32 max = 5;

            UInt32[] previousPattern = PatternResearch.CreatePrimeIntervalPattern(1);

            for (UInt32 i = 2; i <= max; i++)
            {
                Console.WriteLine("At Prime Index {0} (PrimeValue {1})", i, PrimeTable.Values[i]);

                UInt32[] currentPattern = PatternResearch.CreatePrimeIntervalPattern(i);


                UInt32 sum = 1 + currentPattern[0];
                // Verify first                
                Assert.AreEqual(currentPattern[0], previousPattern[0] + previousPattern[1]);
                //Console.WriteLine(currentPattern[0]);

                for (int j = 2; j < previousPattern.Length; j++)
                {
                    if (currentPattern[j - 1] != previousPattern[j])
                    {
                        Console.WriteLine("Mismatch at value {0}: sequence index {1} ({2} != {3})", sum, j - 1, currentPattern[j - 1], previousPattern[j]);
                        break;
                    }
                    sum += currentPattern[j - 1];
                }

                previousPattern = currentPattern;
            }
        }




        [TestMethod]
        public void PrettyFormatPrintPatternToNextPrimeSquared()
        {
            UInt32 maxLength = 55;
            for (UInt32 i = 1; i <= 10; i++)
            {
                PrettyFormatPrintPatternToNextPrimeSquared(i, maxLength);
            }
        }
        void PrettyFormatPrintPatternToNextPrimeSquared(UInt32 maxPrimeIndex, UInt32 maxLength)
        {
            List<UInt32> pattern = CreatePrimeIntervalPatternUpToNextPrimeSquared(maxPrimeIndex);

            Int32 limit = (maxLength <= pattern.Count) ? (Int32)maxLength : pattern.Count;
            Console.Write("MaxPrime {0,5}: ", PrimeTable.Values[maxPrimeIndex]);
            for (Int32 i = 0; i < limit; i++)
            {
                Console.Write("{0,3}", pattern[i]);
            }
            if (maxLength < pattern.Count)
            {
                Console.Write(" ... {0} more value", pattern.Count - maxLength);
            }
            Console.WriteLine();
        }

        public List<UInt32> CreatePrimeIntervalPatternUpToNextPrimeSquared(UInt32 maxPrimeIndex)
        {
            List<UInt32> pattern = new List<UInt32>();

            UInt32 lastValue = 1;
            UInt32 currentValue = 3;

            UInt32 nextPrime = PrimeTable.Values[maxPrimeIndex + 1];
            UInt32 nextPrimeSquared = nextPrime * nextPrime;

            while (currentValue < nextPrimeSquared)
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
                    pattern.Add(currentValue - lastValue);
                    lastValue = currentValue;
                }

                currentValue += 2;
            }

            return pattern;
        }

        //
        // Analyzing difference in sequences
        //
        [TestMethod]
        public void AnalyzeDifferenceInCoprimeIntervalSequences()
        {
            UInt32 max = 5;

            UInt32[] previousPattern = PatternResearch.CreatePrimeIntervalPattern(1);

            for (UInt32 i = 2; i <= max; i++)
            {
                Console.WriteLine("At Prime Index {0} Value {1}", i, PrimeTable.Values[i]);
                UInt32 totalValuesKnockedOutFromPreviousSequence = 0;

                UInt32[] currentPattern = PatternResearch.CreatePrimeIntervalPattern(i);


                UInt32 previousPatternIndex = 0;
                UInt32 currentPatternIndex = 0;

                while (currentPatternIndex < currentPattern.Length)
                {
                    UInt32 currentPatternInterval = currentPattern[currentPatternIndex++];
                    if(currentPatternInterval == previousPattern[previousPatternIndex])
                    {
                        previousPatternIndex++;
                        if (previousPatternIndex >= previousPattern.Length) previousPatternIndex = 0;
                    }
                    else
                    {
                        // Find the sum of the previous patterns
                        UInt32 previousPatternIntervalSum = previousPattern[previousPatternIndex];
                        while (true)
                        {
                            previousPatternIndex++;
                            if (previousPatternIndex >= previousPattern.Length) previousPatternIndex = 0;

                            previousPatternIntervalSum += previousPattern[previousPatternIndex];
                            totalValuesKnockedOutFromPreviousSequence++;

                            if (previousPatternIntervalSum > currentPatternInterval) throw new InvalidOperationException(String.Format(
                                 "previousPatternIntervalSum {0} is greater than currentPatternInterval {1}", previousPatternIntervalSum, currentPatternInterval));

                            if (previousPatternIntervalSum == currentPatternInterval)
                            {
                                //Console.WriteLine("CurrentPattern knocked out {0} numbers from previous pattern", sumValueCount);
                                previousPatternIndex++;
                                if (previousPatternIndex >= previousPattern.Length) previousPatternIndex = 0;
                                break;
                            }
                        }
                    }
                }

                Console.WriteLine("CurrentPattern knocked out {0} numbers from previous pattern", totalValuesKnockedOutFromPreviousSequence);
                previousPattern = currentPattern;
            }
            
        }




        //
        // The CoprimeSequence up to next prime squared
        //
        [TestMethod]
        public void PrettyFormatPrintCoprimeSequenceToNextPrimeSquared()
        {
            UInt32 maxLength = 55;
            for (UInt32 i = 1; i <= 10; i++)
            {
                PrettyFormatPrintCoprimeSequenceToNextPrimeSquared(i, maxLength);
            }
        }
        void PrettyFormatPrintCoprimeSequenceToNextPrimeSquared(UInt32 maxPrimeIndex, UInt32 maxLength)
        {
            List<UInt32> pattern = CreateCoprimeSequenceUpToNextPrimeSquared(maxPrimeIndex);

            Int32 limit = (maxLength <= pattern.Count) ? (Int32)maxLength : pattern.Count;
            Console.Write("MaxPrime {0,5}: ", PrimeTable.Values[maxPrimeIndex]);
            for (Int32 i = 0; i < limit; i++)
            {
                Console.Write("{0,8}", pattern[i]);
            }
            if (maxLength < pattern.Count)
            {
                Console.Write(" ... {0} more value", pattern.Count - maxLength);
            }
            Console.WriteLine();
        }
        public List<UInt32> CreateCoprimeSequenceUpToNextPrimeSquared(UInt32 maxPrimeIndex)
        {
            List<UInt32> pattern = new List<UInt32>();

            UInt32 currentValue = 1;

            UInt32 nextPrime = PrimeTable.Values[maxPrimeIndex + 1];
            UInt32 nextPrimeSquared = nextPrime * nextPrime;

            while (currentValue < nextPrimeSquared)
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
                    pattern.Add(currentValue);
                }

                currentValue += 2;
            }

            return pattern;
        }



        [TestMethod]
        public void PrettyFormatPrintFixedLengthPattern()
        {
            UInt32 patternLength = 50;
            for (UInt32 i = 1; i <= 1000; i++)
            {
                PrettyFormatPrintFixedLengthPattern(i, patternLength);
            }
        }
        void PrettyFormatPrintFixedLengthPattern(UInt32 maxPrimeIndex, UInt32 patternLength)
        {
            UInt32[] pattern = CreatePrimeIntervalPatternUpToGivenLength(maxPrimeIndex, patternLength);
            Console.Write("MaxPrime {0,6}: ", PrimeTable.Values[maxPrimeIndex]);
            for (Int32 i = 0; i < pattern.Length; i++)
            {
                Console.Write("{0,5}", pattern[i]);
            }
            Console.WriteLine();
        }
        public UInt32[] CreatePrimeIntervalPatternUpToGivenLength(UInt32 maxPrimeIndex, UInt32 length)
        {
            UInt32[] pattern = new UInt32[length];
            UInt32 patternOffset = 0;

            UInt32 lastValue = 1;
            UInt32 currentValue = 3;

            UInt32 maxPrime = PrimeTable.Values[maxPrimeIndex];
            UInt32 maxPrimeSquared = maxPrime * maxPrime;

            while (patternOffset < pattern.Length)
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




        //
        // Determine length of coprime sequence
        //
        public List<UInt32> CreateCoprimeIntervalSequenceWithoutPrecomputingLength(UInt32 maxPrimeIndex)
        {
            List<UInt32> pattern = new List<UInt32>();

            UInt32 lastValue = 1;
            UInt32 currentValue = 3;

            UInt32 nextPrime = PrimeTable.Values[maxPrimeIndex + 1];
            UInt32 nextPrimeSquared = nextPrime * nextPrime;

            while (currentValue < nextPrimeSquared)
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
                    pattern.Add(currentValue - lastValue);
                    lastValue = currentValue;
                }

                currentValue += 2;
            }

            return pattern;
        }
    }
}
