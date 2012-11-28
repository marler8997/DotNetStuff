using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Marler.NetworkTools
{
    public static class StopwatchExtensions
    {
        private static Double StopwatchTicksPerMillisecondAsDouble = 1000.0 / Stopwatch.Frequency;

        private static String stopwatchTickString = null;
        public static String StopwatchTickString
        {
            get
            {
                if (stopwatchTickString == null)
                {
                    Int64 frequency = Stopwatch.Frequency;
                    switch (frequency)
                    {
                        case 1000: stopwatchTickString = "milliseconds"; break;
                        case 1000000: stopwatchTickString = "microsecond"; break;
                        case 1000000000: stopwatchTickString = "nanoseconds"; break;
                        default: throw new InvalidOperationException(String.Format("Unknown stopwatch frequency: '{0}' (Expected 1000, 1000000 or 1000000000)", frequency));
                    }
                }
                return stopwatchTickString;
            }
        }


        public static Int64 MillisToStopwatchTicks(this Int32 millis)
        {
            return Stopwatch.Frequency * (Int64)millis / 1000L;
        }
        public static Int64 StopwatchTicksAsMicroseconds(this Int64 stopwatchTicks)
        {
            return stopwatchTicks * 1000000L / Stopwatch.Frequency;
        }
        public static Int32 StopwatchTicksAsInt32Milliseconds(this Int64 stopwatchTicks)
        {
            return (Int32)(stopwatchTicks * 1000 / Stopwatch.Frequency);
        }
        public static Int64 StopwatchTicksAsInt64Milliseconds(this Int64 stopwatchTicks)
        {
            return (Int64)(stopwatchTicks * 1000 / Stopwatch.Frequency);
        }
        public static Double StopwatchTicksAsDoubleMilliseconds(this Int64 stopwatchTicks)
        {
            return StopwatchTicksPerMillisecondAsDouble * stopwatchTicks;
        }
    }
}
