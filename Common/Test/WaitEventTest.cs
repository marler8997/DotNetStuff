using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Marler.Common;
using System.Threading;
using System.Diagnostics;

namespace Marler
{
    class WaitEventVerifier
    {
        public readonly String id;
        public Boolean expectedToHandle;
        public WaitEventVerifier(String id)
        {
            this.id = id;
            this.expectedToHandle = false;
        }

        public int HandleEvent()
        {
            if (this.expectedToHandle == false) Assert.Fail("Did not expect to handle this event yet");
            Console.WriteLine("Handling {0}", id);
            this.expectedToHandle = false;
            return 0;
        }
    }

    [TestClass]
    public class WaitEventTest
    {
        [TestMethod]
        public void TestMethod()
        {
            WaitEventManager waitEvents = new WaitEventManager();

            WaitEventVerifier[] waitEventVerifiers = new WaitEventVerifier[] {
                new WaitEventVerifier("Waiter 0"),
                new WaitEventVerifier("Waiter 1"),
                new WaitEventVerifier("Waiter 2"),
                new WaitEventVerifier("Waiter 3"),
                new WaitEventVerifier("Waiter 4"),
            };


            Int64 startTime = Stopwatch.GetTimestamp();

            waitEvents.Add(new TimeAndWaitEvent(startTime + 700.MillisToStopwatchTicks(), waitEventVerifiers[2].HandleEvent));
            waitEvents.Add(new TimeAndWaitEvent(startTime + 600.MillisToStopwatchTicks(), waitEventVerifiers[1].HandleEvent));
            waitEvents.Add(new TimeAndWaitEvent(startTime + 800.MillisToStopwatchTicks(), waitEventVerifiers[3].HandleEvent));
            waitEvents.Add(new TimeAndWaitEvent(startTime + 0.MillisToStopwatchTicks(), waitEventVerifiers[0].HandleEvent));
            waitEvents.Add(new TimeAndWaitEvent(startTime + 900.MillisToStopwatchTicks(), waitEventVerifiers[4].HandleEvent));


            for (int i = 0; i < waitEventVerifiers.Length; i++)
            {
                Console.WriteLine("(Time={0}) Testing {1}", (Stopwatch.GetTimestamp() - startTime).StopwatchTicksAsInt32Milliseconds(), waitEventVerifiers[i].id);
                waitEventVerifiers[i].expectedToHandle = true;
                Int32 nextSleep = waitEvents.HandleWaitEvents();
                Console.WriteLine("(Time={0}) Next Sleep {1}", (Stopwatch.GetTimestamp() - startTime).StopwatchTicksAsInt32Milliseconds(), nextSleep);
                Assert.IsFalse(waitEventVerifiers[i].expectedToHandle);
                Thread.Sleep(nextSleep + 1);
            }




        }
    }
}
