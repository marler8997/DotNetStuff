using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace More
{
    public interface IRandomGenerator
    {
        void GenerateRandom(Byte[] buffer);
        void GenerateRandom(Byte[] buffer, Int32 offset, Int32 length);
    }

    public class RandomGenerator : IRandomGenerator
    {
        readonly Random random;
        readonly ByteBuffer buffer;
        public RandomGenerator(Random random)
        {
            this.random = random;
            this.buffer = new ByteBuffer();
        }
        public void GenerateRandom(byte[] bytes)
        {
            random.NextBytes(bytes);
        }
        public void GenerateRandom(byte[] bytes, int offset, int length)
        {
            buffer.EnsureCapacityCopyData(length);

        }
    }
}
