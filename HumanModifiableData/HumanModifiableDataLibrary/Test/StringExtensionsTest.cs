using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.Hmd
{
    [TestClass]
    public class StringExtensionsTest
    {
        private void Validate(String str, int offset, params String[] values)
        {
            String[] parsedValues = str.SplitByWhitespace(offset);
            if (values == null)
            {
                Assert.IsNull(parsedValues);
            }
            else
            {
                Assert.AreEqual(values.Length, parsedValues.Length);
                for (int i = 0; i < values.Length; i++)
                {
                    Assert.IsTrue(values[i].Equals(parsedValues[i], StringComparison.CurrentCultureIgnoreCase));
                }
            }
        }

        [TestMethod]
        public void SplitByWhitespaceTest()
        {
            Validate("a b c d", 0, "a", "b", "c", "d");
            Validate(" \n\t  apple  \t\n\n  bear    \tcan dog", 0, "apple", "bear", "can", "dog");

            Validate("a b c d", 1, "b", "c", "d");
            Validate(" \n\t  apple  \t\n\n  bear    \tcan dog", 10, "bear", "can", "dog");
        }

        [TestMethod]
        public void EqualsSubstringTest()
        {
            Assert.IsTrue("dsk".IsSubstring("%cat%dsk%ghd", 5));
        }
    }
}
