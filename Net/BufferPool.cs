using System;
using System.IO;

using Marler.Common;

namespace Marler.Net
{
    public interface IBufferPool
    {
        Int32 CountBuffers(out Int32 totalAllocatedBuffers);
        Byte[] GetBuffer(Int32 size);
        void FreeBuffer(Byte[] buffer);
    }
    public class BufferFactory : ObjectManager<Byte[]>.IObjectFactory
    {
        public readonly Int32 length;
        public BufferFactory(Int32 length)
        {
            this.length = length;
        }
        public Byte[] GenerateObject()
        {
            return new Byte[length];
        }
    }

    //
    // This buffer pool uses a variable bucket size that increases linearly, i.e.
    // If the smallest bucket is 10 bytes, then the next smallest will be 20, 30, 40, an so on.
    //
    // This class is constructed using the maximum buffer size and the bucket count.
    // The range of buffer sizes will be from 0 to maxBufferSize, which is maxBufferSize+1.
    //
    // The equation to get the smallestBucketSize is found in the constructor:
    //     smalletBucketSize = range / (bucketCount+1) / bucketCount * 2
    //
    // This equation can be derived using the fact that the nth bucket has a sice of n * smallestBucketSize,
    // and the range is equal to the sum of the size of the buckets.
    //
    // This class implements its own thread safety because it is intended to be used among multiple threads
    //
    public class LinearBucketSizeBufferPool : IBufferPool
    {
        public static TextWriter DebugLog = null;

        public readonly Int32 maxBufferSize;
        public readonly Int32 smallestBucketSize;

        ObjectManager<Byte[]>[] buckets;
        Int32 [] bucketBufferSizes;

