using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;

namespace More
{
    // returns true to execute the next event
    public delegate void WaitEvent(TimeAndWaitEvent timeAndWaitEvent);
    public class TimeAndWaitEvent
    {
        public Int64 stopwatchTicksToHandleEvent;
        public WaitEvent waitEvent;
        public TimeAndWaitEvent(Int64 stopwatchTicksToHandleEvent, WaitEvent waitEvent)
        {
            this.stopwatchTicksToHandleEvent = stopwatchTicksToHandleEvent;
            this.waitEvent = waitEvent;
        }
        public TimeAndWaitEvent(Int64 now, Int32 millisFromNow, WaitEvent waitEvent)
        {
            this.stopwatchTicksToHandleEvent = now + millisFromNow.MillisToStopwatchTicks();
            this.waitEvent = waitEvent;
        }
        public void SetNextWaitEventTime(Int64 now, Int32 millisFromNow)
        {
            this.stopwatchTicksToHandleEvent = now + millisFromNow.MillisToStopwatchTicks();
        }
        public Int32 MillisFromNow()
        {
            return (stopwatchTicksToHandleEvent - Stopwatch.GetTimestamp()).StopwatchTicksAsInt32Milliseconds();
        }
        public Int32 MillisFromNow(Int64 now)
        {
            return (stopwatchTicksToHandleEvent - now).StopwatchTicksAsInt32Milliseconds();
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
            return (x.stopwatchTicksToHandleEvent > y.stopwatchTicksToHandleEvent) ? -1 :
                ((x.stopwatchTicksToHandleEvent < y.stopwatchTicksToHandleEvent) ? 1 : 0);
        }
    }
    //
    // Not Thread Safe
    //
    public class WaitEventManager
    {
        // If a thread is waiting for events from a WaitEventManger, they should wait on this AutoResetEvent which will
        // pop if a new event is added that happens sooner then the current next event.
        public readonly AutoResetEvent newEventThatHappensSooner;

        readonly SortedList<TimeAndWaitEvent> waitEvents;
        //UInt32 nextEventMillisFromNow;

        public WaitEventManager()
            : this(new AutoResetEvent(false))
        {
        }
        public WaitEventManager(AutoResetEvent eventToSignalWhenNewSoonerEventIsAdded)
        {
            this.newEventThatHappensSooner = eventToSignalWhenNewSoonerEventIsAdded;
            this.waitEvents = new SortedList<TimeAndWaitEvent>(128, 128, TimeAndWaitEventDecreasingComparer.Instance);
        }
        public void Add(TimeAndWaitEvent newEvent)
        {
            if (waitEvents.count <= 0)
            {
                waitEvents.Add(newEvent);
                newEventThatHappensSooner.Set();
            }
            else
            {
                //
                // Check if this new event happens sooner than the next event
                //
                Int64 now = Stopwatch.GetTimestamp();
                Int32 nextEventMillisFromNow = waitEvents.elements[waitEvents.count - 1].MillisFromNow(now);
                Int32 newEventMillisFromNow = newEvent.MillisFromNow(now);

                waitEvents.Add(newEvent);

                if(newEventMillisFromNow < nextEventMillisFromNow)
                {
                    newEventThatHappensSooner.Set();
                }
            }
        }

        //
        // Handles events and return milliseconds from now of next event, or returns 0 for no more events
        //
        public Int32 HandleWaitEvents()
        {
            while (true)
            {
                if (waitEvents.count <= 0) return 0;

                TimeAndWaitEvent nextTimeAndWaitEvent = waitEvents.elements[waitEvents.count - 1];

                Int32 millisFromNow = nextTimeAndWaitEvent.MillisFromNow();
                if (millisFromNow > 0) return millisFromNow;

                waitEvents.count--;
                waitEvents.elements[waitEvents.count] = null; // remove reference

                Int64 nextTimeAndWaitEventTicks = nextTimeAndWaitEvent.stopwatchTicksToHandleEvent;
                nextTimeAndWaitEvent.waitEvent(nextTimeAndWaitEvent);

                if (nextTimeAndWaitEvent.waitEvent != null)
                {
                    if(nextTimeAndWaitEvent.stopwatchTicksToHandleEvent != nextTimeAndWaitEventTicks)
                    {
                        waitEvents.Add(nextTimeAndWaitEvent);
                    }
                }                
            }
        }
    }
}
