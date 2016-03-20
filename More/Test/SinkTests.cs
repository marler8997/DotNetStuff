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
        //[Serializable(true, Al]
        public unsafe void TestStdOutSink()
        {
            Console.WriteLine(typeof(Object).FullName);
            /*
            Byte[] testMessage = new Byte[] {(Byte)'T',(Byte)'e',(Byte)'s',(Byte)'t'};
            fixed (byte* testMessagePtr = testMessage)
            {
                IO.EnsureConsoleOpen();
                IO.StdOut.Write(testMessage);
                IO.StdOut.WriteLine();
                IO.StdOut.Write(testMessagePtr, (uint)testMessage.Length);
            }
             */
        }
    }
}
