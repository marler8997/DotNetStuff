using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.RuntimeAnalyzer
{
    [TestClass]
    public class MemoryAccessOperandsTest
    {
        UInt64 r1 =0x1234567812345678;
        UInt64 r2 =0x00000000FFAABBCC;
        UInt64 r3 =0x98765432AADDEEBB;
        UInt64 r4 =0x0101020203040569;

        Memory memory = new Memory(1024);
        Byte[] byteCode = new Byte[30];

        public class OpAndValue
        {
            public readonly Op op;
            public readonly UInt64 value;
            public OpAndValue(Op op, UInt64 value)
            {
                this.op = op;
                this.value = value;
            }
        }

        public void TestOpAndValue(OpAndValue[] opAndValues)
        {
            for (int i = 0; i < opAndValues.Length; i++)
            {
                UInt64 offset = opAndValues[i].op.Emit(byteCode, 0);

                UInt64 processOffset = 0;
                Memory.ReadOperandLogic readLogic = memory.ParseReadOperand(byteCode, ref processOffset);

                Assert.AreEqual(offset, processOffset);
                Assert.AreEqual(opAndValues[i].value, readLogic.readLogic(readLogic.op));
            }
        }
        public void TestMemoryOpAndValue(OpAndValue[] memoryOpAndValues)
        {
            for (int i = 0; i < memoryOpAndValues.Length; i++)
            {
                UInt64 offset = memoryOpAndValues[i].op.Emit(byteCode, 0);

                UInt64 processOffset = 0;
                Memory.ReadOperandLogic readLogic = memory.ParseWriteOperandForReading(byteCode, ref processOffset);

                Assert.AreEqual(offset, processOffset);
                Assert.AreEqual(memoryOpAndValues[i].value, readLogic.readLogic(readLogic.op));
            }
        }

        [TestMethod]
        public void LiteralOpTest()
        {
            OpAndValue [] literalOps = new OpAndValue[] {
                new OpAndValue(new Op(Operands.LiteralInfoMask, 0),0U),
                new OpAndValue(new Op(Operands.LiteralInfoMask, 0xFF),0xFFU),
                new OpAndValue(new Op(Operands.LiteralInfoMask, 0xFF, 0xAB),0xFFABU),
                new OpAndValue(new Op(Operands.LiteralInfoMask, 0xFF, 0xAB, 0xFF, 0x12, 0x34, 0x56, 0x78, 0x9A),0xFFABFF123456789AU),
            };
            TestOpAndValue(literalOps);
        }

        [TestMethod]
        public void FrameOffsetOpTest()
        {
            memory.Reset();
            memory.DebugSetFramePointer(30);
            memory.DebugSetMemory(30, r2);
            memory.DebugSetMemory(40, r4);
            memory.DebugSetMemory(50, r1);
            memory.DebugSetMemory(60, r3);
            OpAndValue[] frameOffsetOps = new OpAndValue[] {
                new OpAndValue(new Op(0), r2),
                new OpAndValue(new Op(Operands.FrameOffsetInfoMask, 0, 0, 0, 10), r4),
                new OpAndValue(new Op(Operands.FrameOffsetInfoMask, 0, 20),r1),
                new OpAndValue(new Op(Operands.FrameOffsetInfoMask, 0, 0, 0, 0, 0, 30),r3),
            };
            TestOpAndValue(frameOffsetOps);
            
            memory.DebugSetMemory(30 + Operands.WritableOpTypeFrameOffsetShortMax, r3);
            frameOffsetOps[0] = new OpAndValue(new MemoryOp(Operands.WritableOpTypeFrameOffsetShortMax), r3);

            TestMemoryOpAndValue(frameOffsetOps);
        }

        [TestMethod]
        public void FrameOffsetDereferenceOpTest()
        {
            //
            // This test is awesome because it tests ALOT of things at once!
            //
            memory.Reset();
            memory.DebugSetFramePointer(10);
            memory.DebugSetMemory(11, 4);
            memory.DebugSetMemory(13, 5);
            memory.DebugSetMemory(21, 6);
            memory.DebugSetMemory(33, 7);
            memory.DebugSetMemory(4, r3);
            memory.DebugSetMemory(5, r1);
            memory.DebugSetMemory(6, r2);
            memory.DebugSetMemory(7, r4);
            OpAndValue[] frameOffsetOps = new OpAndValue[] {
                new OpAndValue(new Op(Operands.FrameOffsetDereferenceInfoMask, 1), r3),
                new OpAndValue(new Op(Operands.FrameOffsetDereferenceInfoMask, 0, 0, 0, 3), r1),
                new OpAndValue(new Op(Operands.FrameOffsetDereferenceInfoMask, 0, 11),r2),
                new OpAndValue(new Op(Operands.FrameOffsetDereferenceInfoMask, 0, 0, 0, 0, 0, 23),r4),
            };
            TestOpAndValue(frameOffsetOps);
            TestMemoryOpAndValue(frameOffsetOps);
        }

        [TestMethod]
        public void AddressOpTest()
        {
            memory.Reset();
            memory.DebugSetFramePointer(999999);
            memory.DebugSetMemory(35, r1);
            memory.DebugSetMemory(45, r3);
            memory.DebugSetMemory(55, r2);
            memory.DebugSetMemory(65, r4);
            OpAndValue[] frameOffsetOps = new OpAndValue[] {
                new OpAndValue(new Op(Operands.AddressInfoMask, 35), r1),
                new OpAndValue(new Op(Operands.AddressInfoMask, 0, 0, 0, 45), r3),
                new OpAndValue(new Op(Operands.AddressInfoMask, 0, 55),r2),
                new OpAndValue(new Op(Operands.AddressInfoMask, 0, 0, 0, 0, 0, 65),r4),
            };
            TestOpAndValue(frameOffsetOps);
            TestMemoryOpAndValue(frameOffsetOps);
        }

        [TestMethod]
        public void AddressDereferenceOpTest()
        {
            memory.Reset();
            memory.DebugSetFramePointer(999999);
            memory.DebugSetMemory(0x120, 33);
            memory.DebugSetMemory(0x26, 82);
            memory.DebugSetMemory(0x3FF, 46);
            memory.DebugSetMemory(0x369, 999);
            memory.DebugSetMemory(33, r1);
            memory.DebugSetMemory(82, r3);
            memory.DebugSetMemory(46, r2);
            memory.DebugSetMemory(999, r4);
            OpAndValue[] frameOffsetOps = new OpAndValue[] {
                new OpAndValue(new Op(Operands.AddressDereferenceInfoMask, 0x01, 0x20), r1),
                new OpAndValue(new Op(Operands.AddressDereferenceInfoMask, 0, 0, 0, 0x26), r3),
                new OpAndValue(new Op(Operands.AddressDereferenceInfoMask, 0, 0, 0x03, 0xFF),r2),
                new OpAndValue(new Op(Operands.AddressDereferenceInfoMask, 0, 0, 0, 0, 0x03, 0x69),r4),
            };
            TestOpAndValue(frameOffsetOps);
            TestMemoryOpAndValue(frameOffsetOps);
        }
    }
}
