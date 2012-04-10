#define DebugMemory

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Marler.RuntimeAnalyzer
{
    public struct Memory
    {
        private readonly UInt64[] data;

        private readonly ReadCallback[] readCallbacks;
        private readonly WriteCallback[] writeCallbacks;
        private readonly UInt64 ioMapLength;

        private UInt64 framePointer;
        private UInt64 stackPointer;

        public Memory(UInt64 totalMemorySize)
        {
            this.data = new UInt64[totalMemorySize];

            this.readCallbacks = null;
            this.writeCallbacks = null;
            this.ioMapLength = 0;

            this.stackPointer = 0;
            this.framePointer = 0;
        }

        public Memory(UInt64 totalMemorySize, ReadCallback [] readCallbacks, WriteCallback [] writeCallbacks)
        {
            this.data = new UInt64[totalMemorySize];

            //
            // Memory Map
            //
            this.readCallbacks = readCallbacks;
            this.writeCallbacks = writeCallbacks;

            if (this.readCallbacks.Length != writeCallbacks.Length)
            {
                throw new ArgumentException("callback arrays must have same length");
            }
            this.ioMapLength = (UInt64)readCallbacks.Length;

#if DEBUG
            for (UInt64 i = 0; i < ioMapLength; i++)
            {
                if (readCallbacks[i] == null) throw new ArgumentNullException();
                if (writeCallbacks[i] == null) throw new ArgumentNullException();
            }
#endif


            this.stackPointer = ioMapLength;
            this.framePointer = ioMapLength;
        }

        public void Reset()
        {
            this.framePointer = ioMapLength;
            this.stackPointer = ioMapLength;
        }


        public void Print()
        {
            Console.WriteLine("{0} Mapped IO registers", ioMapLength);
            for (UInt64 i = ioMapLength; i < stackPointer; i++)
            {
                Console.WriteLine("[{0:X}] = {1:X} ({1})", i, data[i]);
            }
        }
        public void Print(UInt64 address, UInt64 length)
        {
            Console.WriteLine("{0} Mapped IO registers", ioMapLength);
            for (UInt64 limit = address + length; address < limit; address++)
            {
                Console.WriteLine("[0x{0:X}] = 0x{1:X} ({1})", address, data[address]);
            }
        }

        public void StackAllocate(UInt64 size)
        {
            stackPointer += size;
        }

        public void PushFramePointerAndSetNewFromVar(UInt64 newFramePointerVar)
        {
            data[stackPointer++] = framePointer; // store old frame pointer
            framePointer = data[framePointer + newFramePointerVar];
        }
        public void PushFramePointerAndSetNewFromLiteral(UInt64 newFramePointerLiteral)
        {
            data[stackPointer++] = framePointer; // store old frame pointer
            framePointer = newFramePointerLiteral;
        }

        public void PushLiteral(UInt64 literal)
        {
            data[stackPointer++] = literal;
        }
        public void PushVar(UInt64 address)
        {
            data[stackPointer++] = data[framePointer + address];
        }
          
        public void Increment(AccessOperandAddressLogic accessAddress)
        {
            UInt64 address = accessAddress.accessLogic(accessAddress.op);
            if (address < ioMapLength)
            {
                writeCallbacks[address](readCallbacks[address]() + 1);
            }
            else
            {
                data[address]++;
            }
        }
        public void Decrement(AccessOperandAddressLogic accessAddress)
        {
            UInt64 address = accessAddress.accessLogic(accessAddress.op);
            if (address < ioMapLength)
            {
                writeCallbacks[address](readCallbacks[address]() - 1);
            }
            else
            {
                data[address]--;
            }
        }


        internal void JumpIfEqual(WriteOperandLogic writeOperand1, ReadOperandLogic readOperand2, ReadOperandLogic readOperand3)
        {
            throw new NotImplementedException();
        }

        public void AddVar(UInt64 dstAddress, UInt64 srcAddress)
        {
            data[framePointer + dstAddress] += data[framePointer + srcAddress];
        }
        public void AddLiteral(UInt64 dstAddress, UInt64 literal)
        {
            data[framePointer + dstAddress] += literal;
        }

        public Boolean GreaterThanOrEqualToVar(UInt64 leftVar, UInt64 rightVar)
        {
            return data[framePointer + leftVar] >= data[framePointer + rightVar];
        }
        public Boolean GreaterThenOrEqualToLiteral(UInt64 leftVar, UInt64 literal)
        {
            return data[framePointer + leftVar] >= literal;
        }





        //
        // Test Functions
        //
        public Boolean TestEquals(ReadOperandLogic readOperand1, ReadOperandLogic readOperand2)
        {
            return readOperand1.readLogic(readOperand1.op) == readOperand2.readLogic(readOperand2.op);
        }
        public Boolean TestAddressEqualsValue(UInt64 address, UInt64 value)
        {
            return data[address] == value;
        }
        public void DebugSetMemory(UInt64 address, UInt64 value)
        {
            data[address] = value;
        }
        public void DebugSetFramePointer(UInt64 newFramePointer)
        {
            this.framePointer = newFramePointer;
        }
        public Int32 DebugGetDataLength { get { return data.Length; } }





        //
        //
        //
        public AccessOperandAddressLogic ParseWriteOperandForAddressAccess(Byte[] byteCode, ref UInt64 offset)
        {
            AccessOperandAddressLogic accessAddressLogic;
            Byte info = byteCode[offset++];

            if (info <= Operands.WritableOpTypeFrameOffsetShortMax)
            {
                accessAddressLogic.op = (UInt64)info;
                accessAddressLogic.accessLogic = FrameOffsetAccessAddress;
                return accessAddressLogic;
            }

            switch ((info >> 3) & 0x03)
            {
                case 0:
                    accessAddressLogic.accessLogic = AddressAccessAddress;
                    break;
                case 1:
                    accessAddressLogic.accessLogic = FrameOffsetAccessAddress;
                    break;
                case 2:
                    accessAddressLogic.accessLogic = AddressDereferenceAccessAddress;
                    break;
                case 3:
                    accessAddressLogic.accessLogic = FrameOffsetDereferenceAccessAddress;
                    break;
                default:
                    throw new InvalidOperationException("Internal Code Error while processing byte code operand");
            }
            accessAddressLogic.op = Util.GetUInt64(byteCode, ref offset, info);
            return accessAddressLogic;
        }
        public ReadOperandLogic ParseWriteOperandForReading(Byte[] byteCode, ref UInt64 offset)
        {
            ReadOperandLogic writeOperandReadLogic;
            Byte info = byteCode[offset++];

            if (info <= Operands.WritableOpTypeFrameOffsetShortMax)
            {
                writeOperandReadLogic.op = (UInt64)info;
                writeOperandReadLogic.readLogic = FrameOffsetRead;
                return writeOperandReadLogic;
            }

            switch ((info >> 3) & 0x03)
            {
                case 0:
                    writeOperandReadLogic.readLogic = AddressRead;
                    break;
                case 1:
                    writeOperandReadLogic.readLogic = FrameOffsetRead;
                    break;
                case 2:
                    writeOperandReadLogic.readLogic = AddressDereferenceRead;
                    break;
                case 3:
                    writeOperandReadLogic.readLogic = FrameOffsetDereferenceRead;
                    break;
                default:
                    throw new InvalidOperationException("Internal Code Error while processing byte code operand");
            }
            writeOperandReadLogic.op = Util.GetUInt64(byteCode, ref offset, info);
            return writeOperandReadLogic;
        }
        public WriteOperandLogic ParseWriteOperand(Byte[] byteCode, ref UInt64 offset)
        {
            WriteOperandLogic writeOperand;
            Byte info = byteCode[offset++];

            if (info <= Operands.WritableOpTypeFrameOffsetShortMax)
            {
                writeOperand.op = (UInt64)info;
                writeOperand.writeLogic = FrameOffsetWrite;
                return writeOperand;
            }

            switch ((info >> 3) & 0x03)
            {
                case 0:
                    writeOperand.writeLogic = AddressWrite;
                    break;
                case 1:
                    writeOperand.writeLogic = FrameOffsetWrite;
                    break;
                case 2:
                    writeOperand.writeLogic = AddressDereferenceWrite;
                    break;
                case 3:
                    writeOperand.writeLogic = FrameOffsetDereferenceWrite;
                    break;
                default:
                    throw new InvalidOperationException("Internal Code Error while processing byte code operand");
            }
            writeOperand.op = Util.GetUInt64(byteCode, ref offset, info);
            return writeOperand;
        }


        public ReadOperandLogic ParseReadOperand(Byte[] byteCode, ref UInt64 offset)
        {
            ReadOperandLogic readOperand;
            Byte info = byteCode[offset++];
            if (info <= Operands.ReadableOpTypeFrameOffsetShortMax)
            {
                readOperand.op = (UInt64)info;
                readOperand.readLogic = FrameOffsetRead;
                return readOperand;
            }

            if (info <= Operands.ReadableOpTypeLiteralMax)
            {
                readOperand.op = Util.GetUInt64(byteCode, ref offset, info);
                readOperand.readLogic = ValueRead;
                return readOperand;
            }

            switch ((info >> 3) & 0x03)
            {
                case 0:
                    readOperand.readLogic = AddressRead;
                    break;
                case 1:
                    readOperand.readLogic = FrameOffsetRead;
                    break;
                case 2:
                    readOperand.readLogic = AddressDereferenceRead;
                    break;
                case 3:
                    readOperand.readLogic = FrameOffsetDereferenceRead;
                    break;
                default:
                    throw new InvalidOperationException("Internal Code Error while processing byte code operand");
            }
            readOperand.op = Util.GetUInt64(byteCode, ref offset, info);
            return readOperand;
        }

        public struct AccessOperandAddressLogic
        {
            public UInt64 op;
            public AccessOperandAddressLogicFunction accessLogic;

            public override string ToString()
            {
                return String.Format("AccessOperandAddressLogic(op=0x{0:X})", op);
            }
        }
        public struct WriteOperandLogic
        {
            public UInt64 op;
            public WriteOperandLogicFunction writeLogic;

            public override string ToString()
            {
                return String.Format("WriteOperandLogic(op=0x{0:X})", op);
            }
        }
        public struct ReadOperandLogic
        {
            public UInt64 op;
            public ReadOperandLogicFunction readLogic;

            public override string ToString()
            {
                return String.Format("ReadOperandLogic(op=0x{0:X})", op);
            }
        }
        public delegate void WriteOperandLogicFunction(UInt64 op, UInt64 value);
        public delegate UInt64 ReadOperandLogicFunction(UInt64 op);
        public delegate UInt64 AccessOperandAddressLogicFunction(UInt64 op);

        private void FrameOffsetWrite(UInt64 frameOffset, UInt64 value)
        {
#if DebugMemory
            Console.WriteLine("fp({0}) = 0x{1:X}  ({1})", frameOffset, value); 
#endif
            data[framePointer + frameOffset] = value;
        }
        private UInt64 FrameOffsetRead(UInt64 frameOffset)
        {
            return data[framePointer + frameOffset];
        }
        private UInt64 FrameOffsetAccessAddress(UInt64 frameOffset)
        {
            return framePointer + frameOffset;
        }

        private void FrameOffsetDereferenceWrite(UInt64 frameOffsetWithAddress, UInt64 value)
        {
            UInt64 address = data[framePointer + frameOffsetWithAddress];
            if (address < ioMapLength)
            {
                writeCallbacks[address](value);
            }
            else
            {
                data[address] = value;
            }
        }
        private UInt64 FrameOffsetDereferenceRead(UInt64 frameOffsetWithAddress)
        {
            UInt64 address = data[framePointer + frameOffsetWithAddress];
            return (address < ioMapLength) ? readCallbacks[address]() :
                data[address];
        }
        private UInt64 FrameOffsetDereferenceAccessAddress(UInt64 frameOffsetWithAddress)
        {
            return data[framePointer + frameOffsetWithAddress];
        }
        private void AddressWrite(UInt64 address, UInt64 value)
        {
            if (address < ioMapLength)
            {
                writeCallbacks[address](value);
            }
            else
            {
                data[address] = value;
            }
        }
        private UInt64 AddressRead(UInt64 address)
        {
            return (address < ioMapLength) ? readCallbacks[address]() :
                data[address];
        }
        private UInt64 AddressAccessAddress(UInt64 address)
        {
            return address;
        }
        private void AddressDereferenceWrite(UInt64 addressWithAddress, UInt64 value)
        {
            UInt64 newAddress = data[addressWithAddress];
            if (newAddress < ioMapLength)
            {
                writeCallbacks[newAddress](value);
            }
            else
            {
                data[newAddress] = value;
            }
        }
        private UInt64 AddressDereferenceRead(UInt64 addressWithAddress)
        {
            UInt64 newAddress = data[addressWithAddress];
            return (newAddress < ioMapLength) ? readCallbacks[newAddress]() :
                data[newAddress];
        }
        private UInt64 AddressDereferenceAccessAddress(UInt64 addressWithAddress)
        {
            return data[addressWithAddress];
        }
        private UInt64 ValueRead(UInt64 value)
        {
            return value;
        }
    }
}
