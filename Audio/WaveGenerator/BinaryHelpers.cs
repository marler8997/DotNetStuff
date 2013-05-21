using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marler.Audio
{
    public static class BinaryWriter
    {
        public static Int32 ToBigEndian(this UInt16 value, Byte[] array, Int32 offset)
        {
            array[offset    ] = (Byte)(value >> 8);
            array[offset + 1] = (Byte)(value     );
            return offset + 2;
        }
        public static Int32 ToLittleEndian(this UInt16 value, Byte[] array, Int32 offset)
        {
            array[offset + 1] = (Byte)(value >> 8);
            array[offset    ] = (Byte)(value     );
            return offset + 2;
        }
        public static Int32 ToLittleEndian(this UInt32 value, Byte[] array, Int32 offset)
        {
            array[offset + 3] = (Byte)(value >> 24);
            array[offset + 2] = (Byte)(value >> 16);
            array[offset + 1] = (Byte)(value >>  8);
            array[offset    ] = (Byte)(value      );
            return offset + 4;
        }
    }
}
