using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.RuntimeAnalyzer
{
    [TestClass]
    public class InstructionIncAndDecTest
    {
        Memory memory = new Memory(256U);
        Byte[] byteCode = new Byte[256];

        [TestMethod]
        public void TestIncrement()
        {
            UInt64 offset;

            // Generate Code
            memory.Reset();
            memory.PushVar(0);

            offset = 0;
            offset = ByteCode.EmitIncrement(byteCode, offset, new MemoryOp(Operands.FrameOffsetInfoMask, 0, 0, 0, 0, 0, 0, 0, 0));
            offset = ByteCode.EmitIncrement(byteCode, offset, new MemoryOp(Operands.FrameOffsetInfoMask, 0, 0));
            offset = ByteCode.EmitIncrement(byteCode, offset, new MemoryOp(Operands.FrameOffsetInfoMask, 0, 0, 0, 0, 0, 0));
            offset = ByteCode.EmitIncrement(byteCode, offset, new MemoryOp(0));
            byteCode[offset++] = Instructions.Halt;

            // Run Code
            Assert.AreEqual(offset, InstructionProcessor.Execute(byteCode, 0, memory));

            // Test Results
            Assert.IsTrue(memory.TestAddressEqualsValue(0, 4U));
        }

        [TestMethod]
        public void TestDecrement()
        {
            UInt64 offset;

            // Generate Code
            memory.Reset();
            memory.PushVar(0);

            offset = 0;
            offset = ByteCode.EmitDecrement(byteCode, offset, new MemoryOp(Operands.FrameOffsetInfoMask, 0, 0, 0, 0, 0, 0, 0, 0));
            offset = ByteCode.EmitDecrement(byteCode, offset, new MemoryOp(Operands.FrameOffsetInfoMask, 0, 0));
            offset = ByteCode.EmitDecrement(byteCode, offset, new MemoryOp(0));
            offset = ByteCode.EmitDecrement(byteCode, offset, new MemoryOp(Operands.FrameOffsetInfoMask, 0, 0, 0, 0, 0, 0));
            byteCode[offset++] = Instructions.Halt;

            // Run Code
            Assert.AreEqual(offset, InstructionProcessor.Execute(byteCode, 0, memory));

            // Test Results
            Assert.IsTrue(memory.TestAddressEqualsValue(0, 0xFFFFFFFFFFFFFFFC));
        }
    }
}
