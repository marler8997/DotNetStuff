using System;
using System.Diagnostics;

namespace More
{
    public enum Utf8ExceptionType
    {
        StartedInsideCodePoint,
        MissingBytes,
        OutOfRange,
    }
    public class Utf8Exception : Exception
    {
        const String GenericMessage = "invalid utf8";
        const String StartedInsideCodePointMessage = "utf8 string started inside a utf8 code point";
        const String MissingBytesMessage = "utf8 encoding is missing some bytes";
        const String OutOfRangeMessage = "the utf8 code point is out of range";
        static String GetMessage(Utf8ExceptionType type)
        {
            switch(type)
            {
                case Utf8ExceptionType.StartedInsideCodePoint: return StartedInsideCodePointMessage;
                case Utf8ExceptionType.MissingBytes: return MissingBytesMessage;
                case Utf8ExceptionType.OutOfRange: return OutOfRangeMessage;
            }
            return GenericMessage;
        }
        public readonly Utf8ExceptionType type;
        public Utf8Exception(Utf8ExceptionType type) : base(GetMessage(type))
        {
            this.type = type;
        }
    }
    public static class Utf8
    {
        public static UInt32 Decode(Byte[] array, ref UInt32 offset, UInt32 limit)
        {
#if DEBUG
            //new SegmentByLimit(array, offset, limit); // Verifies that the arguments are valid
#endif

            if(offset >= limit) throw new ArgumentException("Cannot pass an empty data segment to Utf8.Decode");

            UInt32 c = array[offset];
            offset++;
            if(c <= 0x7F) {
                return c;
            }
            if((c & 0x40) == 0) {
                throw new Utf8Exception(Utf8ExceptionType.StartedInsideCodePoint);
            }

            if((c & 0x20) == 0) {
                if (offset >= limit) throw new Utf8Exception(Utf8ExceptionType.MissingBytes);
                return ((c << 6) & 0x7C0U) | (array[offset++] & 0x3FU);
            }

            if((c & 0x10) == 0) {
                offset++;
                if (offset >= limit) throw new Utf8Exception(Utf8ExceptionType.MissingBytes);
                return ((c << 12) & 0xF000U) | ((UInt32)(array[offset-1] << 6) & 0xFC0U) | (array[offset++] & 0x3FU);
            }

            if((c & 0x08) == 0) {
                offset += 2;
                if (offset >= limit) throw new Utf8Exception(Utf8ExceptionType.MissingBytes);
                return ((c << 18) & 0x1C0000U) | ((UInt32)(array[offset - 2] << 12) & 0x3F000U) |
                    ((UInt32)(array[offset - 1] << 6) & 0xFC0U) | (array[offset++] & 0x3FU);
            }

            throw new Utf8Exception(Utf8ExceptionType.OutOfRange);
        }

