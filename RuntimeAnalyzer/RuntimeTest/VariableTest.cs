using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.RuntimeAnalyzer
{
    /*
    [TestClass]
    public class VariableTest
    {
        private static Variable var, var2;

        [TestMethod]
        public void ConstructorTest()
        {
            try { new Variable(0x80); Assert.Fail(); }
            catch (InvalidOperationException) { }
            try { new Variable(0x89); Assert.Fail(); }
            catch (InvalidOperationException) { }
            try { new Variable(0x00); Assert.Fail(); }
            catch (InvalidOperationException) { }
            try { new Variable(0x09); Assert.Fail(); }
            catch (InvalidOperationException) { }

            var = new Variable(0x81);
            Assert.AreEqual(1, var.byteSize);
            Assert.AreEqual(0xFFU, var.mask);

            var = new Variable(0x82);
            Assert.AreEqual(2, var.byteSize);
            Assert.AreEqual(0xFFFFU, var.mask);

            var = new Variable(0x83);
            Assert.AreEqual(3, var.byteSize);
            Assert.AreEqual(0xFFFFFFU, var.mask);

            var = new Variable(0x84);
            Assert.AreEqual(4, var.byteSize);
            Assert.AreEqual(0xFFFFFFFFU, var.mask);

            var = new Variable(0x85);
            Assert.AreEqual(5, var.byteSize);
            Assert.AreEqual(0xFFFFFFFFFFU, var.mask);

            var = new Variable(0x86);
            Assert.AreEqual(6, var.byteSize);
            Assert.AreEqual(0xFFFFFFFFFFFFU, var.mask);

            var = new Variable(0x87);
            Assert.AreEqual(7, var.byteSize);
            Assert.AreEqual(0xFFFFFFFFFFFFFFU, var.mask);

            var = new Variable(0x88);
            Assert.AreEqual(8, var.byteSize);
            Assert.AreEqual(0xFFFFFFFFFFFFFFFFU, var.mask);

            var = new Variable(0x01);
            Assert.AreEqual(1, var.byteSize);
            Assert.AreEqual(0xFFU, var.mask);

            var = new Variable(0x02);
            Assert.AreEqual(2, var.byteSize);
            Assert.AreEqual(0xFFFFU, var.mask);

            var = new Variable(0x03);
            Assert.AreEqual(3, var.byteSize);
            Assert.AreEqual(0xFFFFFFU, var.mask);

            var = new Variable(0x04);
            Assert.AreEqual(4, var.byteSize);
            Assert.AreEqual(0xFFFFFFFFU, var.mask);

            var = new Variable(0x05);
            Assert.AreEqual(5, var.byteSize);
            Assert.AreEqual(0xFFFFFFFFFFU, var.mask);

            var = new Variable(0x06);
            Assert.AreEqual(6, var.byteSize);
            Assert.AreEqual(0xFFFFFFFFFFFFU, var.mask);

            var = new Variable(0x07);
            Assert.AreEqual(7, var.byteSize);
            Assert.AreEqual(0xFFFFFFFFFFFFFFU, var.mask);

            var = new Variable(0x08);
            Assert.AreEqual(8, var.byteSize);
            Assert.AreEqual(0xFFFFFFFFFFFFFFFFU, var.mask);
        }

        [TestMethod]
        public void SetMemWithMaskTest()
        {
            //
            // Signed
            //
            for (Byte i = 0x81; i <= 0x88; i++)
            {
                var = new Variable(i);

                var.SetMemWithMask(0);
                Assert.AreEqual(0U, var.Mem);

                var.SetMemWithMask(1);
                Assert.AreEqual(1U, var.Mem);

                var.SetMemWithMask(var.mask);
                Assert.AreEqual(var.mask, var.Mem);

                var.SetMemWithMask(~var.mask);
                Assert.AreEqual(0U, var.Mem);
            }

            //
            // Unsigned
            //
            for (Byte i = 0x01; i <= 0x08; i++)
            {
                var = new Variable(i);

                var.SetMemWithMask(0);
                Assert.AreEqual(0U, var.Mem);

                var.SetMemWithMask(1);
                Assert.AreEqual(1U, var.Mem);

                var.SetMemWithMask(var.mask);
                Assert.AreEqual(var.mask, var.Mem);

                var.SetMemWithMask(~var.mask);
                Assert.AreEqual(0U, var.Mem);
            }
        }

        [TestMethod]
        public void ValueAsAddressTest()
        {
            //
            // Signed
            //
            for (Byte i = 0x81; i <= 0x83; i++)
            {
                var = new Variable(i);
                var.SetMemWithMask(0);
                Assert.AreEqual(0, var.ValueAsAddress);

                var.SetMemWithMask(var.mask);
                Assert.AreEqual((Int32)var.mask, var.ValueAsAddress);
            }

            for (Byte i = 0x84; i <= 0x88; i++)
            {
                var = new Variable(i);
                var.SetMemWithMask(0x7FFFFFFF);
                Assert.AreEqual(0x7FFFFFFF, var.ValueAsAddress);

                var.SetMemWithMask(0x80000000);
                try { Int32 a = var.ValueAsAddress; Assert.Fail(); }
                catch (InvalidOperationException) { }
            }

            //
            // Unsigned
            //
            for (Byte i = 0x01; i <= 0x03; i++)
            {
                var = new Variable(i);
                var.SetMemWithMask(0);
                Assert.AreEqual(0, var.ValueAsAddress);

                var.SetMemWithMask(var.mask);
                Assert.AreEqual((Int32)var.mask, var.ValueAsAddress);
            }

            for (Byte i = 0x04; i <= 0x08; i++)
            {
                var = new Variable(i);
                var.SetMemWithMask(0x7FFFFFFF);
                Assert.AreEqual(0x7FFFFFFF, var.ValueAsAddress);

                var.SetMemWithMask(0x80000000);
                try { Int32 a = var.ValueAsAddress; Assert.Fail(); }
                catch (InvalidOperationException) { }
            }
        }

        [TestMethod]
        public void ValueAsAddressOffsetTest()
        {
            //
            // Signed
            //
            var = new Variable(0x81);
            var.SetMemWithMask(0xFF);
            Assert.AreEqual(-1, var.ValueAsAddressOffset);

            var = new Variable(0x82);
            var.SetMemWithMask(0xFFFF);
            Assert.AreEqual(-1, var.ValueAsAddressOffset);

            var = new Variable(0x83);
            var.SetMemWithMask(0xFFFFFF);
            Assert.AreEqual(-1, var.ValueAsAddressOffset);

            var = new Variable(0x84);
            var.SetMemWithMask(0xFFFFFFFF);
            Assert.AreEqual(-1, var.ValueAsAddressOffset);
            var.SetMemWithMask(unchecked((UInt64)Int32.MinValue));
            Assert.AreEqual(Int32.MinValue, var.ValueAsAddressOffset);

            for (Byte i = 0x85; i <= 0x88; i++)
            {
                Console.WriteLine(i);
                var = new Variable(i);
                var.SetMemWithMask(0x7FFFFFFF);
                Assert.AreEqual(0x7FFFFFFF, var.ValueAsAddressOffset);
                var.SetMemWithMask(var.mask);
                Assert.AreEqual(-1, var.ValueAsAddressOffset);
                var.SetMemWithMask(var.mask & 0xFFFFFFFF80000000U);
                Assert.AreEqual(Int32.MinValue, var.ValueAsAddressOffset);
            }

            //
            // Unsigned
            //
            var = new Variable(0x1);
            var.SetMemWithMask(0xFF);
            Assert.AreEqual(0xFF, var.ValueAsAddressOffset);

            var = new Variable(0x2);
            var.SetMemWithMask(0xFFFF);
            Assert.AreEqual(0xFFFF, var.ValueAsAddressOffset);

            var = new Variable(0x3);
            var.SetMemWithMask(0xFFFFFF);
            Assert.AreEqual(0xFFFFFF, var.ValueAsAddressOffset);

            for (Byte i = 0x4; i <= 0x8; i++)
            {
                Console.WriteLine(i);
                var = new Variable(i);
                var.SetMemWithMask(Int32.MaxValue);
                Assert.AreEqual(Int32.MaxValue, var.ValueAsAddressOffset);
                
                var.SetMemWithMask(0x80000000);
                try { Int32 temp = var.ValueAsAddressOffset; Assert.Fail();  }
                catch(InvalidOperationException) { }
            }
        }

        [TestMethod]
        public void IncrementDecrementTest()
        {
            for (Byte i = 1; i <= 8; i++)
            {
                var = new Variable(i);
                var.SetMemWithMask(var.mask);
                var.Increment();
                Assert.AreEqual(0U, var.Mem);
                var.Increment();
                Assert.AreEqual(1U, var.Mem);
                var.Decrement();
                Assert.AreEqual(0U, var.Mem);
                var.Decrement();
                Assert.AreEqual(var.mask, var.Mem);
            }
        }

        [TestMethod]
        public void AddVariableTest()
        {
            //
            // Unsigned = Unsigned + Unsigned
            //
            for (Byte i = 0x01; i <= 0x08; i++)
            {
                var = new Variable(i);
                for(Byte j = 0x01; j <= 0x08; j++)
                {
                    var2 = new Variable(j);
                    
                    var.SetMemWithMask(0);
                    var2.SetMemWithMask(0);
                    var.Add(var2);
                    Assert.AreEqual(0U, var.Mem);

                    var2.SetMemWithMask(0xFF);
                    var.Add(var2);
                    Assert.AreEqual(0xFFU, var.Mem);

                    var.SetMemWithMask(0);
                    var2.SetMemWithMask(var2.mask);
                    var.Add(var2);
                    Assert.AreEqual(var.mask & var2.mask, var.Mem);

                    var.SetMemWithMask(var.mask);
                    var2.SetMemWithMask(1);
                    var.Add(var2);
                    Assert.AreEqual(0U, var.Mem);
                }
            }

            //
            // Unsigned = Unsigned + Signed
            //
            for (Byte i = 0x01; i <= 0x08; i++)
            {
                var = new Variable(i);
                for (Byte j = 0x81; j <= 0x88; j++)
                {
                    var2 = new Variable(j);

                    var.SetMemWithMask(0);
                    var2.SetMemWithMask(0);
                    var.Add(var2);
                    Assert.AreEqual(0U, var.Mem);

                    var.SetMemWithMask(0);
                    var2.SetMemWithMask(var2.mask);
                    var.Add(var2);
                    Assert.AreEqual(var.mask, var.Mem);

                    var.Add(var2);
                    Assert.AreEqual(var.mask - 1, var.Mem);

                    var2.SetMemWithMask(var2.mask - 2);
                    var.Add(var2);
                    Assert.AreEqual(var.mask - 4, var.Mem);

                    var2.SetMemWithMask(5);
                    var.Add(var2);
                    Assert.AreEqual(0U, var.Mem);
                }
            }

            //
            // Signed = Signed + Unsigned
            //
            for (Byte i = 0x81; i <= 0x88; i++)
            {
                var = new Variable(i);
                

                for (Byte j = 1; j <(Byte)(i - 0x80); j++)
                {
                    var2 = new Variable(j);

                    var.SetMemWithMask(var.mask - 23);
                    var2.SetMemWithMask(var2.mask - var2.msb);
                    var.Add(var2);
                    Assert.AreEqual(var2.mask - var2.msb - 24, var.Mem);
                }

                for (Byte j = 0x01; j <= 0x08; j++)
                {
                    var2 = new Variable(j);

                    var.SetMemWithMask(0);
                    var2.SetMemWithMask(0);
                    var.Add(var2);
                    Assert.AreEqual(0U, var.Mem);

                    var.SetMemWithMask(var.mask);
                    var2.SetMemWithMask(0);
                    var.Add(var2);
                    Assert.AreEqual(var.mask, var.Mem);

                    var2.SetMemWithMask(1);
                    var.Add(var2);
                    Assert.AreEqual(0U, var.Mem);

                    var.SetMemWithMask(var.mask - 5);
                    var2.SetMemWithMask(67);
                    var.Add(var2);
                    Assert.AreEqual(61U, var.Mem);
                }
            }

            //
            // Signed = Signed + Signed
            //
            for (Byte i = 0x81; i <= 0x88; i++)
            {
                var = new Variable(i);


                for (Byte j = 0x81; j < 0x88; j++)
                {
                    var2 = new Variable(j);

                    var.SetMemWithMask(0);
                    var2.SetMemWithMask(0);
                    var.Add(var2);
                    Assert.AreEqual(0U, var.Mem);

                    var.SetMemWithMask(var.mask);
                    var2.SetMemWithMask(0);
                    var.Add(var2);
                    Assert.AreEqual(var.mask, var.Mem);

                    var.SetMemWithMask(var.mask);
                    var2.SetMemWithMask(var2.mask);
                    var.Add(var2);
                    Assert.AreEqual(var.mask - 1, var.Mem);

                    var.SetMemWithMask(var.mask - 5);
                    var2.SetMemWithMask(67);
                    var.Add(var2);
                    Assert.AreEqual(61U, var.Mem);
                }
            }
        }
    }
    */
}
