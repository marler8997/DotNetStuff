using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

using Marler.Common;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Marler.Net
{
    [TestClass]
    public class PerformanceChecks
    {
        [TestMethod]
        public void TestStringSerializationOptions()
        {
            StringSerializeOptionsPerformance("heytherefjdljakdfjlaskfjlk");
            //StringSerializeOptionsPerformance("heytherefj\\dlj\"akdfjlaskfjlk");
            //StringSerializeOptionsPerformance("heythejdkajfdlasjkfjdlajfka;fjaeijfoajifoasjefa;ojfeiajf8uf0-9u439u8f9upfuaq89fuapjfj3498439125oafjkeajpfje8aj98329tuilajjf9a8j34j;auf8u239j4fi9a83tra98ura9pyu3r84a9fja98rrefjdljakdfjlaskfjlk\\");
        }

        void StringSerializeOptionsPerformance(String testString)
        {
            Char[] stringEscapes = new Char[] {'"','\\'};

            long before;
            String a;


            //
            // This one is best when there are no escape characters, but is by far the worst if it there are escape characters near the end of a long string
            //
            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                if (testString.IndexOfAny(stringEscapes) < 0)
                {
                    a = "\"" + testString + "\"";
                }
                else
                {
                    a = "\"" + testString.Replace(@"\", @"\\").Replace("\"", "\\\"") + "\"";
                }
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());


            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                a = String.Format("\"{0}\"", testString.Replace(@"\", @"\\").Replace("\"", "\\\""));
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                a = '"' + testString.Replace(@"\", @"\\").Replace("\"", "\\\"") + '"';
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                a = "\"" + testString.Replace(@"\", @"\\").Replace("\"", "\\\"") + "\"";
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

        }

        [TestMethod]
        public void ConvertCharToString()
        {
            long before;
            String a;
            Char c = 'a';


            //
            // This one is best when there are no escape characters, but is by far the worst if it there are escape characters near the end of a long string
            //
            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000000; i++)
            {
                a = new String(c, 1);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());


            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000000; i++)
            {
                a = c.ToString();
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }


        [TestMethod]
        public void CharToHexCode()
        {
            ConvertCharToHexCode('a');
            //Console.WriteLine(String.Format(@"\x{0:x4}", (ushort)'a'));
        }
        void ConvertCharToHexCode(Char c)
        {
            long before;
            String a;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                a = String.Format(@"\x{0:x4}", (ushort)c);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                a = @"\x" + String.Format("{0:x4}", (ushort)c);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                a = @"\x" + ((ushort)c).ToString("x4");
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }

        [TestMethod]
        public void CastOrToString()
        {
            CastOrToString("hello");
        }
        public void CastOrToString(Object o)
        {
            long before;
            String a;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000000; i++)
            {
                a = o.ToString();
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000000; i++)
            {
                a = (String)o;
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

        }



        [TestMethod]
        public void IsWhitespace()
        {
            IsWhitespace(' ');
            IsWhitespace('\n');
            IsWhitespace('\t');
            IsWhitespace('\f');
            IsWhitespace('\v');
            IsWhitespace('a');
        }
        public void IsWhitespace(Char c)
        {
            long before;
            Boolean test;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000000; i++)
            {
                test = Char.IsWhiteSpace(c);
            }
            Console.WriteLine("Char.IsWhitespace: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000000; i++)
            {
                test = (c == ' ' || c == '\n' || c == '\t' || c == '\f' || c == '\v');
            }
            Console.WriteLine("Inline           : " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }


        [TestMethod]
        public void TryGetDictionaryValue()
        {
            Dictionary<String, Object> d = new Dictionary<String,Object>();
            d.Add("key", new Object());

            TryGetDictionaryValue(d, "key");
            //TryGetDictionaryValue(d, "key1");
        }
        public void TryGetDictionaryValue(Dictionary<String,Object> d, String key)
        {
            long before;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000000; i++)
            {
                Object o;
                d.TryGetValue(key, out o);
            }
            Console.WriteLine("TryGet: " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 10000000; i++)
            {
                Object o = d[key];
            }
            Console.WriteLine("[]    : " + (Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }

        [TestMethod]
        public void TestSosTypeEquivalence()
        {
            SosEnumDefinition enumDefinition = new SosEnumDefinition();
            enumDefinition.Add("ajdkasjlfjdka", 23432);
            enumDefinition.Add("ferwr", 232);
            enumDefinition.Add("adfffcnz,.nvmzvnzvlvhzvzgfhioawehrhoahchZAfhrffdad", 25432);
            enumDefinition.Add("fdasdffaefkasjlfjasifjelasjfiasjlfjeiajflaejsifjlasjiefjlasdf", 2377);
            enumDefinition.Add("zcdjfkajfljeaijfeoiajfnlzjljfiljlaijlfeiajflajsefiljajfelaljefiajlsefxvzxcvzxvzcxv", 22);
            TestSosTypeEquivalence(enumDefinition);
        }
        public void TestSosTypeEquivalence(SosEnumDefinition enumDefinition)
        {
            String enumDefinitionString = enumDefinition.TypeDefinition();
            String enumDefinitionString2 = enumDefinition.TypeDefinition();


            long before;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                enumDefinition.Equals(enumDefinition);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                enumDefinitionString.Equals(enumDefinitionString2);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }

        [TestMethod]
        public void TestDictionaryIteration()
        {
            List<String> list = new List<String>();
            for (int i = 0; i < 65535; i++)
            {
                list.Add(new String((Char)(i + '~'), 10));
                //Console.WriteLine(list[i]);
            }

            TestDictionaryIteration(list);
        }
        public void TestDictionaryIteration(List<String> values)
        {
            Dictionary<String, String> dictionary = new Dictionary<String, String>();
            foreach (String value in values)
            {
                dictionary.Add(value, value);
            }


            long before;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100; i++)
            {
                foreach (String value in values)
                {

                }
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100; i++)
            {
                foreach (String value in dictionary.Keys)
                {

                }
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }



        /*
        [TestMethod]
        public void TestFloatRecognition()
        {
            TestFloatRecognition("0", 0);
            TestFloatRecognition("-0.00E109", 0);
        }
        public void TestFloatRecognition(String numberString, Int32 offset)
        {
            long before;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                Sos.FloatNumberRegexBase10.Match(numberString, offset, numberString.Length - offset);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                Sos.FloatLength(numberString, offset, numberString.Length);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }

        [TestMethod]
        public void TestWholeNumberRecognition()
        {
            TestWholeNumberRecognition("0", 0);
            TestWholeNumberRecognition("-010923085", 0);
        }
        public void TestWholeNumberRecognition(String numberString, Int32 offset)
        {
            long before;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                Sos.WholeNumberRegexBase10.Match(numberString, offset, numberString.Length - offset);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                Sos.WholeNumberLength(numberString, offset, numberString.Length);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }
        [TestMethod]
        public void TestEnumRecognition()
        {
            TestEnumRecognition("0", 0);
            TestEnumRecognition("-010923085", 0);
            TestEnumRecognition("Apple_Oranges012", 0);
        }
        public void TestEnumRecognition(String numberString, Int32 offset)
        {
            long before;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                Sos.EnumValueNameRegex.Match(numberString, offset, numberString.Length - offset);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                Sos.EnumLength(numberString, offset, numberString.Length);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }
        */

        static readonly Type[] stringParam = new Type[] { typeof(String) };
        [TestMethod]
        public void TestParseNumber()
        {
            TestParseNumber(typeof(UInt16), "32");
            TestParseNumber(typeof(Double), "32.3829E10");
        }
        public void TestParseNumber(Type type, String s)
        {
            long before;


            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                if (type == typeof(SByte))
                {
                    SByte.Parse(s);
                }
                else if (type == typeof(Byte))
                {
                    Byte.Parse(s);
                }
                else if (type == typeof(Int16))
                {
                    Int16.Parse(s);
                }
                else if (type == typeof(UInt16))
                {
                    UInt16.Parse(s);
                }
                else if (type == typeof(Int32))
                {
                    Int32.Parse(s);
                }
                else if (type == typeof(UInt32))
                {
                    UInt32.Parse(s);
                }
                else if (type == typeof(Int64))
                {
                    Int64.Parse(s);
                }
                else if (type == typeof(UInt64))
                {
                    UInt64.Parse(s);
                }
                else if (type == typeof(Single))
                {
                    Single.Parse(s);
                }
                else if (type == typeof(Double))
                {
                    Double.Parse(s);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());



            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 100000; i++)
            {
                MethodInfo parseMethod = type.GetMethod("Parse", stringParam);
                parseMethod.Invoke(null, new Object[] { s });
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }

        [TestMethod]
        public void TestContructionWithReflecion()
        {
            //TestContructionWithReflecion(typeof(UInt16));
            //TestContructionWithReflecion(typeof(Double));
            TestContructionWithReflecion(typeof(TestClasses.ClassWithWeirdTypes));
        }
        public void TestContructionWithReflecion(Type type)
        {
            long before;
            Object obj;

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                obj = Activator.CreateInstance(type);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());

            before = Stopwatch.GetTimestamp();
            for (int i = 0; i < 1000000; i++)
            {
                obj = FormatterServices.GetUninitializedObject(type);
            }
            Console.WriteLine((Stopwatch.GetTimestamp() - before).StopwatchTicksAsInt64Milliseconds());
        }
    }
}
