using System;
using System.Text;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    [TestClass]
    public class PrimeFactorsTest
    {
        [TestMethod]
        public void TestPrimeFactorsInvertTo()
        {
            PoweredPrime[] factors;
            List<PoweredPrime> factorList = new List<PoweredPrime>();

            factors = PoweredPrime.None;
            factors.InvertTo(factorList);
            Assert.AreEqual(0, factorList.Count);

            //
            // 2^1
            //
            factors = new PoweredPrime[] { new PoweredPrime(2, 1) };

            factorList.Clear();
            factors.InvertTo(factorList);
            Assert.AreEqual(1, factorList.Count);
            Assert.AreEqual(2U, factorList[0].prime);
            Assert.AreEqual((SByte)(-1), factorList[0].power);

            //
            // 2^-1
            //
            factors = new PoweredPrime[] { new PoweredPrime(2, -1) };

            factorList.Clear();
            factors.InvertTo(factorList);
            Assert.AreEqual(1, factorList.Count);
            Assert.AreEqual(2U, factorList[0].prime);
            Assert.AreEqual((SByte)(1), factorList[0].power);

            //
            // (2^120)(3^-80)(19^65)
            //
            factors = new PoweredPrime[] { new PoweredPrime(2, 12), new PoweredPrime(3, -8), new PoweredPrime(19, 6) };

            factorList.Clear();
            factors.InvertTo(factorList);
            Assert.AreEqual(3, factorList.Count);
            Assert.AreEqual(2U, factorList[0].prime);
            Assert.AreEqual((SByte)(-12), factorList[0].power);
            Assert.AreEqual(3U, factorList[1].prime);
            Assert.AreEqual((SByte)(8), factorList[1].power);
            Assert.AreEqual(19U, factorList[2].prime);
            Assert.AreEqual((SByte)(-6), factorList[2].power);
        }

        [TestMethod]
        public void TestPrimeFactorsDivideInto()
        {
            PoweredPrime[] divideFactors;
            List<PoweredPrime> factorList = new List<PoweredPrime>();

            divideFactors = PoweredPrime.None;
            divideFactors.DivideInto(factorList);
            Assert.AreEqual(0, factorList.Count);

            //
            // 2^10 / 1
            //
            factorList.Clear();
            factorList.Add(new PoweredPrime(2, 10));

            divideFactors.DivideInto(factorList);
            Assert.AreEqual(1, factorList.Count);
            Assert.AreEqual(2U, factorList[0].prime);
            Assert.AreEqual((SByte)(10), factorList[0].power);

            //
            // 2^10 / 2^8 == 2^2
            //
            factorList.Clear();
            factorList.Add(new PoweredPrime(2, 10));

            new PoweredPrime[] { new PoweredPrime(2, 8) }.DivideInto(factorList);
            Assert.AreEqual(1, factorList.Count);
            Assert.AreEqual(2U, factorList[0].prime);
            Assert.AreEqual((SByte)(2), factorList[0].power);


            //
            // 2^10 / 2^12  == 2^-2
            //
            factorList.Clear();
            factorList.Add(new PoweredPrime(2, 10));

            new PoweredPrime[] { new PoweredPrime(2, 12) }.DivideInto(factorList);
            Assert.AreEqual(1, factorList.Count);
            Assert.AreEqual(2U, factorList[0].prime);
            Assert.AreEqual((SByte)(-2), factorList[0].power);


            //
            // 2^10 / 2^10 == None
            //
            factorList.Clear();
            factorList.Add(new PoweredPrime(2, 10));

            new PoweredPrime[] { new PoweredPrime(2, 10) }.DivideInto(factorList);
            Assert.AreEqual(0, factorList.Count);



            //
            // (2^4)(3^-2)(7^2)(19^3) / (2^1)(3^3)(5^2)(7^2)(23^-2) == (2^3)(3^-5)(5^-2)(19^65)(23^2)
            //
            factorList.Clear();
            factorList.Add(new PoweredPrime(2, 4));
            factorList.Add(new PoweredPrime(3, -2));
            factorList.Add(new PoweredPrime(7, 2));
            factorList.Add(new PoweredPrime(19, 3));

            new PoweredPrime[] {
                new PoweredPrime(2, 1),
                new PoweredPrime(3, 3),
                new PoweredPrime(5, 2),
                new PoweredPrime(7, 2),
                new PoweredPrime(23, -2),
            }.DivideInto(factorList);

            Assert.AreEqual(5, factorList.Count);
            Assert.AreEqual(2U          , factorList[0].prime);
            Assert.AreEqual((SByte)(3)  , factorList[0].power);
            Assert.AreEqual(3U          , factorList[1].prime);
            Assert.AreEqual((SByte)(-5) , factorList[1].power);
            Assert.AreEqual(5U          , factorList[2].prime);
            Assert.AreEqual((SByte)(-2) , factorList[2].power);
            Assert.AreEqual(19U         , factorList[3].prime);
            Assert.AreEqual((SByte)(3)  , factorList[3].power);
            Assert.AreEqual(23U         , factorList[4].prime);
            Assert.AreEqual((SByte)(2) , factorList[4].power);
        }
    }
}
