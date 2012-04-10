using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.Hmd
{
    public static class TestUtil
    {
        private static HmdProperties TestPropertyDictionary = new HmdProperties();

        public static void TestPropertyParserFormatException(String propertyString)
        {
            TestPropertyParserFormatException(propertyString, TestPropertyDictionary);
        }
        public static void TestPropertyParserFormatException(String propertyString, HmdProperties hmdProperties)
        {
            try
            {
                CallParserWithDefaults(propertyString, hmdProperties);
                Assert.Fail(String.Format("Expected parsing \"{0}\" to cause a FormatException, but it didn't",propertyString));
            }
            catch (FormatException formatException)
            {
            }
        }
        public static void TestPropertyParserArgumentException(String propertyString)
        {
            TestPropertyParserArgumentException(propertyString, TestPropertyDictionary);
        }
        public static void TestPropertyParserArgumentException(String propertyString, HmdProperties hmdProperties)
        {
            try
            {
                CallParserWithDefaults(propertyString, hmdProperties);
                Assert.Fail(String.Format("Expected parsing \"{0}\" to cause a ArgumentException, but it didn't", propertyString));
            }
            catch (ArgumentException argumentException)
            {
            }
        }

        public static void TestPropertyParser(String propertyString, ICountProperty expectedCountProperty)
        {
            HmdValueIDProperties valueIDProperties = CallParserWithDefaults(propertyString, TestPropertyDictionary);
            VerifyValueProperties(String.Empty, valueIDProperties, expectedCountProperty, HmdType.String, null);
        }

        public static void TestPropertyParser(String propertyString, HmdType hmdType)
        {
            HmdValueIDProperties valueIDProperties = CallParserWithDefaults(propertyString, TestPropertyDictionary);
            VerifyValueProperties(String.Empty, valueIDProperties, UnrestrictedCount.Instance, hmdType, null);
        }

        public static void TestPropertyParserInlineEnum(String propertyString, params String[] values)
        {
            HmdValueIDProperties valueIDProperties = CallParserWithDefaults(propertyString, TestPropertyDictionary);
            VerifyValueProperties(String.Empty, valueIDProperties, UnrestrictedCount.Instance, HmdType.Enumeration, "");

            //
            // Verify enum values
            //
            HmdEnumReference hmdEnumReference = valueIDProperties.EnumReference;
            HmdEnum hmdEnum = hmdEnumReference.TryGetReference;

            Assert.IsNotNull(hmdEnum);
            for (int i = 0; i < values.Length; i++)
            {
                Assert.IsTrue(hmdEnum.IsValidEnumValue(values[i]));
            }
        }
        
        public static void TestPropertyParserEnumReference(String propertyString, String enumReferenceTypeString)
        {
            HmdValueIDProperties valueIDProperties = CallParserWithDefaults(propertyString, TestPropertyDictionary);
            VerifyValueProperties(String.Empty, valueIDProperties, UnrestrictedCount.Instance, HmdType.Enumeration, enumReferenceTypeString);
        }

        public static void TestPropertyParser(String propertyString, params String[] expectedParents)
        {
            HmdValueIDProperties valueIDProperties = CallParserWithDefaults(propertyString, TestPropertyDictionary);
            VerifyValueProperties(String.Empty, valueIDProperties, UnrestrictedCount.Instance, HmdType.String, null, expectedParents);
        }

        private static HmdValueIDProperties CallParserWithDefaults(String propertyString, HmdProperties hmdProperties)
        {
            return HmdParser.ParseValueProperties(String.Empty, propertyString, hmdProperties.root, hmdProperties);
        }


        private static void VerifyValueProperties(String idString, HmdValueIDProperties valueIDProperties, ICountProperty expectedCountProperty,
            HmdType hmdType, String enumReferenceName, params String[] parentOverrideArray)
        {
            // Verify idString
            Assert.AreEqual(idString, valueIDProperties.idLowerCase);

            // Verify CountProperty is correct
            Assert.IsTrue(expectedCountProperty.Equals(valueIDProperties.CountProperty));

            // Verify Parents are correct
            if (parentOverrideArray != null)
            {
                Assert.AreEqual(parentOverrideArray.Length, valueIDProperties.ParentOverrideCount);
                for (int i = 0; i < parentOverrideArray.Length; i++)
                {
                    Assert.IsTrue(valueIDProperties.IsInParentOverrideList(parentOverrideArray[i]));
                }
            }

            // Verify HmdType is correct
            Assert.AreEqual(hmdType, valueIDProperties.hmdType);
            if (enumReferenceName == null)
            {
                Assert.IsNull(valueIDProperties.EnumReference);
            }
            else
            {
                Assert.AreEqual(enumReferenceName, valueIDProperties.EnumReference.Name);
            }

        }


    }
}
