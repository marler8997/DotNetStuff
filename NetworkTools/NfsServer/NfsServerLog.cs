using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Marler.NetworkTools
{
    public static class NfsServerLog
    {
        public static TextWriter rpcCallLogger;
        public static TextWriter warningLogger;
        public static TextWriter sharedFileSystemLogger;

        public static Boolean storePerformance;


        class CommandAndTime
        {
            public readonly Int64 atStopwatchTicks;
            public readonly Nfs3Command command;
            public readonly Int32 callTimeMicroseconds;
            public CommandAndTime(Nfs3Command command, Int32 callTimeMicroseconds)
            {
                this.atStopwatchTicks = Stopwatch.GetTimestamp() - stopwatchTicksBase;
                this.command = command;
                this.callTimeMicroseconds = callTimeMicroseconds;
            }
        }
        private static Int64 stopwatchTicksBase;
        private static List<CommandAndTime> storedCommands = null;
        public static void StoreNfsCallPerformance(Nfs3Command command, Int32 microseconds)
        {
            if (storedCommands == null)
            {
                stopwatchTicksBase = Stopwatch.GetTimestamp();
                storedCommands = new List<CommandAndTime>();
            }
            storedCommands.Add(new CommandAndTime(command, microseconds));
        }
        public static void PrintNfsCalls(TextWriter writer)
        {
            if (storedCommands != null)
            {
                for (int i = 0; i < storedCommands.Count; i++)
                {
                    CommandAndTime commandAndTime = storedCommands[i];
                    writer.WriteLine("At {0,8:0.00} milliseconds Call '{1,12}' Took {2,8:0.00} milliseconds",
                        commandAndTime.atStopwatchTicks.StopwatchTicksAsDoubleMilliseconds(),
                        commandAndTime.command,
                        (Double)commandAndTime.callTimeMicroseconds / 1000);
                }
            }
        }
    }
}
