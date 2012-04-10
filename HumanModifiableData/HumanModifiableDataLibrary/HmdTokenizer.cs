using System;
using System.IO;
using System.Text;

namespace Marler.Hmd
{
    public enum HmdGlobalTokenType
    {
        ID, Directive, CloseBrace, EOF
    }

    public struct HmdGlobalToken
    {
        public readonly HmdGlobalTokenType type;
        public readonly UInt32 line;
        public readonly String text;

        public HmdGlobalToken(HmdGlobalTokenType type, UInt32 line)
        {
            this.type = type;
            this.line = line;
            this.text = null;
        }
        public HmdGlobalToken(HmdGlobalTokenType type, UInt32 line, String text)
        {
            this.type = type;
            this.line = line;
            this.text = text;
        }
        public override string ToString()
        {
            return String.Format("HmdGlobalToken: type='{0}', line={1}{2}",
                type, line, (text == null) ? String.Empty : String.Format(", text=\"{0}\"", text));
        }
    }

    public class HmdSingleIDTokenizer : HmdTokenizer
    {
        private Int32 blockLevel;
        private Boolean haveReadAFullID;

        public HmdSingleIDTokenizer(TextReader reader)
            : base(reader, 0)
        {
            this.blockLevel = 0;
            this.haveReadAFullID = false;
        }

        public void Reset()
        {
            this.blockLevel = 0;
            this.haveReadAFullID = false;
        }

        public void ReInitialize(TextReader reader)
        {
            this.reader = reader;
            this.blockLevel = 0;
            this.haveReadAFullID = false;
        }

        public override HmdGlobalToken NextGlobalToken()
        {
            if (haveReadAFullID) return new HmdGlobalToken(HmdGlobalTokenType.EOF, currentLineNumber);

            HmdGlobalToken token = base.NextGlobalToken();
            if (token.type == HmdGlobalTokenType.CloseBrace)
            {
                blockLevel--;


                if (blockLevel < 0) throw new FormatException("SingleTokenizer: Got an unmatched close brace");
                if (blockLevel == 0) this.haveReadAFullID = true;
            }
            return token;
        }

        public override Boolean NextIDType(out Boolean isBlockID)
        {
            Boolean isEmptyID = base.NextIDType(out isBlockID);
            if (isBlockID)
            {
                blockLevel++;
            }
            else
            {
                if (blockLevel <= 0)
                {
                    haveReadAFullID = true;
                }
            }
            return isEmptyID;
        }
    }

    public class HmdTokenizer
    {
        private readonly StringBuilder stringBuilder;

        protected TextReader reader;
        private Int32 next;
        protected UInt32 currentLineNumber;

        public HmdTokenizer(TextReader reader, UInt32 startingLineNumber)
        {
            this.stringBuilder = new StringBuilder();
            NewStream(reader, startingLineNumber);
        }

        public void NewStream(TextReader reader, UInt32 startingLineNumber)
        {
            this.reader = reader;
            this.currentLineNumber = startingLineNumber;
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
                if (Char.IsWhiteSpace((Char)next))
                {
                    if ((Char)next == '\n')
                    {
                        currentLineNumber++;
                    }
                    next = reader.Read();
                }
                else if ((Char)next == '/')
                {
                    next = reader.Read();

                    if ((Char)next == '/')
                    {
                        // Single line comment: Go to the next '\n' or EOF
                        while (true)
                        {
                            next = reader.Read();
                            if ((Char)next == '\n')
                            {
                                currentLineNumber++;
                                next = reader.Read();
                                break;
                            }
                            if (next < 0) { return; }
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
                            else if((Char)next == '\n')
                            {
                                currentLineNumber++;
                            }
                        }
                    }
                    else
                    {
                        throw FormatError("Expected '/' or '*' but got '{0}'", (next < 0) ? "EOF" : ((Char)next).ToString());
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
        public virtual HmdGlobalToken NextGlobalToken()
        {
            Boolean isDirective = false;

            next = reader.Read();
            ToNextInterestingCharacter();

            //
            // Make sure you're not at the end of the file
            //
            if (next < 0) { return new HmdGlobalToken(HmdGlobalTokenType.EOF, currentLineNumber); }

            //
            // Match a close brace
            //
            if ((Char)next == '}')
            {
                return new HmdGlobalToken(HmdGlobalTokenType.CloseBrace, currentLineNumber);
            }
            //
            // Match a '%'
            //
            if ((Char)next == '%')
            {
                next = reader.Read();
                if (next < 0) { throw FormatError("NextGlobalToken: expected an id after '%', but got EOF"); }
                isDirective = true;
            }

            //
            // Match an id
            //
            if (((Char)next >= 'a' && (Char)next <= 'z') ||
                ((Char)next >= 'A' && (Char)next <= 'Z') ||
                ((Char)next >= '0' && (Char)next <= '9'))
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
                
                // note that the next byte has already been read inside the loop
                return new HmdGlobalToken(isDirective ? HmdGlobalTokenType.Directive : HmdGlobalTokenType.ID, currentLineNumber, stringBuilder.ToString());
            }
            else if (isDirective)
            {
                if (next < 0) { throw FormatError("NextGlobalToken: expected an id after '%', but got '{0}'", (Char)next); }
            }
            
            throw FormatError("NextGlobalToken: unexpected character '{0}'", (Char)next);
        }
        
        public virtual Boolean NextIDType(out Boolean isBlockID)
        {
            // this method assumes that the next variable already contains the next character in the stream
            ToNextInterestingCharacter();

            //
            // Match a colon
            //
            if ((Char)next == ':') {isBlockID = false; return false;}
            if ((Char)next == '{') {isBlockID = true;  return false;}
            if ((Char)next == ';') {isBlockID = false; return true; }

            throw FormatError("NextIDType: expected ':', '{' or ';', but got '{0}'", (Char)next);
        }

        //
        // This method assumes that the next variable DOES NOT contain the next byte to be read
        //
        public String NextValue()
        {
            stringBuilder.Length = 0;
            while(true)
            {
                next = reader.Read();

                if((Char)next == '\\')
                {
                    next = reader.Read();
                    if (next == ';') stringBuilder.Append(';');
                    else if (next == '\\') stringBuilder.Append('\\');
                    else
                    {
                        if(next < 0) throw FormatError("NextValue: '\\EOF' is an invalid escape sequence");
                        throw FormatError(String.Format("NextValue: invalid escape sequence '\\{0}'", next));
                    }
                }
                else
                {
                    if ((Char)next == ';') return stringBuilder.ToString();
                    if (next < 0) throw FormatError("NextValue: expected ';', but got EOF");
                    if ((Char)next == '\n') throw FormatError("NextValue: expected ';', but got '\\n'");

                    stringBuilder.Append((Char)next);
                }
            }
        }
    }
}
