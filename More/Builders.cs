using System;
using System.IO;
using System.Text;

namespace More
{
    // AppendAscii = The data being appended is guaranteed to be valid ascii (0-127)
    //               Note: this does not include the extended ascii codes (128 - 255)
    // AppendUtf8  = If the data is a Char or String, it will be appended as utf8 encoded
    // AppendNumber = Append the number converted to a string
    public interface ITextBuilder
    {
        void Clear();

        // The caller is guaranteeing that 0 <= c <= 127
        void AppendAscii(Byte c);
        // The caller is guaranteeing that 0 <= c <= 127
        void AppendAscii(Char c);
        // The caller is guaranteeing that every char in str is between 0 and 127 (inclusive)
        void AppendAscii(String str);

        void AppendUtf8(Char c);
        void AppendUtf8(String str);

        void Append(Encoder encoder, String str);

        void AppendBoolean(Boolean value);

        void AppendNumber(UInt32 num);
        void AppendNumber(UInt32 num, Byte @base);
        void AppendNumber(Int32 num);
        void AppendNumber(Int32 num, Byte @base);
    }
    public struct StringDataBuilder : ITextBuilder
    {
        public readonly StringBuilder builder;
        public StringDataBuilder(StringBuilder builder)
        {
            this.builder = builder;
        }
        public void Clear()
        {
            this.builder.Length = 0;
        }
        // The caller is guaranteeing that 0 <= c <= 127
        public void AppendAscii(Byte c)
        {
            builder.Append((Char)c);
        }
        // The caller is guaranteeing that 0 <= c <= 127
        public void AppendAscii(Char c)
        {
            builder.Append(c);
        }
        // The caller is guaranteeing that every char in str is between 0 and 127 (inclusive)
        public void AppendAscii(String str)
        {
            builder.Append(str);
        }

        public void AppendUtf8(Char c)
        {
            builder.Append(c);
        }
        public void AppendUtf8(String str)
        {
            builder.Append(str);
        }

        public void Append(Encoder encoder, String str)
        {
            builder.Append(str);
        }

        public void AppendBoolean(Boolean value)
        {
            builder.Append(value ? "true" : "false");
        }

        public void AppendNumber(UInt32 num)
        {
            builder.Append(num);
        }
        public void AppendNumber(UInt32 num, Byte @base)
        {
            builder.Append(num.ToString("X"));
        }
        public void AppendNumber(Int32 num)
        {
            builder.Append(num);
        }
        public void AppendNumber(Int32 num, Byte @base)
        {
            builder.Append(num.ToString("X"));
        }
    }
    public delegate void ByteAppender(ByteBuilder builder);
    public class ByteBuilder : ITextBuilder
    {
        const UInt32 DefaultInitialLength = 16;

        public Byte[] bytes;
        public UInt32 contentLength;
        public ByteBuilder()
            : this(DefaultInitialLength)
        {
        }
        public ByteBuilder(UInt32 initialLength)
        {
            this.bytes = new Byte[initialLength];
            this.contentLength = 0;
        }
        public void Clear()
        {
            this.contentLength = 0;
        }
        public void EnsureTotalCapacity(UInt32 capacity)
        {
            if (bytes.Length < capacity)
            {
                UInt32 newLength = (UInt32)bytes.Length * 2U;
                if (newLength < capacity)
                {
                    newLength = capacity;
                }
                var newBytes = new Byte[newLength];
                Array.Copy(bytes, newBytes, contentLength);
                bytes = newBytes;
            }
        }
        public void ReadUntilClosed(Stream stream, UInt32 minimumReadBuffer)
        {
            while (true)
            {
                EnsureTotalCapacity(contentLength + minimumReadBuffer);
                Int32 bytesReceived = stream.Read(bytes, (int)contentLength, (int)(bytes.Length - contentLength));
                if (bytesReceived <= 0)
                    return;
                contentLength += (UInt32)bytesReceived;
            }
        }

