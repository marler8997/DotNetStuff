using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [NpcInterface]
    interface StringArrayWrapperInterface
    {
        String[] GetStrings();
        void SetStrings(String[] strings);
        Boolean Equals(String[] strings);
    }

    class TestStringArrayWrapper : StringArrayWrapperInterface
    {
        public String[] strings;

        public TestStringArrayWrapper(String[] strings)
        {
            this.strings = strings;
        }

        public String[] GetStrings()
        {
            return strings;
        }

        public void SetStrings(String[] strings)
        {
            this.strings = strings;
        }

        public Boolean Equals(String[] strings)
        {
            if (this.strings == null) return strings == null;
            if (strings == null) return false;
            if (this.strings.Length != strings.Length) return false;
            for (int i = 0; i < strings.Length; i++)
            {
                if (!this.strings[i].Equals(strings[i])) return false;
            }
            return true;
        }
    }

    [TestClass]
    public class TestArrayParameters
    {
        class ArrayTester
        {
            public Array array;
            public String expected;
            public ArrayTester(Array array, String expected)
            {
                this.array = array;
                this.expected = expected;
            }
        }

        [NpcInterface]
        interface NpcInterfaceWithPrimitiveTypes
        {
            Boolean Method(Char c, Byte b, Int16 i, UInt16 u, Int32 i32, UInt32 u32, String str);
        }

        class ClassWithNpcPrimitiveTypes : NpcInterfaceWithPrimitiveTypes
        {
            public Boolean Method(Char c, Byte b, Int16 i, UInt16 u, Int32 i32, UInt32 u32, String str)
            {
                return true;
            }
        }

        /*
        [TestMethod]
        public void TestNpcToString()
        {
            NpcReflector npcReflector = new NpcReflector(new Object[]{ typeof(ClassWithNpcPrimitiveTypes)});

            ArrayTester[] test = new ArrayTester[] {
                new ArrayTester(new Boolean[] {false,true,true,false,true}, "{False,True,True,False,True}"),
                new ArrayTester(new String[] {@",{}", @"\\\"}, @"{\,\{\},\\\\\\}"),
                new ArrayTester(new String[] {null, "hey", "null"}, @"{null,hey,\\null}"),
            };

            for (int i = 0; i < test.Length; i++)
            {
                Assert.AreEqual(test[i].expected, npcReflector.NpcToString(test[i].array.GetType(), test[i].array));
            }
        }
        */


        class Test
        {
            public readonly String npcString;
            public readonly String[] strings;
            public Test(String npcString, String[] strings)
            {
                this.npcString = npcString;
                this.strings = strings;
            }
        }

        [TestMethod]
        public void TestStringArrays()
        {
            /*
            TestStringArrayWrapper stringArrayWrapper = new TestStringArrayWrapper(null);
            NpcReflector npcReflector = new NpcReflector(new Object[] {stringArrayWrapper } );
            npcReflector.PrintInformation(Console.Out);

            Assert.IsNull(npcReflector.ExecuteWithStrings("More.teststringarraywrapper.getstrings").value);
            Assert.AreEqual("null", npcReflector.ExecuteWithStrings("More.teststringarraywrapper.getstrings").valueSosSerializationString);

            Test[] tests = new Test[] {
                new Test(@"{}", new String[0]),
                new Test(@"{apple,,banana}", new String[]{"apple",String.Empty,"banana"}),
                new Test(@"{null,hey}", new String[]{null,"hey"}),
                new Test(@"{,,}", new String[]{"",String.Empty,String.Empty}),
                new Test(@"{\{}", new String[]{"{"}),
                new Test(@"{\{\}\,\\,\}\}}", new String[]{@"{},\", @"}}"}),

            };

            for(int i = 0; i < tests.Length; i++)
            {
                Test test = tests[i];
                Console.WriteLine("Test {0}", i);
                npcReflector.ExecuteWithStrings("More.teststringarraywrapper.setstrings", test.npcString);

                for (int j = 0; j < test.strings.Length; j++)
                {
                    Assert.AreEqual(test.strings[j], stringArrayWrapper.strings[j]);
                }

                String [] stored = (String[])npcReflector.NpcParseArray(typeof(String),
                    npcReflector.ExecuteWithStrings("More.teststringarraywrapper.getstrings").valueAsNpcString);
                for (int j = 0; j < test.strings.Length; j++)
                {
                    Console.WriteLine("Comparing [{0}] \"{1}\" and \"{2}\"", j, test.strings[j], stored[j]);
                    Assert.AreEqual(test.strings[j], stored[j]);
                }
            }

            // test setting as null
            npcReflector.ExecuteWithStrings("More.teststringarraywrapper.setstrings", "null");
            Assert.IsNull(npcReflector.ExecuteWithStrings("More.teststringarraywrapper.getstrings").value);
            Assert.AreEqual("null", npcReflector.ExecuteWithStrings("More.teststringarraywrapper.getstrings").valueAsNpcString);
            */
        }
    }
}
