using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.Hmd
{

    [TestClass]
    public class ParentPropertyParserTest
    {
        private void TestParseParentList(String props, int offset, params String[] expectedParents)
        {
            int parentLength = expectedParents.Length;
            HmdParentReference[] actualParents = HmdParser.ParseParentList(props, ref offset);

            Assert.AreEqual(actualParents.Length, parentLength);

            Boolean [] actualParentalreadyMatchd = new Boolean[parentLength];
            int lowestNonMatchedParent = 0;

            for (int i = 0; i < parentLength; i++)
            {
                Boolean expectedParentFound = false;
                for (int j = lowestNonMatchedParent; j < parentLength; j++)
                {
                    if(actualParents[j].IDLowerCase.Equals(expectedParents[i],StringComparison.CurrentCultureIgnoreCase))
                    {                        
                        if(actualParentalreadyMatchd[j])
                        {
                            throw new InvalidOperationException("Found the same parent twice...what??");
                        }
                        actualParentalreadyMatchd[j] = true;

                        if(j == lowestNonMatchedParent)
                        {
                            lowestNonMatchedParent++;
                            while(lowestNonMatchedParent < parentLength && actualParentalreadyMatchd[lowestNonMatchedParent])
                            {
                                lowestNonMatchedParent++;
                            }
                        }

                        expectedParentFound = true;
                        break;
                    }
                }
                if (!expectedParentFound)
                {
                    Assert.Fail(String.Format("The expected parent \"{0}\" was not found in the actual parents", expectedParents[i]));
                }
            }
        }

        private void TestParseParentListFormatException(String props, int offset)
        {
            try
            {
                HmdParser.ParseParentList(props, ref offset);
                Assert.Fail("Expected a FormatException, but didn't get one");
            }
            catch (FormatException e)
            {
                Console.WriteLine("Expected Exception: \"{0}\"", e.Message);
            }
        }

        private void TestParseParentListArgumentException(String props, int offset)
        {
            try
            {
                HmdParser.ParseParentList(props, ref offset);
                Assert.Fail("Expected an ArgumentException, but didn't get one");
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Expected Exception: \"{0}\"", e.Message);
            }
        }

        [TestMethod]
        public void DirectParseParentListTest()
        {
            TestParseParentList("   (a b c d)   ", 3, "a", "b", "c", "d");
            TestParseParentList("  (apple bear w112)", 2, "w112", "apple", "bear");
            TestParseParentList("  (h123\t\t\nhello\n  \ta111)", 2, "h123", "hello", "a111");


            TestParseParentListArgumentException("   [a b c d)", 3);
            TestParseParentListArgumentException(" a)", 0);

            TestParseParentListArgumentException("", 0);
            TestParseParentListArgumentException("(a)", 3);

            TestParseParentListFormatException("  (1)", 2);

            TestParseParentListFormatException("(a b c", 0);
            TestParseParentListFormatException("(a b c ", 0);
            TestParseParentListFormatException("(a b c]", 0);
            TestParseParentListFormatException("(", 0);

            TestParseParentListFormatException("()", 0);
            TestParseParentListFormatException("(\t\t )", 0);

        }


        [TestMethod]
        public void General()
        {
            TestUtil.TestPropertyParser("(a)   ", "a");
            TestUtil.TestPropertyParser("(a b c)\t\t ", "a", "b", "c");
            TestUtil.TestPropertyParser("( a)   ", "a");
            TestUtil.TestPropertyParser("( a     )", "a");
            TestUtil.TestPropertyParser("( a   b     defe    )", "a","b","defe");

        }
    }
}
