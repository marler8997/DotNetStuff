using System;

namespace Marler.RuntimeAnalyzer
{
    public static class Const
    {
        public const Byte ByteSize = 8;

        public static readonly UInt64[] masks = new UInt64[ByteSize];

        static Const()
        {
            masks[0] = 0xFFU;
            masks[1] = 0xFFFFU;
            masks[2] = 0xFFFFFFU;
            masks[3] = 0xFFFFFFFFU;
            masks[4] = 0xFFFFFFFFFFU;
            masks[5] = 0xFFFFFFFFFFFFU;
            masks[6] = 0xFFFFFFFFFFFFFFU;
            masks[7] = 0xFFFFFFFFFFFFFFFFU;
        }
    }
}
