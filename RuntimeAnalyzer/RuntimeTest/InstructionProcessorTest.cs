using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.RuntimeAnalyzer
{
    [TestClass]
    public class InstructionProcessorTest
    {
        /*
        public static readonly Byte[] byteCode = new Byte[30];
        static readonly ProcessStack stack = new ProcessStack(1025);

        [TestMethod]
        public void TestIncrementAndDecrement()
        {
            UInt64 offset;


            //
            // Generate Code
            //
            offset = 0;
            offset = ByteCode.EmitIncrement(byteCode, ref offset, 0);
            offset = ByteCode.EmitIncrement(byteCode, ref offset, 1);
            offset = ByteCode.EmitIncrement(byteCode, ref offset, 2);
            offset = ByteCode.EmitIncrement(byteCode, ref offset, 3);
            byteCode[offset++] = ByteCode.EndFunction;

            //
            // Run Code
            //
            Assert.AreEqual(offset, InstructionProcessor.Execute(byteCode, 0, memory));

            //
            // Test Results
            //
            Assert.AreEqual(offset, processOffset);
            Assert.AreEqual(1U, stack[0]);
            Assert.AreEqual(1U, stack[1]);
            Assert.AreEqual(1U, stack[2]);
            Assert.AreEqual(1U, stack[3]);


            //
            // Generate Code
            //
            offset = 0;
            offset = ByteCode.EmitDecrement(byteCode, ref offset, 0);
            offset = ByteCode.EmitDecrement(byteCode, ref offset, 0);
            offset = ByteCode.EmitDecrement(byteCode, ref offset, 1);
            offset = ByteCode.EmitDecrement(byteCode, ref offset, 1);
            offset = ByteCode.EmitDecrement(byteCode, ref offset, 2);
            offset = ByteCode.EmitDecrement(byteCode, ref offset, 2);
            offset = ByteCode.EmitDecrement(byteCode, ref offset, 3);
            offset = ByteCode.EmitDecrement(byteCode, ref offset, 3);
            byteCode[offset++] = ByteCode.EndFunction;

            //
            // Run Code
            //
            Assert.AreEqual(offset, InstructionProcessor.Execute(byteCode, 0, memory));

            //
            // Test Results
            //
            Assert.AreEqual(offset, processOffset);
            Assert.AreEqual(0xFFU, stack[0]);
            Assert.AreEqual(0xFFFFU, stack[1]);
            Assert.AreEqual(0xFFFFFFFFU, stack[2]);
            Assert.AreEqual(0xFFFFFFFFFFFFFFFFU, stack[3]);


            //
            // Generate Code
            //
            offset = 0;
            offset = ByteCode.EmitIncrement(byteCode, ref offset, 0);
            offset = ByteCode.EmitIncrement(byteCode, ref offset, 1);
            offset = ByteCode.EmitIncrement(byteCode, ref offset, 2);
            offset = ByteCode.EmitIncrement(byteCode, ref offset, 3);
            byteCode[offset++] = ByteCode.EndFunction;

            //
            // Run Code
            //
            Assert.AreEqual(offset, InstructionProcessor.Execute(byteCode, 0, memory));

            //
            // Test Results
            //
            Assert.AreEqual(offset, processOffset);
            Assert.AreEqual(0U, stack[0]);
            Assert.AreEqual(0U, stack[1]);
            Assert.AreEqual(0U, stack[2]);
            Assert.AreEqual(0U, stack[3]);
        }
    */
    }
}
