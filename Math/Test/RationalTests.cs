using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using More;

namespace More.Math.Test
{
    [TestClass]
    public class RationalTests
    {

        [TestMethod]
        public void TestRationals()
        {
            for (int i = 0; i < 400; i++)
            {
                Rational rational = new Rational(i);
                Assert.AreEqual((Single)i, rational.ConvertToSingle());
                Assert.AreEqual((Double)i, rational.ConvertToDouble());


                /*
                for (int j = 0; j < 10; j++)
                {
                    Ration
                }
                */
            }

        }
        
        [TestMethod]
        public void TestFactoredIntegers()
        {
            BruteForcePrimeFactorizer factorizer = new BruteForcePrimeFactorizer();
            TestFactoredIntegers(factorizer);
        }
        public void TestFactoredIntegers(IPrimeFactorizer factorizer)
        {
            FactoredInt32 factoredInt32;
            FactoredUInt32 factoredUInt32;

            factoredInt32 = FactoredInt32.Create(0, factorizer);
            Assert.AreEqual(FactoredInt32.Zero, factoredInt32);

            factoredUInt32 = FactoredUInt32.Create(0, factorizer);
            Assert.AreEqual(FactoredUInt32.Zero, factoredUInt32);


            factoredInt32 = FactoredInt32.Create(1, factorizer);
            Assert.AreEqual(FactoredInt32.One, factoredInt32);

            factoredUInt32 = FactoredUInt32.Create(1, factorizer);
            Assert.AreEqual(FactoredUInt32.One, factoredUInt32);

        }





        [TestMethod]
        public void TestFactoredRationals()
        {
            BruteForcePrimeFactorizer factorizer = new BruteForcePrimeFactorizer();
            TestFactoredRationals(factorizer);


        }

        public void TestFactoredRationals(IPrimeFactorizer factorizer)
        {
            FactoredRational factoredRational;
            
            factoredRational = new FactoredRational(Rational.Nan, factorizer);
            Assert.AreEqual(FactoredRational.Nan, factoredRational);

            factoredRational = new FactoredRational(Rational.Infinity, factorizer);
            Assert.AreEqual(FactoredRational.Infinity, factoredRational);

            factoredRational = new FactoredRational(Rational.Zero, factorizer);
            Assert.AreEqual(FactoredRational.Zero, factoredRational);


            //
            // Test simplifying to 1
            //
            for (int i = 1; i < 30; i++)
            {
                factoredRational = new FactoredRational(new Rational(i, (UInt32)i), factorizer);
                Assert.AreEqual((Single)1, factoredRational.ConvertToSingle());
                Assert.AreEqual((Double)1, factoredRational.ConvertToDouble());
            }








            //
            // TODO: Add test logic	here
            //
        }
    }
}
