using System;
using System.Diagnostics;
using System.IO;

using More;

using OpenTK;
using OpenTK.Graphics;

namespace More.OpenTK
{
    public abstract class CustomGameWindow : GameWindow
    {
        protected Int32 minMillisPerFrame = 20;

        public CustomGameWindow(int width, int height, GraphicsMode mode, string title)
            : base(width, height, mode, title)
        {
        }
        public CustomGameWindow(int width, int height, GraphicsMode mode, string title, GameWindowFlags options)
            : base(width, height, mode, title, options)
        {
        }
        public CustomGameWindow(int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device)
            : base(width, height, mode, title, options, device)
        {
        }
        public CustomGameWindow(int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device, int major, int minor, GraphicsContextFlags flags)
            : base(width, height, mode, title, options, device, major, minor, flags)
        {
        }
        public CustomGameWindow(int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device, int major, int minor, GraphicsContextFlags flags, IGraphicsContext sharedContext)
            : base(width, height, mode, title, options, device, major, minor, flags, sharedContext)
        {
        }


        public abstract void Initialize(Int64 nowMicros);
        // return true to exit
        public abstract System.Boolean Update(Int64 nowMicros, Int32 diffMicros);
        public abstract void Render();

        //
        // The Game Loop
        //
        //    1. Render
        //       - Call Render();
        //       - Call SwapBuffers();
        //    2. Wait/Synchronize
        //       - Call sync on the synchronizer class
        //    3. Process Window Messages/Events
        //    4. Update
        //       - Call Update
        //
        //


        protected RollingAverage averageRenderCallsTime;
        protected RollingAverage averageSwapBufferTime;
        protected RollingAverage averageSyncTime;
        protected RollingAverage averageProcessEventsTime;
        protected RollingAverage averageUpdateTime;

        protected RollingAverage averageUpdateTimeDiffMicros;

        public void CustomGameLoop(TextWriter statsFileWriter)
        {
            Int64 before;
            Int64
                renderCallsTime   = 0,
                swapBufferTime    = 0,
                syncTime          = 0,
                processEventsTime = 0,
                updateTime        = 0;
            this.averageRenderCallsTime   = new RollingAverage(64);
            this.averageSwapBufferTime    = new RollingAverage(64);
            this.averageSyncTime          = new RollingAverage(64);
            this.averageProcessEventsTime = new RollingAverage(64);
            this.averageUpdateTime        = new RollingAverage(64);
            this.averageUpdateTimeDiffMicros = new RollingAverage(64);

            if (statsFileWriter != null) statsFileWriter.WriteLine("RenderCallsTime\tSwapBufferTime\tSyncTime\tEventsTime\tUpdateTime\tUpdateTimeDiff");

            //
            // Make the game visible and call the resize method
            //
            Visible = true;
            OnResize(null);

            //
            // Setup Timing Variables
            //
            FpsChoppySynchronizer sync = new FpsChoppySynchronizer();
            sync.Initialize();

            Int64 startTimeStopwatchTicks = Stopwatch.GetTimestamp();
            Int64 startTimeMillis = startTimeStopwatchTicks.StopwatchTicksAsInt64Milliseconds();
            Int64 lastUpdateTimeStopwatchTicks = startTimeStopwatchTicks;
            Int64 lastUpdateTimeMicros = lastUpdateTimeStopwatchTicks.StopwatchTicksAsMicroseconds();

            UInt32 gameLoopCount = 0;

            Initialize(lastUpdateTimeMicros);

            while (true)
            {
                //
                // Render
                //
                before = Stopwatch.GetTimestamp();
                Render();
                renderCallsTime = Stopwatch.GetTimestamp() - before;
                averageRenderCallsTime.AddValue(renderCallsTime);

                //
                // Swap Buffers
                //
                before = Stopwatch.GetTimestamp();
                SwapBuffers();
                swapBufferTime = Stopwatch.GetTimestamp() - before;
                averageSwapBufferTime.AddValue(swapBufferTime);

                //
                // Wait/Synchronize
                //
                before = Stopwatch.GetTimestamp();
                sync.Sync(minMillisPerFrame);
                syncTime = Stopwatch.GetTimestamp() - before;
                averageSyncTime.AddValue(syncTime);

                //
                // Process Events
                //
                before = Stopwatch.GetTimestamp();
                ProcessEvents();
                processEventsTime = Stopwatch.GetTimestamp() - before;
                averageProcessEventsTime.AddValue(processEventsTime);

                if (IsExiting) break;

                //
                // Update
                //
                before = Stopwatch.GetTimestamp();

                Int64 nowMicros = before.StopwatchTicksAsMicroseconds();
                Int32 diffMicros = (Int32)(nowMicros - lastUpdateTimeMicros);

                if (Update(nowMicros, diffMicros)) break;
                updateTime = Stopwatch.GetTimestamp() - before;
                averageUpdateTime.AddValue(updateTime);

                //
                // Add another value to average update time diff
                //
                //Console.WriteLine("DiffMicros = {0}", diffMicros);
                averageUpdateTimeDiffMicros.AddValue(diffMicros);
                lastUpdateTimeStopwatchTicks = before;
                lastUpdateTimeMicros = nowMicros;

                //
                // Write Statistics
                //
                if (statsFileWriter != null)
                    statsFileWriter.WriteLine(String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", renderCallsTime,
                        swapBufferTime, syncTime, processEventsTime, updateTime, diffMicros));

                gameLoopCount++;
            }
        }
    }
}
