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
            Assert.AreEqual(1, AByte.FixedSerializationLength);
            Assert.AreEqual(1, AByte.Serializer.fixedSerializationLength);

            for (Int32 i = 0; i < 0x100; i++)
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
            Assert.AreEqual(1, AnSByte.FixedSerializationLength);
            Assert.AreEqual(1, AnSByte.Serializer.fixedSerializationLength);
            for (Int32 i = 0; i < 0x100; i++)
            {
                AnSByte obj = new AnSByte((SByte)i);
                AnSByte.Serializer.Serialize(bytes, i, obj);
                Assert.AreEqual((SByte)i, (SByte)bytes[i]);
                AnSByte.Serializer.Deserialize(bytes, i, i, out obj);
                Assert.AreEqual((SByte)i, obj.value);
            }

            //
            // UInt16 Object
            //
            {
                Assert.AreEqual(1, AnSByte.FixedSerializationLength);
                Assert.AreEqual(1, AnSByte.Serializer.fixedSerializationLength);
                UInt16[] testValues = new UInt16[] {
                0, 1, 2, 0xFFFE, 0xFFFF, 0x0102, 0xFF01};
                for (Int32 i = 0; i < testValues.Length; i++)
                {
                    AUInt16 obj = new AUInt16(testValues[i]);
                    AUInt16.Serializer.Serialize(bytes, i, obj);
                    Assert.AreEqual(testValues[i], ByteArray.BigEndianReadUInt16(bytes, i));
                    AUInt16.Serializer.Deserialize(bytes, i, i, out obj);
                    Assert.AreEqual(testValues[i], obj.value);
                }
            }

            //
            // Int16 Object
            //
            {
                Assert.AreEqual(1, AnSByte.FixedSerializationLength);
                Assert.AreEqual(1, AnSByte.Serializer.fixedSerializationLength);
                Int16[] testValues = new Int16[] {
                0, 1, 2, 0x7FFE, 0x7FFF, -1, -2, -0x8000, 0x0102, 0x1234};
                for (Int32 i = 0; i < testValues.Length; i++)
                {
                    AnInt16 obj = new AnInt16(testValues[i]);
                    AnInt16.Serializer.Serialize(bytes, i, obj);
                    Assert.AreEqual(testValues[i], ByteArray.BigEndianReadInt16(bytes, i));
                    AnInt16.Serializer.Deserialize(bytes, i, i, out obj);
                    Assert.AreEqual(testValues[i], obj.value);
                }
            }


        }
    }
}
