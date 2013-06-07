using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class AsciiLineParserTest
    {
        [TestMethod]
        public void TestMethod()
        {
            AsciiLineParser lineParser = new AsciiLineParser();


            lineParser.Add(Encoding.ASCII.GetBytes("abcd\n"));
            Assert.AreEqual("abcd", lineParser.GetLine());
            Assert.IsNull(lineParser.GetLine());

            lineParser.Add(Encoding.ASCII.GetBytes("abcd\r\n"));
            Assert.AreEqual("abcd", lineParser.GetLine());
            Assert.IsNull(lineParser.GetLine());

            lineParser.Add(Encoding.ASCII.GetBytes("abcd\nefgh\r\n"));
            Assert.AreEqual("abcd", lineParser.GetLine());
            Assert.AreEqual("efgh", lineParser.GetLine());
            Assert.IsNull(lineParser.GetLine());

            lineParser.Add(Encoding.ASCII.GetBytes("abcd\r\nefghijkl"));
            Assert.AreEqual("abcd", lineParser.GetLine());
            Assert.IsNull(lineParser.GetLine());
            lineParser.Add(Encoding.ASCII.GetBytes("\n"));
            Assert.AreEqual("efghijkl", lineParser.GetLine());
            Assert.IsNull(lineParser.GetLine());


            lineParser.Add(Encoding.ASCII.GetBytes("abcd\n"));
            lineParser.Add(Encoding.ASCII.GetBytes("abcd\r\n"));
            lineParser.Add(Encoding.ASCII.GetBytes("abcd\n"));
            Assert.AreEqual("abcd", lineParser.GetLine());
            Assert.AreEqual("abcd", lineParser.GetLine());
            Assert.AreEqual("abcd", lineParser.GetLine());
            Assert.IsNull(lineParser.GetLine());

            lineParser.Add(Encoding.ASCII.GetBytes("a"));
            Assert.IsNull(lineParser.GetLine());
            lineParser.Add(Encoding.ASCII.GetBytes("bc"));
            Assert.IsNull(lineParser.GetLine());
            lineParser.Add(Encoding.ASCII.GetBytes("d"));
            Assert.IsNull(lineParser.GetLine());
            lineParser.Add(Encoding.ASCII.GetBytes("\r\ntu"));
            lineParser.Add(Encoding.ASCII.GetBytes("v"));
            Assert.AreEqual("abcd", lineParser.GetLine());
            Assert.IsNull(lineParser.GetLine());
            lineParser.Add(Encoding.ASCII.GetBytes("\r"));
            Assert.IsNull(lineParser.GetLine());
            lineParser.Add(Encoding.ASCII.GetBytes("\n"));
            Assert.AreEqual("tuv", lineParser.GetLine());
            Assert.IsNull(lineParser.GetLine());
        }
    }
}
