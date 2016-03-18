using System;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class SinkTests
    {
        [TestMethod]
        public void TestStdOutSink()
        {
            Byte[] testMessage = new Byte[] {(Byte)'T',(Byte)'e',(Byte)'s',(Byte)'t'};
            BytePtr testMessagePtr = GCHandle.Alloc(testMessage);

            IO.EnsureConsoleOpen();
            IO.StdOut.Write(testMessage);
            IO.StdOut.Write(testMessagePtr, (uint)testMessage.Length);
        }
    }
}
