using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class OrderedSetTests
    {
        void TestArray<T>(T[] array)
        {
            OrderedSet<T> set;

            set = OrderedSet.VerifySortedAndGetSet(array);

            Array.Reverse(array);
            try
            {
                set = OrderedSet.VerifySortedAndGetSet(array);
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Got expected exception '{0}'", e.Message);
            }

            set = OrderedSet.SortArrayAndGetSet(array);
            set = OrderedSet.VerifySortedAndGetSet(array);
        }

        [TestMethod]
        public void TestVerifySorted()
        {
            OrderedSet<int> set;

            set = OrderedSet.VerifySortedAndGetSet(new int[] { });
            set = OrderedSet.VerifySortedAndGetSet(new int[] { 0});

            TestArray(new int[] { 0, 1 });
            TestArray(new int[] { 0, 1, 2 });
            TestArray(new int[] { 0, 1, 2, 10, 100, 1774 });


            try
            {
                set = OrderedSet.VerifySortedAndGetSet(new int[] { 0, 0 });
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Got expected exception '{0}'", e.Message);
            }

            try
            {
                set = OrderedSet.VerifySortedAndGetSet(new int[] { 0, 1, 1, 2 });
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Got expected exception '{0}'", e.Message);
            }
        }

        [TestMethod]
        public void TestSortArray()
        {
            OrderedSet<int> set;
            try
            {
                set = OrderedSet.SortArrayAndGetSet(new int[] { 0, 0 });
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Got expected exception '{0}'", e.Message);
            }
        }
    }
}
