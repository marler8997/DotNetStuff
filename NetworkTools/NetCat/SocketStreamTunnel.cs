using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace Marler.NetworkTools
{
    public interface ITunnelCallback
    {
        void ClosedAfterRead();
        void CannotWrite(Byte[] leftoverData, Int32 index, Int32 count);
        void Exception(Exception e);
        void FinallyBlock();
    }

    public class TextReaderToSocket
    {
        private readonly ITunnelCallback callback;
        private readonly TextReader reader;
        private readonly Socket socket;
        private readonly Int32 bufferSize;

        public TextReaderToSocket(ITunnelCallback callback, TextReader reader, Socket socket, Int32 bufferSize)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            if (reader == null) throw new ArgumentNullException("reader");
            if (socket == null) throw new ArgumentNullException("socket");

            this.callback = callback;
            this.reader = reader;
            this.socket = socket;
            this.bufferSize = bufferSize;
        }

        public void Run()
        {
            try
            {
                Char[] charBuffer = new Char[bufferSize];
                Byte[] byteBuffer = new Byte[bufferSize];

                while (true)
                {
                    Int32 bytesRead = reader.Read(charBuffer, 0, charBuffer.Length);

                    if (bytesRead <= 0)
                    {
                        callback.ClosedAfterRead();
                        break;
                    }

                    Encoding.UTF8.GetBytes(charBuffer, 0, bytesRead, byteBuffer, bytesRead);

                    if (!socket.Connected)
                    {
                        callback.CannotWrite(byteBuffer, 0, bytesRead);
                        break;
                    }

                    socket.Send(byteBuffer, 0, bytesRead, SocketFlags.None);
                }
            }
            catch (Exception e)
            {
                callback.Exception(e);
            }
            finally
            {
                callback.FinallyBlock();
            }
        }
    }

    public class SocketToTextWriter
    {
        private readonly ITunnelCallback callback;
        private readonly TextWriter writer;
        private readonly Socket socket;
        private readonly Int32 bufferSize;

        public SocketToTextWriter(ITunnelCallback callback, Socket socket, TextWriter writer, Int32 bufferSize)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            if (socket == null) throw new ArgumentNullException("socket");
            if (writer == null) throw new ArgumentNullException("writer");

            this.callback = callback;
            this.socket = socket;
            this.writer = writer;
            this.bufferSize = bufferSize;
        }

        public void Run()
        {
            try
            {
                Byte[] byteBuffer = new Byte[bufferSize];
                Char[] charBuffer = new Char[bufferSize];

                while (true)
                {
                    Int32 bytesRead = socket.Receive(byteBuffer, 0, byteBuffer.Length, SocketFlags.None);

                    if (bytesRead <= 0)
                    {
                        callback.ClosedAfterRead();
                        break;
                    }

                    Encoding.UTF8.GetChars(byteBuffer, 0, bytesRead, charBuffer, 0);

                    writer.Write(charBuffer, 0, bytesRead);
                }
            }
            catch (Exception e)
            {
                callback.Exception(e);
            }
            finally
            {
                callback.FinallyBlock();
            }
        }
    }


}
