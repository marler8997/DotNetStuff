using System;
using System.Collections.Generic;
using System.Text;

namespace More.OpenTK
{
    public abstract class Pulser
    {
        public readonly Int32 microsPerPulse;
        private Int32 microsIntoPulse;
        protected Pulser(Int32 microsPerPulse)
        {
            this.microsPerPulse = microsPerPulse;
            this.microsIntoPulse = 0;
        }
        public float PulseValue(Int32 diffMicros)
        {
            microsIntoPulse = (microsIntoPulse + diffMicros) % microsPerPulse;
            return PulseFunction(microsIntoPulse);
        }
        protected abstract float PulseFunction(Int32 microsIntoPulse);
    }

    public class LinearPulser : Pulser
    {
        public static LinearPulser CreatePulsesPerMinute(float pulsesPerMinute, Int32 pulseMicrosLength)
        {
            return new LinearPulser((Int32)(60000000f / pulsesPerMinute), pulseMicrosLength);
        }

        public readonly Int32 pulseMicrosLength;
        public LinearPulser(Int32 microsPerPulse, Int32 pulseMicrosLength)
            : base(microsPerPulse)
        {
            this.pulseMicrosLength = pulseMicrosLength;
        }
        protected override float PulseFunction(int microsIntoPulse)
        {
            if (microsIntoPulse >= pulseMicrosLength) return 0;
            return 1 - ((float)microsIntoPulse / (float)pulseMicrosLength);
        }
    }
}
