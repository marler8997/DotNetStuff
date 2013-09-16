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



        [TestMethod]
        public void TestTwinCoprimeCount()
        {
            for (uint i = 3; i < 16; i++)
            {
                TestTwinCoprimeCount(i);
            }
        }
        public void TestTwinCoprimeCount(UInt32 n)
        {
            UInt32 CnCount = (UInt32)Coprimes.CalculateQn(n);
            UInt32 BnCount = (UInt32)Coprimes.CalculateQn(n + 1);

            UInt32 PnPlusOne = PrimeTable.Values[n];
            UInt32 PnPlusOneSquared = PnPlusOne * PnPlusOne;

            DynamicSimpleList<UInt32> CnList = new DynamicSimpleList<UInt32>();
            DynamicSimpleList<UInt32> FnList = new DynamicSimpleList<UInt32>();

            Coprimes.BruteForceCreateCnWithLimit(n, PnPlusOneSquared, CnList, FnList);
            Console.WriteLine("n = {0}, Pn+1 = {1}", n, PnPlusOne);

            UInt32 twinCoprimeCount = 0;
            for (UInt32 i = 2; i + 1 < CnList.Count - 1; i++)
            {
                UInt32 first = CnList[i];
                UInt32 second = CnList[i + 1];
                if (second - first == 2)
                {
                    //Console.WriteLine("Twin Coprime {0}", first);
                    twinCoprimeCount++;
                }
            }

            Console.WriteLine("Cn,2 = {0}  (times Pn+1) = {1}, twin coprimes {2}",
                PnPlusOne, PnPlusOneSquared, twinCoprimeCount);

        }


        [TestMethod]
        public void TestTwinCoprimeCounts()
        {
            TestTwinCoprimeCounts(6);
        }
        public void TestTwinCoprimeCounts(UInt32 n)
        {
            UInt32 CnCount = (UInt32)Coprimes.CalculateQn(n);
            UInt32 BnCount = (UInt32)Coprimes.CalculateQn(n + 1);
            DynamicSimpleList<UInt32> FnList = new DynamicSimpleList<UInt32>();
            UInt32[] Bn = Coprimes.BruteForceCreateCn(n, BnCount + 1, FnList);
            UInt32 PnPlusOne = Bn[1];
            Console.WriteLine("n = {0}, Pn+1 = {1}", n, PnPlusOne);

            Int32 i;
            for (i = 1; i < CnCount; i++)
            {
                UInt32 coprime = Bn[i];
                UInt32 limit = coprime * PnPlusOne;

                UInt32 twinCoprimeCount = 0;
                for (int j = i; j+1 <= BnCount; j++)
                {
                    UInt32 second = Bn[j + 1];
                    if (second >= limit) break;                    
                    UInt32 first = Bn[j];
                    if (second - first == 2)
                    {
                        //Console.WriteLine("Twin Coprime {0}", first);
                        twinCoprimeCount++;
                    }
                }
                
                Console.WriteLine("Cn,{0} = {1}  (times Pn+1) = {2}, twin coprimes {3}",
                    i + 1, coprime, limit, twinCoprimeCount);
            }

        }
    }
}
