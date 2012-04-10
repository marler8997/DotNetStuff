using System;
using System.IO;
using System.Text;
using System.Globalization;

namespace Marler.RuntimeAnalyzer
{
    public enum GlobalTokenType
    {
        Label, Command, DotCommand, EOF
    }

    public struct GlobalToken
    {
        public readonly GlobalTokenType type;
        public readonly UInt32 line;
        public readonly String text;

        public GlobalToken(GlobalTokenType type, UInt32 line)
        {
            this.type = type;
            this.line = line;
            this.text = null;
        }
        public GlobalToken(GlobalTokenType type, UInt32 line, String text)
        {
            this.type = type;
            this.line = line;
            this.text = text;
        }
        public override string ToString()
        {
            return String.Format("[GlobalToken: type='{0}', line={1}{2}]",
                type, line, (text == null) ? String.Empty : String.Format(", text=\"{0}\"", text));
        }
    }

    public enum OperandTokenType
    {
        Literal = 0, Label = 1, FramePointer = 2, DataPointer = 3
    }

    public class OperandToken
    {
        public readonly OperandTokenType type;

        public readonly Boolean isAddressOf;
        public readonly Boolean isDereference;
        public readonly UInt64 op;

        public readonly UInt32 line;
        public readonly String text;

        public OperandToken(OperandTokenType type, Boolean isAddressOf,
            Boolean isDereference, UInt64 op, UInt32 line, String text)
        {
            this.type = type;

            this.isAddressOf = isAddressOf;
            this.isDereference = isDereference;
            this.op = op;

            this.line = line;
            this.text = text;
        }

        public MemoryOp GetMemoryOp(AssemblyBuilder.AssemblyFunctionBuilder currentFunction)
        {
            switch (type)
            {
                case OperandTokenType.Literal:
                    if (isAddressOf) throw new InvalidOperationException();
                    if (!isDereference) throw new InvalidOperationException("You must dereference a literal (add '(#num)') if you are writing to it");

                    return new MemoryOp(Operands.AddressInfoMask, op);
                case OperandTokenType.Label:
                    Boolean isFrameOffset = true;
                    UInt64 labelOp;
                    if (!currentFunction.frameOffsetLabels.TryGetValue(text, out labelOp))
                    {
                        //
                        // TODO: Add check for dataOffset.  If it is true, then set isFrameOffset to false
                        //
                        throw new FormatException(String.Format("For Op {0}, could not find label '{1}'",
                            ToString(), text));
                    }

                    if (isAddressOf)
                        throw new InvalidOperationException("For a memory op, you can't use address of ('&') on a label because it is not writable");

                    if (isDereference)
                    {
                        return new MemoryOp(isFrameOffset ? Operands.FrameOffsetDereferenceInfoMask : Operands.AddressDereferenceInfoMask,
                            labelOp + op);
                    }
                    return new MemoryOp(isFrameOffset ? Operands.FrameOffsetInfoMask : Operands.AddressInfoMask, labelOp);

                case OperandTokenType.FramePointer:
                    if (isAddressOf) throw new InvalidOperationException();
                    if (!isDereference) throw new NotImplementedException("Right now, the frame pointer must be dereferenced");

                    return new MemoryOp(Operands.FrameOffsetInfoMask, op);
                case OperandTokenType.DataPointer:
                    if (isAddressOf) throw new InvalidOperationException();
                    if (!isDereference) throw new NotImplementedException("Right now, the data pointer must be dereferenced");

                    return new MemoryOp(Operands.AddressInfoMask, op);
                default:
                    throw new InvalidOperationException(String.Format("Toke Type {0} unrecognized", type));
            }
        }

        public Op GetOp(AssemblyBuilder.AssemblyFunctionBuilder currentFunction)
        {
            switch (type)
            {
                case OperandTokenType.Literal:
                    if (isAddressOf) throw new InvalidOperationException();


                    return new Op(isDereference ? Operands.AddressInfoMask : Operands.LiteralInfoMask, op);
                case OperandTokenType.Label:
                    Boolean isFrameOffset = true;
                    UInt64 labelOp;
                    if (!currentFunction.frameOffsetLabels.TryGetValue(text, out labelOp))
                    {
                        //
                        // TODO: Add check for dataOffset.  If it is true, then set isFrameOffset to false
                        //
                        throw new FormatException(String.Format("For Op {0}, could not find label '{1}'",
                            ToString(), text));
                    }
                    if (isAddressOf)
                    {
                        throw new NotImplementedException();
                    }

                    if (isDereference)
                    {
                        return new Op(isFrameOffset ? Operands.FrameOffsetDereferenceInfoMask : Operands.AddressDereferenceInfoMask,
                            labelOp + op);
                    }
                    return new Op(isFrameOffset ? Operands.FrameOffsetInfoMask : Operands.AddressInfoMask,
                        labelOp);

                case OperandTokenType.FramePointer:
                    if (isAddressOf) throw new InvalidOperationException();
                    if (!isDereference) throw new NotImplementedException("Right now, the frame pointer must be dereferenced");

                    return new Op(Operands.FrameOffsetInfoMask, op);
                case OperandTokenType.DataPointer:
                    if (isAddressOf) throw new InvalidOperationException();
                    if (!isDereference) throw new NotImplementedException("Right now, the data pointer must be dereferenced");

                    return new Op(Operands.AddressInfoMask, op);
                default:
                    throw new InvalidOperationException(String.Format("Toke Type {0} unrecognized", type));
            }
        }

