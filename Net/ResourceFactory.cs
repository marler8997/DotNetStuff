using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace More.Net
{
    public class ResourceTracker
    {
        private readonly UInt32 maximumResourceCount;
        private readonly EventWaitHandle waitForAvailable;

        private UInt32 availableResources;
        public ResourceTracker(UInt32 maximumResourceCount)
        {
            if (maximumResourceCount < 1) throw new ArgumentOutOfRangeException("maximumResourceCount");

            this.maximumResourceCount = maximumResourceCount;
            this.waitForAvailable = new EventWaitHandle(false, EventResetMode.AutoReset);

            this.availableResources = maximumResourceCount;
        }

        public Boolean Available { get { return availableResources > 0; } }

        public void Reserve()
        {
            while (true)
            {
                lock (waitForAvailable)
                {
                    if (availableResources > 0)
                    {
                        availableResources--;
                        return;
                    }
                }
                
                waitForAvailable.WaitOne();
            }
        }

        public void Free()
        {
            lock (waitForAvailable)
            {
                if (availableResources < maximumResourceCount)
                {
                    availableResources++;
                }
                if (availableResources <= 1)
                {
                    waitForAvailable.Set();
                }
            }
        }
    }
}