        // TODO: implement this correctly later
        public static Boolean IsUpper(UInt32 c)
        {
            return
                (c >= 'A' && c <= 'Z');
        }
        // TODO: implement this correctly later
        public static Boolean IsLower(UInt32 c)
        {
            return
                (c >= 'a' && c <= 'z');
        }
        // TODO: implement this correctly later
        public static UInt32 ToUpper(UInt32 c)
        {
            if (c >= 'a' && c <= 'z') return c - 'a' + 'A';
            return c;
        }
        // TODO: implement this correctly later
        public static UInt32 ToLower(UInt32 c)
        {
            if (c >= 'A' && c <= 'Z') return c - 'A' + 'a';
            return c;
        }
        public static Boolean IsNormalWhiteSpace(UInt32 c)
        {
            return
                (c <= 0x000D && c >= 0x0009) || // TAB (U+0009)
                                                // LINE FEED (U+000A)
                                                // LINE TAB (U+000B)
                                                // FORM FEED (U+000C)
                                                // CARRIAGE RETURN (U+000D)
                (c == 0x0020) ;                 // SPACE (U+0020)
                /*
                (c == 0x0085) ||                // NEXT LINE (U+0085)
                (c == 0x00A0) ||                // NO-BREAK SPACE (U+00A0)
                (c == 0x1680) ||                // OGHAM SPACE MARK (U+1680)
                (c >= 0x2000 && c <= 0x200A) || // EN QUAD (U+2000)
                                                // EM QUAD (U+2001)
                                                // EN SPACE (U+2002)
                                                // EM SPACE (U+2003)
                                                // THREE-PER-EM SPACE (U+2004)
                                                // FOUR-PER-EM SPACE (U+2005)
                                                // SIX-PER-EM SPACE (U+2006)
                                                // FIGURE SPACE (U+2007)
                                                // PUNCTUATION SPACE (U+2008)
                                                // THIN SPACE (U+2009)
                                                // HAIR SPACE (U+200A)
                */
            /* 
             * 
             * OGHAM SPACE MARK (U+1680)

             * NARROW NO-BREAK SPACE (U+202F)
             * MEDIUM MATHEMATICAL SPACE (U+205F)
             * IDEOGRAPHIC SPACE (U+3000)*/
        }

        /// <summary>Peel the first string surrounded by whitespace</summary>
        /// <returns>The first string surrounded by whitespace BY LIMIT</returns>
        public static Segment Peel(Byte[] array, ref UInt32 offset, UInt32 limit)
        {
            Debug.Assert(SegmentByLimit.InValidState(array, offset, limit));

            if (offset >= limit)
            {
                return new Segment(array, offset, limit);
            }

            UInt32 c;
            UInt32 save;

            //
            // Skip beginning whitespace
            //
            while (true)
            {
                if (offset >= limit)
                {
                    return new Segment(array, offset, limit);
                }
                save = offset;
                c = Decode(array, ref offset, limit);
                if (!IsNormalWhiteSpace(c)) break;
            }

            UInt32 peelStart = save;

            //
            // Find next whitespace
            //
            while (true)
            {
                if (offset >= limit)
                {
                    return new Segment(array, peelStart, offset);
                }
                save = offset;
                c = Decode(array, ref offset, limit);
                if (IsNormalWhiteSpace(c)) break;
            }

            UInt32 peelLimit = save;

            //
            // Remove whitespace till rest
            //
            while (true)
            {
                if (offset >= limit)
                {
                    offset = save;
                    return new Segment(array, peelStart, peelLimit);
                }
                save = offset;
                c = Decode(array, ref offset, limit);
                if (!IsNormalWhiteSpace(c))
                {
                    offset = save;
                    return new Segment(array, peelStart, peelLimit);
                }
            }
        }


        public static Boolean EqualsString(Byte[] array, UInt32 offset, UInt32 limit, String compare, Boolean ignoreCase)
        {
            return EqualsString(array, offset, limit, compare, 0, (UInt32)compare.Length, ignoreCase);
        }
        public static Boolean EqualsString(Byte[] array, UInt32 offset, UInt32 limit,
            String compare, UInt32 compareOffset, UInt32 compareLimit, Boolean ignoreCase)
        {
            UInt32 c;

            while(true)
            {
                if (offset >= limit)
                {
                    return compareOffset >= compareLimit;
                }
                if (compareOffset >= compareLimit)
                {
                    return false;
                }

                c = Decode(array, ref offset, limit);
                if (c != (UInt32)compare[(int)compareOffset])
                {
                    if (!ignoreCase) return false;
                    if (Char.IsUpper(compare[(int)compareOffset]))
                    {
                        if (IsUpper(c)) return false;
                        if (ToUpper(c) != compare[(int)compareOffset]) return false;
                    }
                    else
                    {
                        if (IsLower(c)) return false;
                        if (ToLower(c) != (UInt32)compare[(int)compareOffset]) return false;
                    }

                }
                compareOffset++;
            }
        }
    }
}