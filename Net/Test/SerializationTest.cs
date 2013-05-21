using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.Net
{
    [TestClass]
    public class SerializationTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            PcapGlobalHeader header = new PcapGlobalHeader(0, 0, 100, PcapDataLinkType.Ethernet);

            Byte[] array = new Byte[header.SerializationLength()];
            header.Serialize(array, 0);

            Console.WriteLine(BitConverter.ToString(array));
        }
    }
}
