using System;
using System.IO;

namespace Marler.Audio
{
    public static class WaveWriter
    {




        public static void WriteWaveFile(Stream waveFile, byte[] waveData)
        {
            Int32 offset;

            //
            // Make the format chunk
            //
            Byte[] waveFormatChunk = new Byte[24];

            waveFormatChunk[0] = Wave.FmtHeaderID[0];
            waveFormatChunk[1] = Wave.FmtHeaderID[1];
            waveFormatChunk[2] = Wave.FmtHeaderID[2];
            waveFormatChunk[3] = Wave.FmtHeaderID[3];
            offset = ((UInt32)waveFormatChunk.Length - 8).ToLittleEndian(waveFormatChunk, 4);

            offset = ((UInt16)1).ToLittleEndian(waveFormatChunk, offset);     // audio format
            offset = ((UInt16)1).ToLittleEndian(waveFormatChunk, offset);     // number of channels
            offset = ((UInt32)44100).ToLittleEndian(waveFormatChunk, offset); // sample rate
            offset = ((UInt32)0).ToLittleEndian(waveFormatChunk, offset);     // Bytes per second
            offset = ((UInt16)2).ToLittleEndian(waveFormatChunk, offset);     // Block align
            offset = ((UInt16)16).ToLittleEndian(waveFormatChunk, offset);     // Bits per sample


            //
            // Write riff header
            //
            RiffHelper.WriteChunkHeader(waveFile, Riff.RiffHeaderID, (UInt32)(waveFormatChunk.Length + waveData.Length));
            waveFile.Write(Wave.WaveHeaderID, 0, Wave.WaveHeaderID.Length);

            //
            // Write the wave "fmt " chunk
            //
            waveFile.Write(waveFormatChunk, 0, offset);



            //
            // Write the data
            //
            RiffHelper.WriteChunkHeader(waveFile, Wave.DataHeaderID, (UInt32)(waveData.Length));
            waveFile.Write(waveData, 0, waveData.Length);

        }
    }
}
