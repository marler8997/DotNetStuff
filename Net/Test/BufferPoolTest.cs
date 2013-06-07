using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace More.Net
{
    public static class BufferPoolTestExtensions
    {
        public static void AssertBufferCount(this IBufferPool bufferPool, Int32 expectedReserveCount, Int32 expectedTotalAllocatedCount)
        {
            Int32 actualTotalAllocatedCount;
            Int32 actualReserveCount = bufferPool.CountBuffers(out actualTotalAllocatedCount);

            Assert.AreEqual(expectedReserveCount, actualReserveCount);
            Assert.AreEqual(expectedTotalAllocatedCount, expectedTotalAllocatedCount);
        }
    }
    /// <summary>
    /// Summary description for BufferPoolTest
    /// </summary>
    [TestClass]
    public class BufferPoolTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            LinearBucketSizeBufferPool bufferPool = new LinearBucketSizeBufferPool(10, 4, 20, 5);



            bufferPool = new LinearBucketSizeBufferPool(400, 10, 1, 0);

            bufferPool.AssertBufferCount(0, 0);

            Byte[] temp = bufferPool.GetBuffer(0);
            bufferPool.AssertBufferCount(1, 1);
            bufferPool.FreeBuffer(temp);
            bufferPool.AssertBufferCount(0, 1);

            temp = bufferPool.GetBuffer(400);
            bufferPool.AssertBufferCount(1, 1);
            bufferPool.FreeBuffer(temp);
            bufferPool.AssertBufferCount(0, 1);

        }
    }
}
