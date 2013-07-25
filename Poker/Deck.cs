using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.Poker
{
    public class Deck
    {
        readonly Random random;
        readonly Byte[] randomBuffer;

        public readonly Byte[] cards;
        public Deck(Random random, Int32 size)
        {
            this.random = random;
            this.randomBuffer = new Byte[size];

            cards = new Byte[size];
            for (Byte i = 0; i < cards.Length; i++)
            {
                cards[i] = i;
            }
        }
        public void Shuffle()
        {
            random.NextBytes(randomBuffer);

            for (Byte i = 0; i < cards.Length; i++)
            {
                Byte randomIndex = (Byte)(randomBuffer[i] % cards.Length);

                // Swap the next card with the random index
                Byte temp = cards[randomIndex];
                cards[randomIndex] = cards[i];
                cards[i] = temp;
            }
        }
    }
}
