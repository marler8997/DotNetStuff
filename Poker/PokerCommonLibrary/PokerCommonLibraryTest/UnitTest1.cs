using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Marler.Poker
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1
    {

        // This function makes sure that every rank/suit combination has it's own unique
        // card id from 0 to 51
        [TestMethod]
        public void TestCardIDsAreValid()
        {
            Boolean[] cardPicked = new Boolean[52];
            for (int i = 0; i < 52; i++)
            {
                cardPicked[i] = false;
            }

            CardRank[] allCardRanks = (CardRank[])Enum.GetValues(typeof(CardRank));
            for(int i = 0; i < allCardRanks.Length; i++)
            {
                cardPicked[Class.GetCardID(allCardRanks[i], Suit.Club   )] = true;
                cardPicked[Class.GetCardID(allCardRanks[i], Suit.Spade  )] = true;
                cardPicked[Class.GetCardID(allCardRanks[i], Suit.Diamond)] = true;
                cardPicked[Class.GetCardID(allCardRanks[i], Suit.Heart  )] = true;
            }

            for (int i = 0; i < 52; i++)
            {
                Assert.IsTrue(cardPicked[i], String.Format("GetCardID(rank,suit) is invalid because no card rank/suit combination has an id of {0}", i));
            }



        }


        [TestMethod]
        public void TestMath2NCombineK()
        {
            PascalsTriangle pascalsTriange = new PascalsTriangle(52);
            pascalsTriange.Print();

            Assert.AreEqual(1326, pascalsTriange.NChooseK(52, 2));

            Assert.AreEqual(52, pascalsTriange.NChooseK(52, 1));
            Assert.AreEqual(52, pascalsTriange.NChooseK(52, 51));
            Assert.AreEqual(495918532948104, pascalsTriange.NChooseK(52, 26));

            for (int i = 0; i <= 26; i++)
            {
                Assert.AreEqual(pascalsTriange.NChooseK(51, i), pascalsTriange.NChooseK(51, 51 - i));
            }
        }

    }
}
