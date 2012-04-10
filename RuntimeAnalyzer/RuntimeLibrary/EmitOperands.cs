using System;
using System.Diagnostics;
using System.IO;

namespace Marler.RuntimeAnalyzer
{

    public class Op
    {
        public Byte byteLength;
        public Byte infoMask;
        public Byte[] op;

        protected Op()
        {
        }

        public Op(Byte frameOffset)
        {
            Debug.Assert(frameOffset <= Operands.ReadableOpTypeFrameOffsetShortMax);

            this.byteLength = 1;
            this.infoMask = frameOffset;
            this.op = null;
        }
        public Op(Byte infoMask, params Byte[] op)
        {
            Debug.Assert(op.Length <= 8 &&
                (infoMask == Operands.LiteralInfoMask ||
                infoMask == Operands.AddressInfoMask ||
                infoMask == Operands.FrameOffsetInfoMask ||
                infoMask == Operands.AddressDereferenceInfoMask ||
                infoMask == Operands.FrameOffsetDereferenceInfoMask));

            this.byteLength = (Byte)(op.Length + 1);
            this.infoMask = infoMask;
            this.op = op;
        }
        public Op(Byte infoMask, UInt64 op)
        {
            Debug.Assert(infoMask == Operands.LiteralInfoMask ||
                infoMask == Operands.AddressInfoMask ||
                infoMask == Operands.FrameOffsetInfoMask ||
                infoMask == Operands.AddressDereferenceInfoMask ||
                infoMask == Operands.FrameOffsetDereferenceInfoMask);

            this.op = Util.GetBytes(op);
            this.byteLength = (Byte)(this.op.Length + 1);
            this.infoMask = infoMask;
        }

        public UInt64 Emit(Byte[] byteCode, UInt64 offset)
        {
            if(op == null)
            {
                byteCode[offset++] = infoMask;
            }
            else
            {
                byteCode[offset++] = (Byte)(infoMask | (op.Length - 1));
                byteCode[offset++] = op[0];
                for (Byte i = 1; i < op.Length; i++)
                {
                    byteCode[offset++] = op[i];
                }
            }
            return offset;
        }
        public void Emit(Stream stream)
        {
            if (op == null)
            {
                stream.WriteByte(infoMask);
            }
            else
            {
                stream.WriteByte((Byte)(infoMask | (op.Length - 1)));
                stream.Write(op, 0, op.Length);
            }
        }

        public override string ToString()
        {
            return String.Format("OP({0})", infoMask);
        }
    }

    public class MemoryOp : Op
    {
        public MemoryOp(Byte frameOffset)
        {
            Debug.Assert(frameOffset <= Operands.WritableOpTypeFrameOffsetShortMax);

            this.byteLength = 1;
            this.infoMask = frameOffset;
            this.op = null;
        }
        public MemoryOp(Byte infoMask, params Byte[] op)
            : base()
        {
            Debug.Assert(op.Length <= 8 &&
                (infoMask == Operands.AddressInfoMask ||
                infoMask == Operands.FrameOffsetInfoMask ||
                infoMask == Operands.AddressDereferenceInfoMask ||
                infoMask == Operands.FrameOffsetDereferenceInfoMask));

            this.byteLength = (Byte)(op.Length + 1);
            this.infoMask = infoMask;
            this.op = op;
        }
        public MemoryOp(Byte infoMask, UInt64 op)
        {
            Debug.Assert(infoMask == Operands.AddressInfoMask ||
                infoMask == Operands.FrameOffsetInfoMask ||
                infoMask == Operands.AddressDereferenceInfoMask ||
                infoMask == Operands.FrameOffsetDereferenceInfoMask);

            this.op = Util.GetBytes(op);
            this.byteLength = (Byte)(this.op.Length + 1);
            this.infoMask = infoMask;
        }

        public override string  ToString()
        {
            return String.Format("Writable OP({0})", infoMask);
        }
    }

    /*
    public struct JumpRelativeAddressOp
    {
        public Byte infoByte;
        public Byte [] byteCodeOffset;

        public JumpRelativeAddressOp(Byte byteCodeOffset)
        {
            Debug.Assert(byteCodeOffset <= Operands.JumpRelativeAddressOpMaxShort);

            this.infoByte = byteCodeOffset;
            this.byteCodeOffset = null;
        }
        public JumpRelativeAddressOp(Byte[] byteCodeOffset)
        {
            this.byteCodeOffset = byteCodeOffset;
            if(byteCodeOffset.Length == 4)
            {
                this.infoByte = Operands.JumpRelativeAddressOp4Bytes;
            }
            else if(byteCodeOffset.Length == 8)
            {
                this.infoByte = Operands.JumpRelativeAddressOp4Bytes;
            }
            else
            {
                throw new ArgumentOutOfRangeException("byteCodeOffset");
            }
        }
        public JumpRelativeAddressOp(UInt64 op)
        {
            if (op <= Operands.JumpRelativeAddressOpMaxShort)
            {
                this.infoByte = (Byte)op;
                this.byteCodeOffset = null;
            }
            else if (op <= 0xFFFFFFFFU)
            {
                this.infoByte = Operands.JumpRelativeAddressOp4Bytes;
                this.byteCodeOffset = new Byte[4];
                this.byteCodeOffset[0] = (Byte)(op >> 24);
                this.byteCodeOffset[1] = (Byte)(op >> 16);
                this.byteCodeOffset[2] = (Byte)(op >> 8 );
                this.byteCodeOffset[3] = (Byte)(op      );
            }
            else
            {
                this.infoByte = Operands.JumpRelativeAddressOp8Bytes;
                this.byteCodeOffset = new Byte[8];
                this.byteCodeOffset[0] = (Byte)(op >> 56);
                this.byteCodeOffset[1] = (Byte)(op >> 48);
                this.byteCodeOffset[2] = (Byte)(op >> 40);
                this.byteCodeOffset[3] = (Byte)(op >> 32);
                this.byteCodeOffset[4] = (Byte)(op >> 24);
                this.byteCodeOffset[5] = (Byte)(op >> 16);
                this.byteCodeOffset[6] = (Byte)(op >> 8);
                this.byteCodeOffset[7] = (Byte)(op);
            }
        }

        public UInt64 Emit(Byte[] byteCode, UInt64 offset)
        {
            byteCode[offset++] = infoByte;

            if(byteCodeOffset == null)
            {
                return offset;
            }

            byteCodeOffset[offset++] = infoByte;
            byteCode[offset++] = byteCodeOffset[0];
            byteCode[offset++] = byteCodeOffset[1];
            byteCode[offset++] = byteCodeOffset[2];
            byteCode[offset++] = byteCodeOffset[3];

            if(infoByte == Operands.JumpRelativeAddressOp8Bytes)
            {
                byteCode[offset++] = byteCodeOffset[4];
                byteCode[offset++] = byteCodeOffset[5];
                byteCode[offset++] = byteCodeOffset[6];
                byteCode[offset++] = byteCodeOffset[7];
            }
            return offset;
        }

        public override string ToString()
        {
            return String.Format("JumpRelativeOP({0})", infoByte);
        }

    }
    */
}
