﻿using System;
using System.Collections.Generic;

using More;

namespace Marler.Audio
{
    public struct SoundTime
    {
        public UInt32 samplesPerSecond;
        public UInt32 sampleCount;
        public SoundTime(UInt32 samplesPerSecond, UInt32 sampleCount)
        {
            this.samplesPerSecond = samplesPerSecond;
            this.sampleCount = sampleCount;
        }
        public static SoundTime operator +(SoundTime a, SoundTime b) 
        {
            if(a.samplesPerSecond != b.samplesPerSecond) throw new NotImplementedException();
            return new SoundTime(a.samplesPerSecond, a.sampleCount + b.sampleCount);
        }
        public UInt32 TranslateSampleCountFrom(UInt32 samplesPerSecond)
        {
            if (this.samplesPerSecond == samplesPerSecond) return sampleCount;
            throw new NotImplementedException("Havent impelmented translating sound time yet");
        }       
    }

    public interface ISoundWave
    {
        SoundTime Time();
        UInt32 Write(Byte[] soundData, UInt32 offset, UInt32 samplesPerSecond);
    }

    public class SoundWaves : ISoundWave
    {
        List<ISoundWave> soundWaves;

        public SoundWaves(params ISoundWave[] soundWaves)
        {
            this.soundWaves = new List<ISoundWave>(soundWaves);
        }
        public SoundWaves(List<ISoundWave> soundWaves)
        {
            this.soundWaves = soundWaves;
        }
        public SoundTime Time()
        {
            SoundTime sum = new SoundTime(44100, 0);
            for(int i = 0; i < soundWaves.Count; i++)
            {
                sum += soundWaves[i].Time();
            }
            return sum;
        }
        public UInt32 Write(Byte[] soundData, UInt32 offset, uint samplesPerSecond)
        {
            for (int i = 0; i < soundWaves.Count; i++)
            {
                offset = soundWaves[i].Write(soundData, offset, samplesPerSecond);
            }
            return offset;
        }
    }



    /*
    public class SillyOscillator : ISoundWave
    {
        SoundTime time;
        Byte cycleIncrementer;
        public SillyOscillator(SoundTime time, Byte cycleIncrementer)
        {
            this.time = time;
            this.cycleIncrementer = cycleIncrementer;
        }
        public SoundTime Time()
        {
            return time;
        }
        public int Write(byte[] soundData, int offset, uint samplesPerSecond)
        {
            Int32 incrementer = 5;

            Int32 note = 100;
            for(UInt32 i = 0; i < time.sampleCount; i++)
            {
                offset = ((UInt16)note).ToBigEndian(soundData, offset);


                note += incrementer;
                if (note <= 100 || note >= 700)
                {
                    incrementer *= -1;
                }
            }
            return offset;
        }
    }
    */

    public class SillyOscillator : ISoundWave
    {
        SoundTime time;
        Byte cycleIncrementer;
        public SillyOscillator(SoundTime time, Byte cycleIncrementer)
        {
            this.time = time;
            this.cycleIncrementer = cycleIncrementer;
        }
        public SoundTime Time()
        {
            return time;
        }
        public UInt32 Write(Byte[] soundData, UInt32 offset, UInt32 samplesPerSecond)
        {
            double incrementer = .06;

            double note = 0;
            for (UInt32 i = 0; i < time.sampleCount; i++)
            {
                soundData.BigEndianSetInt16(offset, (Int16)(10 * Math.Sin(note)));
                offset += 2;
                //Console.WriteLine("note = {0} func = {1}", note, (UInt16)(100 * Math.Sin(note)));

                note += incrementer;
                if (note + incrementer <= 0 || note + incrementer >= 2)
                {
                    incrementer *= -1;
                }
            }
            return offset;
        }
    }



    public class SinOscillator : ISoundWave
    {
        SoundTime time;
        UInt32 cyclesPerSecond;
        Int16 amplitude;
        public SinOscillator(SoundTime time, UInt32 cyclesPerSecond, Int16 amplitude)
        {
            this.time = time;
            this.cyclesPerSecond = cyclesPerSecond;
            this.amplitude = amplitude;
        }
        public SoundTime Time()
        {
            return time;
        }
        public UInt32 Write(Byte[] soundData, UInt32 offset, UInt32 samplesPerSecond)
        {
            Double incrementValue = (Double)(cyclesPerSecond * 2) * Math.PI / (Double)samplesPerSecond;
            Double note = 0;

            for (UInt32 i = 0; i < time.sampleCount; i++)
            {
                soundData.BigEndianSetInt16(offset, (Int16)(amplitude * Math.Sin(note)));
                offset += 2;
                //Console.WriteLine("note = {0} func = {1}", note, (UInt16)(100 * Math.Sin(note)));

                note += incrementValue;
                if (note > 2 * Math.PI)
                {
                    note -= 2 * Math.PI;
                }
            }
            return offset;
        }
    }


    /*
    public class Oscillator : ISoundWave
    {
        SoundTime time;
        UInt32 cyclesPerSecond;
        public Oscillator(SoundTime time, UInt32 cyclesPerSecond)
        {
            this.time = time;
            this.cyclesPerSecond = cyclesPerSecond;
        }
        public SoundTime Time()
        {
            return time;
        }
        public int Write(byte[] soundData, int offset, uint samplesPerSecond)
        {

        }
    }
    */
}
