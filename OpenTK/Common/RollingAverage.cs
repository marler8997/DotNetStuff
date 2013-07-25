using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.OpenTK.Common
{
    public class RollingAverage
    {
        readonly float[] history;
        Int32 nextIndex;
        Boolean rolledOnce;

        float lastAverageCalculated;
        public float LastAverageCalculated { get { return lastAverageCalculated; } }

        public RollingAverage(Int32 historyLength)
        {
            this.history = new float[historyLength];
            Reset();
        }
        public void Reset()
        {
            this.nextIndex = 0;
            this.rolledOnce = false;
        }
        public void AddValue(float value)
        {
            history[nextIndex] = value;
            nextIndex++;
            if (nextIndex >= history.Length)
            {
                rolledOnce = true;
                nextIndex = 0;
            }
        }
        public void CalulateAverage()
        {
            Int32 length = rolledOnce ? history.Length : nextIndex;
            float lengthAsFloat = (float)length;
            float average = 0;
            for (int i = 0; i < length; i++)
            {
                average += history[i] / lengthAsFloat;
            }
            this.lastAverageCalculated = average;
        }
    }
}
