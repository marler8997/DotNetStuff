using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Marler.Hmd
{
    /// <summary>
    /// Summary description for HmdEnumTest
    /// </summary>
    [TestClass]
    public class HmdEnumTest
    {
        private void ValidateHmdEnumInlineCreateEnum(String enumInlineDefinition, params String[] expectedValues)
        {
            ValidateHmdEnum(new HmdEnum(String.Empty, enumInlineDefinition), String.Empty, expectedValues);
        }

        public void ValidateHmdEnum(HmdEnum hmdEnum, String name, params String[] values)
        {
            Assert.IsTrue(hmdEnum.name.Equals(name, StringComparison.CurrentCultureIgnoreCase));

            Assert.AreEqual(values.Length, hmdEnum.ValueCount);
            for (int i = 0; i < values.Length; i++)
            {
                Assert.IsTrue(hmdEnum.IsValidEnumValue(values[i]));
            }
        }

        public void ValidateHmdEnumFormatException(String enumDefinition)
        {
            try
            {
                new HmdEnum(enumDefinition);
                Assert.Fail("Expected a FormatException, but didn't get one");
            }
            catch (FormatException e)
            {
                Console.WriteLine("Got Expected Exception \"{0}\"", e.Message);
            }
        }

        [TestMethod]
        public void HmdEnumInlineCreateEnumTest()
        {
            ValidateHmdEnumInlineCreateEnum("apple bear","apple","bear");
        }


        [TestMethod]
        public void HmdEnumReferenceInlineTest()
        {
            HmdEnum hmdEnum;

            hmdEnum = new HmdEnum(null, "  \n\n full\t\t  high");
            Assert.IsTrue(hmdEnum.IsValidEnumValue("high"));
            Assert.IsTrue(hmdEnum.IsValidEnumValue("HIGH"));
            Assert.IsTrue(hmdEnum.IsValidEnumValue("full"));
            Assert.IsTrue(hmdEnum.IsValidEnumValue("FuLl"));

            hmdEnum = new HmdEnum(null, "  a1234");
            Assert.IsTrue(hmdEnum.IsValidEnumValue("a1234"));
            Assert.IsTrue(hmdEnum.IsValidEnumValue("A1234"));
        }


        [TestMethod]
        public void HmdEnumConstructorTest()
        {
            ValidateHmdEnum(new HmdEnum("1 2"), "1","2");
            ValidateHmdEnum(new HmdEnum("enumName apple bear can dog elephant"), "ENUMNAME", "Apple", "BEAR", "cAN", "dog", "ElePHant");
            ValidateHmdEnum(new HmdEnum("  tESTeNUM   \n\n a  b \tc \n\n  "), "teStEnum", "a", "b", "c");
            ValidateHmdEnum(new HmdEnum("    \n\n\t  blahblah    \n\n haha11  z1234 \te"), "blahblah", "haha11", "z1234", "e");

            ValidateHmdEnumFormatException(String.Empty);
            ValidateHmdEnumFormatException("   \t\t\n   ");
            ValidateHmdEnumFormatException("NoValues");
            ValidateHmdEnumFormatException("\n\n  \t NoValues  \t");

        }


        [TestMethod]
        public void ResolveEnumReferenceTest()
        {
            List<HmdEnum> enumReferenceList = new List<HmdEnum>();



        }
    }
}
