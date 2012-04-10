using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.Hmd
{

    [TestClass]
    public class CountPropertyParserTest
    {
        [TestMethod]
        public void UnrestrictedCountParserTest()
        {
            TestUtil.TestPropertyParser("0-*", UnrestrictedCount.Instance);
            TestUtil.TestPropertyParser("   0-*   ", UnrestrictedCount.Instance);
            TestUtil.TestPropertyParser("0-*    \t\t\n", UnrestrictedCount.Instance);
            TestUtil.TestPropertyParser("\t\t  0-* \n\n \t", UnrestrictedCount.Instance);
        }

        [TestMethod]
        public void StaticCountParserTest()
        {
            StaticCount static0Count = new StaticCount(0);
            StaticCount static939Count = new StaticCount(939);
            StaticCount static65535Count = new StaticCount(65535);

            TestUtil.TestPropertyParser("0", static0Count);
            TestUtil.TestPropertyParser("\t\t  0 \n\n \t", static0Count);

            TestUtil.TestPropertyParser("939", static939Count);
            TestUtil.TestPropertyParser("\t\t  939 \n\n \t", static939Count);

            TestUtil.TestPropertyParser("      65535", static65535Count);
            TestUtil.TestPropertyParser("   \t\t    65535 \n   \n  \t", static65535Count);

            //
            // Test some larger static counts
            //
            for (UInt32 i = 0; i < 1000000; i += 95416)
            {
                StaticCount staticCount = (StaticCount)CountProperty.Parse(i.ToString());
                Assert.AreEqual(i, staticCount.count);
            }
        }

        [TestMethod]
        public void CountWithMinParserTest()
        {
            CountWithMin countWithMinOf1 = new CountWithMin(1);
            CountWithMin countWithMinOf99999 = new CountWithMin(99999);

            TestUtil.TestPropertyParser("1-*", countWithMinOf1);
            TestUtil.TestPropertyParser("\t\t  1-* \n\n \t", countWithMinOf1);

            TestUtil.TestPropertyParser("99999-*", countWithMinOf99999);
            TestUtil.TestPropertyParser("\t\t  99999-* \n\n \t", countWithMinOf99999);

            //
            // Test that it disallows CountWithMin(0)
            //
            #if DEBUG
            try
            {
                CountWithMin countWithMin = new CountWithMin(0);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine(String.Format("Caught expected exception {0}", e));
            }
            #endif

            //
            // Test some larger Count with min
            //
            for (UInt32 i = 1; i < 1000000; i += 95416)
            {
                CountWithMin countWithMin = (CountWithMin)CountProperty.Parse(i.ToString() + "-*");
                Assert.AreEqual(i, countWithMin.min);
            }
        }

        [TestMethod]
        public void CountWithMaxParserTest()
        {
            CountWithMax countWithMaxOf1 = new CountWithMax(1);
            CountWithMax countWithMaxOf83729 = new CountWithMax(83729);

            TestUtil.TestPropertyParser("0-1", countWithMaxOf1);
            TestUtil.TestPropertyParser("\t\t  0-1 \n\n \t", countWithMaxOf1);

            TestUtil.TestPropertyParser("0-83729", countWithMaxOf83729);
            TestUtil.TestPropertyParser("\t\t  0-83729 \n\n \t", countWithMaxOf83729);


            //
            // Test that it disallows CountWithMax(0)
            //
            #if DEBUG
            try
            {
                CountWithMax countWithMax = new CountWithMax(0);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine(String.Format("Caught expected exception {0}", e));
            }
            #endif

            //
            // Test some larger Count with max
            //
            for (UInt32 i = 1; i < 1000000; i += 95416)
            {
                CountWithMax countWithMax = (CountWithMax)CountProperty.Parse("0-" + i.ToString());
                Assert.AreEqual(i, countWithMax.max);
            }
        }

        [TestMethod]
        public void CountWithMinAndMaxParserTest()
        {
            CountWithMinAndMax countWithMinAndMaxA = new CountWithMinAndMax(1,2);
            CountWithMinAndMax countWithMinAndMaxB = new CountWithMinAndMax(4,9938);

            TestUtil.TestPropertyParser("1-2", countWithMinAndMaxA);
            TestUtil.TestPropertyParser("\t\t  1-2 \n\n \t", countWithMinAndMaxA);

            TestUtil.TestPropertyParser("4-9938", countWithMinAndMaxB);
            TestUtil.TestPropertyParser("\t\t  4-9938 \n\n \t", countWithMinAndMaxB);


            //
            // Test that it disallows certain min/max pairs
            //
            try
            {
                CountWithMinAndMax countWithMinAndMax = new CountWithMinAndMax(2, 1);
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(String.Format("Caught expected exception {0}", e));
            }
            try
            {
                CountWithMinAndMax countWithMinAndMax = new CountWithMinAndMax(999, 999);
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(String.Format("Caught expected exception {0}", e));
            }

            //
            // Test some larger values
            //
            for (UInt32 i = 1; i < 1000000; i += 95416)
            {
                for (UInt32 j = i + 1; j < 1000000; j += 95416)
                {
                    CountWithMinAndMax countWithMinAndMax = (CountWithMinAndMax)CountProperty.Parse(String.Format("{0}-{1}", i, j));
                    Assert.AreEqual(i, countWithMinAndMax.min);
                    Assert.AreEqual(j, countWithMinAndMax.max);
                }
            }
        }
    }
}