        // The caller is guaranteeing that 0 <= c <= 127
        public void AppendAscii(Byte c)
        {
            EnsureTotalCapacity(contentLength + 1);
            bytes[contentLength++] = c;
        }
        // The caller is guaranteeing that 0 <= c <= 127
        public void AppendAscii(Char c)
        {
            EnsureTotalCapacity(contentLength + 1);
            bytes[contentLength++] = (Byte)c;
        }
        // The caller is guaranteeing that every char in str is between 0 and 127 (inclusive)
        public void AppendAscii(String str)
        {
            EnsureTotalCapacity(contentLength + (uint)str.Length);
            for (int i = 0; i < str.Length; i++)
            {
                bytes[contentLength + i] = (Byte)str[i]; // Can do since this must be an Ascii string
            }
            contentLength += (uint)str.Length;
        }

        public void AppendUtf8(Char c)
        {
            EnsureTotalCapacity(contentLength + Utf8.MaxCharEncodeLength);
            contentLength = Utf8.EncodeChar(c, bytes, contentLength);
        }
        public void AppendUtf8(String str)
        {
            UInt32 encodeLength = Utf8.GetEncodeLength(str);
            EnsureTotalCapacity(contentLength + encodeLength);
            Utf8.Encode(str, bytes, contentLength);
            contentLength += encodeLength;
        }

        public void Append(Encoder encoder, String str)
        {
            var encodeLength = encoder.GetEncodeLength(str);
            EnsureTotalCapacity(contentLength + encodeLength);
            encoder.Encode(str, bytes, contentLength);
            contentLength += encodeLength;
        }

        public void Append(Byte[] content)
        {
            EnsureTotalCapacity(contentLength + (UInt32)content.Length);
            Array.Copy(content, 0, bytes, contentLength, content.Length);
            contentLength += (UInt32)content.Length;
        }
        public void Append(Byte[] content, UInt32 offset, UInt32 length)
        {
            EnsureTotalCapacity(contentLength + length);
            Array.Copy(content, offset, bytes, contentLength, length);
            contentLength += length;
        }

        public void AppendBoolean(Boolean value)
        {
            AppendAscii(value ? "true" : "false");
        }

        public void AppendNumber(Int32 num)
        {
            AppendNumber(num, 10);
        }
        public void AppendNumber(Int32 num, Byte @base)
        {
            // Enusure Capacity for max value
            if (@base >= 10)
            {
                EnsureTotalCapacity(contentLength + 11); // 11 = -2147483648 (also works with larger bases)
            }
            else
            {
                EnsureTotalCapacity(contentLength + 33); // 32 = '-' 11111111 11111111 1111111 11111111
            }
            if (num < 0)
            {
                bytes[contentLength++] = (Byte)'-';
                AppendNumber((UInt32) (-num), @base);
            }
            else
            {
                AppendNumber((UInt32)num, @base);
            }
        }
        public const String Chars = "0123456789ABCDEF";
        public void AppendNumber(UInt32 num)
        {
            AppendNumber(num, 10);
        }
        public void AppendNumber(UInt32 num, Byte @base)
        {
            if(@base > Chars.Length)
                throw new ArgumentOutOfRangeException("base", @base, String.Format("base cannot be greater than {0}", Chars.Length));

            // Enusure Capacity for max value
            if (@base >= 10)
            {
                EnsureTotalCapacity(contentLength + 10); // 10 = 4294967295 (also works with larger bases)
            }
            else
            {
                EnsureTotalCapacity(contentLength + 32); // 32 = 11111111 11111111 1111111 11111111
            }

            if (num == 0)
            {
                bytes[contentLength++] = (Byte)'0';
            }
            else
            {
                var start = contentLength;
                do
                {
                    bytes[contentLength++] = (Byte)Chars[(int)(num % @base)];
                    num = num / @base;
                } while (num != 0);
                
                // reverse the string
                UInt32 limit = ( (contentLength - start) / 2);
                for (UInt32 i = 0; i < limit; i++)
                {
                    var temp = bytes[start + i];
                    bytes[start + i] = bytes[contentLength - 1 - i];
                    bytes[contentLength - 1 - i] = temp;
                }
            }
        }
        /*
        public String Decode(Encoding encoding)
        {
            return encoding.GetString(bytes, 0, (Int32)contentLength);
        }
        */
    }
}
