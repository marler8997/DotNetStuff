using System;
using System.Collections.Generic;
using System.Text;

namespace More
{
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
        public readonly String idOriginalCase, idLowerInvariantCase;
        public readonly String [] fields;

        public readonly UInt32 actualLineNumber;

        public LfdLine(LfdLine parent, String comment, UInt32 actualLineNumber)
        {
            this.parent = parent;
            this.idOriginalCase = comment;
            this.idLowerInvariantCase = null;
            this.fields = null;

            this.actualLineNumber = actualLineNumber;
        }
        public LfdLine(LfdLine parent, String id, String[] fields, UInt32 actualLineNumber)
        {
            if (id == null) throw new ArgumentNullException("id");

            this.parent = parent;
            this.idOriginalCase = id;
            this.idLowerInvariantCase = id.ToLowerInvariant();

            this.fields = fields;

            this.actualLineNumber = actualLineNumber;
        }
        public Boolean IsComment()
        {
            return idLowerInvariantCase == null;
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

            if (parents.Count <= 1) return idLowerInvariantCase;

            // Pop off the root
            parents.Pop();

            StringBuilder stringBuilder = new StringBuilder();
            while (parents.Count > 0)
            {
                stringBuilder.Append(parents.Pop().idLowerInvariantCase);
                stringBuilder.Append('.');
            }
            stringBuilder.Append(idLowerInvariantCase);
            return stringBuilder.ToString();
        }
        public override String ToString()
        {
            if(fields == null || fields.Length <= 0) return idOriginalCase;
            
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(idOriginalCase);
            stringBuilder.Append(' ');
            for(int i = 0; i < fields.Length - 1; i++)
            {
                stringBuilder.Append(fields[i]);
                stringBuilder.Append(' ');
            }
            stringBuilder.Append(fields[fields.Length - 1]);
            return stringBuilder.ToString();
        }
    }
}
