using System;
using System.Collections.Generic;

namespace More.Net
{
    public class Tls
    {
        public const Byte Tls10MajorVersion = 0x03;
        public const Byte Tls10MinorVersion = 0x01;

        public static Byte[] CreateRandom(Random random)
        {
            Byte[] packet = new Byte[28];
            random.NextBytes(packet);
            return packet;
        }
    }
}
