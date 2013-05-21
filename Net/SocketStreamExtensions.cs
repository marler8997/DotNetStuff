using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Marler.Net
{
    public static class SocketStreamExtensions
    {
        public static void SendFile(Socket socket, String filename, Byte[] transferBuffer)
        {
            if (filename == null)
            {
                Console.WriteLine("Please supply a filename");
                return;
            }

            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(filename, FileMode.Open);
                Int32 bytesRead;
                while ((bytesRead = fileStream.Read(transferBuffer, 0, transferBuffer.Length)) > 0)
                {
                    socket.Send(transferBuffer, 0, bytesRead, SocketFlags.None);
                }
            }
            finally
            {
                if (fileStream != null) fileStream.Close();
            }
        }
        public static void ReadFullSize(this Socket socket, byte[] buffer, int offset, int size)
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
    }
}
