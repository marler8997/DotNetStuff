using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Marler.RuntimeAnalyzer
{
    public static class Util
    {

        public static UInt64 InsertUInt64(byte[] bytes, UInt64 offset, UInt64 value)
        {
            while (value > 0xFFU)
            {
                bytes[offset++] = (Byte)value;
                value >>= 8;
            }

            bytes[offset++] = (Byte)value;
            return offset;
        }

        public static UInt64 InsertUInt64(byte[] bytes, UInt64 offset, Byte extraBytes, UInt64 uint64)
        {
            // extraBytes must be <= 7

            UInt64 i = offset + extraBytes;
            bytes[i--] = (Byte)uint64;
            while (i >= offset)
            {
                uint64 >>= 8;
                bytes[i--] = (Byte)uint64;
            }

            return offset + (UInt64)extraBytes + 1U;
        }

        public static Byte[] GetBytesWithExtraBytesPrefix(UInt64 value)
        {
            if (value <= 0xFFU)
            {
                return new Byte[] {0,
                    (Byte)value};
            }            
            if (value <= 0xFFFFU)
            {
                return new Byte[] {1,
                    (Byte)(value>>8),
                    (Byte)value};
            }
            if (value <= 0xFFFFFFU)
            {
                return new Byte[] { 2, 
                    (Byte)(value >> 16), 
                    (Byte)(value >> 8), 
                    (Byte)value };
            }
            if (value <= 0xFFFFFFFFU)
            {
                return new Byte[] { 3, 
                    (Byte)(value >> 24), 
                    (Byte)(value >> 16), 
                    (Byte)(value >> 8), 
                    (Byte)value };
            }
            if (value <= 0xFFFFFFFFFFU)
            {
                return new Byte[] { 4, 
                    (Byte)(value >> 32), 
                    (Byte)(value >> 24), 
                    (Byte)(value >> 16), 
                    (Byte)(value >> 8), 
                    (Byte)value };
            }
            if (value <= 0xFFFFFFFFFFFFU)
            {
                return new Byte[] { 5, 
                    (Byte)(value >> 40), 
                    (Byte)(value >> 32), 
                    (Byte)(value >> 24), 
                    (Byte)(value >> 16), 
                    (Byte)(value >> 8), 
                    (Byte)value };
            }
            if (value <= 0xFFFFFFFFFFFFFFU)
            {
                return new Byte[] { 6, 
                    (Byte)(value >> 48), 
                    (Byte)(value >> 40), 
                    (Byte)(value >> 32), 
                    (Byte)(value >> 24), 
                    (Byte)(value >> 16), 
                    (Byte)(value >> 8), 
                    (Byte)value };
            }

            return new Byte[] { 7, 
                (Byte)(value >> 56), 
                (Byte)(value >> 48), 
                (Byte)(value >> 40), 
                (Byte)(value >> 32), 
                (Byte)(value >> 24), 
                (Byte)(value >> 16), 
                (Byte)(value >> 8), 
                (Byte)value };
        }


        public static Byte[] GetBytes(UInt64 value)
        {
            if (value <= 0xFFU)
            {
                return new Byte[] {
                    (Byte)value};
            }
            if (value <= 0xFFFFU)
            {
                return new Byte[] {
                    (Byte)(value>>8),
                    (Byte)value};
            }
            if (value <= 0xFFFFFFU)
            {
                return new Byte[] {
                    (Byte)(value >> 16), 
                    (Byte)(value >> 8), 
                    (Byte)value };
            }
            if (value <= 0xFFFFFFFFU)
            {
                return new Byte[] {
                    (Byte)(value >> 24), 
                    (Byte)(value >> 16), 
                    (Byte)(value >> 8), 
                    (Byte)value };
            }
            if (value <= 0xFFFFFFFFFFU)
            {
                return new Byte[] {
                    (Byte)(value >> 32), 
                    (Byte)(value >> 24), 
                    (Byte)(value >> 16), 
                    (Byte)(value >> 8), 
                    (Byte)value };
            }
            if (value <= 0xFFFFFFFFFFFFU)
            {
                return new Byte[] {
                    (Byte)(value >> 40), 
                    (Byte)(value >> 32), 
                    (Byte)(value >> 24), 
                    (Byte)(value >> 16), 
                    (Byte)(value >> 8), 
                    (Byte)value };
            }
            if (value <= 0xFFFFFFFFFFFFFFU)
            {
                return new Byte[] {
                    (Byte)(value >> 48), 
                    (Byte)(value >> 40), 
                    (Byte)(value >> 32), 
                    (Byte)(value >> 24), 
                    (Byte)(value >> 16), 
                    (Byte)(value >> 8), 
                    (Byte)value };
            }

            return new Byte[] {
                (Byte)(value >> 56), 
                (Byte)(value >> 48), 
                (Byte)(value >> 40), 
                (Byte)(value >> 32), 
                (Byte)(value >> 24), 
                (Byte)(value >> 16), 
                (Byte)(value >> 8), 
                (Byte)value };
        }
        public static UInt64 GetUInt64(byte[] bytes, ref UInt64 offset, Byte extraBytes)
        {
            switch (extraBytes & 0x07)
            {
                case 0:
                    return bytes[offset++];
                case 1:
                    return
                        (((UInt32)bytes[offset++]) << 8) |
                        (((UInt32)bytes[offset++]));
                case 2:
                    return
                        (((UInt32)bytes[offset++]) << 16) |
                        (((UInt32)bytes[offset++]) << 8) |
                        (((UInt32)bytes[offset++]));
                case 3:
                    return
                        (((UInt32)bytes[offset++]) << 24) |
                        (((UInt32)bytes[offset++]) << 16) |
                        (((UInt32)bytes[offset++]) << 8) |
                        (((UInt32)bytes[offset++]));
                case 4:
                    return
                        (((UInt64)bytes[offset++]) << 32) |
                        (((UInt64)bytes[offset++]) << 24) |
                        (((UInt64)bytes[offset++]) << 16) |
                        (((UInt64)bytes[offset++]) << 8) |
                        (((UInt64)bytes[offset++]));
                case 5:
                    return
                        (((UInt64)bytes[offset++]) << 40) |
                        (((UInt64)bytes[offset++]) << 32) |
                        (((UInt64)bytes[offset++]) << 24) |
                        (((UInt64)bytes[offset++]) << 16) |
                        (((UInt64)bytes[offset++]) << 8) |
                        (((UInt64)bytes[offset++]));
                case 6:
                    return
                        (((UInt64)bytes[offset++]) << 48) |
                        (((UInt64)bytes[offset++]) << 40) |
                        (((UInt64)bytes[offset++]) << 32) |
                        (((UInt64)bytes[offset++]) << 24) |
                        (((UInt64)bytes[offset++]) << 16) |
                        (((UInt64)bytes[offset++]) << 8) |
                        (((UInt64)bytes[offset++]));
                case 7:
                    return
                        (((UInt64)bytes[offset++]) << 56) |
                        (((UInt64)bytes[offset++]) << 48) |
                        (((UInt64)bytes[offset++]) << 40) |
                        (((UInt64)bytes[offset++]) << 32) |
                        (((UInt64)bytes[offset++]) << 24) |
                        (((UInt64)bytes[offset++]) << 16) |
                        (((UInt64)bytes[offset++]) << 8) |
                        (((UInt64)bytes[offset++]));
            }
            throw new ArgumentOutOfRangeException("extraBytes");
        }

        public static Byte[] ReadFullSize(Stream stream, Int32 size)
        {
            Byte[] buffer = new Byte[size];
            Int32 offset = 0;

            Int32 lastBytesRead;

            do
            {
                lastBytesRead = stream.Read(buffer, offset, size);
                size -= lastBytesRead;
                if (size <= 0)
                {
                    return buffer;
                }
                offset += lastBytesRead;
            } while (lastBytesRead > 0);

            throw new EndOfStreamException();
        }
        public static void ReadFullSize(Stream stream, Byte [] buffer, Int32 offset, Int32 size)
        {
            Int32 lastBytesRead;

            do
            {
                lastBytesRead = stream.Read(buffer, offset, size);
                size -= lastBytesRead;
                if (size <= 0)
                {
                    return;
                }
                offset += lastBytesRead;
            } while (lastBytesRead > 0);

            throw new EndOfStreamException();
        }

        public static Int32 ToAddressOffset(byte[] bytes, ref UInt64 offset, Byte len)
        {
            switch (len)
            {
                case 1:
                    return (Int32)(SByte)bytes[offset++];
                case 2:
                    return (Int32)(Int16) (
                            ((bytes[offset++] & 0xFF) << 8) |
                            ((bytes[offset++] & 0xFF))
                            );
                case 3:
                    Int32 i = (bytes[offset++] << 16) |
                       ((bytes[offset++] & 0xFF) << 8) |
                       ((bytes[offset++] & 0xFF));
                    return ((i & 0x800000) != 0) ? (unchecked((Int32)0xFF000000) | i) : i;
                case 4:
                    return (bytes[offset++] << 24) |
                          ((bytes[offset++] & 0xFF) << 16) |
                          ((bytes[offset++] & 0xFF) << 8) |
                          ((bytes[offset++] & 0xFF));

            }
            throw new ArgumentOutOfRangeException("len");
        }

        public static byte[] GetBytesFromFile(string fullFilePath)
        {
            // this method is limited to 2^32 byte files (4.2 GB)

            FileStream fs = File.OpenRead(fullFilePath);
            try
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                fs.Close();
                return bytes;
            }
            finally
            {
                fs.Close();
            }

        }

        public static UInt64 ReadAddressValue(Stream stream, ref UInt64 incrementBytesRead)
        {
            Int32 infoByte = stream.ReadByte();
            if (infoByte < 0) throw new EndOfStreamException();

            UInt64 offset = 0;

            UInt64 addressValue = GetUInt64(ReadFullSize(stream, infoByte + 1), ref offset, (Byte)infoByte);

            incrementBytesRead += offset + 1;
            return addressValue;
        }

    }
}
