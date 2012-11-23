using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.NetworkTools
{
    [TestClass]
    public class SortedListTest
    {

        private void AssertIncreasing(SortedList<Int32> list)
        {
            for (int i = 0; i < list.count - 1; i++)
            {
                Assert.IsTrue(list.elements[i] <= list.elements[i + 1]);
            }
        }
        private void AssertDecreasing(SortedList<Int32> list)
        {
            for (int i = 0; i < list.count - 1; i++)
            {
                Assert.IsTrue(list.elements[i] >= list.elements[i + 1]);
            }
        }

        public void Print(SortedList<Int32> list)
        {
            for (int print = 0; print < list.count; print++)
            {
                Console.WriteLine("{0,20}", list.elements[print]);
            }
            Console.WriteLine();
        }

        [TestMethod]
        public void TestIncreasingSortedList()
        {
            Random generator = new Random();



            SortedList<Int32> increasingList = new SortedList<Int32>(0, 1, Int32IncreasingComparer.Instance);

            for (int i = 0; i < 100; i++)
            {                
                for (int j = 0; j < 50; j++)
                {
                    increasingList.Add(generator.Next());
                    AssertIncreasing(increasingList);
                }

                Print(increasingList);

                //
                // remove some
                //
                int removeCount = generator.Next(increasingList.count);
                Console.WriteLine("remove count {0}", removeCount);
                for (int j = 0; j < removeCount; j++)
                {
                    int removeIndex = generator.Next(increasingList.count - 1);

                    increasingList.Remove(increasingList.elements[removeIndex]);
                    AssertIncreasing(increasingList);
                }

                Print(increasingList);

                increasingList.Clear();
                Assert.AreEqual(0, increasingList.count);

            }

        }
        [TestMethod]
        public void TestDecreasingSortedList()
        {
            Random generator = new Random();

            SortedList<Int32> decreasingList = new SortedList<Int32>(0, 1, Int32DecreasingComparer.Instance);

            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    decreasingList.Add(generator.Next());
                    AssertDecreasing(decreasingList);
                }

                Print(decreasingList);

                //
                // remove some
                //
                int removeCount = generator.Next(decreasingList.count);
                Console.WriteLine("remove count {0}", removeCount);
                for (int j = 0; j < removeCount; j++)
                {
                    int removeIndex = generator.Next(decreasingList.count - 1);

                    decreasingList.Remove(decreasingList.elements[removeIndex]);
                    AssertDecreasing(decreasingList);
                }

                Print(decreasingList);

                decreasingList.Clear();
                Assert.AreEqual(0, decreasingList.count);

            }
        }
    }
}
