using System;
using System.IO;
using System.Text;

using Marler.Common;

namespace Marler.Audio
{
    public static class Riff
    {
        public static readonly Byte[] RiffHeaderID = new Byte[] { (Byte)'R', (Byte)'I', (Byte)'F', (Byte)'F' };

        public static UInt32 VerifyChunkIdAndGetSize(BinaryStream stream, Byte[] expectedChunkID)
        {
            Byte[] chunkID = stream.ReadFullSize(4);
            if (chunkID[0] != expectedChunkID[0] || chunkID[1] != expectedChunkID[1] ||
                chunkID[2] != expectedChunkID[2] || chunkID[3] != expectedChunkID[3])
            {
                throw new FormatException(String.Format(
                    "Expected chunk id to be '{0}{1}{2}{3}' (0x{4:X2}{5:X2}{6:X2}{7:X2}) but got '{8}{9}{10}{11}' (0x{12})",
                    (Char)expectedChunkID[0], (Char)expectedChunkID[1], (Char)expectedChunkID[2], (Char)expectedChunkID[3],
                    expectedChunkID[0], expectedChunkID[1], expectedChunkID[2], expectedChunkID[3],
                    (Char)chunkID[0], (Char)chunkID[1], (Char)chunkID[2], (Char)chunkID[3],
                    chunkID.ToHexString(0, 4)));
            }

            UInt32 size = stream.LittleEndianReadUInt32();
            Console.WriteLine("RIFF '{0}{1}{2}{3}' size {4}",
                    (Char)chunkID[0], (Char)chunkID[1], (Char)chunkID[2], (Char)chunkID[3],
                    size);
            return size;

        }

        public static void Verify(BinaryStream stream, Byte[] expectedBytes, String context)
        {
            Byte[] actualBytes = stream.ReadFullSize(expectedBytes.Length);
            for (int i = 0; i < expectedBytes.Length; i++)
            {
                if (actualBytes[i] != expectedBytes[i])
                    throw new FormatException(String.Format(
                        "{0} '{1}' (0x{2}) but got '{3}' (0x{4})",
                        context, Encoding.ASCII.GetString(actualBytes, 0, expectedBytes.Length),
                        actualBytes.ToHexString(0, expectedBytes.Length),
                        Encoding.ASCII.GetString(expectedBytes, 0, expectedBytes.Length),
                        expectedBytes.ToHexString(0, expectedBytes.Length)));
            }
        }
    }
}