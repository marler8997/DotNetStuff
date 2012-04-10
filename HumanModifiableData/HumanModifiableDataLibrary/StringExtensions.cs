using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.Hmd
{
    public static class HmdStringExtensions
    {
        public static String Concatenate(this String[] strings)
        {
            Int32 totalLength = 0;
            for(int i = 0; i < strings.Length; i++)
            {
                totalLength += strings[i].Length;
            }
            StringBuilder stringBuilder = new StringBuilder(totalLength, totalLength + strings.Length);

            for (int i = 0; i < strings.Length; i++)
            {
                stringBuilder.Append('%');
                stringBuilder.Append(strings[i]);
            }

            return stringBuilder.ToString();
        }

        public static Boolean IsSubstring(this String str, String compare, int offset)
        {
            //Console.WriteLine("Comparing \"{0}\" with \"{1}\" at offset {2}", str, compare, offset);
            if (str.Length + offset > compare.Length)
            {
                //Console.WriteLine("\"{0}\" is too long to match", str.Length);
                return false;
            }

            for (int i = 0; i < str.Length; i++)
            {
                // Case Insensitive Compare
                if (str[i] != compare[offset])
                {
                    if (Char.IsLower(str[i]))
                    {
                        if (Char.IsLower(compare[offset]))
                        {
                            return false;
                        }
                        if (str[i] != Char.ToLower(compare[offset]))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (Char.IsUpper(compare[offset]))
                        {
                            return false;
                        }
                        if (str[i] != Char.ToUpper(compare[offset]))
                        {
                            return false;
                        }

                    }
                }
                offset++;
            }
            return true;
        }

        public static String[] SplitByWhitespace(this String str, int offset)
        {
            int saveOffset;
            List<String> enumValuesList = new List<String>();

            while (true)
            {
                // skip whitespace
                while (true)
                {
                    if (offset >= str.Length)
                    {
                        return enumValuesList.ToArray();
                    }
                    if (!Char.IsWhiteSpace(str[offset])) break;
                    offset++;
                }

                saveOffset = offset;

                while (true)
                {
                    offset++;
                    if (offset >= str.Length) break;
                    if (Char.IsWhiteSpace(str[offset])) break;
                }

                enumValuesList.Add(str.Substring(saveOffset, offset - saveOffset));

                offset++;
            }

        }

    }




    public class HmdStringComparer : IComparer<String>
    {
        private static readonly HmdStringComparer instance = new HmdStringComparer();

        public static HmdStringComparer Instance
        {
            get { return instance; }
        }

        public static int CompareStatic(String x, String y)
        {
            return instance.Compare(x, y);
        }

        public int Compare(String x, String y)
        {
            for (int i = 0; i < x.Length && i < y.Length; i++)
            {
                if (x[i] != y[i])
                {
                    return x[i] - y[i];
                }
            }
            return x.Length - y.Length;
        }
    }

}
