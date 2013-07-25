using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

using Marler.Audio;
using More;
using Marler.OpenTK.Common;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;
using System.IO;
using System.Collections.Generic;

namespace Marler.Tank
{
    class TankGameSettings
    {
        

    }




    class TankProgram
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            /*
            try
            {
             */
                SystemInformation info = new SystemInformation();


                //
                // Load Game Settings
                //









                /*
                // The 'using' idiom guarantees proper resource cleanup.
                // We request 30 UpdateFrame events per second, and unlimited
                // RenderFrame events (as fast as the computer can handle).

                AudioContext audioContext = null;
                StreamWriter statsFileWriter = null;
                TankGame game = null;


                try
                {

                    try
                    {
                        audioContext = new AudioContext();



                        //
                        // Load Audio
                        //
                        int audioBufferHandle = AL.GenBuffer();
                        int audioSourceHandle = AL.GenSource();
                        //int state;

                        WaveFormat soundFileFormat;
                        Byte[] soundData = Wave.LoadWaveFile("CitadelKDrew.wav", out soundFileFormat);
                        Console.WriteLine(soundFileFormat);
                        AL.BufferData(audioBufferHandle, GetSoundFormat(soundFileFormat), soundData, soundData.Length, (int)soundFileFormat.samplesPerSecond);

                        AL.Source(audioSourceHandle, ALSourcei.Buffer, audioBufferHandle);
                        //AL.Source(audioSourceHandle, ALSourcei.ByteOffset, 0);
                        AL.SourcePlay(audioSourceHandle);

                        // Query the source to find out when it stops playing.
                        do
                        {
                            Thread.Sleep(250);
                            Trace.Write(".");
                            AL.GetSource(source, ALGetSourcei.SourceState, out state);
                        }
                        while ((ALSourceState)state == ALSourceState.Playing);

                        Trace.WriteLine("");

                        AL.SourceStop(source);
                        AL.DeleteSource(source);
                        AL.DeleteBuffer(buffer);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Sound failed: " + e.ToString());
                    }

                    String statisticsFileName = @"C:\temp\hexagon.stats";
                    try
                    {
                        statsFileWriter = new StreamWriter(new FileStream(statisticsFileName, FileMode.Create));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to open statistics file '{0}'", statisticsFileName);
                    }

                    game = new TankGame();
                    //game.CustomGameLoop(statsFileWriter);
                    game.CustomGameLoop(null);

                }
                finally
                {
                    if (game != null) game.Dispose();
                    if (statsFileWriter != null) statsFileWriter.Dispose();
                    if (audioContext != null) audioContext.Dispose();
                }


                */

                TankLevel level = new TankLevel();
                level.width = 800;
                level.height = 800;


                TankGame game = new TankGame(level);
                game.CustomGameLoop(null);

                /*
            }
            catch (Exception e)
            {
                throw;
            }
                 */
        }
    }
}