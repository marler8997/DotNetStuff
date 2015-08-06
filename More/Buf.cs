using System;
using System.Diagnostics;

namespace More
{
    public struct BufStruct
    {
        public Byte[] buf;
        public UInt32 contentLength;
    }


    // This class wraps a Byte array that can be passed to and from functions
    // that will ensure that the array will be expanded to accomodate as much data is needed.
    public class Buf
    {
        public const UInt32 DefaultExpandLength = 128;
        public const UInt32 DefaultInitialCapacity = 128;

        public Byte[] array;
        public readonly UInt32 expandLength;

        public Buf()
            : this(DefaultInitialCapacity, DefaultExpandLength)
        {
        }
        public Buf(UInt32 initialCapacity)
        {
            this.expandLength = initialCapacity;
            this.array = new Byte[initialCapacity];
        }
        public Buf(UInt32 initialCapacity, UInt32 expandLength)
        {
            this.expandLength = expandLength;
            this.array = new Byte[initialCapacity];
        }
        public void EnsureCapacityNoCopy(UInt32 capacity)
        {
            if ((UInt32)array.Length < capacity)
            {
                UInt32 diff = capacity - (UInt32)array.Length;
                UInt32 newSizeDiff = (diff > expandLength) ? diff : expandLength;
                this.array = new Byte[array.Length + newSizeDiff];
            }
        }
        public void EnsureCapacityCopyData(Int32 capacity)
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
        public void EnsureCapacityCopyData(UInt32 capacity)
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
    // This class wraps an array that can be passed to and from functions
    // that will ensure that the array size is expanded when needed
    public class Expandable<T>
    {
        public const UInt32 DefaultExpandLength = 128;
        public const UInt32 DefaultInitialCapacity = 128;

        public T[] array;
        public readonly UInt32 expandLength;

        public Expandable()
            : this(DefaultInitialCapacity, DefaultExpandLength)
        {
        }
        public Expandable(UInt32 initialCapacity, UInt32 expandLength)
        {
            this.expandLength = expandLength;
            this.array = new T[initialCapacity];
        }
        public void EnsureCapacityNoCopy(UInt32 capacity)
        {
            if ((UInt32)array.Length < capacity)
            {
                UInt32 diff = capacity - (UInt32)array.Length;
                UInt32 newSizeDiff = (diff > expandLength) ? diff : expandLength;
                this.array = new T[array.Length + newSizeDiff];
            }
        }
        public void EnsureCapacityCopyData(Int32 capacity)
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
        public void EnsureCapacityCopyData(UInt32 capacity)
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
    /*
    public class Allocater<T>
    {
        public const UInt32 DefaultExpandLength = 128;
        public const UInt32 DefaultInitialCapacity = 128;

        public readonly UInt32 expandLength;

        public T[] array;
        public UInt32 length;

        public Allocater()
            : this(DefaultInitialCapacity, DefaultExpandLength)
        {
        }
        public Allocater(UInt32 initialCapacity, UInt32 expandLength)
        {
            this.expandLength = expandLength;

            this.array = new T[initialCapacity];
            this.length = 0;
        }
        public UInt32 Allocate()
        {
            if (length >= array.Length)
            {
                T[] newArray = new T[array.Length + expandLength];
                System.Array.Copy(array, newArray, array.Length);
                this.array = newArray;
            }
            UInt32 index = length;
            length++;
            return index;
        }
    }
    */
}
