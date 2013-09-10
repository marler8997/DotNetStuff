using System;

namespace More
{
    public interface IBuffer
    {
        Byte[] Array { get; }
        void EnsureCapacity(Int32 capacity);
    }


    //
    // This class wraps a byte array that can be passed to and from functions
    // that will ensure that the array size is changed accordingly
    //
    public class ByteBuffer : IBuffer
    {
        public const UInt32 DefaultExpandLength = 128;
        public const UInt32 DefaultInitialCapacity = 128;

        public Byte[] array;
        public readonly UInt32 expandLength;

        public ByteBuffer()
            : this(DefaultInitialCapacity, DefaultExpandLength)
        {
        }
        public ByteBuffer(UInt32 initialCapacity, UInt32 expandLength)
        {
            this.expandLength = expandLength;
            this.array = new Byte[initialCapacity];
        }
        public Byte[] Array { get { return array; } }
        public void EnsureCapacity(Int32 capacity)
        {
            if (array.Length < capacity)
            {
                UInt32 diff = (UInt32)(capacity - array.Length);
                UInt32 newSizeDiff = (diff > expandLength) ? diff : expandLength;

                Byte[] newArray = new Byte[array.Length + newSizeDiff];
                System.Array.Copy(array, newArray, array.Length);
                this.array = newArray;
            }
        }
        public void EnsureCapacity(UInt32 capacity)
        {
            if ((UInt32)array.Length < capacity)
            {
                UInt32 diff = capacity - (UInt32)array.Length;
                UInt32 newSizeDiff = (diff > expandLength) ? diff : expandLength;

                Byte[] newArray = new Byte[array.Length + newSizeDiff];
                System.Array.Copy(array, newArray, array.Length);
                this.array = newArray;
            }
        }
    }

    public class Buffer<T>
    {
        public const UInt32 DefaultExpandLength = 128;
        public const UInt32 DefaultInitialCapacity = 128;

        public T[] array;
        public readonly UInt32 expandLength;

        public Buffer()
            : this(DefaultInitialCapacity, DefaultExpandLength)
        {
        }
        public Buffer(UInt32 initialCapacity, UInt32 expandLength)
        {
            this.expandLength = expandLength;
            this.array = new T[initialCapacity];
        }
        public void EnsureCapacity(Int32 capacity)
        {
            if (array.Length < capacity)
            {
                UInt32 diff = (UInt32)(capacity - array.Length);
                UInt32 newSizeDiff = (diff > expandLength) ? diff : expandLength;

                T[] newArray = new T[array.Length + newSizeDiff];
                System.Array.Copy(array, newArray, array.Length);
                this.array = newArray;
            }
        }
        public void EnsureCapacity(UInt32 capacity)
        {
            if ((UInt32)array.Length < capacity)
            {
                UInt32 diff = capacity - (UInt32)array.Length;
                UInt32 newSizeDiff = (diff > expandLength) ? diff : expandLength;

                T[] newArray = new T[array.Length + newSizeDiff];
                System.Array.Copy(array, newArray, array.Length);
                this.array = newArray;
            }
        }

    }
}
