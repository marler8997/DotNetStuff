using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.RuntimeAnalyzer
{
    /*
    [TestClass]
    public class InstructionAddTest
    {
        public static readonly Byte[] byteCode = new Byte[30];
        static readonly ProcessStack stack = new ProcessStack(1025);

        [TestMethod]
        public void AddTest()
        {
            UInt64 offset;

            //
            // Generate Code
            //
            offset = 0;
            memory.AssignLiteral(128, 0xFFFFFF00);

            offset = ByteCode.EmitAdd(byteCode, ref offset, new EmitVariableOperandQuickVar(Operands.VariableTypeMaxQuick), 0xFF);
            byteCode[offset++] = ByteCode.EndFunction;

            //
            // Run Code
            //
            Assert.AreEqual(offset, InstructionProcessor.Execute(byteCode, 0, memory));

            //
            // Test Results
            //
            Assert.AreEqual(offset, processOffset);
            Assert.AreEqual(0xFFFFFFFF, stack[128]);

        }
    }
    */
}
