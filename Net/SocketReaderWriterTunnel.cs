using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace More.Net
{
    public class StreamToSocket : ITunnel
    {
        readonly ITunnelCallback callback;

        readonly Stream readStream;
        readonly Socket writeSocket;

        readonly Int32 bufferSize;

        public StreamToSocket(ITunnelCallback callback, Stream readStream, Socket writeSocket, Int32 bufferSize)
        {
            this.callback = callback;

            if (readStream == null) throw new ArgumentNullException("readStream");
            if (writeSocket == null) throw new ArgumentNullException("writeSocket");

            this.readStream = readStream;
            this.writeSocket = writeSocket;

            this.bufferSize = bufferSize;
        }
        public void Run()
        {
            try
            {
                Byte[] buffer = new Byte[bufferSize];

                while (true)
                {
                    Int32 bytesRead;
                    try
                    {
                        bytesRead = readStream.Read(buffer, 0, buffer.Length);
                    }
                    catch (SocketException)
                    {
                        break;
                    }

                    if (bytesRead <= 0) break;

                    writeSocket.Send(buffer, 0, bytesRead, SocketFlags.None);
                }
            }
            finally
            {
                if (callback != null) callback.TunnelClosed(this);
            }
        }
    }
    public class SocketToStream : ITunnel
    {
        readonly ITunnelCallback callback;
        
        readonly Socket readSocket;
        readonly Stream writeStream;

        readonly Int32 bufferSize;

        public SocketToStream(ITunnelCallback callback, Socket readSocket, Stream writeStream, Int32 bufferSize)
        {
            this.callback = callback;

            if (readSocket == null) throw new ArgumentNullException("readSocket");
            if (writeStream == null) throw new ArgumentNullException("writeStream");

            this.readSocket = readSocket;
            this.writeStream = writeStream;

            this.bufferSize = bufferSize;
        }
        public void Run()
        {
            try
            {
                Byte[] buffer = new Byte[bufferSize];

                while (true)
                {
                    Int32 bytesRead;
                    try
                    {
                        bytesRead = readSocket.Receive(buffer, buffer.Length, SocketFlags.None);
                    }
                    catch (SocketException)
                    {
                        break;
                    }

                    if (bytesRead <= 0) break;

                    writeStream.Write(buffer, 0, bytesRead);
                }
            }
            finally
            {
                if (callback != null) callback.TunnelClosed(this);
            }
        }
    }

    public class TextReaderToSocket : ITunnel
    {
        readonly ITunnelCallback callback;

        readonly Encoding encoding;
        readonly TextReader reader;
        readonly Socket writeSocket;

        readonly Int32 charBufferSize;

        public TextReaderToSocket(ITunnelCallback callback, Encoding encoding,
            TextReader reader, Socket writeSocket, Int32 charBufferSize)
        {
            this.callback = callback;

            if (encoding == null) throw new ArgumentNullException("encoding");
            if (reader == null) throw new ArgumentNullException("reader");
            if (writeSocket == null) throw new ArgumentNullException("writeSocket");

            this.encoding = encoding;
            this.reader = reader;
            this.writeSocket = writeSocket;

            this.charBufferSize = charBufferSize;
        }
        public void Run()
        {
            try
            {
                Char[] charBuffer = new Char[charBufferSize];
                Byte[] byteBuffer = new Byte[encoding.GetMaxByteCount(charBufferSize)];

                while (true)
                {
                    Int32 charsRead;
                    try
                    {
                        charsRead = reader.Read(charBuffer, 0, charBuffer.Length);
                    }
                    catch (SocketException)
                    {
                        break;
                    }

                    if (charsRead <= 0) break;

                    Int32 byteCount = encoding.GetBytes(charBuffer, 0, charsRead, byteBuffer, 0);
                    writeSocket.Send(byteBuffer, 0, byteCount, SocketFlags.None);
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
        readonly ITunnelCallback callback;

        readonly Encoding encoding;
        readonly Socket socket;
        readonly TextWriter writer;

        readonly Int32 socketBufferSize;

        public SocketToTextWriter(ITunnelCallback callback, Encoding encoding,
            Socket socket, TextWriter writer, Int32 socketBufferSize)
        {
            if (encoding == null) throw new ArgumentNullException("encoding");
            if (socket == null) throw new ArgumentNullException("socket");
            if (writer == null) throw new ArgumentNullException("writer");

            this.callback = callback;

            this.encoding = encoding;
            this.socket = socket;
            this.writer = writer;

            this.socketBufferSize = socketBufferSize;
        }
        public void Run()
        {
            try
            {
                Byte[] byteBuffer = new Byte[socketBufferSize];
                Char[] charBuffer = new Char[encoding.GetMaxCharCount(socketBufferSize)];

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

                    Int32 charCount = encoding.GetChars(byteBuffer, 0, bytesRead, charBuffer, 0);
                    writer.Write(charBuffer, 0, charCount);
                }
            }
            finally
            {
                if (callback != null) callback.TunnelClosed(this);
            }
        }
    }
}
