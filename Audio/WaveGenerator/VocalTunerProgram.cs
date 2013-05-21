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
            if(args.Length != 1)
            {
                Console.WriteLine("Error: expectd 1 argument but got {0}", args.Length);
                Usage();
                return -1;
            }

            String wavFile = args[0];

            /*
	    using(BinaryReader reader = new BinaryReader(File.Open(wavFile, FileMode.Open)))
            {
                // WaveFormat waveFormat = new WaveFormat(reader);
                //Console.WriteLine(waveFormat);

                for(int i = 0; i < 20; i++)
                {
                    Console.WriteLine("[{0}] 0x{1:X2}", i, reader.ReadByte());
                }
            }
            */
            
            Byte[] fileBytes = File.ReadAllBytes(wavFile);
            WaveParser waveParser = new WaveParser(fileBytes);

            Console.WriteLine(waveParser);

            return 0;
        }
    }
}