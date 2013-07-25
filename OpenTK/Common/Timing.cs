using System;
using System.Diagnostics;
using System.Threading;

using More;

namespace Marler.OpenTK.Common
{
    public class FpsSynchronizer
    {
        Int64 millisecondSynchronizer;

        public void Initialize()
        {
            this.millisecondSynchronizer = Stopwatch.GetTimestamp().StopwatchTicksAsInt64Milliseconds();
        }
        public void Sync(Int32 millisPerFrame)
        {
            while (true)
            {
                Int32 lastTimeDiffMillis = (Int32)(Stopwatch.GetTimestamp().StopwatchTicksAsInt64Milliseconds() - millisecondSynchronizer);

                Int32 timeLeftMillis = millisPerFrame - lastTimeDiffMillis;
                if (timeLeftMillis <= 0) break;

                Thread.Sleep(timeLeftMillis);
            }

            millisecondSynchronizer += millisPerFrame;
        }
    }


    //
    // This synchronizer will make sure to wait the given amount of time between frames...
    // however...if a particular frame takes too long...it will not try to compensate by making
    // future frames shorter.
    // This synchronizer simply guarantees that no frame will be shorter than the given millisPerFrame
    public class FpsChoppySynchronizer
    {
        Int64 lastTimeMillis;

        public void Initialize()
        {
            this.lastTimeMillis = Stopwatch.GetTimestamp().StopwatchTicksAsInt64Milliseconds();
        }
        public void Sync(Int32 millisPerFrame)
        {
            Int64 nowMillis;
            while (true)
            {
                nowMillis = Stopwatch.GetTimestamp().StopwatchTicksAsInt64Milliseconds();

                Int32 lastTimeDiffMillis = (Int32)(nowMillis - lastTimeMillis);

                Int32 timeLeftMillis = millisPerFrame - lastTimeDiffMillis;
                if (timeLeftMillis <= 0) break;

                Thread.Sleep(timeLeftMillis);
            }

            lastTimeMillis = nowMillis;
        }
    }

    public class FpsVariableOptimizer
    {
        public readonly Int32 minimumMillisecondsPerFrame;
        public FpsVariableOptimizer(Int32 minimumMillisecondsPerFrame)
        {
            this.minimumMillisecondsPerFrame = minimumMillisecondsPerFrame;
        }
    }

}
