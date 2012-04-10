using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.Hmd
{
    public static class IntegerHelper
    {
        public static Boolean IsValidInteger(this String value, Boolean isUnsigned, Int32 byteLength)
        {
            int offset = 0;

            // Skip Whitespace
            while (true)
            {
                if (offset >= value.Length) return false;
                if (!Char.IsWhiteSpace(value[offset])) break;
                offset++;
            }

            // Check for optional '-'
            if (value[offset] == '-')
            {
                if (isUnsigned) return false;
                offset++;
                if (offset >= value.Length) return false;
            }

            // Check first digit
            if (value[offset] < '0' && value[offset] > '9') return false;

            do
            {
                offset++;
                if (offset >= value.Length) return true;
            } while (value[offset] >= '0' && value[offset] <= '9');


            while(true)
            {
                if (!Char.IsWhiteSpace(value[offset])) return false;
                offset++;
                if (offset >= value.Length) return true;
            }
        }
    }
}
