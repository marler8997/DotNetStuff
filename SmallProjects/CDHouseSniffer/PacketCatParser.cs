using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace More
{
    public class PacketCatParser
    {
        static UInt32 ParseHex(Byte[] bytes)
        {
            return (UInt32)(
                (0xFF000000U & ( ( (bytes[0].HexValue() << 4) + bytes[1].HexValue()) << 24) )|
                (0x00FF0000U & ( ( (bytes[2].HexValue() << 4) + bytes[3].HexValue()) << 16) )|
                (0x0000FF00U & ( ( (bytes[4].HexValue() << 4) + bytes[5].HexValue()) <<  8) )|
                (0x000000FFU & ( ( (bytes[6].HexValue() << 4) + bytes[7].HexValue())      ) ));
        } 

        public readonly Stream packetCatStream;
        readonly ByteBuffer packetBuffer = new ByteBuffer(1024, 1024);
        public PacketCatParser(Stream packetCatStream)
        {
            this.packetCatStream = packetCatStream;
        }
        public Byte[] GetPacket(out UInt32 packetSize)
        {
            //
            // Read the packet size
            //
            packetCatStream.ReadFullSize(packetBuffer.array, 0, 8);
            packetSize = ParseHex(packetBuffer.array);
            packetBuffer.EnsureCapacityNoCopy(packetSize * 2); // 2 bytes for each hex char

            //
            // Read the packet data
            //
            Byte[] packetArray = packetBuffer.array;
            packetCatStream.ReadFullSize(packetArray, 0, (Int32)(packetSize * 2));

            //
            // Convert hex to binary
            //
            UInt32 hexOffset = 0;
            for (UInt32 i = 0; i < packetSize; i++)
            {
                packetArray[i] = (Byte)((packetArray[hexOffset].HexValue() << 4) + packetArray[hexOffset + 1].HexValue());
                hexOffset += 2;
            }

            return packetArray;
        }
    }
}
