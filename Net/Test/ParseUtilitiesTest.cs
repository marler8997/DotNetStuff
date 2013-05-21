using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.Net
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
            ValidateStringArray(ParseUtilities.SplitCorrectly("1,2,3,4", ','), "1", "2", "3", "4");
            ValidateStringArray(ParseUtilities.SplitCorrectly("1", ','), "1");
            ValidateStringArray(ParseUtilities.SplitCorrectly("100", ','), "100");

            try
            {
                ValidateStringArray(ParseUtilities.SplitCorrectly(",1", ','), "1");
                Assert.Fail();
            }
            catch (FormatException) { }
            try
            {
                ValidateStringArray(ParseUtilities.SplitCorrectly(",", ','), "1");
                Assert.Fail();
            }
            catch (FormatException) { }
            try
            {
                ValidateStringArray(ParseUtilities.SplitCorrectly("1,", ','), "1");
                Assert.Fail();
            }
            catch (FormatException) { }

        }
    }


}
