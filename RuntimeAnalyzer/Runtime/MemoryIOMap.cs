using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.RuntimeAnalyzer
{

    public delegate void WriteCallback(UInt64 value);
    public delegate UInt64 ReadCallback();

    public class MemoryIOMap
    {
        private readonly ReadCallback[] readCallbacks;
        private readonly WriteCallback[] writeCallbacks;

        public readonly UInt64 length;

        public MemoryIOMap(ReadCallback[] readCallbacks, WriteCallback[] writeCallbacks)
        {
            this.readCallbacks = readCallbacks;
            this.writeCallbacks = writeCallbacks;

            if (this.readCallbacks.Length != writeCallbacks.Length)
            {
                throw new ArgumentException("callback arrays must have same length");
            }
            this.length = (UInt32)readCallbacks.Length;

#if DEBUG
            for (UInt64 i = 0; i < length; i++)
            {
                if (readCallbacks[i] == null) throw new ArgumentNullException();
                if (writeCallbacks[i] == null) throw new ArgumentNullException();
            }
#endif
        }

        public UInt64 this[UInt64 address]
        {
            get
            {
                return readCallbacks[address]();
            }
            set
            {
                writeCallbacks[address](value);
            }
        }

        /*
        public UInt64 Read(UInt64 address)
        {
            return readCallbacks[address]();
        }
        public void Write(UInt64 address, UInt64 value)
        {
            writeCallbacks[address](value);
        }
        */
    }
}
