using System;
using System.Text;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using More.Net.PdlTestObjects;

namespace More.Net
{
    /// <summary>
    /// Summary description for TestPdl
    /// </summary>
    [TestClass]
    public class TestPdl
    {
        [TestMethod]
        public void TestIntegerObjects()
        {
            Byte[] bytes = new Byte[1024];

            //
            // Byte Object
            //
            Assert.AreEqual(1U, AByte.FixedSerializationLength);
            Assert.AreEqual(1U, AByte.Serializer.fixedSerializationLength);

            for (UInt32 i = 0; i < 0x100; i++)
            {
                AByte obj = new AByte((Byte)i);
                AByte.Serializer.Serialize(bytes, i, obj);
                Assert.AreEqual((Byte)i, bytes[i]);
                AByte.Serializer.Deserialize(bytes, i, i, out obj);
                Assert.AreEqual((Byte)i, obj.value);
            }

            //
            // SByte Object
            //
            Assert.AreEqual(1U, AnSByte.FixedSerializationLength);
            Assert.AreEqual(1U, AnSByte.Serializer.fixedSerializationLength);
            for (UInt32 i = 0; i < 0x100; i++)
            {
                AnSByte obj = new AnSByte((SByte)i);
                AnSByte.Serializer.Serialize(bytes, i, obj);
                Assert.AreEqual((SByte)i, (SByte)bytes[i]);
                AnSByte.Serializer.Deserialize(bytes, i, i, out obj);
                Assert.AreEqual((SByte)i, obj.value);
            }


        }
    }
}
