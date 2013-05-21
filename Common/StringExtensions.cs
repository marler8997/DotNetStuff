using System;

namespace Marler.Common
{
    public static class StringExtensions
    {
        public static String Peel(this String str, out String rest)
        {
            return Peel(str, 0, out rest);
        }

        // Peel the first string until whitespace
        public static String Peel(this String str, Int32 offset, out String rest)
        {
            if (str == null)
            {
                rest = null;
                return null;
            }

            Char c;

            //
            // Skip beginning whitespace
            //
            while (true)
            {
                if (offset >= str.Length)
                {
                    rest = null;
                    return null;
                }
                c = str[offset];
                if (!Char.IsWhiteSpace(c)) break;
                offset++;
            }

            Int32 startOffset = offset;

            //
            // Find next whitespace
            //
            while (true)
            {
                offset++;
                if (offset >= str.Length)
                {
                    rest = null;
                    return str.Substring(startOffset);
                }
                c = str[offset];
                if (Char.IsWhiteSpace(c)) break;
            }

            Int32 peelLimit = offset;

            //
            // Remove whitespace till rest
            //
            while (true)
            {
                offset++;
                if (offset >= str.Length)
                {
                    rest = null;
                }
                if (!Char.IsWhiteSpace(str[offset]))
                {
                    rest = str.Substring(offset);
                    break;
                }
            }
            return str.Substring(startOffset, peelLimit - startOffset);
        }
        public static Boolean SubstringEquals(String str, Int32 offset, String compare)
        {
            if (offset + compare.Length > str.Length)
            {
                return false;
            }

            for (int i = 0; i < compare.Length; i++)
            {
                if (str[offset] != compare[i])
                {
                    if (Char.IsUpper(str[offset]))
                    {
                        if (Char.IsUpper(compare[i])) return false;
                        if (Char.ToUpper(compare[i]) != str[offset]) return false;
                    }
                    else
                    {
                        if (Char.IsLower(compare[i])) return false;
                        if (Char.ToLower(compare[i]) != str[offset]) return false;
                    }
                }
                offset++;
            }
            return true;
        }
    }
}