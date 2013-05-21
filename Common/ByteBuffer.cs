using System;

namespace Marler.Common
{
    //
    // This class wraps a byte array that can be passed to and from functions
    // that will ensure that the array size is changed accordingly
    //
    public class ByteBuffer
    {
        public const Int32 DefaultExpandLength = 128;
        public const Int32 DefaultInitialCapacity = 128;

        private readonly Int32 expandLength;

        public Byte[] array;

        public ByteBuffer()
            : this(DefaultInitialCapacity, DefaultExpandLength)
        {
        }
        public ByteBuffer(Int32 initialCapacity, Int32 expandLength)
        {
            this.expandLength = expandLength;
            this.array = new Byte[initialCapacity];
        }
        public void EnsureCapacity(Int32 capacity)
        {
            if (array.Length < capacity)
            {
                Int32 diff = capacity - array.Length;
                Int32 newSizeDiff = (diff > expandLength) ? diff : expandLength;

                Byte[] newArray = new Byte[array.Length + newSizeDiff];
                Array.Copy(array, newArray, array.Length);
                this.array = newArray;
            }
        }
    }
}
