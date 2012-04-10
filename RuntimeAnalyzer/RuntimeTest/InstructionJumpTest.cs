using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.RuntimeAnalyzer
{
    [TestClass]
    public class InstructionJumpTest
    {
        readonly Byte[] byteCode = new Byte[30];
        readonly Memory memory = new Memory(1024);

        [TestMethod]
        public void JumpLiteralTest()
        {
            UInt64 offset;

            // Generate Code
            memory.Reset();

            offset = 0;
            offset = ByteCode.EmitJumpF(byteCode, offset, new Op(Operands.LiteralInfoMask, 0, 0, 0, 0, 0, 0, 0, 0));
            offset = ByteCode.EmitJumpF(byteCode, offset, new Op(Operands.LiteralInfoMask, 0, 8));
            offset = ByteCode.EmitJumpF(byteCode, offset, new Op(Operands.LiteralInfoMask, 0, 0, 0, 0, 0, 6));
            offset = ByteCode.EmitJumpB(byteCode, offset, new Op(Operands.LiteralInfoMask, 0, 0, 0, 14));
            byteCode[offset++] = Instructions.Halt;

            // Run Code
            Assert.AreEqual(offset, InstructionProcessor.Execute(byteCode, 0, memory));
        }





        [TestMethod]
        public void JumpEQTest()
        {
            UInt64 offset;

            //
            // Generate Code
            //
            offset = 0;
            memory.Reset();
            memory.PushLiteral(0);
            memory.PushLiteral(5);
            offset = ByteCode.EmitJumpFIfEQ(byteCode, offset, new MemoryOp(0), new Op(Operands.LiteralInfoMask, 0), new Op(0));
            offset = ByteCode.EmitJumpFIfEQ(byteCode, offset, new MemoryOp(1), new Op(Operands.LiteralInfoMask, 0), new Op(0));
            offset = ByteCode.EmitJumpFIfEQ(byteCode, offset, new MemoryOp(0), new Op(Operands.LiteralInfoMask, 0), new Op(0));
            offset = ByteCode.EmitJumpFIfEQ(byteCode, offset, new MemoryOp(1), new Op(Operands.LiteralInfoMask, 0), new Op(0));
            offset = ByteCode.EmitJumpFIfEQ(byteCode, offset, new MemoryOp(0), new Op(Operands.LiteralInfoMask, 0), new Op(0));
            byteCode[offset++] = Instructions.Halt;

            //
            // Run Code
            //
            Assert.AreEqual(offset, InstructionProcessor.Execute(byteCode, 0, memory));


            //
            // Generate Code
            //
            Console.WriteLine("FBF");
            offset = 0;
            memory.Reset();
            memory.PushLiteral(0);
            offset = ByteCode.EmitJumpFIfEQ(byteCode, offset, new MemoryOp(0), new Op(Operands.LiteralInfoMask, 0), new Op(Operands.LiteralInfoMask, 6));
            offset = ByteCode.EmitJumpFIfEQ(byteCode, offset, new MemoryOp(0), new Op(Operands.LiteralInfoMask, 0), new Op(Operands.LiteralInfoMask, 6));
            offset = ByteCode.EmitJumpBIfEQ(byteCode, offset, new MemoryOp(0), new Op(Operands.LiteralInfoMask, 0), new Op(Operands.LiteralInfoMask, 12));
            byteCode[offset++] = Instructions.Halt;

            //
            // Run Code
            //
            Assert.AreEqual(offset, InstructionProcessor.Execute(byteCode, 0, memory));
            /*
            //
            // Generate Code
            //
            offset = 0;
            offset = ByteCode.EmitJumpLiteral(byteCode, ref offset, 0, 0, 0, 0);
            offset = ByteCode.EmitJumpLiteral(byteCode, ref offset, 0, 0, 4);
            offset = ByteCode.EmitJumpLiteral(byteCode, ref offset, 0, 5);
            offset = ByteCode.EmitJumpLiteral(byteCode, ref offset, 0xFF, 0xFF, unchecked((Byte)(-9)));
            byteCode[offset++] = ByteCode.EndFunction;

            //
            // Run Code
            //
            Assert.AreEqual(offset, InstructionProcessor.Execute(byteCode, 0, memory));

            //
            // Test Results
            //


            //
            // Generate Code
            //
            offset = 0;
            offset = ByteCode.EmitJumpLiteral(byteCode, ref offset, 0, 0, 0, 0);
            offset = ByteCode.EmitJumpLiteral(byteCode, ref offset, 0, 3);
            offset = ByteCode.EmitJumpLiteral(byteCode, ref offset, 5);
            offset = ByteCode.EmitJumpLiteral(byteCode, ref offset, 0xFF, 0xFF, unchecked((Byte)(-8)));
            byteCode[offset++] = ByteCode.EndFunction;

            //
            // Run Code
            //
            Assert.AreEqual(offset, InstructionProcessor.Execute(byteCode, 0, memory));



            //
            // Generate Code
            //
            offset = 0;
            stack[0] = new Variable(0x81);
            stack[1] = new Variable(0x81);
            stack[2] = new Variable(0x81);
            stack[0].SetMemWithMask(0);
            stack[1].SetMemWithMask(2);
            stack[2].SetMemWithMask(unchecked((UInt64)(-4)));
            offset = ByteCode.EmitJumpVar(byteCode, ref offset, 0);
            offset = ByteCode.EmitJumpVar(byteCode, ref offset, 1);
            offset = ByteCode.EmitJumpVar(byteCode, ref offset, 1);
            offset = ByteCode.EmitJumpVar(byteCode, ref offset, 2);
            byteCode[offset++] = ByteCode.EndFunction;

            //
            // Run Code
            //
            Assert.AreEqual(offset, InstructionProcessor.Execute(byteCode, 0, memory));

            //
            // Test Results
            //

            //
            // Generate Code
            //
            offset = 0;
            stack[0] = new Variable(0x84);
            stack[1] = new Variable(0x83);
            stack[2] = new Variable(0x82);
            stack[0].SetMemWithMask(0);
            stack[1].SetMemWithMask(2);
            stack[2].SetMemWithMask(unchecked((UInt64)(-4)));
            offset = ByteCode.EmitJumpVar(byteCode, ref offset, 0);
            offset = ByteCode.EmitJumpVar(byteCode, ref offset, 1);
            offset = ByteCode.EmitJumpVar(byteCode, ref offset, 1);
            offset = ByteCode.EmitJumpVar(byteCode, ref offset, 2);
            byteCode[offset++] = ByteCode.EndFunction;

            //
            // Run Code
            //
            Assert.AreEqual(offset, InstructionProcessor.Execute(byteCode, 0, memory));

            //
            // Test Results
            //
            */
        }
        /*
        [TestMethod]
        public void JumpToTest()
        {
            UInt64 offset;

            //
            // Generate Code
            //
            offset = 0;
            offset = ByteCode.EmitJumpToLiteral(byteCode, ref offset, 0, 0, 0, 6);
            offset = ByteCode.EmitJumpToLiteral(byteCode, ref offset, 0, 13);
            offset = ByteCode.EmitJumpToLiteral(byteCode, ref offset, 18);
            offset = ByteCode.EmitJumpToLiteral(byteCode, ref offset, 0, 0, 10);
            byteCode[offset++] = ByteCode.EndFunction;

            //
            // Run Code
            //
            InstructionProcessor.Execute(byteCode, 0, memory);

            //
            // Test Results
            //
            Assert.AreEqual(offset, processOffset);

            //
            // Generate Code
            //
            offset = 0;
            stack[0].SetMemWithMask(2);
            stack[1].SetMemWithMask(4);
            stack[2].SetMemWithMask(6);
            stack[3].SetMemWithMask(8);
            offset = ByteCode.EmitJumpToVar(byteCode, ref offset, 0);
            offset = ByteCode.EmitJumpToVar(byteCode, ref offset, 2);
            offset = ByteCode.EmitJumpToVar(byteCode, ref offset, 3);
            offset = ByteCode.EmitJumpToVar(byteCode, ref offset, 1);
            byteCode[offset++] = ByteCode.EndFunction;

            //
            // Run Code
            //
            InstructionProcessor.Execute(byteCode, 0, memory);

            //
            // Test Results
            //
            Assert.AreEqual(offset, processOffset);
        }
        */
    }
}
