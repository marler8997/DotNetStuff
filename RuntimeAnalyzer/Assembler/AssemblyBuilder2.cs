using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marler.RuntimeAnalyzer
{
    public class AssemblyBuilder2
    {
        private UInt64 startOffset;
        private readonly List<MachineInstruction> instructions;


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


    }
}
