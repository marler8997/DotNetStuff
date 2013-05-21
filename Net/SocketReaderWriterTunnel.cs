using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Marler.Net
{
    public class TextReaderToSocket : ITunnel
    {
        private readonly ITunnelCallback callback;
        private readonly TextReader reader;
        private readonly Socket socket;
        private readonly Int32 bufferSizes;

        public TextReaderToSocket(ITunnelCallback callback, TextReader reader, Socket socket, Int32 bufferSizes)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            if (socket == null) throw new ArgumentNullException("socket");

            this.callback = callback;

            this.reader = reader;
            this.socket = socket;
            this.bufferSizes = bufferSizes;
        }
        public void Run()
        {
            try
            {
                Char[] charBuffer = new Char[bufferSizes];
                Byte[] byteBuffer = new Byte[bufferSizes];

                while (true)
                {
                    Int32 bytesRead;
                    try
                    {
                        bytesRead = reader.Read(charBuffer, 0, charBuffer.Length);
                    }
                    catch (SocketException)
                    {
                        break;
                    }

                    if (bytesRead <= 0) return;

                    Encoding.UTF8.GetBytes(charBuffer, 0, bytesRead, byteBuffer, 0);
                    socket.Send(byteBuffer, 0, bytesRead, SocketFlags.None);
                }
            }
            finally
            {
                if (callback != null) callback.TunnelClosed(this);
            }
        }
    }
    public class SocketToTextWriter : ITunnel
    {
        private readonly ITunnelCallback callback;
        private readonly TextWriter writer;
        private readonly Socket socket;
        private readonly Int32 bufferSizes;

        public SocketToTextWriter(ITunnelCallback callback, Socket socket, TextWriter writer, Int32 bufferSizes)
        {
            if (socket == null) throw new ArgumentNullException("socket");
            if (writer == null) throw new ArgumentNullException("writer");

            this.callback = callback;

            this.socket = socket;
            this.writer = writer;
            this.bufferSizes = bufferSizes;
        }
        public void Run()
        {
            try
            {
                Byte[] byteBuffer = new Byte[bufferSizes];
                Char[] charBuffer = new Char[bufferSizes];

                while (true)
                {
                    Int32 bytesRead;
                    try
                    {
                        bytesRead = socket.Receive(byteBuffer, 0, byteBuffer.Length, SocketFlags.None);
                    }
                    catch (SocketException)
                    {
                        break;
                    }

                    if (bytesRead <= 0) return;

                    Encoding.UTF8.GetChars(byteBuffer, 0, bytesRead, charBuffer, 0);
                    writer.Write(charBuffer, 0, bytesRead);
                }
            }
            finally
            {
                if (callback != null) callback.TunnelClosed(this);
            }
        }
    }
}
