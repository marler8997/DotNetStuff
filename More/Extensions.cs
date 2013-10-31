using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

#if WindowsCE
using ByteParser = System.MissingInCEByteParser;
using ArrayCopier = System.MissingInCEArrayCopier;
#else
using ByteParser = System.Byte;
using ArrayCopier = System.Array;
#endif



namespace More
{
    /*
    public static class GenericExtensions
    {
        public static void DataString<T>(this T value, StringBuilder builder)
        {
            builder.Append(value.ToString());
        }
    }
    */
#if !WindowsCE
    public static class GCExtensions
    {
        static Int32[] lastGenCount;

        static void VerifyLastGenCountSize(Int32 generation)
        {
            Int32 size = (generation > 2) ? generation + 1 : 3;

            if(lastGenCount == null)
            {
                lastGenCount = new Int32[size];
            }
            else
            {
                if(lastGenCount.Length < size)
                {
                    Int32[] newLastGenCount = new Int32[size];
                    Array.Copy(lastGenCount, newLastGenCount, lastGenCount.Length);
                    lastGenCount = newLastGenCount;
                }
            }
        }
        public static Int32 CountDiff(Int32 generation)
        {
            Int32 currentCount = GC.CollectionCount(generation);

            lock(typeof(GCExtensions))
            {
                VerifyLastGenCountSize(generation);

                Int32 diff = currentCount - lastGenCount[generation];
                lastGenCount[generation] = currentCount;
                return diff;
            }
        }
    }
#endif


    public enum EncodingType
    {
        Default = 0,
        Ascii   = 1,
        Utf7    = 2,
        Utf8    = 3,
        Utf32   = 4,
        Unicode = 5,
    }
    public static class EncodingParser
    {
        public static Encoding GetEncoding(this EncodingType encodingType)
        {
            switch (encodingType)
            {
                case EncodingType.Default: return Encoding.Default;
                case EncodingType.Ascii: return Encoding.ASCII;
                case EncodingType.Utf7: return Encoding.UTF7;
                case EncodingType.Utf8: return Encoding.UTF8;
#if !WindowsCE
                case EncodingType.Utf32: return Encoding.UTF32;
#endif
                case EncodingType.Unicode: return Encoding.Unicode;
            }
            throw new InvalidOperationException(String.Format("Unknown EncodingType '{0}'", encodingType));
        }
    }

    public class ArrayBuilder
    {
        const Int32 InitialArraySize = 16;

        Type elementType;

        Array array;
        Int32 count;

        public ArrayBuilder(Type elementType)
            : this(elementType, InitialArraySize)
        {
        }
        public ArrayBuilder(Type elementType, Int32 initialArraySize)
        {
            this.elementType = elementType;
            this.array = Array.CreateInstance(elementType, initialArraySize);
            this.count = 0;
        }
        public void Add(Object obj)
        {
            if (this.count >= array.Length)
            {
                Array newArray = Array.CreateInstance(elementType, this.array.Length * 2);
                Array.Copy(this.array, newArray, this.count);
                this.array = newArray;
            }
            this.array.SetValue(obj, this.count++);
        }
        public Array Build()
        {
            if (array.Length != count)
            {
                Array newArray = Array.CreateInstance(elementType, this.count);
                Array.Copy(this.array, newArray, this.count);
                this.array = newArray;
            }

            return this.array;
        }
    }
    public class GenericArrayBuilder<T>
    {
        const Int32 InitialArraySize = 16;

        T[] array;
        Int32 count;

        public GenericArrayBuilder()
            : this(InitialArraySize)
        {
        }
        public GenericArrayBuilder(Int32 initialArraySize)
        {
            this.array = new T[initialArraySize];
            this.count = 0;
        }
        public void Add(T obj)
        {
            if (this.count >= array.Length)
            {
                T[] newArray = new T[this.array.Length * 2];
                Array.Copy(this.array, newArray, this.count);
                this.array = newArray;
            }
            this.array[this.count++] = obj;
        }
        public T[] Build()
        {
            if (array.Length != count)
            {
                T[] newArray = new T[this.count];
                Array.Copy(this.array, newArray, this.count);
                this.array = newArray;
            }
            return this.array;
        }
    }

