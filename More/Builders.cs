using System;
using System.IO;
using System.Text;

namespace More
{
    public delegate void ByteAppender(ByteBuilder builder);
    public class ByteBuilder
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
        public void Append(Byte c)
        {
            EnsureTotalCapacity(contentLength + 1);
            bytes[contentLength++] = c;
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
        public void Append(Encoding encoding, String content)
        {
            UInt32 encodedLength = (UInt32)encoding.GetByteCount(content);
            EnsureTotalCapacity(contentLength + encodedLength);
            encoding.GetBytes(content, 0, content.Length, bytes, (int)contentLength);
            contentLength += encodedLength;
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
        public String Decode(Encoding encoding)
        {
            return encoding.GetString(bytes, 0, (Int32)contentLength);
        }
    }
}
