using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class TestNpcProtocolParsers
    {
        void TestNpcReturnLine(NpcReturnLine expectedReturnLine, String returnLineString)
        {
            Console.WriteLine("Testing '{0}'", returnLineString);
            NpcReturnLine actualReturnLine = new NpcReturnLine(returnLineString);

            Assert.AreEqual(expectedReturnLine.exceptionMessage, actualReturnLine.exceptionMessage);
            Assert.AreEqual(expectedReturnLine.sosTypeName, actualReturnLine.sosTypeName);
            Assert.AreEqual(expectedReturnLine.sosSerializationString, actualReturnLine.sosSerializationString);
        }

        [TestMethod]
        public void TestNpcReturnLines()
        {
            TestNpcReturnLine(new NpcReturnLine(typeof(void).SosTypeName(), null),
                NpcReturnObject.NpcReturnLineSuccessPrefix + typeof(void).SosTypeName());
        }
    }
}