    public static class CharExtensions
    {
        /*
        static Char ToLower(this Char c)
        {
            return (c >= 'A' && c <= 'Z') ? (Char)(c + 'a' - 'A') : c;
        }
        static Char ToUpper(this Char c)
        {
            return (c >= 'a' && c <= 'z') ? (Char)(c + 'A' - 'a') : c;
        }
        */
        public static Int32 HexValue(this Char c)
        {
            if (c >= '0' && c <= '9') return (c - '0');
            if (c >= 'A' && c <= 'F') return (c - 'A') + 10;
            if (c >= 'a' && c <= 'f') return (c - 'a') + 10;
            throw new FormatException(String.Format("Expected 0-9, A-F, or a-f but got '{0}' (charcode={1})", c, (UInt32)c));
        }
    }
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
        public static String[] SplitCorrectly(this String str, Char seperator)
        {
            if (str == null || str.Length == 0) return null;

            if (str[0] == seperator) throw new FormatException(String.Format("In the string '{0}', the first character can't be a seperator '{1}'",
                str, seperator));
            if (str[str.Length - 1] == seperator) throw new FormatException(String.Format("In the string '{0}', the last character can't be a seperator '{1}'",
                str, seperator));

            Int32 seperatorCount = 0;
            for (int i = 1; i < str.Length - 1; i++)
            {
                if (str[i] == seperator)
                {
                    if (str[i - 1] == seperator)
                    {
                        throw new FormatException(String.Format("In the string '{0}', expected something in between the seperator '{1}'",
                            str, seperator));
                    }
                    seperatorCount++;
                }
            }

            String[] splitStrings = new String[seperatorCount + 1];
            Int32 splitOffset = 0;

            Int32 lastOffset = 0;
            Int32 currentOffset = 1;
            while (currentOffset < str.Length)
            {
                if (str[currentOffset] == seperator)
                {
                    splitStrings[splitOffset++] = str.Substring(lastOffset, currentOffset - lastOffset);
                    lastOffset = currentOffset + 1;
                    currentOffset += 2;
                }
                else
                {
                    currentOffset++;
                }

            }

            splitStrings[splitOffset++] = str.Substring(lastOffset, currentOffset - lastOffset);

            return splitStrings;
        }
        /*
        Escape  Character Name              Unicode encoding
        ======  ==============              ================
        \\      Backslash                   0x005C
        \0      Null                        0x0000
        \a      Alert                       0x0007
        \b      Backspace                   0x0008
        \f      Form feed                   0x000C
        \n      New line                    0x000A
        \r      Carriage return             0x000D
        \t      Horizontal tab              0x0009
        \v      Vertical tab                0x000B
        \x      Hexadecimal Byte            \x41 = "A" = 0x41
        */
        public static Byte[] ParseStringLiteral(this String literal, Int32 offset, out Int32 outLength)
        {
            Int32 length = 0;
            Byte[] buffer = new Byte[literal.Length];

            Int32 save;

            while (true)
            {
                if (offset >= literal.Length)
                {
                    outLength = length;
                    return buffer;
                    //return builder.ToString();
                }

                save = offset;
                while (true)
                {
                    if (literal[offset] == '\\') break;
                    offset++;
                    if (offset >= literal.Length)
                    {
                        do
                        {
                            buffer[length++] = (byte)literal[save++]; // do I need to do an Encoding?
                        } while (save < literal.Length);
                        outLength = length;
                        return buffer;
                    }
                }

                // the character at i is '\'
                while (save < offset)
                {
                    buffer[length++] = (byte)literal[save++]; // do I need to do an Encoding?
                }
                offset++;
                if (offset >= literal.Length) throw new FormatException("Your literal string ended with '\'");

                char escapeChar = literal[offset];
                if (escapeChar == 'n') buffer[length++] = (byte)'\n';
                else if (escapeChar == '\\') buffer[length++] = (byte)'\\';
                else if (escapeChar == '0') buffer[length++] = (byte)'\0';
                else if (escapeChar == 'a') buffer[length++] = (byte)'\a';
                else if (escapeChar == 'r') buffer[length++] = (byte)'\r';
                else if (escapeChar == 't') buffer[length++] = (byte)'\t';
                else if (escapeChar == 'v') buffer[length++] = (byte)'\v';
                else if (escapeChar == 'x')
                {
                    offset++;
                    if (offset + 1 >= literal.Length) throw new FormatException("The escape character 'x' needs at least 2 digits");

                    Byte output;
                    String sequence = literal.Substring(offset, 2);
                    if (!ByteParser.TryParse(sequence, System.Globalization.NumberStyles.HexNumber, null, out output))
                    {
                        throw new FormatException(String.Format("Could not parse the hexadecimal escape sequence '\\x{0}' as a hexadecimal byte", sequence));
                    }
                    Console.WriteLine("Parsed '\\x{0}' as '{1}' (0x{2:X2}) ((char)0x{3:X2})", sequence, (char)output, output, (byte)(char)output);
                    buffer[length++] = output;
                    offset++;
                }
                else throw new FormatException(String.Format("Unrecognized escape sequence '\\{0}'", escapeChar));

                offset++;
            }
        }

