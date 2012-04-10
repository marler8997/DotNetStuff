using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.Hmd
{
    [TestClass]
    public class TypePropertyParserTest
    {
        [TestMethod]
        public void BoolTypeParserTest()
        {
            TestUtil.TestPropertyParser("bool", HmdType.Boolean);
            TestUtil.TestPropertyParser("    \n\n bool\t\t", HmdType.Boolean);
            
            // forgot a character
            TestUtil.TestPropertyParserFormatException("\t\t  boo");

            // one of the characters is wrong
            TestUtil.TestPropertyParserFormatException("\t\t  boOl");

            // specified it twice!
            TestUtil.TestPropertyParserFormatException("boolbool");
            TestUtil.TestPropertyParserFormatException("bool bool");
        }

        [TestMethod]
        public void EmptyTypeParserTest()
        {
            TestUtil.TestPropertyParser("empty", HmdType.Empty);
            TestUtil.TestPropertyParser("    \n\n empty\t\t", HmdType.Empty);

            // forgot a character
            TestUtil.TestPropertyParserFormatException("\t\t  empt");

            // one of the characters is wrong
            TestUtil.TestPropertyParserFormatException("\t\t  emPty");

            // specified it twice!
            TestUtil.TestPropertyParserFormatException("emptyempty");
            TestUtil.TestPropertyParserFormatException("empty empty");
        }

        [TestMethod]
        public void StringTypeParserTest()
        {
            TestUtil.TestPropertyParser("string", HmdType.String);
            TestUtil.TestPropertyParser("    \n\n string\t\t", HmdType.String);

            // forgot a character
            TestUtil.TestPropertyParserFormatException("\t\t  strin");

            // one of the characters is wrong
            TestUtil.TestPropertyParserFormatException("\t\t  stRing");

            // specified it twice!
            TestUtil.TestPropertyParserFormatException("stringstring");
            TestUtil.TestPropertyParserFormatException("string string");
        }

        [TestMethod]
        public void TestPropertyParserIntegerType()
        {
            TestUtil.TestPropertyParser("int", HmdType.Int);
            TestUtil.TestPropertyParser("uint", HmdType.UInt);
            TestUtil.TestPropertyParser("int4", HmdType.Int4);
            TestUtil.TestPropertyParser("uint4", HmdType.UInt4);
            TestUtil.TestPropertyParser("int16", HmdType.Int16);
            TestUtil.TestPropertyParser("uint16", HmdType.UInt16);
            TestUtil.TestPropertyParser("    \n\n int9\t\t", HmdType.Int9);

            TestUtil.TestPropertyParserArgumentException("int17");
            TestUtil.TestPropertyParserArgumentException("uint17");

            TestUtil.TestPropertyParserFormatException("\t\t  in");
            TestUtil.TestPropertyParserFormatException("\t\t  uin");
            TestUtil.TestPropertyParserFormatException("\t\t  iNt");
            TestUtil.TestPropertyParserFormatException("\t\t  Uint");
            TestUtil.TestPropertyParserFormatException("uintint");
            TestUtil.TestPropertyParserFormatException("int4int");
        }


        [TestMethod]
        public void TestPropertyParserEnumType()
        {
            TestUtil.TestPropertyParserEnumReference("enum MyEnum", "MyEnum");
            TestUtil.TestPropertyParserEnumReference("    \n\n enum MyEnum\t\t", "MyEnum");
            TestUtil.TestPropertyParserEnumReference("    \n\n enum\n\t   MyEnum\t\t", "MyEnum");

            TestUtil.TestPropertyParserFormatException("e");
            TestUtil.TestPropertyParserFormatException("en");
            TestUtil.TestPropertyParserFormatException("enu");
            TestUtil.TestPropertyParserFormatException("enum");
            TestUtil.TestPropertyParserFormatException("\t\tenum\t \n");
            TestUtil.TestPropertyParserFormatException("enum Hey enum What");
        }

        [TestMethod]
        public void TestPropertyParserEnumInlineType()
        {
            TestUtil.TestPropertyParserInlineEnum("enum(A C B EeEef Dee gghij)", "a", "c", "b", "eeeef", "DEE", "GGHIJ");
            TestUtil.TestPropertyParserInlineEnum("    \n\n enum (z paint1 HelloHowAreYou what zzoz)\t\t", "Z", "PaInT1", "hELLOhOWaREyOU", "WHAT", "ZZOZ");
            TestUtil.TestPropertyParserInlineEnum("enum(Off Port1 Port2 Port3 Port4)", "off", "PORT1", "port2", "pOrT3", "PoRt4");

            TestUtil.TestPropertyParserInlineEnum("enum  (     A  C     B  EeEef Dee gghij)", "a", "c", "b", "EEEEf", "dEE", "gGhIj");

            TestUtil.TestPropertyParserFormatException("\t\t  enum(hello how are\t\t");
            TestUtil.TestPropertyParserFormatException("\t\t  enun(MyEnum\ta)");
            TestUtil.TestPropertyParserFormatException("enum(hey what) enum(hen pig)");
        }

    }
}
