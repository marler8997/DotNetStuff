using System;

namespace Marler.Poker
{

    //
    //
    // There are 1,326 (52 Choose 2) unique hole card combinations
    // Since suit has no relative value (AhJh=AsJs), many combinations are equivalent in value.
    // A "hand" is defined as a bucket of hold card combinations equivalent in value.
    //   i.e. The hand AJs "Ace Jack Suited" is the bucket for AhJh,AsJs,AcJc,AdJd
    //   or   The hand 29o "29 offsuit" is the bucket for 2c9s,2c9d,2c9h,2s9d,2s9h,2d9h
    //   or   The hand QQ "Queens" is the bucket for QcQs,QcQd,QcQh,QsQd,QsQh,QdQh
    //
    // There are 169 non-equivalent "hands" categorized by the following 3 "Hand Shapes":
    //   13 Pocket Pairs                    +
    //   13 Choose 2 (or 78) Suited Hands   +
    //   13 Choose 2 (or 78) Unsuited Hands +
    // If 2 hands have the same shape, they are equally likely to appear according to the following table:
    // 
    // Hand Shape    | Hand  | Hold Card Combinations        | Hold Card Combinations | Probabilty of this Hand Shape    | Probability of a Specific Hand in this Hand Shape
    //               | Count | Per Hand (Hand Bucket Size)   |     Per Hand Shape     |                                  |
    // ---------------------------------------------------------------------------------------------------------------------
    // Pocket Pair   |  13   | 4 Choose 2 = 6                | 13 * 6  = 78           | 78  / 1326 ~  5.88% ~ 1 in 17    | 6 / 1326 ~ 0.452% ~ 1 in 220 |
    // Suited        |  78   | 4 Choose 1 = 4                | 78 * 4  = 312          | 312 / 1326 ~ 25.53% ~ 1 in 4.25  | 4 / 1326 ~ 0.3%   ~ 1 in 331 |
    // Unsuited      |  78   | 4 Choose 1 * 3 Choose 2 = 12  | 78 * 12 = 936          | 936 / 1326 ~ 70.59% ~ 1 in 1.417 | 12/ 1326 ~ 0.9%   ~ 1 in 110 |
    //
    // Note: A hand's chance of appearing is not dependent on the chance that the hand will win
    //    Proof) 22 is harder to get than AKo, but less likely to win, but then AA is harder to get than 72o but is extremely likely to win.
    //    The point of mentioning this is that you shouldn't assume that if a hand is harder to get than another hand, that it is more likely to win.
    //
    // Note: One important stat of poker players is their variance.  See if there is a way to capture this.  Line graps should be used.

    public enum CardRank {
        Ace   =  1,
        Two   =  2,
        Three =  3,
        Four  =  4,
        Five  =  5,
        Size  =  6,
        Seven =  7,
        Eight =  8,
        Nine  =  9,
        Ten   = 10,
        Jack  = 11,
        Queen = 12,
        King  = 13
    }

    public enum Suit {
        Club    = 0,
        Spade   = 1,
        Diamond = 2,
        Heart   = 3
    }

    public static class Constants
    {
        public static readonly int totalNumberOfPreflopHands = 1326; // 52 Combine 2 = 52! / ( 2! * (52-2)! )

    }


    public class Class
    {


        // Card ID
        // =====================================================================================
        // Every Card Rank/Suit Combination has a unique integer called it's "Card ID" from 0 to 51.
        //
        public static byte GetCardID(CardRank rank, Suit suit)
        {
            return (byte)(rank - 1 + 13 * (int)suit);
        }





    }
}
