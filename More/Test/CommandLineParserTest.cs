using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class CommandLineParserTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            CLParser parser = new CLParser();

            CLSwitch apple = new CLSwitch('a', "apple", "Should I use apples?"); 
            parser.Add(apple);

            CLSwitch banana = new CLSwitch('b', "banana", "Should I use bananas?");
            parser.Add(banana);

            CLGenericArgument<Int32> integer = new CLGenericArgument<Int32>(Int32.Parse, 'i', "int", "This options has a long description, I am going to keep talking about this argument until I reach a good long length for this description");
            parser.Add(integer);

            CLEnumArgument<DayOfWeek> day = new CLEnumArgument<DayOfWeek>('d', "day", "The day of the week");
            parser.Add(day);

            parser.PrintUsage();


            parser.Parse(new String[] {
                "-a", "--day", "Sunday", "-b", "-i", "32", "43", 
            });
            Assert.AreEqual(true, apple.set);
            Assert.AreEqual(true, banana.set);
            Assert.AreEqual(true, integer.set);
            Assert.AreEqual(true, day.set);
            Assert.AreEqual(DayOfWeek.Sunday, day.ArgValue);
        }

        [TestMethod]
        public void TestFailures()
        {
            CLParser parser = new CLParser();

            parser.Add(new CLSwitch('a', "apple", "Should I use apples?"));

            try
            {
                parser.Add(new CLSwitch('a', "Should I use apples?"));
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
