using System;
using System.Text;
using System.Net.Sockets;

namespace More
{
    public class AsciiLineParser
    {
        readonly ByteBuffer buffer;
        Int32 nextStartOfLineOffset;
        Int32 nextIndexToCheck;
        Int32 dataOffsetLimit;

        public AsciiLineParser()
        {
            this.buffer = new ByteBuffer();
            this.nextStartOfLineOffset = 0;
            this.nextIndexToCheck = 0;
            this.dataOffsetLimit = 0;
        }

        public void Add(Byte[] data)
        {
            buffer.EnsureCapacity(this.dataOffsetLimit + data.Length);
            Array.Copy(data, 0, buffer.array, this.dataOffsetLimit, data.Length);
            this.dataOffsetLimit += data.Length;
        }
        public void Add(Byte[] data, Int32 offset, Int32 length)
        {
            buffer.EnsureCapacity(this.dataOffsetLimit + length);
            Array.Copy(data, offset, buffer.array, this.dataOffsetLimit, length);
            this.dataOffsetLimit += length;
        }

        public String GetLine()
        {
            while (this.nextIndexToCheck < this.dataOffsetLimit)
            {
                if (buffer.array[this.nextIndexToCheck] == '\n')
                {
                    String line = Encoding.ASCII.GetString(buffer.array, this.nextStartOfLineOffset, this.nextIndexToCheck + 
                        ((this.nextIndexToCheck > this.nextStartOfLineOffset && buffer.array[nextIndexToCheck - 1] == '\r') ? -1 : 0) - this.nextStartOfLineOffset);

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
            
            Int32 copyLength = this.dataOffsetLimit - this.nextStartOfLineOffset;
            Array.Copy(buffer.array, this.nextStartOfLineOffset, buffer.array, 0, copyLength);
            this.nextStartOfLineOffset = 0;
            this.nextIndexToCheck = 0;
            this.dataOffsetLimit = copyLength;

            return null;
        }
    }

    public class AsciiSocket : IDisposable
    {
        public readonly Socket socket;
        readonly AsciiLineParser lineParser;
        readonly Byte[] receiveBuffer;

        public AsciiSocket(Socket socket)
        {
            this.socket = socket;
            this.lineParser = new AsciiLineParser();
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

                lineParser.Add(receiveBuffer, 0, bytesRead);
            }
        }
    }
}
