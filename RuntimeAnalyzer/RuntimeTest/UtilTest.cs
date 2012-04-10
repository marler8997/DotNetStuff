using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.RuntimeAnalyzer
{
    [TestClass]
    public class UtilTest
    {
        [TestMethod]
        public void GetUInt64Test()
        {
            byte[] bytes = new byte[] {0xFF, 0xFA, 0x2B, 0xC5, 0xEE, 0x81, 0x8F, 0x01, 0x52};
            UInt64 value = 0xFA2BC5EE818F0152;

            Int32 shift = 56;
            for (Byte i = 0; i < 8; i++)
            {
                UInt64 offset = 1;
                UInt64 u = Util.GetUInt64(bytes, ref offset, i);
                Assert.AreEqual((UInt64)(2 + i), offset);
                Assert.AreEqual(value >> shift, u);
                shift -= 8;
            }
        }

        [TestMethod]
        public void InsertUInt64Test()
        {
            byte[] bytes = new byte[10];
            UInt64 value = 0xFA2BC5EE818F0152;

            for (Byte i = 0; i <= 7; i++)
            {
                UInt64 offset = Util.InsertUInt64(bytes, 1, i, value);
                Assert.AreEqual((UInt64)(i + 2), offset);

                Int32 shift = i*8;
                for (Byte j = 1; j < i + 2; j++)
                {
                    Console.WriteLine("i = {0}, j = {1}, byte[j] = {2}, value = {3:x}, shift = {4}", i, j, bytes[j], value, shift);
                    Assert.AreEqual((byte)(value >> shift), bytes[j]);
                    shift -= 8;
                }
            }
        }
    }
}
