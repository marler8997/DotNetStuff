using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.Net
{
    /// <summary>
    /// Summary description for CdpTest
    /// </summary>
    public class CdpTest
    {
        public static void AssertEqual(Byte[] expectedBuffer, Byte[] actualBuffer)
        {
            AssertEqual(expectedBuffer, actualBuffer, expectedBuffer.Length);
        }
        public static void AssertEqual(Byte[] expectedBuffer, Byte[] actualBuffer, Int32 length)
        {
            for (int i = 0; i < length; i++)
            {
                Assert.AreEqual(expectedBuffer[i], actualBuffer[i],
                    String.Format("Expected byte at index {0} to be {1} but was {2}", i, expectedBuffer[i], actualBuffer[i]));
            }
        }

    }
}
