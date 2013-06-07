using System;
using System.IO;

using More;

namespace Marler.Audio
{
    public static class Wave
    {
        public static readonly Byte[] WaveHeaderID = new Byte[] { (Byte)'W', (Byte)'A', (Byte)'V', (Byte)'E' };
        public static readonly Byte[] FmtHeaderID = new Byte[] { (Byte)'f', (Byte)'m', (Byte)'t', (Byte)' ' };
        public static readonly Byte[] DataHeaderID = new Byte[] { (Byte)'d', (Byte)'a', (Byte)'t', (Byte)'a' };

        public static Byte[] LoadWaveFile(String filename, out WaveFormat waveFormat)
        {
            using (FileStream fileStream = new FileStream(filename, FileMode.Open))
            {
                BinaryStream fileBinaryStream = new BinaryStream(fileStream);

                waveFormat = new WaveFormat(fileBinaryStream);

                Byte[] waveData = new Byte[waveFormat.dataChunkSize];
                fileBinaryStream.ReadFullSize(waveData, 0, waveData.Length);
                return waveData;
            }
        }

    }
    public class WaveFormat
    {
        public readonly UInt16 audioFormat;       // most often PCM = 1      
        public readonly UInt16 channelCount;      // number of channels      

        public readonly UInt32 samplesPerSecond;  // samples per second eg 44100     
        public readonly UInt32 avgBytesPerSecond; // bytes per second eg 176000
                                                  // = channelCount * samplesPerSecond * bitsPerSample / 8

        public readonly UInt16 blockAlign;        // Number of bytes for one samples for all channels
                                                  // = channelCount * bitsPerSample / 8
        public readonly UInt16 bitsPerSample;     // bits per sample, 8, 16, 24

        public readonly UInt32 dataChunkSize;

        public WaveFormat(BinaryStream stream)
        {
            UInt32 riffChunkSize = Riff.VerifyChunkIdAndGetSize(stream, Riff.RiffHeaderID);

            Console.WriteLine("Riff Chunk Size {0}", riffChunkSize);

            Riff.Verify(stream, Wave.WaveHeaderID, "Expected RIFF format to be");

            UInt32 formatChunkSize = Riff.VerifyChunkIdAndGetSize(stream, Wave.FmtHeaderID);
            if (formatChunkSize < 16)
                throw new FormatException(String.Format("Expected wave 'fmt ' chunk size to be at least 16 but is {0}", formatChunkSize));


            this.audioFormat = stream.LittleEndianReadUInt16();
            this.channelCount = stream.LittleEndianReadUInt16();

            this.samplesPerSecond = stream.LittleEndianReadUInt32();
            this.avgBytesPerSecond = stream.LittleEndianReadUInt32();

            this.blockAlign = stream.LittleEndianReadUInt16();
            this.bitsPerSample = stream.LittleEndianReadUInt16();

            // skip extra parameters
            stream.Skip((Int32)formatChunkSize - 16);

            //
            this.dataChunkSize = Riff.VerifyChunkIdAndGetSize(stream, Wave.DataHeaderID);
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine("AudioFormat:       " + audioFormat.ToString());
            sb.AppendLine("ChannelCount:      " + channelCount.ToString());

            sb.AppendLine("SamplesPerSecond:  " + samplesPerSecond.ToString());
            sb.AppendLine("AvgBytesPerSecond: " + avgBytesPerSecond.ToString());

            sb.AppendLine("BlockAlign:        " + blockAlign.ToString());
            sb.AppendLine("BitsPerSample:     " + bitsPerSample.ToString());

            sb.AppendLine("Data Size:         " + dataChunkSize.ToString());
            return sb.ToString();
        }
    }
}
