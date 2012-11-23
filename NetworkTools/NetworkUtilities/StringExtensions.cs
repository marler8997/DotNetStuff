using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.NetworkTools
{
    public static class StringExtensions
    {

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
