using System;
using System.Collections.Generic;
using System.IO;

namespace More
{
    public static class ByteArrayExtensions
    {
        public static UInt16 LittleEndianReadUInt16(this Byte[] bytes, Int32 offset)
        {
            return (UInt16)(
                (0xFF00 & (bytes[offset + 1] << 8)) |
                (0x00FF & (bytes[offset])));
        }
        public static UInt32 LittleEndianReadUInt32(this Byte[] bytes, Int32 offset)
        {
            return (UInt32)(
                (0xFF000000U & (bytes[offset + 3] << 24)) |
                (0x00FF0000U & (bytes[offset + 2] << 16)) |
                (0x0000FF00U & (bytes[offset + 1] << 8)) |
                (0x000000FFU & (bytes[offset])));
        }
        public static String ToHexString(this Byte[] bytes, Int32 offset, Int32 length)
        {
            Char[] hexBuffer = new Char[length * 2];

            Int32 hexOffset = 0;
            Int32 offsetLimit = offset + length;

            while (offset < offsetLimit)
            {
                String hex = bytes[offset].ToString("X2");
                hexBuffer[hexOffset] = hex[0];
                hexBuffer[hexOffset + 1] = hex[1];
                offset++;
                hexOffset += 2;
            }
            return new String(hexBuffer);
        }
        public static String ToHexString(this Char[] chars, Int32 offset, Int32 length)
        {
            Char[] hexBuffer = new Char[length * 2];

            Int32 hexOffset = 0;
            Int32 offsetLimit = offset + length;

            while (offset < offsetLimit)
            {
                String hex = ((Byte)chars[offset]).ToString("X2");
                hexBuffer[hexOffset] = hex[0];
                hexBuffer[hexOffset + 1] = hex[1];
                offset++;
                hexOffset += 2;
            }
            return new String(hexBuffer);
        }
    }
    public class BinaryStream
    {
        readonly byte[] eightByteBuffer = new byte[8];
        Stream stream;

        public BinaryStream(Stream stream)
        {
            this.stream = stream;
        }
        public void Skip(Int32 length)
        {
            stream.Position += length;
        }
        public Byte[] ReadFullSize(Int32 length)
        {
            Byte[] buffer = new Byte[length];
            stream.ReadFullSize(buffer, 0, length);
            return buffer;
        }
        public void ReadFullSize(Byte[] buffer, Int32 offset, Int32 length)
        {
            stream.ReadFullSize(buffer, offset, length);
        }
        public UInt16 LittleEndianReadUInt16()
        {
            ReadFullSize(eightByteBuffer, 0, 2);
            return eightByteBuffer.LittleEndianReadUInt16(0);
        }
        public UInt32 LittleEndianReadUInt32()
        {
            ReadFullSize(eightByteBuffer, 0, 4);
            return eightByteBuffer.LittleEndianReadUInt32(0);
        }
    }
}
