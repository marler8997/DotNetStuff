using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class CoprimeTests
    {
        [TestMethod]
        public void TestCnFromCnMinusOne()
        {
            UInt32[] CnMinusOne = new UInt32[]{1, 3, 5, 7};
            UInt32[] Cn = new UInt32[3];

            UInt32 CnLength;
            Coprimes.CnMinusOneToCn(CnMinusOne, (UInt32)CnMinusOne.Length, Cn, out CnLength);

            Assert.AreEqual(3U, CnLength);
            Assert.AreEqual(1U, Cn[0]);
            Assert.AreEqual(5U, Cn[1]);
            Assert.AreEqual(7U, Cn[2]);
        }
    }
}
