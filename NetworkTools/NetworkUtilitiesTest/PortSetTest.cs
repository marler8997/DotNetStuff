using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.NetworkTools
{
    [TestClass]
    public class PortSetTest
    {
        public void ValidatePortSetSingle(PortSet portSet, UInt16 port)
        {
            // Verify is right class type
            Assert.IsTrue(portSet is PortSetSingle);

            // Verify Length
            Assert.AreEqual(1, portSet.Length);

            // Verify port
            Assert.AreEqual(port, portSet[0]);

            // Verify Contains Method
            Assert.IsTrue(portSet.Contains(port));

            // Verify Exception
            try
            {
                UInt16 secondPort = portSet[1];
                Assert.Fail();
            }
            catch (IndexOutOfRangeException) { }
        }

        public void ValidatePortSetDouble(PortSet portSet, UInt16 lowPort, UInt16 highPort)
        {
            // Verify is right class type
            Assert.IsTrue(portSet is PortSetDouble);

            // Verify Length
            Assert.AreEqual(2, portSet.Length);

            // Verify ports
            Assert.AreEqual(lowPort, portSet[0]);
            Assert.AreEqual(highPort, portSet[1]);

            // Verify Contains Method
            Assert.IsTrue(portSet.Contains(lowPort));
            Assert.IsTrue(portSet.Contains(highPort));

            // Verify Exception
            try
            {
                UInt16 thirdPort = portSet[2];
                Assert.Fail();
            }
            catch (IndexOutOfRangeException) { }

            // Verify Order
            Assert.IsTrue(portSet[0] < portSet[1]);
        }

        public void ValidatePortSetArray(PortSet portSet, params UInt16[] sortedPorts)
        {
            // Verify is right class type
            Assert.IsTrue(portSet is PortSetArray);

            // Verify Length
            Assert.AreEqual(sortedPorts.Length, portSet.Length);

            // Verify ports and Contains method
            for (int i = 0; i < sortedPorts.Length; i++)
            {
                Assert.AreEqual(sortedPorts[i], portSet[i]);
                Assert.IsTrue(portSet.Contains(sortedPorts[i]));
            }

            // Verify Exception
            if (sortedPorts.Length < UInt16.MaxValue)
            {
                try
                {
                    UInt16 thirdPort = portSet[sortedPorts.Length + 1];
                    Assert.Fail();
                }
                catch (IndexOutOfRangeException) { }
            }

            // Verify Order
            for (int i = 1; i < portSet.Length; i++)
            {
                Assert.IsTrue(portSet[i] > portSet[i - 1]);
            }
        }

        [TestMethod]
        public void PortSetSingleTest()
        {
            try
            {
                PortSetSingle temp = new PortSetSingle(0);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException) { }

            ValidatePortSetSingle(new PortSetSingle(1), 1);
            ValidatePortSetSingle(new PortSetSingle(2), 2);
            ValidatePortSetSingle(new PortSetSingle(65535), 65535);

            PortSet portSet = new PortSetSingle(10);

            ValidatePortSetSingle(portSet.Combine(10), 10);
            ValidatePortSetSingle(portSet.Combine(portSet), 10);

            ValidatePortSetDouble(portSet.Combine(1), 1, 10);
            ValidatePortSetDouble(portSet.Combine(50), 10, 50);
            ValidatePortSetDouble(portSet.Combine(65535), 10, 65535);

            ValidatePortSetDouble(portSet.Combine(new PortSetSingle(11)), 10, 11);
            ValidatePortSetDouble(portSet.Combine(new PortSetSingle(1)), 1, 10);
            ValidatePortSetDouble(portSet.Combine(new PortSetSingle(9)), 9, 10);
            ValidatePortSetDouble(portSet.Combine(new PortSetSingle(11)), 10, 11);
            ValidatePortSetDouble(portSet.Combine(new PortSetSingle(65535)), 10, 65535);

            ValidatePortSetArray(portSet.Combine(new PortSetDouble(2, 1)), 1, 2, 10);
            ValidatePortSetArray(portSet.Combine(new PortSetDouble(11, 1)), 1, 10, 11);
            ValidatePortSetArray(portSet.Combine(new PortSetDouble(65535, 65534)), 10, 65534, 65535);

            ValidatePortSetArray(portSet.Combine(new PortSetArray(new UInt16[] { 1, 2, 3 })), 1, 2, 3, 10);
            ValidatePortSetArray(portSet.Combine(new PortSetArray(new UInt16[] { 1, 2, 11 })), 1, 2, 10, 11);
            ValidatePortSetArray(portSet.Combine(new PortSetArray(new UInt16[] { 1, 11, 12 })), 1, 10, 11, 12);
            ValidatePortSetArray(portSet.Combine(new PortSetArray(new UInt16[] { 11, 12, 13 })), 10, 11, 12, 13);
        }

        [TestMethod]
        public void PortSetDoubleTest()
        {
            try
            {
                PortSetDouble temp = new PortSetDouble(10, 10);
                Assert.Fail();
            }
            catch (ArgumentException) { }
            try
            {
                PortSetDouble temp = new PortSetDouble(0, 1);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException) { }

            ValidatePortSetDouble(new PortSetDouble(2, 1), 1, 2);
            ValidatePortSetDouble(new PortSetDouble(65535, 20), 20, 65535);
            ValidatePortSetDouble(new PortSetDouble(9915, 5), 5, 9915);

            PortSet portSet = new PortSetDouble(100, 6000);

            ValidatePortSetDouble(portSet.Combine(new PortSetSingle(100)), 100, 6000);
            ValidatePortSetDouble(portSet.Combine(new PortSetSingle(6000)), 100, 6000);
            ValidatePortSetArray(portSet.Combine(new PortSetSingle(99)), 99, 100, 6000);
            ValidatePortSetArray(portSet.Combine(new PortSetSingle(101)), 100, 101, 6000);
            ValidatePortSetArray(portSet.Combine(new PortSetSingle(6001)), 100, 6000, 6001);
            ValidatePortSetArray(portSet.Combine(new PortSetDouble(101, 5999)), 100, 101, 5999, 6000);
            ValidatePortSetArray(portSet.Combine(new PortSetDouble(99, 5999)), 99, 100, 5999, 6000);
            ValidatePortSetArray(portSet.Combine(new PortSetDouble(1, 3000)), 1, 100, 3000, 6000);
            ValidatePortSetArray(portSet.Combine(new PortSetDouble(3000, 65535)), 100, 3000, 6000, 65535);

            //ValidatePortSetArray(portSet.Combine(new PortSetSingle(9)).Combine(new PortSetSingle(11)), 9, 10, 11);
            //ValidatePortSetSingle(portSet.Combine(new PortSetSingle(10)).Combine(new PortSetSingle(10)), 10);
        }



        public String ToString(UInt16[] array)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Int32 i;
            for (i = 0; i < array.Length - 1; i++)
            {
                stringBuilder.Append(array[i]);
                stringBuilder.Append(' ');
            }
            stringBuilder.Append(array[i]);
            return stringBuilder.ToString();
        }

        public void ValidateCombineArray(UInt16[] array, params UInt16[] expectedArray)
        {
            Assert.AreEqual(expectedArray.Length, array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                Assert.AreEqual(expectedArray[i], array[i],
                    String.Format("Mismatch at index: {0}, Expected Array: {1}, Actual Array: {2}",
                    i, ToString(expectedArray), ToString(array)));
            }
        }

        [TestMethod]
        public void SortedCombineTest()
        {
            ValidateCombineArray(PortSetMethods.SortedCombine(
                new UInt16[] { 1 }, new UInt16[] { 1, 2, 3, 4 }),
                1, 2, 3, 4);
            ValidateCombineArray(PortSetMethods.SortedCombine(
                new UInt16[] { 1, 2 }, new UInt16[] { 1, 2, 3, 4 }),
                1, 2, 3, 4);
            ValidateCombineArray(PortSetMethods.SortedCombine(
                new UInt16[] { 1, 2, 3 }, new UInt16[] { 1, 2, 3, 4 }),
                1, 2, 3, 4);
            ValidateCombineArray(PortSetMethods.SortedCombine(
                new UInt16[] { 1, 2, 3, 4 }, new UInt16[] { 1, 2, 3, 4 }),
                1, 2, 3, 4); ;

            ValidateCombineArray(PortSetMethods.SortedCombine(
                new UInt16[] { 1, 2, 3, 4 }, new UInt16[] { 1 }),
                1, 2, 3, 4);
            ValidateCombineArray(PortSetMethods.SortedCombine(
                new UInt16[] { 1, 2, 3, 4 }, new UInt16[] { 1, 2 }),
                1, 2, 3, 4);
            ValidateCombineArray(PortSetMethods.SortedCombine(
                new UInt16[] { 1, 2, 3, 4 }, new UInt16[] { 1, 2, 3 }),
                1, 2, 3, 4);
            ValidateCombineArray(PortSetMethods.SortedCombine(
                new UInt16[] { 1, 2, 3, 4 }, new UInt16[] { 1, 2, 3, 4 }),
                1, 2, 3, 4);

            ValidateCombineArray(PortSetMethods.SortedCombine(
                new UInt16[] { 1, 3, 5, 7 }, new UInt16[] { 2, 4, 6, 8 }),
                1, 2, 3, 4, 5, 6, 7, 8);
        }


    }
}