        public Op GetJumpOp(AssemblyBuilder.AssemblyFunctionBuilder currentFunction)
        {
            switch (type)
            {
                case OperandTokenType.Literal:
                    if (isAddressOf) throw new InvalidOperationException();


                    return new Op(isDereference ? Operands.AddressInfoMask : Operands.LiteralInfoMask, op);
                case OperandTokenType.Label:
                    Boolean isFrameOffset = true;
                    UInt64 labelOp;
                    if (!currentFunction.instructionLabels.TryGetValue(text, out labelOp))
                    {
                        //
                        // TODO: Add check for dataOffset.  If it is true, then set isFrameOffset to false
                        //
                        throw new FormatException(String.Format("For Op {0}, could not find instruction label '{1}'",
                            ToString(), text));
                    }
                    if (isAddressOf)
                    {
                        throw new NotImplementedException();
                    }

                    if (isDereference)
                    {
                        return new Op(isFrameOffset ? Operands.FrameOffsetDereferenceInfoMask : Operands.AddressDereferenceInfoMask,
                            labelOp + op);
                    }
                    return new Op(isFrameOffset ? Operands.FrameOffsetInfoMask : Operands.AddressInfoMask,
                        labelOp);

                case OperandTokenType.FramePointer:
                    throw new InvalidOperationException();
                case OperandTokenType.DataPointer:
                    throw new InvalidOperationException();
                default:
                    throw new InvalidOperationException(String.Format("Toke Type {0} unrecognized", type));
            }
        }

        public override string ToString()
        {
            return String.Format("[OperandToken: type='{0}', isAddressOf={1}, isDereference={2}, op=0x{3:X}, line={4}, text='{5}']",
                type, isAddressOf, isDereference, op, line, text);
        }
    }


    public class AssemblyTokenizer
    {
        private readonly StringBuilder stringBuilder;

        protected TextReader reader;
        private Int32 next;
        protected UInt32 currentLineNumber;

        public UInt32 Line { get { return currentLineNumber; } }

        public AssemblyTokenizer()
        {
            this.reader = null;
            this.stringBuilder = new StringBuilder();
        }


        public void SetStream(TextReader reader, UInt32 startingLineNumber)
        {
            this.currentLineNumber = startingLineNumber;
            if (this.reader == null)
            {
                next = reader.Read();
            }
            this.reader = reader;
        }


        public FormatException FormatError(String message)
        {
            return new FormatException(String.Format("Tokenizer line {0}: {1}", currentLineNumber, message));
        }
        public FormatException FormatError(String fmt, params Object[] obj)
        {
            return FormatError(String.Format(fmt, obj));
        }

