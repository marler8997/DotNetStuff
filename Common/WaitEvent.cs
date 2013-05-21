using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;

namespace Marler.Common
{
    public delegate Int32 WaitEvent();
    public class TimeAndWaitEvent
    {
        public Int64 stopwatchTimeToHandleEvent;
        public readonly WaitEvent waitEvent;
        public TimeAndWaitEvent(Int64 stopwatchTimeToHandleEvent, WaitEvent waitEvent)
        {
            this.stopwatchTimeToHandleEvent = stopwatchTimeToHandleEvent;
            this.waitEvent = waitEvent;
        }
        public Int64 MillisFromNow()
        {
            return (stopwatchTimeToHandleEvent - Stopwatch.GetTimestamp()).StopwatchTicksAsInt64Milliseconds();
        }
    }
    public class TimeAndWaitEventDecreasingComparer : IComparer<TimeAndWaitEvent>
    {
        private static TimeAndWaitEventDecreasingComparer instance = null;
        public static TimeAndWaitEventDecreasingComparer Instance
        {
            get
            {
                if (instance == null) instance = new TimeAndWaitEventDecreasingComparer();
                return instance;
            }
        }
        private TimeAndWaitEventDecreasingComparer() { }
        public Int32 Compare(TimeAndWaitEvent x, TimeAndWaitEvent y)
        {
            return (x.stopwatchTimeToHandleEvent > y.stopwatchTimeToHandleEvent) ? -1 :
                ((x.stopwatchTimeToHandleEvent < y.stopwatchTimeToHandleEvent) ? 1 : 0);
        }
    }
    public class WaitEventManager
    {
        // If a thread is waiting for events from a WaitEventManger, they should wait on this AutoResetEvent which will
        // pop if a new event is added that happens sooner then the current next event.
        public readonly AutoResetEvent newEventThatHappensSooner;
        Int32 nextEventMillisFromNow;

        SortedList<TimeAndWaitEvent> waitEvents;

        public WaitEventManager()
            : this(new AutoResetEvent(false))
        {
        }
        public WaitEventManager(AutoResetEvent eventToSignalWhenNewSoonerEventIsAdded)
        {
            this.newEventThatHappensSooner = eventToSignalWhenNewSoonerEventIsAdded;
            this.nextEventMillisFromNow = -1;
            this.waitEvents = new SortedList<TimeAndWaitEvent>(128, 128, TimeAndWaitEventDecreasingComparer.Instance);
        }


        public void Add(TimeAndWaitEvent timeAndWaitEvent)
        {
            waitEvents.Add(timeAndWaitEvent);

            //
            // Check if the new event happens sooner than the current next event
            //
            if (nextEventMillisFromNow < 0 || waitEvents.count <= 0)
            {
                nextEventMillisFromNow = (Int32)timeAndWaitEvent.MillisFromNow();
                newEventThatHappensSooner.Set();
            }
            else
            {
                TimeAndWaitEvent nextTimeAndWaitEvent = waitEvents.elements[waitEvents.count - 1];
                Int64 millisFromNow = nextTimeAndWaitEvent.MillisFromNow();
                if (millisFromNow < nextEventMillisFromNow)
                {
                    nextEventMillisFromNow = (Int32)millisFromNow;
                    newEventThatHappensSooner.Set();
                }
            }
        }
        /*
        public void Add(Int32 waitTimeMillis, IWaitEvent waitEvent)
        {
            TimeAndWaitEvent newTimeAndWaitEvent = new TimeAndWaitEvent(waitEvent);
            newTimeAndWaitEvent.nextWaitTimeInStopwatchTicks = Stopwatch.GetTimestamp() + waitTimeMillis.MillisToStopwatchTicks();

            waitEvents.Add(newTimeAndWaitEvent);
        }
        */

        //
        // Handles events and return milliseconds from now of next event, or returns 0 for no more events
        //
        public Int32 HandleWaitEvents()
        {
            while (true)
            {
                if (waitEvents.count <= 0)
                {
                    nextEventMillisFromNow = -1;
                    return 0;
                }

                TimeAndWaitEvent nextTimeAndWaitEvent = waitEvents.elements[waitEvents.count - 1];

                Int64 millisFromNow = nextTimeAndWaitEvent.MillisFromNow();
                if (millisFromNow > 0)
                {
                    // return milliseconds till next event
                    nextEventMillisFromNow = (Int32)millisFromNow;
                    return nextEventMillisFromNow;
                }

                waitEvents.count--;
                waitEvents.elements[waitEvents.count] = null; // remove reference

                Int32 nextWaitMilliseconds = nextTimeAndWaitEvent.waitEvent();
                if (nextWaitMilliseconds > 0)
                {
                    nextTimeAndWaitEvent.stopwatchTimeToHandleEvent =
                        Stopwatch.GetTimestamp() + nextWaitMilliseconds.MillisToStopwatchTicks();
                    waitEvents.Add(nextTimeAndWaitEvent);
                }
            }
        }
    }
}
