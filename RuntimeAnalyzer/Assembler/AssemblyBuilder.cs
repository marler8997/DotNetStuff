using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Marler.RuntimeAnalyzer
{
    public class AssemblyBuilder
    {
        private Int32 mainFunctionIndex;

        private readonly List<AssemblyFunctionBuilder> functionList;

        public Boolean inFunction;
        public AssemblyFunctionBuilder currentFunction;

        private UInt64 currentFunctionGlobalByteCodeOffset;

        public AssemblyBuilder()
        {
            this.mainFunctionIndex = -1;
            this.functionList = new List<AssemblyFunctionBuilder>();

            this.inFunction = false;
            this.currentFunctionGlobalByteCodeOffset = 0;
        }

        public void CurrentFunctionIsMain()
        {
            Debug.Assert(functionList.Count > 0);

            if (!inFunction) throw new InvalidOperationException("Not in a function");
            if (mainFunctionIndex >= 0) throw new InvalidOperationException("You've already set a main function");

            this.mainFunctionIndex = functionList.Count - 1;
            currentFunction.isMainFunction = true;
        }

        public void EndOfFile()
        {
            if (inFunction) currentFunction.End();
        }

        public void NewFunction()
        {
            if (inFunction)
            {
                currentFunction.End();
                this.currentFunctionGlobalByteCodeOffset += currentFunction.currentByteCodeOffset;
            }

            currentFunction = new AssemblyFunctionBuilder(currentFunctionGlobalByteCodeOffset);
            inFunction = true;
            functionList.Add(currentFunction);
        }

        public void Output(Stream output)
        {
            Console.WriteLine();
            Console.WriteLine("Outputing Machine Code...");

            if (mainFunctionIndex < 0)
            {
                throw new InvalidOperationException("No main function has been defined");
            }

            //
            // Executable Format
            //
            // [val Execution Start Address]
            //
            // [ExecutionByteCode]
            // 


            //
            // 1. Output MetaData
            //
            ByteCode.EmitMetadataUInt64(output, functionList[mainFunctionIndex].globalByteCodeOffset);

            //
            // 2. Output Executable
            //
            for (int i = 0; i < functionList.Count; i++)
            {
                functionList[i].Output(output);
            }
        }

        public void Print()
        {
            for (int i = 0; i < functionList.Count; i++)
            {
                functionList[i].Print();
            }
        }
        

        public class AssemblyFunctionBuilder
        {
            public readonly UInt64 globalByteCodeOffset;

            public Boolean isMainFunction;
            public UInt64 currentByteCodeOffset;
            private UInt64 currentStackFrameOffset;

            public Dictionary<String, UInt64> frameOffsetLabels;

            public Boolean includeException;
            public Boolean returnPointerIsTable;
            public UInt32 returnPointersSize;

            private readonly List<Instruction> instructionList;
            public readonly Dictionary<String, UInt64> instructionLabels;
            public Boolean ended;

            public AssemblyFunctionBuilder(UInt64 globalByteCodeOffset)
            {
                this.globalByteCodeOffset = globalByteCodeOffset;

                this.isMainFunction = false;
                this.currentByteCodeOffset = 0;
                this.currentStackFrameOffset = 0;


                this.frameOffsetLabels = null;

                this.includeException = false;
                this.returnPointerIsTable = false;
                this.returnPointersSize = 1;

                this.instructionList = new List<Instruction>();
                this.instructionLabels = new Dictionary<String, UInt64>();

                this.ended = false;
            }

            public void IncludeException()
            {
                if (includeException) throw new InvalidOperationException("You've already called IncludeException() on this function");
                if (currentStackFrameOffset != 0 || frameOffsetLabels != null || returnPointerIsTable)
                {
                    throw new InvalidOperationException("IncludeException must be the first thing done to the frame");
                }

                includeException = true;
                currentStackFrameOffset++;
            }            

            public void AddFrameOffsetLabel(String frameOffsetLabel, UInt64 size)
            {
                if (frameOffsetLabels == null)
                {
                    frameOffsetLabels = new Dictionary<String, UInt64>();
                }
                else if (frameOffsetLabels.ContainsKey(frameOffsetLabel))
                {
                    throw new InvalidOperationException(String.Format("'{0}' is already a frame variable", frameOffsetLabel));
                }

                frameOffsetLabels[frameOffsetLabel] = currentStackFrameOffset;
                this.currentStackFrameOffset += size;
            }

            public void FrameAllocateVariable(UInt64 size)
            {
                this.currentStackFrameOffset += size;
            }

            public void SetReturnPointerSettings(Boolean isTable, UInt32 size)
            {
                if (size <= 1) throw new ArgumentOutOfRangeException("size");
                if (this.returnPointersSize != 1) throw new InvalidOperationException(String.Format(
                     "You've already set the return pointer settings (isTable = {0}, returnPointersSize = {1})",
                    this.returnPointerIsTable, this.returnPointersSize));

                this.returnPointerIsTable = isTable;
                this.returnPointersSize = size;
            }

            public void AddInstruction(Instruction instruction)
            {
                if (instruction.label != null)
                {
                    if (instructionLabels.ContainsKey(instruction.label))
                    {
                        throw new InvalidOperationException(String.Format("You alread added the label '{0}'", instruction.label));
                    }
                    instructionLabels.Add(instruction.label, currentByteCodeOffset);
                }
                this.instructionList.Add(instruction);
                this.currentByteCodeOffset += instruction.byteLength;
            }

            public void End()
            {
                this.ended = true;
            }

            public void Output(Stream output)
            {
                Console.WriteLine("Outputing Function...");
                for (int i = 0; i < instructionList.Count; i++)
                {
                    Instruction instruction = instructionList[i];
                    instruction.Emit(output);
                }

                if (isMainFunction)
                {
                    output.WriteByte(Instructions.Halt);
                }
            }

            public void Print()
            {
                Console.WriteLine("func");
                if (includeException)
                {
                    Console.WriteLine("\t.exception");
                }

                Console.WriteLine("// Frame Stack Length: {0}", currentStackFrameOffset);
                if (frameOffsetLabels != null)
                {
                    foreach (KeyValuePair<String, UInt64> pair in frameOffsetLabels)
                    {
                        Console.WriteLine("// {0}: Frame Offset: {1}", pair.Key, pair.Value);
                    }
                }

                if (returnPointerIsTable || returnPointersSize > 1)
                {
                    Console.WriteLine("\t.return{0} {1}", returnPointerIsTable ? "table" : "", returnPointersSize);
                }

                for (int i = 0; i < instructionList.Count; i++)
                {
                    Instruction instruction = instructionList[i];
                    if (instruction.label != null)
                    {
                        Console.WriteLine("  {0}:", instruction.label);
                    }
                    Console.WriteLine("\t{0}", instruction);
                }
            }
        }

    }


    public struct AddressAndFrameOffset
    {
        public readonly UInt64 address, frameOffset;
        public AddressAndFrameOffset(UInt64 address, UInt64 frameOffset)
        {
            this.address = address;
            this.frameOffset = frameOffset;
        }
    }
    public struct OffsetAndLength
    {
        public readonly UInt64 offset,length;
        public OffsetAndLength(UInt64 offset, UInt64 length)
        {
            this.offset = offset;
            this.length = length;
        }
    }

    public abstract class Instruction
    {
        public readonly String label;
        public readonly Byte instruction;
        public readonly Byte byteLength;

        public Instruction(String label, Byte instruction, Byte byteLength)
        {
            this.label = label;
            this.instruction = instruction;
            this.byteLength = byteLength;
        }

        public abstract void Emit(Stream stream);
        //public abstract UInt64 Emit(byte[] byteCode, UInt64 offset);
    }

    public class InstructionMemoryOpAndOp : Instruction
    {
        public readonly MemoryOp memoryOp;
        public readonly Op op;

        public InstructionMemoryOpAndOp(String label, Byte instruction, MemoryOp memoryOp, Op op)
            : base(label, instruction, (Byte)(1 + memoryOp.byteLength + op.byteLength))
        {
            this.memoryOp = memoryOp;
            this.op = op;
        }

        public override void Emit(Stream stream)
        {
            Byte[] emitCode = new Byte[byteLength];
            emitCode[0] = instruction;
            UInt64 offset = memoryOp.Emit(emitCode, 1);
            offset = op.Emit(emitCode, offset);
            stream.Write(emitCode, 0, emitCode.Length);
        }

        public UInt64 Emit(Byte[] byteCode, UInt64 offset)
        {
            byteCode[offset++] = instruction;
            offset = memoryOp.Emit(byteCode, offset);
            return op.Emit(byteCode, offset);
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2}", Instructions.ToString(instruction),
                memoryOp, op);
        }
    }

    public class InstructionMemoryOpAndOpAndOp : Instruction
    {
        public readonly MemoryOp memoryOp;
        public readonly Op op1;
        public readonly Op op2;

        public InstructionMemoryOpAndOpAndOp(String label, Byte instruction, MemoryOp memoryOp, Op op1, Op op2)
            : base(label, instruction, (Byte)(1 + memoryOp.byteLength + op1.byteLength + op2.byteLength))
        {
            this.memoryOp = memoryOp;
            this.op1 = op1;
            this.op2 = op2;
        }

        public override void Emit(Stream stream)
        {
            Byte[] emitCode = new Byte[byteLength];
            emitCode[0] = instruction;
            UInt64 offset = memoryOp.Emit(emitCode, 1);
            offset = op1.Emit(emitCode, offset);
            offset = op2.Emit(emitCode, offset);
            stream.Write(emitCode, 0, emitCode.Length);
        }

        public UInt64 Emit(Byte[] byteCode, UInt64 offset)
        {
            byteCode[offset++] = instruction;
            offset = memoryOp.Emit(byteCode, offset);
            offset = op1.Emit(byteCode, offset);
            return op2.Emit(byteCode, offset);
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2} {3}", Instructions.ToString(instruction),
                memoryOp, op1, op2);
        }
    }




}
