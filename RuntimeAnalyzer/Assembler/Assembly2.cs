using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Marler.RuntimeAnalyzer
{
    public class MachineCode
    {
        private readonly List<MachineInstruction> instructions
            = new List<MachineInstruction>();

        public MachineCode()
        {
        }
    }

    public abstract class MachineInstruction
    {
        public readonly Byte instruction;
        public readonly Byte byteLength;

        public MachineInstruction(Byte instruction, Byte byteLength)
        {
            this.instruction = instruction;
            this.byteLength = byteLength;
        }

        public abstract UInt64 Emit(Byte[] byteCode, UInt64 offset);
        public abstract void Emit(Stream stream);
    }

    public class MachineInstructionMemoryOp : MachineInstruction
    {
        public readonly MemoryOp memoryOp;

        public MachineInstructionMemoryOp(Byte instruction, MemoryOp memoryOp)
            : base(instruction, memoryOp.byteLength)
        {
        }

        public override UInt64 Emit(Byte[] byteCode, UInt64 offset)
        {
            byteCode[offset++] = instruction;
            return memoryOp.Emit(byteCode, offset);
        }

        public override void Emit(Stream stream)
        {
            stream.WriteByte(instruction);
            memoryOp.Emit(stream);
        }
    }


}
