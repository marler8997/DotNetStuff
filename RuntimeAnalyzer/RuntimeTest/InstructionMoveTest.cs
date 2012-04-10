using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.RuntimeAnalyzer
{
    [TestClass]
    public class InstructionMoveTest
    {
        Memory memory = new Memory(0x401);
        Byte[] byteCode = new Byte[256];

        private class MemAndValue
        {
            public readonly MemoryOp dst;
            public readonly Op src;

            public MemAndValue(MemoryOp ew, Op er)
            {
                this.dst = ew;
                this.src = er;
            }
        }

        private void MoveTest(params MemAndValue[] t)
        {
            UInt64 offset;
            memory.Reset();

            //
            // Generate Code
            //
            offset = 0;
            for (int i = 0; i < t.Length; i++)
            {
                offset = ByteCode.EmitAssign(byteCode, offset, t[i].dst, t[i].src);
            }
            byteCode[offset++] = Instructions.Halt;

            //
            // Run Code
            //
            Assert.AreEqual(offset, InstructionProcessor.Execute(byteCode, 0, memory));

            //
            // Test Results
            //
            memory.Print(0, 16);
            for (int i = 0; i < t.Length; i++)
            {
                MemoryOp dst = t[i].dst;
                Op src = t[i].src;
                Console.WriteLine("Asserting [{0}] EmitMem={1}, EmitVal={2}", i, dst, src);

                Byte[] emitWriteCode = new Byte[dst.byteLength];
                Byte[] emitReadCode = new Byte[src.byteLength];

                dst.Emit(emitWriteCode, 0);
                src.Emit(emitReadCode, 0);

                offset = 0;
                Memory.ReadOperandLogic emitWriteReadOperand = memory.ParseWriteOperandForReading(emitWriteCode, ref offset);
                Assert.AreEqual((UInt64)emitWriteCode.Length, offset);

                offset = 0;
                Memory.ReadOperandLogic emitReadReadOperand = memory.ParseReadOperand(emitReadCode, ref offset);
                Assert.AreEqual((UInt64)emitReadCode.Length, offset);

                Assert.IsTrue(memory.TestEquals(emitWriteReadOperand, emitReadReadOperand));
            }
        }

        [TestMethod]
        public void MoveTest1()
        {
            MoveTest(new MemAndValue[] {
                new MemAndValue(new MemoryOp(0), new Op(Operands.LiteralInfoMask,0x12)),
                new MemAndValue(new MemoryOp(1), new Op(Operands.LiteralInfoMask,0x12, 0x34, 0x56)),
                new MemAndValue(new MemoryOp(2), new Op(Operands.LiteralInfoMask,0x78, 0x82, 0xAF, 0xC2, 0x10)),
                new MemAndValue(new MemoryOp(Operands.WritableOpTypeFrameOffsetShortMax), new Op(Operands.LiteralInfoMask,0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0)),
            });

        }

        [TestMethod]
        public void MoveTest2()
        {
            MoveTest(new MemAndValue[] {
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask, 0, 0), new Op(Operands.LiteralInfoMask,0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask, 0, 0, 0, 0, 0, 0, 0, 1), new Op(Operands.LiteralInfoMask,0xBC, 0xDE, 0xF0)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 4, 0), new Op(Operands.LiteralInfoMask,0xBC, 0xDE, 0xF0)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,Operands.WritableOpTypeFrameOffsetShortMax+1), new Op(Operands.LiteralInfoMask,0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0)),
            });
        }

        [TestMethod]
        public void MoveTest3()
        {
            MoveTest(new MemAndValue[] {
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask, 0, 0), new Op(Operands.LiteralInfoMask,0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask, 0, 0, 0, 0, 0, 0, 0, 1), new Op(Operands.LiteralInfoMask,0xBC, 0xDE, 0xF0)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask, 0, 0, 0, 2), new Op(Operands.LiteralInfoMask,0xBC, 0xDE, 0xF0)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask, 0, 0, 0, 0, 0, 0, 3), new Op(Operands.LiteralInfoMask,0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0)),

                new MemAndValue(new MemoryOp(4), new Op(0)),
                new MemAndValue(new MemoryOp(5), new Op(Operands.FrameOffsetInfoMask,0, 0, 0, 1)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 6), new Op(Operands.FrameOffsetInfoMask,0, 0, 0, 0, 0, 2)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 7), new Op(3)),
            });
        }

        [TestMethod]
        public void MoveTest4()
        {
            MoveTest(new MemAndValue[] {
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0), new Op(Operands.LiteralInfoMask,4)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 0, 0, 0, 0, 0, 1), new Op(Operands.LiteralInfoMask,0, 0, 0, 5)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 0, 2), new Op(Operands.LiteralInfoMask,0, 6)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 0, 0, 0, 0, 3), new Op(Operands.LiteralInfoMask,0, 0, 0, 0, 7)),

                new MemAndValue(new MemoryOp(Operands.FrameOffsetDereferenceInfoMask,0, 0), new Op(Operands.LiteralInfoMask,0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetDereferenceInfoMask,0, 0, 0, 0, 0, 0, 0, 1), new Op(Operands.LiteralInfoMask,0xBC, 0xDE, 0xF0)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetDereferenceInfoMask,0, 0, 0, 2), new Op(Operands.LiteralInfoMask,0xBC, 0xDE, 0xF0)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetDereferenceInfoMask,0, 0, 0, 0, 0, 0, 3), new Op(Operands.LiteralInfoMask,0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0)),
            });
        }

        [TestMethod]
        public void MoveTest5()
        {
            MoveTest(new MemAndValue[] {
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0), new Op(Operands.LiteralInfoMask,8)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 0, 0, 0, 0, 0, 1), new Op(Operands.LiteralInfoMask,0, 0, 0, 9)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 0, 2), new Op(Operands.LiteralInfoMask,0, 10)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 0, 0, 0, 0, 3), new Op(Operands.LiteralInfoMask,0, 0, 0, 0, 11)),

                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 4), new Op(Operands.LiteralInfoMask,0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 0, 0, 0, 0, 0, 5), new Op(Operands.LiteralInfoMask,0xBC, 0xDE, 0xF0)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 0, 6), new Op(Operands.LiteralInfoMask,0xBC, 0xDE, 0xF0)),
                new MemAndValue(new MemoryOp(7), new Op(Operands.LiteralInfoMask,0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0)),

                new MemAndValue(new MemoryOp(Operands.FrameOffsetDereferenceInfoMask,0, 0), new Op(4)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetDereferenceInfoMask,0, 0, 0, 0, 0, 0, 0, 1), new Op(Operands.FrameOffsetInfoMask,0,0,5)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetDereferenceInfoMask,0, 0, 0, 2), new Op(Operands.FrameOffsetInfoMask,0,0,0,0,6)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetDereferenceInfoMask,0, 0, 0, 0, 0, 0, 3), new Op(Operands.FrameOffsetInfoMask,0,7)),
            });
        }

        [TestMethod]
        public void MoveTest6()
        {
            MoveTest(new MemAndValue[] {
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0), new Op(Operands.LiteralInfoMask,12)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 0, 0, 0, 0, 0, 1), new Op(Operands.LiteralInfoMask,0, 0, 0, 13)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 0, 2), new Op(Operands.LiteralInfoMask,0, 14)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 0, 0, 0, 0, 3), new Op(Operands.LiteralInfoMask,0, 0, 0, 0, 15)),

                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 4), new Op(Operands.LiteralInfoMask,16)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 0, 0, 0, 0, 0, 5), new Op(Operands.LiteralInfoMask,0, 0, 0, 17)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 0, 6), new Op(Operands.LiteralInfoMask,0, 18)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 0, 0, 0, 0, 7), new Op(Operands.LiteralInfoMask,0, 0, 0, 0, 19)),

                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 8), new Op(Operands.LiteralInfoMask,0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 0, 0, 0, 0, 0, 9), new Op(Operands.LiteralInfoMask,0xBC, 0xDE, 0xF0)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetInfoMask,0, 0, 0, 10), new Op(Operands.LiteralInfoMask,0xBC, 0xDE, 0xF0)),
                new MemAndValue(new MemoryOp(11), new Op(Operands.LiteralInfoMask,0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0)),

                new MemAndValue(new MemoryOp(Operands.FrameOffsetDereferenceInfoMask,0, 0), new Op(Operands.FrameOffsetDereferenceInfoMask,4)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetDereferenceInfoMask,0, 0, 0, 0, 0, 0, 0, 1), new Op(Operands.FrameOffsetDereferenceInfoMask,0,0,5)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetDereferenceInfoMask,0, 0, 0, 2), new Op(Operands.FrameOffsetDereferenceInfoMask,0,0,0,0,6)),
                new MemAndValue(new MemoryOp(Operands.FrameOffsetDereferenceInfoMask,0, 0, 0, 0, 0, 0, 3), new Op(Operands.FrameOffsetDereferenceInfoMask,0,7)),
            });
        }
    }
}
