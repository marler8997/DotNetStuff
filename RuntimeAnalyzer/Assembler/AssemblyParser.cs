using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace Marler.RuntimeAnalyzer
{

    /*
    public enum ParseState
    {
        StartOfFile,
        StartOfFrame,
        Frame,
    }
    */

    public class AssemblyParser
    {
        private delegate void ParseDotCommand(String label);    
        private delegate void ParseCommand(String label);    
        
        private readonly TextWriter debugOutput;
        private readonly AssemblyTokenizer tokenizer;
        private readonly Dictionary<String, ParseDotCommand> dotCommandFunctions;
        private readonly Dictionary<String, ParseCommand> commandFunctions;

        private AssemblyBuilder assemblyBuilder;

        public AssemblyParser(TextWriter debugOutput, AssemblyTokenizer tokenizer)
        {
            this.debugOutput = debugOutput;
            this.tokenizer = tokenizer;

            this.dotCommandFunctions = new Dictionary<String, ParseDotCommand>();
            this.dotCommandFunctions.Add("start", dotStart);
            //this.dotCommandFunctions.Add("stack", dotStack);
            //this.dotCommandFunctions.Add("return", dotReturn);
            //this.dotCommandFunctions.Add("returntable", dotReturnTable);

            this.commandFunctions = new Dictionary<String, ParseCommand>();
            this.commandFunctions.Add("MOV", MOV);
            this.commandFunctions.Add("JE", JE);
        }

        public AssemblyBuilder Parse()
        {
            assemblyBuilder = new AssemblyBuilder();

            while (true)
            {
                String currentLabel = null;
                GlobalToken globalToken = tokenizer.NextGlobalToken();

                if (globalToken.type == GlobalTokenType.EOF)
                {
                    Debug("EOF");
                    return assemblyBuilder;
                }

                if (globalToken.type == GlobalTokenType.Label)
                {
                    Debug("LABEL : '{0}'", globalToken.text);
                    if (!assemblyBuilder.inFunction) throw ParseError("A Label needs to be in a function");
                    currentLabel = globalToken.text;
                    
                    globalToken = tokenizer.NextGlobalToken();
                    if (globalToken.type == GlobalTokenType.EOF)                        
                        throw ParseError("Unexpected EOF after label '{0}'", currentLabel);
                    if (globalToken.type == GlobalTokenType.Label)
                        throw ParseError("Two Labels in a row '{0}' and '{1}'", currentLabel, globalToken.text);                
                }


                if (globalToken.type == GlobalTokenType.DotCommand)
                {
                    ParseDotCommand stateFunction;
                    if (!dotCommandFunctions.TryGetValue(globalToken.text, out stateFunction))
                    {
                        throw ParseError("Unknown Dot Command '{0}'", globalToken.text);
                    }
                    stateFunction(currentLabel);
                }
                else if (globalToken.type == GlobalTokenType.Command)
                {
                    ParseCommand stateFunction;
                    if (!commandFunctions.TryGetValue(globalToken.text, out stateFunction))
                    {
                        throw ParseError("Unknown Command '{0}'", globalToken.text);
                    }

                    stateFunction(currentLabel);
                }
                else
                {
                    throw ParseError("Uknown GlobalTokenType '{0}' ({1})", globalToken.type, (Int32)globalToken.type);
                }
                currentLabel = null;
            }
        }


        public void Debug(String msg)
        {
            if (debugOutput != null) debugOutput.WriteLine("LINE: {0} {1}", tokenizer.Line, msg);
        }
        public void Debug(String fmt, params Object[] obj)
        {
            Debug(String.Format(fmt, obj));
        }

        public FormatException ParseError(String msg)
        {
            return new FormatException(String.Format("LINE: {0} ParseError: {1}", tokenizer.Line, msg));
        }
        public FormatException ParseError(String fmt, params Object[] obj)
        {
            return ParseError(String.Format(fmt,obj));
        }

        public void dotStart(String label)
        {
            Debug(".start");

            if (!assemblyBuilder.inFunction) throw ParseError(".main command must be in a func");
            if (label != null) throw ParseError(".main can't be labeled");

            assemblyBuilder.CurrentFunctionIsMain();
            //parseState = ParseState.Frame;
        }

        /*
        public void dotMain(String label)
        {
            Debug(".main");

            if (!assembly.inFunction) throw ParseError(".main command must be in a func");
            if (label != null) throw ParseError(".main can't be labeled");

            assembly.CurrentFunctionIsMain();
            parseState = ParseState.Frame;
        }

        public void dotException(String label)
        {
            Debug(".exception");

            if (!assembly.inFunction) throw ParseError(".exception command must be in a func");
            if(parseState != ParseState.StartOfFrame) throw ParseError(".exception must be at the start of a FRAME");

            assembly.currentFunction.IncludeException();
            parseState = ParseState.Frame;
        }
        public void dotStack(String label)
        {
            Debug(".stack");
            UInt64 num = tokenizer.GetUnsignedNumber();

            if (!assembly.inFunction) throw ParseError(".stack command must be in a func");

            if (label == null)
            {
                assembly.currentFunction.FrameAllocateVariable(num);
            }
            else
            {
                assembly.currentFunction.AddFrameOffsetLabel(label, num);
            }
            parseState = ParseState.Frame;
        }
        public void dotReturn(String label)
        {
            Debug(".return");
            if (label != null) throw ParseError("A return statement can't have a Label?");

            UInt64 size = tokenizer.GetUnsignedNumber();

            if (!assembly.inFunction) throw ParseError(".return command must be in a func");
            assembly.currentFunction.SetReturnPointerSettings(false, (UInt32)size);
        }
        public void dotReturnTable(String label)
        {
            Debug(".returntable");
            if (label != null) throw ParseError("A return statement can't have a Label?");

            UInt64 size = tokenizer.GetUnsignedNumber();

            if (!assembly.inFunction) throw ParseError(".return-table command must be in a func");
            assembly.currentFunction.SetReturnPointerSettings(true, (UInt32)size);
        }

        public void func(String label)
        {
            Debug("func");
            if (label != null) throw ParseError("A func command can't have a label");

            assembly.NewFunction();
           

            parseState = ParseState.StartOfFrame;
        }
        */

        public void MOV(String label)
        {
            Debug("MOV");
            if (!assemblyBuilder.inFunction) throw ParseError("MOV Instruction must be in a func");

            OperandToken dstOperand = tokenizer.NextOperand();
            Debug("DST : {0}", dstOperand);
            OperandToken srcOperand = tokenizer.NextOperand();
            Debug("SRC : {0}", srcOperand);

            MemoryOp dstOp = dstOperand.GetMemoryOp(assemblyBuilder.currentFunction);

            Op srcOp = srcOperand.GetOp(assemblyBuilder.currentFunction);

            assemblyBuilder.currentFunction.AddInstruction(new InstructionMemoryOpAndOp(label, Instructions.Move,
                dstOp, srcOp));
        }

        public void JE(String label)
        {
            Debug("MOV");
            if (!assemblyBuilder.inFunction) throw ParseError("JE Instruction must be in a func");

            OperandToken condOperand1 = tokenizer.NextOperand();
            Debug("COND1 : {0}", condOperand1);
            OperandToken condOperand2 = tokenizer.NextOperand();
            Debug("COND2 : {0}", condOperand2);
            OperandToken jumpOperand = tokenizer.NextOperand();
            Debug("JMP  : {0}", jumpOperand);

            MemoryOp condOp1 = condOperand1.GetMemoryOp(assemblyBuilder.currentFunction);
            Op condOp2 = condOperand2.GetOp(assemblyBuilder.currentFunction);
            Op jumpOp = jumpOperand.GetJumpOp(assemblyBuilder.currentFunction);

            assemblyBuilder.currentFunction.AddInstruction(new InstructionMemoryOpAndOpAndOp(label, Instructions.Move,
                condOp1, condOp2, jumpOp));
        }




        public static Dictionary<String, UInt64> Pass1()
        {
            return null;
            /*
            while (true)
            {
                GlobalToken globalToken = tokenizer.NextGlobalToken();

                if (globalToken.type == GlobalTokenType.EOF)
                {
                    if (debugOutput != null) debugOutput.WriteLine("EOF");
                    return;
                }



            }
            */
        }
    

    }
}