        public LinearBucketSizeBufferPool(Int32 maxBufferSize, Int32 bucketCount, 
            Int32 smallestInitialCapacity, Int32 initialCapacityDiffForLargerBuckets)
        {
            this.maxBufferSize = maxBufferSize;

            Double smallestBucketSizeAsDouble = (Double)(
                (Double)(maxBufferSize+1)                /
                (Double)((bucketCount + 1) *bucketCount) * 
                2.0
            );

            this.buckets = new ObjectManager<Byte[]>[bucketCount];
            this.bucketBufferSizes = new Int32[bucketCount];

            Int32 bucketIndex;
            Int32 bucketBufferSize = 0;
            Int32 initAndExtend = smallestInitialCapacity;
            for (bucketIndex = 0; bucketIndex < bucketCount - 1; bucketIndex++)
            {
                Int32 nextBucketRange = (Int32)(smallestBucketSizeAsDouble * (Double)(bucketIndex + 1));
                bucketBufferSize += nextBucketRange;

                if(DebugLog != null) Console.WriteLine("[BufferPoolDebug] Bucket '{0}' Size '{1}' Range '{2}' Bytes (init cap={3})",
                    bucketIndex, bucketBufferSize, nextBucketRange, initAndExtend);
                this.buckets[bucketIndex] = new ObjectManager<Byte[]>(new BufferFactory(bucketBufferSize), initAndExtend, initAndExtend);
                this.bucketBufferSizes[bucketIndex] = bucketBufferSize;

                initAndExtend -= initialCapacityDiffForLargerBuckets;
                if(initAndExtend < 1) initAndExtend = 1;
            }

            Int32 lastBucketSize = maxBufferSize - bucketBufferSize;

            if (DebugLog != null) Console.WriteLine("[BufferPoolDebug] Bucket '{0}' Size '{1}' Range '{2}' Bytes (init cap={3})",
                bucketIndex, maxBufferSize, lastBucketSize, initAndExtend);
            this.buckets[bucketIndex] = new ObjectManager<Byte[]>(new BufferFactory(maxBufferSize), initAndExtend, initAndExtend);
            this.bucketBufferSizes[bucketIndex] = maxBufferSize;
        }
        // returns buffers that are currently reserved
        public Int32 CountBuffers(out Int32 totalAllocatedBuffers)
        {
            totalAllocatedBuffers = 0;
            Int32 reservedBuffers = 0;
            for (int i = 0; i < buckets.Length; i++)
            {
                ObjectManager<Byte[]> bucket = buckets[i];
                lock (bucket)
                {
                    totalAllocatedBuffers += bucket.AllocatedObjectsCount();
                    reservedBuffers += bucket.ReservedObjectsCount();
                }
            }
            return reservedBuffers;
        }
        public Byte[] GetBuffer(Int32 size)
        {
            if (size > maxBufferSize)
            {
                throw new InvalidOperationException(String.Format("Buffer size '{0}' is too big", size));
                //throw new InvalidOperationException(String.Format("Negative buffer size '{0}' is invalid", size));
            }

            for (Int32 i = 0; i < bucketBufferSizes.Length; i++)
            {
                if (bucketBufferSizes[i] >= size)
                {
                    if (DebugLog != null) Console.WriteLine("[BufferPoolDebug] GetBuffer({0}) returned {1} size buffer from bucket {2}",
                        size, bucketBufferSizes[i], i);
                    ObjectManager<Byte[]> bucket = buckets[i];
                    lock (bucket)
                    {
                        return buckets[i].Reserve();
                    }
                }
            }

            throw new InvalidOperationException(String.Format("Failed to find a bucket for buffer of size {0}", size));
        }
        public void FreeBuffer(Byte[] buffer)
        {
            if (DebugLog != null) Console.WriteLine("[BufferPoolDebug] FreeBuffer({0})", buffer.Length);

            Int32 bufferLength = buffer.Length;
            for (Int32 i = 0; i < bucketBufferSizes.Length; i++)
            {
                if (bufferLength == bucketBufferSizes[i])
                {
                    ObjectManager<Byte[]> bucket = buckets[i];
                    lock(bucket)
                    {
                        buckets[i].Release(buffer);
                    }
                    return;
                }
            }
            throw new InvalidOperationException(String.Format("Failed to find a bucket for buffer of size {0}", bufferLength));
        }
    }
    /*
    public class BufferPool : IBufferPool
    {
        ObjectManager<Byte[]> bufferList16Bytes;
        ObjectManager<Byte[]> bufferList64Bytes;
        ObjectManager<Byte[]>[] bufferList256ByteMultiples;

        // Note: bufferListBy256[x].Length will always be 256*(x+1)

        public BufferPool()
        {
            //
            // Initialize 16 byte buffers
            //
            this.bufferList16Bytes = new ObjectManager<Byte[]>(
                new BufferFactory(16), 256, 256);

            //
            // Initialize 64 byte buffers
            //
            this.bufferList64Bytes = new ObjectManager<Byte[]>(
                new BufferFactory(64), 256, 256);

            //
            // Initialize 256 byte multiple buffers
            //
            this.bufferList256ByteMultiples = new ObjectManager<Byte[]>[256];

            //
            // Initialize 256 byte buffers
            //
            for (int i = 0; i < bufferList256ByteMultiples.Length; i++)
            {
                Int32 initAndExtend = 256 - i;
                this.bufferList256ByteMultiples[i] = new ObjectManager<Byte[]>(
                    new BufferFactory((i + 1) * 256), initAndExtend, initAndExtend);
            }
        }
        public Int32 CountBuffers(out Int32 totalAllocatedBuffers)
        {
            throw new NotImplementedException();
        }
        public Byte[] GetBuffer(Int32 size)
        {
            if ((size & 0xFFFF) != size)
            {
                if (size > 0xFFFF) throw new InvalidOperationException(String.Format("Buffer size '{0}' is too big", size));
                throw new InvalidOperationException(String.Format("Negative buffer size '{0}' is invalid", size));
            }

            if (size <= 16) return bufferList16Bytes.Reserve();
            if (size <= 64) return bufferList64Bytes.Reserve();

            Int32 multipleOf256Index = (0xFF & ((size - 1) >> 8));// Range(0,254)

            ObjectManager<Byte[]> bufferManager = bufferList256ByteMultiples[multipleOf256Index];
            if (bufferManager.ThereExistsAllocatedObjectsThatAreFree())
            {
                Console.WriteLine("[BufferPoolDebug] GetBuffer({0}) returned preallocated buffer", size);
                return bufferManager.Reserve();
            }

            //
            // Check if there is another larger buffer that has been allocated within an acceptable range
            //
            Int32 acceptableBufferIndexRange = multipleOf256Index >> 1; // will accept buffers that have already been allocated up
            // to 1.5 times the size
            //Int32 acceptableBufferIndexRange = multipleOf256Index >> 2; // will accept buffers that have already been allocated up
            //                                                            // to 1.25 times the size
            Int32 bufferManagerIndex = multipleOf256Index + 1;
            for (int i = 0; i < acceptableBufferIndexRange && bufferManagerIndex < 256; i++)
            {
                ObjectManager<Byte[]> largerBufferManger = bufferList256ByteMultiples[bufferManagerIndex];
                if (largerBufferManger.ThereExistsAllocatedObjectsThatAreFree())
                {
                    Console.WriteLine("[BufferPoolDebug] GetBuffer({0}) returned larger preallocated buffer of size '{1}'", size, (i + 1) * 256);
                    return largerBufferManger.Reserve();
                }
                bufferManagerIndex++;
            }

            Console.WriteLine("[BufferPoolDebug] GetBuffer({0}) returned  newly allocated buffer", size);
            return bufferManager.Reserve();
        }
        public void FreeBuffer(Byte[] buffer)
        {
            Console.WriteLine("[BufferPoolDebug] FreeBuffer({0})", buffer.Length);

            Int32 bufferLength = buffer.Length;
            if (bufferLength == 16) bufferList16Bytes.Release(buffer);
            if (bufferLength == 64) bufferList64Bytes.Release(buffer);

            bufferList256ByteMultiples[(0xFF & ((buffer.Length - 1) >> 8))].Release(buffer);
        }
    }
    */
}
