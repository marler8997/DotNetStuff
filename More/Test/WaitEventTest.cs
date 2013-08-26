using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
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

        public void HandleEvent(TimeAndWaitEvent timeAndWaitEvent)
        {
            if (this.expectedToHandle == false) Assert.Fail("Did not expect to handle this event yet");
            Console.WriteLine("Handling {0}", id);
            this.expectedToHandle = false;
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

            waitEvents.Add(new TimeAndWaitEvent(startTime, 700, waitEventVerifiers[2].HandleEvent));
            waitEvents.Add(new TimeAndWaitEvent(startTime, 600, waitEventVerifiers[1].HandleEvent));
            waitEvents.Add(new TimeAndWaitEvent(startTime, 800, waitEventVerifiers[3].HandleEvent));
            waitEvents.Add(new TimeAndWaitEvent(startTime, 0  , waitEventVerifiers[0].HandleEvent));
            waitEvents.Add(new TimeAndWaitEvent(startTime, 900, waitEventVerifiers[4].HandleEvent));


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
