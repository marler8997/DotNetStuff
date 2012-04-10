using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.Hmd
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class HmdStringComparerTest
    {
        [TestMethod]
        public void StringCompareTest()
        {
            var testArray = new[] { 
                new { x = "a", y = "a", result = 0 },
                new { x = "a", y = "az", result = -1 },
                new { x = "az", y = "a", result = 1 },
                new { x = "a", y = "b", result = -1 },
                new { x = "b", y = "a", result = 1 },
                new { x = "abcd", y = "abcde", result = -1 },
                new { x = "abcde", y = "abcd", result = 1 },
                new { x = "hello", y = "zow", result = -1 },
                new { x = "hello", y = "hello", result = 0 }
            };

            foreach (var test in testArray)
            {
                int result = HmdStringComparer.CompareStatic(test.x, test.y);
                Console.WriteLine("Compare \"{0}\" \"{1}\" result = {2}", test.x, test.y,result);
                if (result < 0)
                {
                    Assert.IsTrue(test.result < 0);
                }
                else if (result > 0)
                {
                    Assert.IsTrue(test.result > 0);
                }
                else
                {
                    Assert.AreEqual(test.result, result);
                }
            }


        }
    }
}
