using System;
using System.Diagnostics;

namespace More
{
    public static class Segments
    {
        public static SegmentByLimit SegmentByLimit(this Byte[] array)
        {
            return new SegmentByLimit(array, 0, (UInt32)array.Length);
        }
        public static SegmentByLength SegmentByLength(this Byte[] array)
        {
            return new SegmentByLength(array, 0, (UInt32)array.Length);
        }
    }
    public struct Segment
    {
        public Byte[] array;
        public UInt32 offset;
        public UInt32 lengthOrLimit;
        public Segment(Byte[] array, UInt32 offset, UInt32 lengthOrLimit)
        {
            Debug.Assert(array == null || offset <= array.Length);
            this.array = array;
            this.offset = offset;
            this.lengthOrLimit = lengthOrLimit;
        }
    }
    public struct SegmentByLimit
    {
        public Byte[] array;
        public UInt32 offset;
        public UInt32 limit;
        public SegmentByLimit(Byte[] array, UInt32 offset, UInt32 limit)
        {
            this.array = array;
            this.offset = offset;
            this.limit = limit;

            Debug.Assert(InValidState());
        }
        public static Boolean InValidState(Byte[] array, UInt32 offset, UInt32 limit)
        {
            return (offset <= limit) &&
                (
                    (
                        (array != null) && (limit <= array.Length)
                    ) || (
                        (array == null) && (limit - offset == 0)
                    )
                );
        }
        public Boolean InValidState()
        {
            return (offset <= limit) &&
                (
                    (
                        (array != null) && (limit <= array.Length)
                    ) || (
                        (array == null) && (limit - offset == 0)
                    )
                );
        }
        public SegmentByLength ByLength()
        {
            return new SegmentByLength(array, offset, offset + limit);
        }
    }
    public struct SegmentByLength
    {
        public Byte[] array;
        public UInt32 offset;
        public UInt32 length;
        public SegmentByLength(Byte[] array, UInt32 offset, UInt32 length)
        {
            this.array = array;
            this.offset = offset;
            this.length = length;

            Debug.Assert(InValidState());
        }
        public SegmentByLength(Byte[] array)
        {
            this.array = array;
            this.offset = 0;
            this.length = (UInt32)array.Length;
        }
        public static Boolean InValidState(Byte[] array, UInt32 offset, UInt32 length)
        {
            return length == 0 || (array != null && offset + length <= array.Length);
        }
        public Boolean InValidState()
        {
            return length == 0 || (array != null && offset + length <= array.Length);
        }
        public static explicit operator SegmentByLength(Byte[] array)
        {
            return new SegmentByLength(array, 0, (UInt32)array.Length);
        }
        /*
        public Boolean EqualsString(String compare, Boolean ignoreCase)
        {
            if (length != compare.Length) return false;

            for (UInt32 i = 0; i < length; i++)
            {
                if ((Char)array[offset + i] != compare[(int)i])
                {
                    if (!ignoreCase) return false;
                    if (Char.IsUpper(compare[(int)i]))
                    {
                        if (Char.IsUpper((Char)array[offset + i])) return false;
                        if (Char.ToUpper((Char)array[offset + i]) != compare[(int)i]) return false;
                    }
                    else
                    {
                        if (Char.IsLower((Char)array[offset + i])) return false;
                        if (Char.ToLower((Char)array[offset + i]) != compare[(int)i]) return false;
                    }
                }
            }
            return true;
        }
        */



        // Peel the first string until whitespace
        public static SegmentByLength PeelAscii(ref SegmentByLength segment)
        {
            Debug.Assert(segment.InValidState());

            if (segment.length == 0)
            {
                return new SegmentByLength(segment.array, segment.offset, 0);
            }

            Char c;

            UInt32 offset = segment.offset;
            UInt32 segmentLimit = offset + segment.length;

            //
            // Skip beginning whitespace
            //
            while (true)
            {
                if (offset >= segmentLimit)
                {
                    segment.offset = offset;
                    segment.length = 0;
                    return new SegmentByLength(segment.array, offset, 0);
                }
                c = (Char)segment.array[offset];
                if (!Char.IsWhiteSpace(c)) break;
                offset++;
            }

            UInt32 startOffset = offset;

            //
            // Find next whitespace
            //
            SegmentByLength peelSegment;

            while (true)
            {
                offset++;
                if (offset >= segmentLimit)
                {
                    peelSegment = new SegmentByLength(segment.array, startOffset, offset - startOffset);
                    segment.offset = offset;
                    segment.length = 0;
                    return peelSegment;
                }
                c = (Char)segment.array[offset];
                if (Char.IsWhiteSpace(c)) break;
            }

            peelSegment = new SegmentByLength(segment.array, startOffset, offset - startOffset);

            //
            // Remove whitespace till rest
            //
            while (true)
            {
                offset++;
                if (offset >= segmentLimit)
                {
                    segment.offset = offset;
                    segment.length = 0;
                    return peelSegment;
                }
                if (!Char.IsWhiteSpace((Char)segment.array[offset]))
                {
                    segment.length -= (offset - segment.offset);
                    segment.offset = offset;
                    return peelSegment;
                }
            }
        }
    }





    public struct Segment<T>
    {
        public T[] array;
        public UInt32 offset;
        public UInt32 length;
    }
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

    public interface IBuffer
    {
        Byte[] Array { get; }
        void EnsureCapacityCopyData(Int32 capacity);
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
}
