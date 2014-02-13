using System;
using System.Collections.Generic;
using System.Text;

namespace More
{
    public class LfdFormatException : FormatException
    {
        public readonly UInt32 lineNumber;
        public readonly String line;

        public LfdFormatException(UInt32 lineNumber, String line, String msg)
            : base(String.Format("Line {0} \"{1}\" : {2}", lineNumber, line, msg))
        {
        }
    }
    //public delegate void LfdConfigHandler(ILineReader reader, List<String> fields);
    public delegate void LfdLineConfigHandler(LfdLine line);
    public delegate LfdLine LfdConfigHandler(LfdReader reader, LfdLine line);
    public class LfdConfiguration
    {
        readonly Dictionary<String, LfdLineConfigHandler> lineParsers = new Dictionary<String, LfdLineConfigHandler>();
        readonly Dictionary<String, LfdConfigHandler>     readerParsers = new Dictionary<String, LfdConfigHandler>();

        public void Add(String name, LfdLineConfigHandler handler)
        {
            lineParsers.Add(name.ToLowerInvariant(), handler);
        }
        public void AddWithNameLowerInvariant(String nameLowerInvarient, LfdLineConfigHandler handler)
        {
            lineParsers.Add(nameLowerInvarient, handler);
        }

        public void Add(String name, LfdConfigHandler handler)
        {
            readerParsers.Add(name.ToLowerInvariant(), handler);
        }
        public void AddWithNameLowerInvariant(String nameLowerInvarient, LfdConfigHandler handler)
        {
            readerParsers.Add(nameLowerInvarient, handler);
        }

        public LfdLine Handle(LfdReader reader, LfdLine parentLine)
        {
            LfdLine line = reader.ReadLineIgnoreComments();
            while (true)
            {
                if (line == null) return null;
                if (line.parent != parentLine) return line;

                LfdLineConfigHandler lineConfigHandler;
                LfdConfigHandler readerConfigHandler;
                if (lineParsers.TryGetValue(line.idLowerInvariant, out lineConfigHandler))
                {
                    lineConfigHandler(line);
                    line = reader.ReadLineIgnoreComments();
                }
                else if (readerParsers.TryGetValue(line.idLowerInvariant, out readerConfigHandler))
                {
                    line = readerConfigHandler(reader, line);
                }
                else
                {
                    throw new FormatException(String.Format("Unknown config name '{0}'", line.idOriginalCase));
                }
            }
        }
        public void Parse(LfdReader reader)
        {
            LfdLine line = reader.ReadLineIgnoreComments();
            while (true)
            {
                if (line == null) break;

                LfdLineConfigHandler lineConfigHandler;
                LfdConfigHandler readerConfigHandler;
                if (lineParsers.TryGetValue(line.idLowerInvariant, out lineConfigHandler))
                {
                    lineConfigHandler(line);
                    line = reader.ReadLineIgnoreComments();
                }
                else if (readerParsers.TryGetValue(line.idLowerInvariant, out readerConfigHandler))
                {
                    line = readerConfigHandler(reader, line);
                }
                else
                {
                    throw new FormatException(String.Format("Unknown config name '{0}'", line.idOriginalCase));
                }
            }
        }
    }
    public class LfdLine
    {
        public static void ParseLine(List<String> fields, Byte[] line, Int32 offset, Int32 offsetPlusLength)
        {
            while (true)
            {
                Int32 nextFieldStart = offset;

                Byte c;

                //
                // Skip whitespace
                //
                while (true)
                {
                    if (nextFieldStart >= offsetPlusLength) return;
                    c = line[nextFieldStart];
                    if (c != ' ' && c != '\t') break;
                    nextFieldStart++;
                }

                if (c != '"')
                {
                    offset = nextFieldStart + 1;
                    while (true)
                    {
                        if (offset >= offsetPlusLength)
                        {
                            fields.Add(Encoding.UTF8.GetString(line, nextFieldStart, offset - nextFieldStart));
                            return;
                        }
                        c = line[offset];
                        if (c == ' ' || c == '\t')
                        {
                            fields.Add(Encoding.UTF8.GetString(line, nextFieldStart, offset - nextFieldStart));
                            nextFieldStart = offset + 1;
                            break;
                        }
                        offset++;
                    }
                }
                else
                {
                    nextFieldStart++;
                    if (nextFieldStart >= offsetPlusLength)
                    {
                        fields.Add(String.Empty);
                    }

                    offset = nextFieldStart + 1;
                    while (true)
                    {
                        if (offset >= offsetPlusLength)
                        {
                            fields.Add(Encoding.UTF8.GetString(line, nextFieldStart, offset - nextFieldStart));
                            return;
                        }
                        c = line[offset];
                        if (c == '"')
                        {
                            fields.Add(Encoding.UTF8.GetString(line, nextFieldStart, offset - nextFieldStart));
                            nextFieldStart = offset + 1;
                            break;
                        }
                        offset++;
                    }
                }
            }
        }
        public static void ParseLine(List<String> fields, String line, Int32 offset, Int32 length)
        {
            Int32 offsetPlusLength = offset + length;

            while (true)
            {
                Int32 nextFieldStart = offset;

                Char c;

                //
                // Skip whitespace
                //
                while (true)
                {
                    if (nextFieldStart >= offsetPlusLength) return;
                    c = line[nextFieldStart];
                    if (c != ' ' && c != '\t') break;
                    nextFieldStart++;
                }

                if (c != '"')
                {
                    offset = nextFieldStart + 1;
                    while (true)
                    {
                        if (offset >= offsetPlusLength)
                        {
                            fields.Add(line.Substring(nextFieldStart, offset - nextFieldStart));
                            return;
                        }
                        c = line[offset];
                        if (c == ' ' || c == '\t')
                        {
                            fields.Add(line.Substring(nextFieldStart, offset - nextFieldStart));
                            nextFieldStart = offset + 1;
                            break;
                        }
                        offset++;
                    }
                }
                else
                {
                    nextFieldStart++;
                    if (nextFieldStart >= offsetPlusLength)
                    {
                        fields.Add(String.Empty);
                    }

                    offset = nextFieldStart + 1;
                    while (true)
                    {
                        if (offset >= offsetPlusLength)
                        {
                            fields.Add(line.Substring(nextFieldStart, offset - nextFieldStart));
                            return;
                        }
                        c = line[offset];
                        if (c == '"')
                        {
                            fields.Add(line.Substring(nextFieldStart, offset - nextFieldStart));
                            nextFieldStart = offset + 1;
                            break;
                        }
                        offset++;
                    }
                }
            }
        }
        public readonly LfdLine parent;
        public readonly String idOriginalCase, idLowerInvariant;
        public readonly String[] fields;

