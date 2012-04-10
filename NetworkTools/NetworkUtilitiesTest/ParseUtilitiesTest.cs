using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.NetworkTools
{
    [TestClass]
    public class ParseUtilitiesTest
    {


        public void ValidateLiteralString(String literal, String actual)
        {
            Int32 outLength;
            byte[] data = ParseUtilities.ParseLiteralString(literal, 0, out outLength);

            byte[] expectedBytes = Encoding.UTF8.GetBytes(actual);
            Assert.AreEqual(expectedBytes.Length, outLength);
            for (int i = 0; i < expectedBytes.Length; i++)
            {
                Assert.AreEqual(expectedBytes[i], data[i]);
            }


        }

        [TestMethod]
        public void ParseLiteralStringTest()
        {
            Int32 outLength;
            try
            {
                ParseUtilities.ParseLiteralString(@"\", 0, out outLength);
                Assert.Fail();
            }
            catch (FormatException) { }
            try
            {
                ParseUtilities.ParseLiteralString(@"\e", 0, out outLength);
                Assert.Fail();
            }
            catch (FormatException) { }
            try
            {
                ParseUtilities.ParseLiteralString(@"\x0", 0, out outLength);
                Assert.Fail();
            }
            catch (FormatException) { }

            ValidateLiteralString(@"\n", "\n");
            ValidateLiteralString(@"\n\\\0\a\r\t\v\x01", "\n\\\0\a\r\t\v\x01");
            ValidateLiteralString(@"hey \nwhat \\I am \0 testing\a\r\t\v \x67", "hey \nwhat \\I am \0 testing\a\r\t\v \x67");
        }


        public void ValidateStringArray(String[] actualStrings, params String[] expectedStrings)
        {
            Assert.AreEqual(expectedStrings.Length, actualStrings.Length);

            for (int i = 0; i < expectedStrings.Length; i++)
            {
                Assert.AreEqual(expectedStrings[i], actualStrings[i]);
            }
        }

        [TestMethod]
        public void SplitCorrectlyTest()
        {
            ValidateStringArray("1,2,3,4".SplitCorrectly(','), "1", "2", "3", "4");
            ValidateStringArray("1".SplitCorrectly(','), "1");
            ValidateStringArray("100".SplitCorrectly(','), "100");

            try
            {
                ValidateStringArray(",1".SplitCorrectly(','), "1");
                Assert.Fail();
            }
            catch (FormatException) { }
            try
            {
                ValidateStringArray(",".SplitCorrectly(','), "1");
                Assert.Fail();
            }
            catch (FormatException) { }
            try
            {
                ValidateStringArray("1,".SplitCorrectly(','), "1");
                Assert.Fail();
            }
            catch (FormatException) { }

        }
    }


}
