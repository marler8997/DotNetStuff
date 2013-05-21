using System;
using System.IO;

namespace Marler.Audio
{
    public static class VocalTunerProgram
    {
        static void Usage()
        {
            Console.WriteLine("VocalTuner.exe wav-file");
        }
        public static Int32 Main(String[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Error: expectd 1 argument but got {0}", args.Length);
                Usage();
                return -1;
            }

            String wavFile = args[0];

            /*

            Byte[] fileBytes = File.ReadAllBytes(wavFile);
            WaveParser waveParser = new WaveParser(fileBytes);

            Console.WriteLine(waveParser);
            */


            //
            // Generate Wave File
            //
            /*
            SoundWaves waves = new SoundWaves(
                new SillyOscillator(new SoundTime(44100, 44100), 1),
                new SillyOscillator(new SoundTime(44100, 44100), 2),
                new SillyOscillator(new SoundTime(44100, 44100), 50));
             */
            SillyOscillator sillyOscillator = new SillyOscillator(new SoundTime(44100, 88200), 1);

            byte[] waveData = new Byte[2 * sillyOscillator.Time().sampleCount];
            sillyOscillator.Write(waveData, 0, 44100);


            using (FileStream fileStream = new FileStream(wavFile, FileMode.Create))
            {
                WaveWriter.WriteWaveFile(fileStream, waveData);
            }


            return 0;
        }
    }
}