#define DebugProcessor

using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.RuntimeAnalyzer
{
    public static class InstructionProcessor
    {

        public static UInt64 Execute(byte[] byteCode, UInt64 offset, Memory memory)
        {
            Memory.WriteOperandLogic writeOperand1, writeOperand2;
            Memory.ReadOperandLogic readOperand1, readOperand2, readOperand3;
            Memory.AccessOperandAddressLogic accessAddressOperand1, accessAddressOperand2, accessAddressOperand3;

        Loop:
            if (offset >= (UInt64)byteCode.Length) throw new FormatException("Instruction Block did not end with ByteCode.Halt");

#if DebugProcessor
            Console.WriteLine("PC: {0} INSTR: {1} 0x{2:X2} ({2})", offset, Instructions.ToString(byteCode[offset]), byteCode[offset]);
#endif
            Byte instruction = byteCode[offset++];

            switch (instruction)
            {
                case Instructions.Move:

                    writeOperand1 = memory.ParseWriteOperand(byteCode, ref offset);
                    readOperand2 = memory.ParseReadOperand(byteCode, ref offset);
                    writeOperand1.writeLogic(writeOperand1.op, readOperand2.readLogic(readOperand2.op));

                    goto Loop;
                case Instructions.ON:

                    writeOperand1 = memory.ParseWriteOperand(byteCode, ref offset);
                    writeOperand1.writeLogic(writeOperand1.op, UInt64.MaxValue);

                    goto Loop;
                case Instructions.Off:

                    writeOperand1 = memory.ParseWriteOperand(byteCode, ref offset);
                    writeOperand1.writeLogic(writeOperand1.op, 0);

                    goto Loop;
                case Instructions.Increment:

                    accessAddressOperand1 = memory.ParseWriteOperandForAddressAccess(byteCode, ref offset);
                    memory.Increment(accessAddressOperand1);

                    goto Loop;
                case Instructions.Decrement:

                    accessAddressOperand1 = memory.ParseWriteOperandForAddressAccess(byteCode, ref offset);
                    memory.Decrement(accessAddressOperand1);

                    goto Loop;
                case Instructions.Negate:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.NegateTo:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.Compliment:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.ComplimentTo:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.LeadingZeros:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.LeadingZerosTo:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.LeadingOnes:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.LeadingOnesTo:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.And:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.AndTo:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.Or:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.OrTo:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.XOr:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.XOrTo:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.ShiftLeft:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.ShiftLeftTo:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.ShiftRight:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.ShiftRightTo:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.RotateLeft:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.RotateLeftTo:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.RotateRight:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.RotateRightTo:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.Add:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.AddTo:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.Mult:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.MultTo:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.Sub:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.SubTo:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.SubReverse:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.Div:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.DivTo:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.DivReverse:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpF:

                    readOperand1 = memory.ParseReadOperand(byteCode, ref offset);
#if DebugProcessor
                    Console.WriteLine("Jumping Forward");
#endif
                    offset += readOperand1.readLogic(readOperand1.op);

                    goto Loop;
                case Instructions.JumpB:

                    readOperand1 = memory.ParseReadOperand(byteCode, ref offset);
#if DebugProcessor
                    Console.WriteLine("Jumping Backward");
#endif
                    offset -= readOperand1.readLogic(readOperand1.op);

                    goto Loop;
                case Instructions.JumpFIfZero:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpBIfZero:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpFIfNotZero:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpBIfNotZero:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpFIfPositive:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpBIfPositive:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpFIfNotNegative:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpBIfNotNegative:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpFIfNegative:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpBIfNegative:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpFIfNotPositive:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpBIfNotPositive:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpFIfEQ:

                    readOperand1 = memory.ParseWriteOperandForReading(byteCode, ref offset);
                    readOperand2 = memory.ParseReadOperand(byteCode, ref offset);
                    readOperand3 = memory.ParseReadOperand(byteCode, ref offset);

                    if (readOperand1.readLogic(readOperand1.op) == readOperand2.readLogic(readOperand2.op))
                    {
#if DebugProcessor
                        Console.WriteLine("Jumping");
#endif
                        offset += readOperand3.readLogic(readOperand3.op);
                    }

                    goto Loop;
                case Instructions.JumpBIfEQ:

                    readOperand1 = memory.ParseWriteOperandForReading(byteCode, ref offset);
                    readOperand2 = memory.ParseReadOperand(byteCode, ref offset);
                    readOperand3 = memory.ParseReadOperand(byteCode, ref offset);

                    if (readOperand1.readLogic(readOperand1.op) == readOperand2.readLogic(readOperand2.op))
                    {
#if DebugProcessor
                        Console.WriteLine("Jumping");
#endif
                        offset -= readOperand3.readLogic(readOperand3.op);
                    }

                    goto Loop;
                case Instructions.JumpFIfNEQ:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpBIfNEQ:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpFIfGT:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpBIfGT:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpFIfGTE:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpBIfGTE:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpFIfLT:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpBIfLT:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpFIfLTE:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.JumpBIfLTE:
                    throw new NotImplementedException();
                    goto Loop;
                case Instructions.Halt:
                    return offset;         
                default:
                    throw new FormatException(String.Format("UnrecognizedInstruction {0}", instruction));
            }

            // you should never get here
            throw new InvalidOperationException("Came to a restricted part of the Execute function"); 

        }
    }
}
