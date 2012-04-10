using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.RuntimeAnalyzer
{
    [TestClass]
    public class InstructionOnAndOffTest
    {
        Memory memory = new Memory(256U);
        Byte[] byteCode = new Byte[256];

        [TestMethod]
        public void OnAndOffTest()
        {
            UInt64 offset;
            memory.Reset();

            //
            // Generate Code
            //
            offset = 0;
            offset = ByteCode.EmitON(byteCode, offset, new MemoryOp(Operands.FrameOffsetInfoMask, 0,0,0,0,0,0,0,0));
            offset = ByteCode.EmitON(byteCode, offset, new MemoryOp(Operands.FrameOffsetInfoMask, 0, 0, 0, 1));
            offset = ByteCode.EmitON(byteCode, offset, new MemoryOp(Operands.FrameOffsetInfoMask, 0, 0, 0, 0, 0, 2));
            offset = ByteCode.EmitON(byteCode, offset, new MemoryOp(3));
            byteCode[offset++] = Instructions.Halt;

            //
            // Run Code
            //
            Assert.AreEqual(offset, InstructionProcessor.Execute(byteCode, 0, memory));

            //
            // Test Results
            //
            Assert.IsTrue(memory.TestAddressEqualsValue(0,UInt64.MaxValue));
            Assert.IsTrue(memory.TestAddressEqualsValue(1, UInt64.MaxValue));
            Assert.IsTrue(memory.TestAddressEqualsValue(2, UInt64.MaxValue));
            Assert.IsTrue(memory.TestAddressEqualsValue(3, UInt64.MaxValue));


            //
            // Generate Code
            //
            offset = 0;
            offset = ByteCode.EmitOff(byteCode, offset, new MemoryOp(0));
            offset = ByteCode.EmitOff(byteCode, offset, new MemoryOp(Operands.FrameOffsetInfoMask, 0, 0, 0, 0, 1));
            offset = ByteCode.EmitOff(byteCode, offset, new MemoryOp(Operands.FrameOffsetInfoMask, 0, 0, 0, 0, 0, 0, 0, 2));
            offset = ByteCode.EmitOff(byteCode, offset, new MemoryOp(Operands.FrameOffsetInfoMask, 0, 0, 3));
            byteCode[offset++] = Instructions.Halt;

            //
            // Run Code
            //
            Assert.AreEqual(offset, InstructionProcessor.Execute(byteCode, 0, memory));

            //
            // Test Results
            //
            Assert.IsTrue(memory.TestAddressEqualsValue(0, 0U));
            Assert.IsTrue(memory.TestAddressEqualsValue(1, 0U));
            Assert.IsTrue(memory.TestAddressEqualsValue(2, 0U));
            Assert.IsTrue(memory.TestAddressEqualsValue(3, 0U));
        }
    }
}
