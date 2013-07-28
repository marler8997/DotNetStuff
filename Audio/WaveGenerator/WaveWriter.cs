using System;
using System.IO;

using More;

namespace Marler.Audio
{
    public static class WaveWriter
    {
        public static void WriteWaveFile(Stream waveFile, Byte[] waveData)
        {
            //
            // Make the format chunk
            //
            Byte[] waveFormatChunk = new Byte[24];

            waveFormatChunk[0] = Wave.FmtHeaderID[0];
            waveFormatChunk[1] = Wave.FmtHeaderID[1];
            waveFormatChunk[2] = Wave.FmtHeaderID[2];
            waveFormatChunk[3] = Wave.FmtHeaderID[3];

            waveFormatChunk.LittleEndianSetUInt32(4, (UInt32)waveFormatChunk.Length - 8);

            waveFormatChunk.LittleEndianSetUInt16( 8,     1); // audio format
            waveFormatChunk.LittleEndianSetUInt16(10,     1); // number of channels
            waveFormatChunk.LittleEndianSetUInt32(12, 44100); // sample rate
            waveFormatChunk.LittleEndianSetUInt32(16,     0); // Bytes per second
            waveFormatChunk.LittleEndianSetUInt16(20,     2); // Block align
            waveFormatChunk.LittleEndianSetUInt16(22,    16); // Bits per sample

            //
            // Write riff header
            //
            RiffHelper.WriteChunkHeader(waveFile, Riff.RiffHeaderID, (UInt32)(waveFormatChunk.Length + waveData.Length));
            waveFile.Write(Wave.WaveHeaderID, 0, Wave.WaveHeaderID.Length);

            //
            // Write the wave "fmt " chunk
            //
            waveFile.Write(waveFormatChunk, 0, 24);

            //
            // Write the data
            //
            RiffHelper.WriteChunkHeader(waveFile, Wave.DataHeaderID, (UInt32)(waveData.Length));
            waveFile.Write(waveData, 0, waveData.Length);
        }
    }
}
