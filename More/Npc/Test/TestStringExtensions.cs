using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class TestStringExtensions
    {
        /*
        [TestMethod]
        public void TestEscapeSpecialNpcArrayCharacters()
        {
            String[][] testStrings = new String[][] {
                new String[]{ @"", @"" },
                new String[]{ @"no-escapes", @"no-escapes" },
                new String[]{ @"leftbrace{", @"leftbrace\{" },
                new String[]{ @"rightbrace}", @"rightbrace\}" },
                new String[]{ @"comma,", @"comma\," },
                new String[]{ @"backslash\", @"backslash\\" },
                new String[]{ @"{{,\,,\}},\\\", @"\{\{\,\\\,\,\\\}\}\,\\\\\\"},
                new String[]{ @"{anarray,what\\,,,}", @"\{anarray\,what\\\\\,\,\,\}" },
            };

            for (int i = 0; i < testStrings.Length; i++)
            {
                Assert.AreEqual(testStrings[i][1], testStrings[i][0].EscapeSpecialNpcArrayCharacters());
            }

            for (int i = 0; i < testStrings.Length; i++)
            {
                Assert.AreEqual(testStrings[i][0], testStrings[i][1].UnescapeSpecialNpcArrayCharacters());
            }
        }
        */
    }
}
