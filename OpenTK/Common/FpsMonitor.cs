using System;
using System.Diagnostics;

using More;

namespace Marler.OpenTK.Common
{
    public class FpsMonitor
    {
        //float fps;
        long last = 0;

        Int32[] history;
        Int32 nextHistoryIndex;

        public FpsMonitor(Int32 historyLength)
        {
            this.history = new Int32[historyLength];
        }
        public void Frame()
        {
            if (last <= 0)
            {
                last = Stopwatch.GetTimestamp();
                this.nextHistoryIndex = 0;
                return;
            }

            nextHistoryIndex++;
            if (nextHistoryIndex >= history.Length) nextHistoryIndex = 0;

            long now = Stopwatch.GetTimestamp();
            history[nextHistoryIndex] = (int)(now - last);
            this.last = now;
        }

        public Int32 CalculateFps()
        {
            long totalTime = 0;
            for (int i = 0; i < history.Length; i++)
            {
                totalTime += history[i];
            }
            return (Int32)(history.Length * 1000d / (Double)StopwatchExtensions.StopwatchTicksAsDoubleMilliseconds(totalTime));
        }
    }
}
