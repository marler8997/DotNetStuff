using System;
using System.IO;

namespace Marler.Audio
{
    public static class RiffHelper
    {
        public static void WriteChunkHeader(Stream stream, Byte[] chunkId, UInt32 dataLength)
        {
            stream.WriteByte(chunkId[0]);
            stream.WriteByte(chunkId[1]);
            stream.WriteByte(chunkId[2]);
            stream.WriteByte(chunkId[3]);

            stream.WriteByte((Byte)(dataLength >> 24));
            stream.WriteByte((Byte)(dataLength >> 16));
            stream.WriteByte((Byte)(dataLength >> 8));
            stream.WriteByte((Byte)(dataLength));

        }
    }
}
