using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Marler.Poker
{
    [TestClass]
    public class DeckTest
    {
        [TestMethod]
        public void ManuallyVerify()
        {

            for (int i = 0; i < 10; i++)
            {
                Deck deck = new Deck(new Random((Int32)Stopwatch.GetTimestamp()), 3);
                deck.Shuffle();


                Console.WriteLine("-----------------------");
                for (int cardIndex = 0; cardIndex < deck.cards.Length; cardIndex++)
                {
                    Console.WriteLine(deck.cards[cardIndex]);
                }
            }

        }
    }
}