        public readonly UInt32 actualLineNumber;

        public LfdLine(LfdLine parent, String comment, UInt32 actualLineNumber)
        {
            this.parent = parent;
            this.idOriginalCase = comment;
            this.idLowerInvariant = null;
            this.fields = null;

            this.actualLineNumber = actualLineNumber;
        }
        public LfdLine(LfdLine parent, String id, String[] fields, UInt32 actualLineNumber)
        {
            if (id == null) throw new ArgumentNullException("id");

            this.parent = parent;
            this.idOriginalCase = id;
            this.idLowerInvariant = id.ToLowerInvariant();

            this.fields = fields;

            this.actualLineNumber = actualLineNumber;
        }
        public Boolean IsComment()
        {
            return idLowerInvariant == null;
        }
        public String CreateContextString()
        {
            LfdLine currentParent = parent;
            Stack<LfdLine> parents = new Stack<LfdLine>();
            while (currentParent != null)
            {
                parents.Push(currentParent);
                currentParent = currentParent.parent;
            }

            if (parents.Count <= 1) return idLowerInvariant;

            // Pop off the root
            parents.Pop();

            StringBuilder stringBuilder = new StringBuilder();
            while (parents.Count > 0)
            {
                stringBuilder.Append(parents.Pop().idLowerInvariant);
                stringBuilder.Append('.');
            }
            stringBuilder.Append(idLowerInvariant);
            return stringBuilder.ToString();
        }
        public override String ToString()
        {
            if (fields == null || fields.Length <= 0) return idOriginalCase;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(idOriginalCase);
            stringBuilder.Append(' ');
            for (int i = 0; i < fields.Length - 1; i++)
            {
                stringBuilder.Append(fields[i]);
                stringBuilder.Append(' ');
            }
            stringBuilder.Append(fields[fields.Length - 1]);
            return stringBuilder.ToString();
        }
    }
}