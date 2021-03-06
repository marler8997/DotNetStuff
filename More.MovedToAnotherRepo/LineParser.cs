﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

#if WindowsCE
using ArrayCopier = System.MissingInCEArrayCopier;
#else
using ArrayCopier = System.Array;
#endif

namespace More
{
    public class LineParser
    {
        public readonly Encoding encoding;

        public readonly Buf buffer;
        UInt32 nextStartOfLineOffset;
        UInt32 nextIndexToCheck;
        UInt32 dataOffsetLimit;

        public LineParser(Encoding encoding, UInt32 lineBufferInitialCapacity, UInt32 lineBufferExpandLength)
        {
            this.encoding = encoding;

            this.buffer = new Buf(lineBufferInitialCapacity, lineBufferExpandLength);
            this.nextStartOfLineOffset = 0;
            this.nextIndexToCheck = 0;
            this.dataOffsetLimit = 0;
        }
        public void Add(Byte[] data)
        {
            buffer.EnsureCapacityCopyData(this.dataOffsetLimit + (UInt32)data.Length);
            ArrayCopier.Copy(data, 0, buffer.array, this.dataOffsetLimit, data.Length);
            this.dataOffsetLimit += (UInt32)data.Length;
        }
        public void Add(Byte[] data, UInt32 offset, UInt32 length)
        {
            buffer.EnsureCapacityCopyData(this.dataOffsetLimit + length);
            ArrayCopier.Copy(data, offset, buffer.array, this.dataOffsetLimit, length);
            this.dataOffsetLimit += length;
        }
        public String Flush()
        {
            if (dataOffsetLimit <= 0) return null;
            String rest = encoding.GetString(buffer.array, 0, (Int32)dataOffsetLimit);
            this.nextStartOfLineOffset = 0;
            this.nextIndexToCheck = 0;
            this.dataOffsetLimit = 0;
            return rest;
        }
        public String GetLine()
        {
            while (this.nextIndexToCheck < this.dataOffsetLimit)
            {
                if (buffer.array[this.nextIndexToCheck] == '\n')
                {
                    String line = encoding.GetString(buffer.array, (Int32)this.nextStartOfLineOffset, (Int32)
                        (this.nextIndexToCheck + ((this.nextIndexToCheck > this.nextStartOfLineOffset && buffer.array[nextIndexToCheck - 1] == '\r') ? -1 : 0) - this.nextStartOfLineOffset));

                    this.nextIndexToCheck++;
                    this.nextStartOfLineOffset = this.nextIndexToCheck;
                    return line;
                }
                this.nextIndexToCheck++;
            }

            //
            // Move remaining data to the beginning of the buffer
            //
            if (this.nextStartOfLineOffset <= 0 || this.nextStartOfLineOffset >= this.dataOffsetLimit) return null;
            
            UInt32 copyLength = this.dataOffsetLimit - this.nextStartOfLineOffset;
            ArrayCopier.Copy(buffer.array, this.nextStartOfLineOffset, buffer.array, 0, copyLength);
            this.nextStartOfLineOffset = 0;
            this.nextIndexToCheck = 0;
            this.dataOffsetLimit = copyLength;

            return null;
        }
        public UInt32 GetLineBytes(out UInt32 outOffset)
        {
            while (this.nextIndexToCheck < this.dataOffsetLimit)
            {
                if (buffer.array[this.nextIndexToCheck] == '\n')
                {
                    outOffset = this.nextStartOfLineOffset;
                    UInt32 nextLineLength = this.nextIndexToCheck - this.nextStartOfLineOffset;
                    if(this.nextIndexToCheck > this.nextStartOfLineOffset && buffer.array[nextIndexToCheck - 1] == '\r')
                    {
                        nextLineLength--;
                    }

                    this.nextIndexToCheck++;
                    this.nextStartOfLineOffset = this.nextIndexToCheck;
                    return nextLineLength;
                }
                this.nextIndexToCheck++;
            }

            //
            // Move remaining data to the beginning of the buffer
            //
            if (this.nextStartOfLineOffset <= 0 || this.nextStartOfLineOffset >= this.dataOffsetLimit)
            {
                outOffset = 0;
                return 0;
            }

            UInt32 copyLength = this.dataOffsetLimit - this.nextStartOfLineOffset;
            ArrayCopier.Copy(buffer.array, this.nextStartOfLineOffset, buffer.array, 0, copyLength);
            this.nextStartOfLineOffset = 0;
            this.nextIndexToCheck = 0;
            this.dataOffsetLimit = copyLength;

            outOffset = 0;
            return 0;
        }
    }
    public interface ILineReader : IDisposable
    {
        String ReadLine();
    }
    public class TextLineReader : ILineReader
    {
        readonly TextReader reader;
        public TextLineReader(TextReader reader)
        {
            this.reader = reader;
        }
        public String ReadLine() { return reader.ReadLine(); }
        public void Dispose() { reader.Dispose();  }
    }
    public class StreamLineReader : ILineReader
    {
        readonly Stream stream;
        readonly LineParser lineParser;
        readonly Byte[] receiveBuffer;

        public StreamLineReader(Encoding encoding, Stream stream)
            : this(encoding, stream, Buf.DefaultInitialCapacity, Buf.DefaultExpandLength)
        {
        }
        public StreamLineReader(Encoding encoding, Stream stream, UInt32 lineBufferInitialCapacity, UInt32 lineBufferExpandLength)
        {
            this.stream = stream;
            this.lineParser = new LineParser(encoding, lineBufferInitialCapacity, lineBufferExpandLength);
            this.receiveBuffer = new Byte[512];
        }
        public void Dispose()
        {
            stream.Dispose();
        }
        public string ReadLine()
        {
            while (true)
            {
                String line = lineParser.GetLine();
                if (line != null) return line;

                Int32 bytesRead = stream.Read(receiveBuffer, 0, receiveBuffer.Length);
                if (bytesRead <= 0) return null;

                lineParser.Add(receiveBuffer, 0, (UInt32)bytesRead);
            }
        }
    }
    public class SocketLineReader : ILineReader
    {
        public readonly Socket socket;
        readonly LineParser lineParser;
        readonly Byte[] receiveBuffer;

        public SocketLineReader(Socket socket, Encoding encoding, UInt32 lineBufferInitialCapacity, UInt32 lineBufferExpandLength)
        {
            this.socket = socket;
            this.lineParser = new LineParser(encoding, lineBufferInitialCapacity, lineBufferExpandLength);
            this.receiveBuffer = new Byte[512];
        }
        public void Dispose()
        {
            Socket socket = this.socket;
            if (socket != null)
            {
                try
                {
                    if (socket.Connected) socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {
                }
                try { socket.Close(); }
                catch (Exception) { }
            }
        }
        public String ReadLine()
        {
            while (true)
            {
                String line = lineParser.GetLine();
                if (line != null) return line;

                Int32 bytesRead = socket.Receive(receiveBuffer);
                if (bytesRead <= 0) return null;

                lineParser.Add(receiveBuffer, 0, (UInt32)bytesRead);
            }
        }
    }
}