        public static String UnderscoreToCamelCase(this String underscoreString)
        {
            if (String.IsNullOrEmpty(underscoreString)) return underscoreString;

            Char[] buffer = new Char[underscoreString.Length];
            Int32 bufferIndex = 0;

            Int32 offset = 0;

            while (true)
            {
                if (underscoreString[offset] != '_') break;
                offset++;
                if (offset >= underscoreString.Length) return "";
            }

            buffer[bufferIndex++] = Char.ToUpper(underscoreString[offset]);
            offset++;

            while (offset < underscoreString.Length)
            {
                Char c = underscoreString[offset];
                if (c != '_')
                {
                    buffer[bufferIndex++] = Char.ToLower(c);
                }
                else
                {
                    while (true)
                    {
                        offset++;
                        if (offset >= underscoreString.Length) goto DONE;
                        c = underscoreString[offset];
                        if (c != '_') break;
                    }
                    buffer[bufferIndex++] = Char.ToUpper(c);
                }
                offset++;
            }
            DONE:
            return new String(buffer, 0, bufferIndex);
        }
        /*
        public static String CamelToUpperUnderscoreCase(this String camelString)
        {
            if (String.IsNullOrEmpty(camelString)) return camelString;

            Char[] buffer = new Char[underscoreString.Length];
            Int32 bufferIndex = 0;

            Int32 offset = 0;

            while (true)
            {
                if (underscoreString[offset] != '_') break;
                offset++;
                if (offset >= underscoreString.Length) return "";
            }

            buffer[bufferIndex++] = Char.ToUpper(underscoreString[offset]);
            offset++;

            while (offset < underscoreString.Length)
            {
                Char c = underscoreString[offset];
                if (c != '_')
                {
                    buffer[bufferIndex++] = Char.ToLower(c);
                }
                else
                {
                    while (true)
                    {
                        offset++;
                        if (offset >= underscoreString.Length) goto DONE;
                        c = underscoreString[offset];
                        if (c != '_') break;
                    }
                    buffer[bufferIndex++] = Char.ToUpper(c);
                }
                offset++;
            }
        DONE:
            return new String(buffer, 0, bufferIndex);
        }
        */
    }
    public static class Int24
    {
        public const Int32 MinValue = -0x800000;
        public const Int32 MaxValue =  0x7FFFFF;
    }
    public static class UInt24
    {
        public const Int32 MinValue = 0;
        public const Int32 MaxValue = 0xFFFFFF;
    }
    public static class ByteArray
    {
        //
        // Set 1-yte types
        //
        public static void SetByteEnum<EnumType>(this Byte[] bytes, UInt32 offset, EnumType value)
        {
            bytes[offset] = Convert.ToByte(value);
        }
        //
        // Read 1-byte types
        //
        public static EnumType ReadByteEnum<EnumType>(this Byte[] bytes, UInt32 offset)
        {
            return (EnumType)Enum.ToObject(typeof(EnumType), bytes[offset]);
        }
        //
        // Set 2-byte types
        //
        public static void BigEndianSetUInt16(this Byte[] bytes, UInt32 offset, UInt16 value)
        {
            bytes[offset    ] = (Byte)(value >> 8);
            bytes[offset + 1] = (Byte)value;
        }
        public static void BigEndianSetInt16(this Byte[] bytes, UInt32 offset, Int16 value)
        {
            bytes[offset    ] = (Byte)(value >> 8);
            bytes[offset + 1] = (Byte)value;
        }
        public static void BigEndianSetUInt16Enum<EnumType>(this Byte[] bytes, UInt32 offset, EnumType value)
        {
            UInt16 valueAsUInt16 = Convert.ToUInt16(value);
            bytes[offset    ] = (Byte)(valueAsUInt16 >> 8);
            bytes[offset + 1] = (Byte)valueAsUInt16;
        }
        //
        // Read 2-byte types
        //
        public static UInt16 BigEndianReadUInt16(this Byte[] bytes, UInt32 offset)
        {
            return (UInt16)(bytes[offset] << 8 | bytes[offset + 1]);
        }
        public static Int16 BigEndianReadInt16(this Byte[] bytes, UInt32 offset)
        {
            return (Int16)(bytes[offset] << 8 | bytes[offset + 1]);
        }
        public static EnumType BigEndianReadUInt16Enum<EnumType>(this Byte[] bytes, UInt32 offset)
        {
            return (EnumType)Enum.ToObject(typeof(EnumType), (UInt16)(
                (0xFF00U & (bytes[offset    ] << 8)) |
                (0x00FFU & (bytes[offset + 1]))));
        }
        //
        // Set 3-byte types
        //
        public static void BigEndianSetUInt24(this Byte[] bytes, UInt32 offset, UInt32 value)
        {
            bytes[offset    ] = (Byte)(value >> 16);
            bytes[offset + 1] = (Byte)(value >> 8);
            bytes[offset + 2] = (Byte)value;
        }
        public static void BigEndianSetUInt24Enum<EnumType>(this Byte[] bytes, UInt32 offset, EnumType value)
        {
            UInt32 valueAsUInt32 = Convert.ToUInt32(value);
            bytes[offset    ] = (Byte)(valueAsUInt32 >> 16);
            bytes[offset + 1] = (Byte)(valueAsUInt32 >> 8);
            bytes[offset + 2] = (Byte)valueAsUInt32;
        }
        public static void BigEndianSetInt24(this Byte[] bytes, UInt32 offset, Int32 value)
        {
            bytes[offset    ] = (Byte)(value >> 16);
            bytes[offset + 1] = (Byte)(value >> 8);
            bytes[offset + 2] = (Byte)value;
        }
        //
        // Read 3-byte types
        //
        public static UInt32 BigEndianReadUInt24(this Byte[] bytes, UInt32 offset)
        {
            return (UInt32)(
                (0x00FF0000U & (bytes[offset    ] << 16)) |
                (0x0000FF00U & (bytes[offset + 1] <<  8)) |
                (0x000000FFU & (bytes[offset + 2]      )) );
        }
        public static EnumType BigEndianReadUInt24Enum<EnumType>(this Byte[] bytes, UInt32 offset)
        {
            return (EnumType)Enum.ToObject(typeof(EnumType), (UInt32)(
                (0x00FF0000U & (bytes[offset    ] << 16)) |
                (0x0000FF00U & (bytes[offset + 1] <<  8)) |
                (0x000000FFU & (bytes[offset + 2]      )) ));
        }
        public static Int32 BigEndianReadInt24(this Byte[] bytes, UInt32 offset)
        {
            return (Int32)(
                ( ((bytes[offset] & 0x80) == 0x80) ? 0xFF000000 : 0) | // Sign Extend
                (0x00FF0000U & (bytes[offset    ] << 16)) |
                (0x0000FF00U & (bytes[offset + 1] <<  8)) |
                (0x000000FFU & (bytes[offset + 2]      )) );
        }
        //
        // Set 4-byte types
        //
        public static void BigEndianSetUInt32(this Byte[] bytes, UInt32 offset, UInt32 value)
        {
            bytes[offset    ] = (Byte)(value >> 24);
            bytes[offset + 1] = (Byte)(value >> 16);
            bytes[offset + 2] = (Byte)(value >> 8);
            bytes[offset + 3] = (Byte)value;
        }
        public static void BigEndianSetUInt32Enum<EnumType>(this Byte[] bytes, UInt32 offset, EnumType value)
        {
            UInt32 valueAsUInt32 = Convert.ToUInt32(value);
            bytes[offset    ] = (Byte)(valueAsUInt32 >> 24);
            bytes[offset + 1] = (Byte)(valueAsUInt32 >> 16);
            bytes[offset + 2] = (Byte)(valueAsUInt32 >> 8);
            bytes[offset + 3] = (Byte)valueAsUInt32;
        }
        public static void BigEndianSetInt32(this Byte[] bytes, UInt32 offset, Int32 value)
        {
            bytes[offset    ] = (Byte)(value >> 24);
            bytes[offset + 1] = (Byte)(value >> 16);
            bytes[offset + 2] = (Byte)(value >> 8);
            bytes[offset + 3] = (Byte)value;
        }
        //
        // Read 4-byte types
        //
        public static UInt32 BigEndianReadUInt32(this Byte[] bytes, UInt32 offset)
        {
            return (UInt32) (
                (bytes[offset    ] << 24) |
                (bytes[offset + 1] << 16) |
                (bytes[offset + 2] <<  8) |
                (bytes[offset + 3]      ) );
        }
        public static EnumType BigEndianReadUInt32Enum<EnumType>(this Byte[] bytes, UInt32 offset)
        {
            return (EnumType)Enum.ToObject(typeof(EnumType), (UInt32) (
                (bytes[offset    ] << 24) |
                (bytes[offset + 1] << 16) |
                (bytes[offset + 2] <<  8) |
                (bytes[offset + 3]      ) ));
        }
        public static Int32 BigEndianReadInt32(this Byte[] bytes, UInt32 offset)
        {
            return (Int32)(
                (0xFF000000U & (bytes[offset    ] << 24)) |
                (0x00FF0000U & (bytes[offset + 1] << 16)) |
                (0x0000FF00U & (bytes[offset + 2] <<  8)) |
                (0x000000FFU & (bytes[offset + 3]      )) );
        }
        //
        // Set 8-byte types
        //
        public static void BigEndianSetUInt64(this Byte[] bytes, UInt32 offset, UInt64 value)
        {
            bytes[offset    ] = (Byte)(value >> 56);
            bytes[offset + 1] = (Byte)(value >> 48);
            bytes[offset + 2] = (Byte)(value >> 40);
            bytes[offset + 3] = (Byte)(value >> 32);
            bytes[offset + 4] = (Byte)(value >> 24);
            bytes[offset + 5] = (Byte)(value >> 16);
            bytes[offset + 6] = (Byte)(value >>  8);
            bytes[offset + 7] = (Byte)value;
        }
        /*
        public static void BigEndianSetUInt32Enum<EnumType>(this Byte[] bytes, Int32 offset, EnumType value)
        {
            UInt32 valueAsUInt32 = Convert.ToUInt32(value);
            bytes[offset    ] = (Byte)(valueAsUInt32 >> 24);
            bytes[offset + 1] = (Byte)(valueAsUInt32 >> 16);
            bytes[offset + 2] = (Byte)(valueAsUInt32 >> 8);
            bytes[offset + 3] = (Byte)valueAsUInt32;
        }
        public static void BigEndianSetInt32(this Byte[] bytes, Int32 offset, Int32 value)
        {
            bytes[offset    ] = (Byte)(value >> 24);
            bytes[offset + 1] = (Byte)(value >> 16);
            bytes[offset + 2] = (Byte)(value >> 8);
            bytes[offset + 3] = (Byte)value;
        }
        */
        //
        // Read 8-byte types
        //
        public static UInt64 BigEndianReadUInt64(this Byte[] bytes, UInt32 offset)
        {
            return (UInt64)(
                (bytes[offset    ] << 56) |
                (bytes[offset    ] << 48) |
                (bytes[offset    ] << 40) |
                (bytes[offset    ] << 32) |
                (bytes[offset    ] << 24) |
                (bytes[offset + 1] << 16) |
                (bytes[offset + 2] <<  8) |
                (bytes[offset + 3]      ) );
        }

        public static Boolean IsInRange(UInt32 value, Byte byteCount)
        {
            switch (byteCount)
            {
                case 1:
                    return ((value & 0xFF) == value);
                case 2:
                    return ((value & 0xFFFF) == value);
                case 3:
                    return ((value & 0xFFFFFF) == value);
                case 4:
                    return ((value & 0xFFFFFFFF) == value);
            }
            throw new InvalidOperationException(String.Format("Expected byteCount to be 1,2,3 or 4 but was {0}", byteCount));
        }
        public static void BigEndianSetUInt32Subtype(this byte[] array, UInt32 offset, UInt32 value, Byte byteCount)
        {
            switch (byteCount)
            {
                case 1:
                    array[offset] = (Byte)value;
                    return;
                case 2:
                    array[offset    ] = (Byte)(value >> 8);
                    array[offset + 1] = (Byte)value;
                    return;
                case 3:
                    array[offset    ] = (Byte)(value >> 16);
                    array[offset + 1] = (Byte)(value >> 8);
                    array[offset + 2] = (Byte)value;
                    return;
                case 4:
                    array[offset    ] = (Byte)(value >> 24);
                    array[offset + 1] = (Byte)(value >> 16);
                    array[offset + 2] = (Byte)(value >> 8);
                    array[offset + 3] = (Byte)value;
                    return;
            }
            throw new InvalidOperationException(String.Format("Expected byteCount to be 1,2,3 or 4 but was {0}", byteCount));
        }
        public static UInt32 BigEndianReadUInt32Subtype(this byte[] bytes, UInt32 offset, Byte byteCount)
        {
            switch (byteCount)
            {
                case 1:
                    return (UInt32)(
                        (0x000000FFU & (bytes[offset])));
                case 2:
                    return (UInt32)(
                        (0x0000FF00U & (bytes[offset    ] << 8)) |
                        (0x000000FFU & (bytes[offset + 1])));
                case 3:
                    return (UInt32)(
                        (0x00FF0000U & (bytes[offset    ] << 16)) |
                        (0x0000FF00U & (bytes[offset + 1] <<  8)) |
                        (0x000000FFU & (bytes[offset + 2]      )) );
                case 4:
                    return (UInt32) (
                        (bytes[offset    ] << 24) |
                        (bytes[offset + 1] << 16) |
                        (bytes[offset + 2] <<  8) |
                        (bytes[offset + 3]      ) );
            }
            throw new InvalidOperationException(String.Format("Expected byteCount to be 1,2,3 or 4 but was {0}", byteCount));
        }

        public static void LittleEndianSetUInt16(this Byte[] array, UInt32 offset, UInt16 value)
        {
            array[offset + 1] = (Byte)(value >> 8);
            array[offset    ] = (Byte)(value     );
        }
        public static UInt16 LittleEndianReadUInt16(this Byte[] bytes, UInt32 offset)
        {
            return (UInt16)(
                (0xFF00 & (bytes[offset + 1] << 8)) |
                (0x00FF & (bytes[offset])));
        }
        public static void LittleEndianSetUInt32(this Byte[] array, UInt32 offset, UInt32 value)
        {
            array[offset + 3] = (Byte)(value >> 24);
            array[offset + 2] = (Byte)(value >> 16);
            array[offset + 1] = (Byte)(value >>  8);
            array[offset    ] = (Byte)(value      );
        }
        public static UInt32 LittleEndianReadUInt32(this Byte[] bytes, UInt32 offset)
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
        public static void ParseHex(this Byte[] bytes, Int32 offset, String hexString, Int32 hexStringOffset, Int32 hexStringLength)
        {
            if (hexStringLength % 2 == 1) throw new InvalidOperationException(String.Format(
                 "The input hex string length must be even but you provided an odd length {0}", hexStringLength));
            Int32 hexStringLimit = hexStringOffset + hexStringLength;
            while (hexStringOffset < hexStringLimit)
            {
                bytes[offset++] = (Byte)(
                    (hexString[hexStringOffset    ].HexValue() << 4) +
                     hexString[hexStringOffset + 1].HexValue()       );
                hexStringOffset += 2;
            }
        }
        public static Byte[] CreateSubArray(this Byte[] bytes, UInt32 offset, UInt32 length)
        {
            Byte[] subArray = new Byte[length];
            ArrayCopier.Copy(bytes, offset, subArray, 0, length);
            return subArray;
        }
        public static SByte[] CreateSubSByteArray(this Byte[] bytes, UInt32 offset, UInt32 length)
        {
            SByte[] subArray = new SByte[length];
            ArrayCopier.Copy(bytes, offset, subArray, 0, length);
            return subArray;
        }
    }
    public static class ListExtensions
    {
        public static String ToDataString<T>(this List<T> list)
        {
            StringBuilder builder = new StringBuilder();
            ToDataString<T>(list, builder);
            return builder.ToString();
        }
        public static void ToDataString<T>(this List<T> list, StringBuilder builder)
        {
            if (list == null)
            {
                builder.Append("null");
                return;
            }

            builder.Append('[');
            Boolean atFirst = true;
            for (int i = 0; i < list.Count; i++)
            {
                if (atFirst) { atFirst = false; } else { builder.Append(','); }
                builder.Append(list[i]);
            }
            builder.Append("]");
        }
    }

    public static class IListExtensions
    {
        public static String ListString<T>(this IList<T> list)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0) builder.Append(',');
                T item = list[i];
                if(item == null) builder.Append("null");
                else builder.Append(item.ToString());
            }
            builder.Append(']');
            return builder.ToString();
        }
    }

    public static class DictionaryExtensions
    {
        public static U GetValueNiceErrorMessage<T,U>(this Dictionary<T,U> dictionary, T key)
        {
            U value;
            if (!dictionary.TryGetValue(key, out value)) throw new KeyNotFoundException(String.Format("Key '{0}' was not found in the dictionary",
                 key.SerializeObject()));
            return value;
        }
        public static String ToDataString<T,U>(this Dictionary<T,U> dictionary)
        {
            StringBuilder builder = new StringBuilder();
            ToDataString<T,U>(dictionary, builder);
            return builder.ToString();
        }
        public static void ToDataString<T,U>(this Dictionary<T,U> dictionary, StringBuilder builder)
        {
            if (dictionary == null)
            {
                builder.Append("null");
                return;
            }

            builder.Append('{');
            Boolean atFirst = true;
            foreach (KeyValuePair<T,U> pair in dictionary)
            {
                if (atFirst) { atFirst = false; } else { builder.Append(','); }
                builder.Append(pair.Key);
                builder.Append(':');
                builder.Append(pair.Value);
            }
            builder.Append('}');
        }
        public static U GetOrCreate<T, U>(this Dictionary<T, U> dictionary, T key) where U : new()
        {
            U value;
            if (dictionary.TryGetValue(key, out value)) return value;

            value = new U();
            dictionary.Add(key, value);
            return value;
        }
    }
    public static class StackExtensions
    {
        public static void Print<T>(this Stack<T> stack, TextWriter writer)
        {
            if (stack.Count <= 0)
            {
                writer.WriteLine("Empty");
            }
            else
            {
                foreach (T item in stack)
                {
                    writer.WriteLine(item);
                }
            }
        } 
    }
    public static class DateTimeExtensions
    {
        public static readonly DateTime UnixZeroTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /*
        public static double ToUnixTimeDouble(this DateTime dateTime)
        {
            return (double)(dateTime - UnixZeroTime).TotalSeconds;
        }
        */
        public static UInt32 ToUnixTime(this DateTime dateTime)
        {
            return (UInt32)(dateTime - UnixZeroTime).TotalSeconds;
        }
    }
    public static class StopwatchExtensions
    {
        private static Double StopwatchTicksPerMillisecondAsDouble = 1000.0 / Stopwatch.Frequency;
        //private static Double StopwatchTicksPerMicrosecondAsDouble = 1000000.0 / Stopwatch.Frequency;

        private static String stopwatchTickIntervalString = null;
        public static String StopwatchTickIntervalString
        {
            get
            {
                if (stopwatchTickIntervalString == null)
                {
                    Int64 frequency = Stopwatch.Frequency;
                    switch (frequency)
                    {
                        case 1000: stopwatchTickIntervalString = "milliseconds"; break;
                        case 1000000: stopwatchTickIntervalString = "microsecond"; break;
                        case 1000000000: stopwatchTickIntervalString = "nanoseconds"; break;
                        default: throw new InvalidOperationException(String.Format("Unknown stopwatch frequency: '{0}' (Expected 1000, 1000000 or 1000000000)", frequency));
                    }
                }
                return stopwatchTickIntervalString;
            }
        }
        public static Int64 MillisToStopwatchTicks(this Int32 millis)
        {
            return Stopwatch.Frequency * (Int64)millis / 1000L;
        }
        public static Int64 MillisToStopwatchTicks(this UInt32 millis)
        {
            return Stopwatch.Frequency * (Int64)millis / 1000L;
        }
        public static Int64 StopwatchTicksAsMicroseconds(this Int64 stopwatchTicks)
        {
            return stopwatchTicks * 1000000L / Stopwatch.Frequency;
        }
        public static Double StopwatchTicksAsDoubleMicroseconds(this Int64 stopwatchTicks)
        {
            return (Double)(stopwatchTicks * 1000000L) / (Double)Stopwatch.Frequency;
        }
        public static Int32 StopwatchTicksAsInt32Milliseconds(this Int64 stopwatchTicks)
        {
            return (Int32)(stopwatchTicks * 1000 / Stopwatch.Frequency);
        }
        public static UInt32 StopwatchTicksAsUInt32Milliseconds(this Int64 stopwatchTicks)
        {
            return (UInt32)(stopwatchTicks * 1000 / Stopwatch.Frequency);
        }
        public static Int64 StopwatchTicksAsInt64Milliseconds(this Int64 stopwatchTicks)
        {
            return (Int64)(stopwatchTicks * 1000 / Stopwatch.Frequency);
        }
        public static Double StopwatchTicksAsDoubleMilliseconds(this Int64 stopwatchTicks)
        {
            return StopwatchTicksPerMillisecondAsDouble * stopwatchTicks;
        }
    }
    public static class SocketExtensions
    {
        public static String SafeLocalEndPointString(this Socket socket)
        {
            try { return socket.LocalEndPoint.ToString(); }
            catch (Exception) { return "<not-bound>"; }
        }
        public static String SafeRemoteEndPointString(this Socket socket)
        {
            try { return socket.RemoteEndPoint.ToString(); } catch (Exception) { return "<disconnected>";  }
        }
        public static void ReadFullSize(this Socket socket, Byte[] buffer, Int32 offset, Int32 size)
        {
            int lastBytesRead;

            do
            {
                lastBytesRead = socket.Receive(buffer, offset, size, SocketFlags.None);
                size -= lastBytesRead;

                if (size <= 0) return;

                offset += lastBytesRead;
            } while (lastBytesRead > 0);

            throw new IOException(String.Format("reached end of stream: still needed {0} bytes", size));
        }
        public static void SendFile(this Socket socket, String filename, Byte[] transferBuffer)
        {
            using(FileStream fileStream = new FileStream(filename, FileMode.Open))
            {
                Int32 bytesRead;
                while ((bytesRead = fileStream.Read(transferBuffer, 0, transferBuffer.Length)) > 0)
                {
                    socket.Send(transferBuffer, 0, bytesRead, SocketFlags.None);
                }
            }
        }
        public static void ShutdownAndDispose(this Socket socket)
        {
            if (socket != null)
            {
                try { if (socket.Connected) { socket.Shutdown(SocketShutdown.Both); } }
                catch (Exception) { }
                socket.Close();
            }
        }
    }
    public static class TextWriterExtensions
    {
        public static void WriteLine(this TextWriter writer, UInt32 spaces, String fmt, params Object[] obj)
        {
            writer.Write(String.Format("{{0,{0}}}", spaces), String.Empty);
            writer.WriteLine(fmt, obj);
        }
        public static void WriteLine(this TextWriter writer, UInt32 spaces, String str)
        {
            writer.Write(String.Format("{{0,{0}}}", spaces), String.Empty);
            writer.WriteLine(str);
        }

        public static void Write(this TextWriter writer, UInt32 spaces, String fmt, params Object[] obj)
        {
            writer.Write(String.Format("{{0,{0}}}", spaces), String.Empty);
            writer.Write(fmt, obj);
        }
        public static void Write(this TextWriter writer, UInt32 spaces, String str)
        {
            writer.Write(String.Format("{{0,{0}}}", spaces), String.Empty);
            writer.Write(str);
        }
    }
    public static class StreamExtensions
    {
        public static void ReadFullSize(this Stream stream, Byte[] buffer, Int32 offset, Int32 size)
        {
            int lastBytesRead;

            do
            {
                lastBytesRead = stream.Read(buffer, offset, size);
                size -= lastBytesRead;

                if (size <= 0) return;

                offset += lastBytesRead;
            } while (lastBytesRead > 0);

            throw new IOException(String.Format("Reached end of stream but still expected {0} bytes", size));
        }
        public static void ReadFullSize(this Stream stream, StringBuilder builder, Encoding encoding, Byte[] buffer, Int32 size)
        {
            int lastBytesRead;

            do
            {
                lastBytesRead = stream.Read(buffer, 0, buffer.Length);
                size -= lastBytesRead;

                if (size <= 0) return;

                builder.Append(encoding.GetString(buffer, 0, lastBytesRead));
            } while (lastBytesRead > 0);

            throw new IOException(String.Format("Reached end of stream but still expected {0} bytes", size));
        }

        /*
        public static String ReadLine(this Stream stream, StringBuilder builder)
        {
            builder.Length = 0;
            while (true)
            {
                int next = stream.ReadByte();
                if (next < 0)
                {
                    if (builder.Length == 0) return null;
                    return builder.ToString();
                }

                if (next == '\n') return builder.ToString();
                if (next == '\r')
                {
                    do
                    {
                        next = stream.ReadByte();
                        if (next < 0)
                        {
                            if (builder.Length == 0) return null;
                            return builder.ToString();
                        }
                        if (next == '\n') return builder.ToString();
                        builder.Append('\r');
                    } while (next == '\r');
                }

                builder.Append((char)next);
            }
        }
        */
    }

    public static class FileExtensions
    {
        public static Byte[] ReadFile(String filename)
        {
            //
            // 1. Get file size
            //
            FileInfo fileInfo = new FileInfo(filename);
            Int32 fileLength = (Int32)fileInfo.Length;
            Byte[] buffer = new Byte[fileLength];

            //
            // 2. Read the file contents
            //
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(filename, FileMode.Open);
                fileStream.ReadFullSize(buffer, 0, fileLength);
            }
            finally
            {
                if (fileStream != null) fileStream.Dispose();
            }

            return buffer;
        }
        public static String ReadFileToString(String filename)
        {
            FileStream fileStream = null;
            StreamReader reader = null;
            try
            {
                fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                reader = new StreamReader(fileStream);

                return reader.ReadToEnd();
            }
            finally
            {
                if(reader != null)
                {
                    reader.Dispose();
                }
                else
                {
                    if (fileStream != null) fileStream.Dispose();
                }
            }
        }

        public static Int32 ReadFile(FileInfo fileInfo, Int32 fileOffset, Byte[] buffer, FileShare shareOptions, out Boolean reachedEndOfFile)
        {
            fileInfo.Refresh();

            Int64 fileSize = fileInfo.Length;

            if (fileOffset >= fileSize)
            {
                reachedEndOfFile = true;
                return 0;
            }

            Int64 fileSizeFromOffset = fileSize - fileOffset;

            Int32 readLength;
            if (fileSizeFromOffset > (Int64)buffer.Length)
            {
                reachedEndOfFile = false;
                readLength = buffer.Length;
            }
            else
            {
                reachedEndOfFile = true;
                readLength = (Int32)fileSizeFromOffset;
            }
            if (readLength <= 0) return 0;

            using (FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, shareOptions))
            {
                fileStream.Position = fileOffset;
                fileStream.ReadFullSize(buffer, 0, readLength);
            }

            return readLength;
        }


        public static void SaveStringToFile(String filename, FileMode mode, String contents)
        {
            FileStream fileStream = null;
            StreamWriter writer = null;
            try
            {
                fileStream = new FileStream(filename, mode, FileAccess.Write);
                writer = new StreamWriter(fileStream);

                writer.Write(contents);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Dispose();
                }
                else
                {
                    if (fileStream != null) fileStream.Dispose();
                }
            }
        }

    }
}
