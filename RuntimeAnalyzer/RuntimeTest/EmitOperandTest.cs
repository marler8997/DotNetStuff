using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.RuntimeAnalyzer
{
    [TestClass]
    public class EmitOperandTest
    {

        [TestMethod]
        public void Test()
        {
            Int32 off;
            byte[] byteCode = new byte[10];
            MemoryOp[] emitW = new MemoryOp[32];
            Op[] emitR = new Op[32];

            off = 0;
            emitW[off++] = new MemoryOp(0);
            emitW[off++] = new MemoryOp(1);
            emitW[off++] = new MemoryOp(23);
            emitW[off++] = new MemoryOp(127);
            emitW[off++] = new MemoryOp(Operands.WritableOpTypeFrameOffsetShortMax);
            for (int i = 0; i < off; i++)
            {
                Assert.AreEqual(1U, emitW[i].Emit(byteCode, 0));
                Assert.AreEqual(emitW[i].infoMask, byteCode[0]);
            }

            off = 0;
            emitR[off++] = new Op(0);
            emitR[off++] = new Op(1);
            emitR[off++] = new Op(23);
            emitR[off++] = new Op(127);
            emitR[off++] = new Op(Operands.ReadableOpTypeFrameOffsetShortMax);
            for (int i = 0; i < off; i++)
            {
                Assert.AreEqual(1U, emitW[i].Emit(byteCode, 0));
                Assert.AreEqual(emitW[i].infoMask, byteCode[0]);
            }

            off = 0;
            emitW[off++] = new MemoryOp(Operands.AddressInfoMask, 248);
            emitW[off++] = new MemoryOp(Operands.AddressInfoMask, 88, 1);
            emitW[off++] = new MemoryOp(Operands.AddressInfoMask, 1, 2, 255);
            emitW[off++] = new MemoryOp(Operands.AddressInfoMask, 55, 23, 255, 8, 2, 1, 100, 98);
            emitW[off++] = new MemoryOp(Operands.AddressInfoMask, 32, 2, 255, 204);
            for (int i = 0; i < off; i++)
            {
                MemoryOp w = emitW[i];

                Assert.AreEqual((UInt64)(w.op.Length + 1), emitW[i].Emit(byteCode, 0));
                //Assert.AreEqual((Byte)(Operands.LongVariableMask | (l.longAddress.Length-1)), byteCode[0]);
                for (Byte j = 0; j < w.op.Length; j++)
                {
                    Assert.AreEqual(w.op[j], byteCode[j + 1]);
                }
            }
            /*
            off = 0;
            emitW[off++] = new EmitLiteral(34);
            emitW[off++] = new EmitLiteral(88, 1);
            emitW[off++] = new EmitLiteral(1, 2, 255);
            emitW[off++] = new EmitLiteral(55, 23, 255, 8, 2, 1, 100, 98);
            emitW[off++] = new EmitLiteral(32, 2, 255, 204);
            for (int i = 0; i < off; i++)
            {
                EmitLiteral lit = (EmitLiteral)emitW[i];

                Assert.AreEqual((UInt64)(lit.literal.Length + 1), emitW[i].Emit(byteCode, 0));
                //Assert.AreEqual((Byte)(Operands.LiteralInfoMask | (lit.literal.Length - 1)), byteCode[0]);
                for (Byte j = 0; j < lit.literal.Length; j++)
                {
                    Assert.AreEqual(lit.literal[j], byteCode[j + 1]);
                }
            }
            */
        }
    }
}
