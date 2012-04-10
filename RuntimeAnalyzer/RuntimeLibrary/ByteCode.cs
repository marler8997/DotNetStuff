using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Marler.RuntimeAnalyzer
{

    public static class ByteCode
    {
        public static void EmitMetadataUInt64(Stream stream, UInt64 value)
        {
            Byte[] bytes = Util.GetBytesWithExtraBytesPrefix(value);
            stream.Write(bytes, 0, bytes.Length);
        }
        public static UInt64 EmitMetadataUInt64(byte[] byteCode, UInt64 offset, UInt64 value)
        {
            UInt64 newOffset = offset + 1;
            newOffset = Util.InsertUInt64(byteCode, newOffset, value);
            byteCode[offset] = (Byte)((newOffset - offset) - 2);
            return newOffset;
        }


        public static UInt64 EmitAssign(byte[] byteCode, UInt64 offset, MemoryOp dst, Op src)
        {
            byteCode[offset++] = Instructions.Move;
            offset = dst.Emit(byteCode, offset);
            return src.Emit(byteCode, offset);
        }

        public static UInt64 EmitON(byte[] byteCode, UInt64 offset, MemoryOp var)
        {
            byteCode[offset++] = Instructions.ON;
            return var.Emit(byteCode, offset);
        }

        public static UInt64 EmitOff(byte[] byteCode, UInt64 offset, MemoryOp var)
        {
            byteCode[offset++] = Instructions.Off;
            return var.Emit(byteCode, offset);
        }

        public static UInt64 EmitIncrement(byte[] byteCode, UInt64 offset, MemoryOp var)
        {
            byteCode[offset++] = Instructions.Increment;
            return var.Emit(byteCode, offset);
        }

        public static UInt64 EmitDecrement(byte[] byteCode, UInt64 offset, MemoryOp var)
        {
            byteCode[offset++] = Instructions.Decrement;
            return var.Emit(byteCode, offset);
        }

        public static UInt64 EmitJumpF(byte[] byteCode, UInt64 offset, Op jmp)
        {
            byteCode[offset++] = Instructions.JumpF;
            return jmp.Emit(byteCode, offset);
        }
        public static UInt64 EmitJumpB(byte[] byteCode, UInt64 offset, Op jmp)
        {
            byteCode[offset++] = Instructions.JumpB;
            return jmp.Emit(byteCode, offset);
        }

        public static UInt64 EmitJumpFIfEQ(byte[] byteCode, UInt64 offset, MemoryOp cnd1, Op cnd2, Op jmp)
        {
            byteCode[offset++] = Instructions.JumpFIfEQ;
            offset = cnd1.Emit(byteCode, offset);
            offset = cnd2.Emit(byteCode, offset);
            return jmp.Emit(byteCode, offset);
        }
        public static UInt64 EmitJumpBIfEQ(byte[] byteCode, UInt64 offset, MemoryOp cnd1, Op cnd2, Op jmp)
        {
            byteCode[offset++] = Instructions.JumpBIfEQ;
            offset = cnd1.Emit(byteCode, offset);
            offset = cnd2.Emit(byteCode, offset);
            return jmp.Emit(byteCode, offset);
        }

        /*

        public static UInt64 EmitAddVar(byte[] byteCode, UInt64 offset, Byte dstVar, Byte srcVar)
        {
            byteCode[offset++] = Instructions.Add;
            byteCode[offset++] = dstVar;

            //if (srcVar >= Operands.MaxQuickDirectReference) byteCode[offset++] = Operands.MaxQuickDirectReference;
            byteCode[offset++] = srcVar;
        }

        public static UInt64 EmitAddLiteral(byte[] byteCode, UInt64 offset, Byte dstVar, params Byte[] literal)
        {
            byteCode[offset++] = Instructions.Add;
            byteCode[offset++] = dstVar;

            //byteCode[offset++] = (byte)(literal.Length + Operands.Literal1Byte - 1);
            for (byte i = 0; i < literal.Length; i++)
            {
                byteCode[offset++] = literal[i];
            }
        }



        public static UInt64 EmitJumpVar(byte[] byteCode, UInt64 offset, Byte var)
        {
            byteCode[offset++] = Instructions.Jump;
            //if (var >= Operands.MaxQuickDirectReference) byteCode[offset++] = Operands.MaxQuickDirectReference;
            byteCode[offset++] = var;
        }
        public static UInt64 EmitJumpLiteral(byte[] byteCode, UInt64 offset, params Byte[] literal)
        {
            byteCode[offset++] = Instructions.Jump;
            //byteCode[offset++] = (byte)(literal.Length + Operands.Literal1Byte - 1);
            for (byte i = 0; i < literal.Length; i++)
            {
                byteCode[offset++] = literal[i];
            }
        }
        public static UInt64 EmitJumpToVar(byte[] byteCode, UInt64 offset, Byte var)
        {
            byteCode[offset++] = Instructions.JumpTo;
            //if (var >= Operands.MaxQuickDirectReference) byteCode[offset++] = Operands.MaxQuickDirectReference;
            byteCode[offset++] = var;
        }
        public static UInt64 EmitJumpToLiteral(byte[] byteCode, UInt64 offset, params Byte[] literal)
        {
            byteCode[offset++] = Instructions.JumpTo;
            //byteCode[offset++] = (byte)(literal.Length + Operands.Literal1Byte - 1);
            for (byte i = 0; i < literal.Length; i++)
            {
                byteCode[offset++] = literal[i];
            }
        }

        private static void PrivateEmitInstructionVarAndLiteral(Byte instruction, byte[] byteCode, ref Int32 offset, Byte var, params Byte[] literal)
        {
            byteCode[offset++] = instruction;
            byteCode[offset++] = var;
            byteCode[offset++] = (byte)(literal.Length + Operands.Literal1Byte - 1);
            for (byte i = 0; i < literal.Length; i++)
            {
                byteCode[offset++] = literal[i];
            }
        }
        */

    }
}