        private void ToNextInterestingCharacter()
        {
            // this method assumes the next variable already contains the next byte
            while (true)
            {
                if ((Char)next == '/')
                {
                    next = reader.Read();
                    if (next < 0)
                    {
                        throw FormatError("Expected '/' or '*' but got EOF");
                    }

                    if ((Char)next == '/')
                    {
                        // Single line comment: Go to the next '\n' or EOF
                        while (true)
                        {
                            next = reader.Read();
                            if (next < 0) { return; }
                            if ((Char)next == '\n')
                            {
                                currentLineNumber++;
                                next = reader.Read();
                                break;
                            }
                        }
                    }
                    else if ((Char)next == '*')
                    {
                        // Multi line comment: Go until you see "*/"
                        while (true)
                        {
                            next = reader.Read();
                            if (next < 0)
                            {
                                throw FormatError("Expected end of multiline comment \"*/\", but got EOF");
                            }
                            else if ((Char)next == '*')
                            {
                                next = reader.Read();
                                if (next < 0)
                                {
                                    throw FormatError("Expected end of multiline comment \"*/\", but got EOF");
                                }
                                else if ((Char)next == '/')
                                {
                                    next = reader.Read();
                                    break;
                                }
                            }
                            else if ((Char)next == '\n')
                            {
                                currentLineNumber++;
                            }

                        }
                    }
                    else
                    {
                        throw FormatError("Expected '/' or '*' but got '{0}'", (Char)next);
                    }
                }
                else if (Char.IsWhiteSpace((Char)next))
                {
                    if ((Char)next == '\n')
                    {
                        currentLineNumber++;
                    }
                    next = reader.Read();
                    if (next < 0)
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }

        //
        // This method assumes that the next variable DOES NOT contain the next byte to be read
        //
        public GlobalToken NextGlobalToken()
        {
            ToNextInterestingCharacter();

            //
            // Make sure you're not at the end of the file
            //
            if (next < 0) return new GlobalToken(GlobalTokenType.EOF, currentLineNumber);


            //
            // Check if it's a Frame command (begins with '.')
            //
            if ((Char)next == '.')
            {
                next = reader.Read();
                if (next < 0) throw FormatError("At '.', expected a lower-case Frame command but got EOF");

                stringBuilder.Length = 0;
                while ((Char)next >= 'a' && (Char)next <= 'z')
                {
                    stringBuilder.Append((Char)next);
                    next = reader.Read();
                }
                if (stringBuilder.Length <= 0) throw FormatError(
                     "Found a format command beginning with '.', expected a lower-case command but got '{0}'", (Char)next);
                return new GlobalToken(GlobalTokenType.DotCommand, currentLineNumber, stringBuilder.ToString());
            }


            //
            // Check if it's a Label or a Command
            //
            if ((next >= 'a' && next <= 'z') || (next >= 'A' && next <= 'Z'))
            {
                stringBuilder.Length = 0;

                do
                {
                    stringBuilder.Append((Char)next);
                    next = reader.Read();
                }
                while (((Char)next >= 'a' && (Char)next <= 'z') ||
                        ((Char)next >= 'A' && (Char)next <= 'Z') ||
                        ((Char)next >= '0' && (Char)next <= '9'));

                ToNextInterestingCharacter();

                if ((Char)next == ':')
                {
                    next = reader.Read();
                    return new GlobalToken(GlobalTokenType.Label, currentLineNumber, stringBuilder.ToString());
                }

                return new GlobalToken(GlobalTokenType.Command, currentLineNumber, stringBuilder.ToString());
            }

            throw FormatError("NextGlobalToken: Unexpected character '{0}'", (Char)next);
        }

        public OperandToken NextOperand()
        {
            Boolean isAddressOf = false;

            ToNextInterestingCharacter();

            //
            // Make sure you're not at the end of the file
            //
            if (next < 0) throw FormatError("NextOperand: Unexpected EOF");

            if ((Char)next == '&')
            {
                isAddressOf = true;
                next = reader.Read();
                if (next < 0) throw FormatError("NextOperand: Unexpected EOF");
            }

            OperandTokenType tokenType = OperandTokenType.Label;
            String opString = null;

            if (next == '-' || (next >= '0' && next <= '9'))
            {
                tokenType = OperandTokenType.Literal;

                stringBuilder.Length = 0;

                do
                {
                    stringBuilder.Append((Char)next);
                    next = reader.Read();
                } while (((Char)next >= '0' && (Char)next <= '9') ||
                    ((Char)next >= 'A' && (Char)next <= 'F') ||
                    ((Char)next >= 'a' && (Char)next <= 'f') ||
                    (Char)next == 'x' || (Char)next == 'b');

                if (stringBuilder.Length <= 1 && stringBuilder[0] == '-') throw FormatError("'-' is not a valid operand");
                opString = stringBuilder.ToString();
            }

            if (tokenType != OperandTokenType.Literal)
            {
                //
                // Check if it's a Label or a Command
                //
                if (((Char)next >= 'a' && (Char)next <= 'z') || ((Char)next >= 'A' && (Char)next <= 'Z'))
                {
                    stringBuilder.Length = 0;

                    do
                    {
                        stringBuilder.Append((Char)next);
                        next = reader.Read();
                    }
                    while (((Char)next >= 'a' && (Char)next <= 'z') ||
                            ((Char)next >= 'A' && (Char)next <= 'Z') ||
                            ((Char)next >= '0' && (Char)next <= '9'));


                    if (stringBuilder.Length == 2)
                    {
                        if (stringBuilder[1] == 'p')
                        {
                            if (stringBuilder[0] == 'f') tokenType = OperandTokenType.FramePointer;
                            else if (stringBuilder[1] == 'd') tokenType = OperandTokenType.DataPointer;
                        }
                    }
                    opString = stringBuilder.ToString();
                }
                else
                {
                    throw FormatError("Expected Operand but could not recognize character '{0}' (0x{1:X}) ({1})", (Char)next, next);
                }
            }

            //
            // Check for dereference
            //
            Boolean isDereference = false;
            UInt64 dereferenceOffset = 0;
            if (next == '(')
            {
                isDereference = true;
                next = reader.Read();
                if (next < 0) throw new FormatException("Expected ')' or '0-9' but got EOF");

                if (next == ')')
                {
                    next = reader.Read();
                    return new OperandToken(tokenType, isAddressOf, true, 0, currentLineNumber, opString);
                }

                if ((next >= '0' && next <= '9'))
                {
                    stringBuilder.Length = 0;

                    do
                    {
                        stringBuilder.Append((Char)next);
                        next = reader.Read();
                        if (next < 0) throw FormatError("Expected ')' but got EOF");
                    } while (next != ')');

                    next = reader.Read();

                    // TODO: Need to parse dereference offset
                    dereferenceOffset = UInt64.Parse(stringBuilder.ToString());
                }
                else
                {
                    throw FormatError("After '(' expected ')' or '0-9' but got '{0}'", (Char)next);
                }
            }


            if (tokenType == OperandTokenType.Literal)
            {
                UInt64 literal = UInt64.Parse(opString);
                return isDereference ? new OperandToken(tokenType, isAddressOf, isDereference, literal + dereferenceOffset, currentLineNumber, opString) :
                    new OperandToken(tokenType, isAddressOf, false, literal, currentLineNumber, opString);
            }
            return new OperandToken(tokenType, isAddressOf, isDereference, dereferenceOffset, currentLineNumber, opString);


            throw FormatError("NextGlobalToken: Unexpected character '{0}'", (Char)next);
        }

        public Int32 GetNumber()
        {
            ToNextInterestingCharacter();

            if (next < 0) throw FormatError("GetNumber: Unexpected EOF");

            Boolean isNegative = false;
            NumberStyles parseNumberStyle = NumberStyles.None;

            if ((Char)next == '-')
            {
                isNegative = true;
                next = reader.Read();
                if (next < 0) throw FormatError("GetNumber: Unexpected EOF");
            }

            if ((Char)next == '0')
            {
                next = reader.Read();
                if (next < 0) return 0;
                if (next != 'x') throw FormatError("GetNumber: Found '0', expected 'x' but got '{0}'", (Char)next);
                parseNumberStyle = NumberStyles.HexNumber;
                next = reader.Read();
                if (next < 0) throw FormatError("GetNumber: Unexpected EOF");
            }

            stringBuilder.Length = 0;
            stringBuilder.Append(next);
            while ((Char)next >= '0' && (Char)next <= '9')
            {
                next = reader.Read();
            }

            if (stringBuilder.Length <= 0) FormatError("GetNumber: Expected '0-9', but got '{0}'", (Char)next);

            return isNegative ? -Int32.Parse(stringBuilder.ToString(), parseNumberStyle) :
                Int32.Parse(stringBuilder.ToString(), parseNumberStyle);
        }

        public UInt64 GetUnsignedNumber()
        {
            ToNextInterestingCharacter();

            if (next < 0) throw FormatError("GetNumber: Unexpected EOF");

            NumberStyles parseNumberStyle = NumberStyles.None;

            if ((Char)next == '0')
            {
                next = reader.Read();
                if (next < 0) return 0;
                if (Char.IsWhiteSpace((Char)next)) return 0;
                if (next != 'x') throw FormatError("GetNumber: Found '0', expected 'x' but got '{0}'", (Char)next);
                parseNumberStyle = NumberStyles.HexNumber;
                next = reader.Read();
                if (next < 0) throw FormatError("GetNumber: Unexpected EOF");
            }

            stringBuilder.Length = 0;

            while ((Char)next >= '0' && (Char)next <= '9')
            {
                stringBuilder.Append((Char)next);
                next = reader.Read();
            }

            if (stringBuilder.Length <= 0) FormatError("GetNumber: Expected '0-9', but got '{0}'", (Char)next);

            //Console.WriteLine("Unsigned Number {0}", stringBuilder.ToString());
            return UInt64.Parse(stringBuilder.ToString(), parseNumberStyle);
        }



        //
        // This method assumes that the next variable DOES NOT contain the next byte to be read
        //
        public String NextValue()
        {
            next = reader.Read();

            stringBuilder.Length = 0;
            while (true)
            {
                if (next < 0)
                {
                    throw new FormatException(
                        String.Format("NextValueToken (line {0}): expected a ';', but got EOF", currentLineNumber));
                }
                if ((Char)next == '\n')
                {
                    throw new FormatException(
                        String.Format("NextValueToken (line {0}): expected a ';', but got '\\n'", currentLineNumber));
                }
                if ((Char)next == ';')
                {
                    return stringBuilder.ToString();
                    //return new Token(TokenType.Value, currentLineNumber, stringBuilder.ToString());
                }
                stringBuilder.Append((Char)next);
                next = reader.Read();
            }
        }


    }
}
