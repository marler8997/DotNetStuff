using System;
using System.Reflection;

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


    public static class Card
    {
        public const Byte AceClubs = 0;
        public const Byte TwoClubs = 1;
        public const Byte ThreeClubs = 2;
        public const Byte FourClubs = 3;
        public const Byte FiveClubs = 4;
        public const Byte SixClubs = 5;
        public const Byte SevenClubs = 6;
        public const Byte EightClubs = 7;
        public const Byte NineClubs = 8;
        public const Byte TenClubs = 9;
        public const Byte JackClubs = 10;
        public const Byte QueenClubs = 11;
        public const Byte KingClubs = 12;

        public const Byte AceDiamonds   = 13;
        public const Byte TwoDiamonds   = 14;
        public const Byte ThreeDiamonds = 15;
        public const Byte FourDiamonds = 16;
        public const Byte FiveDiamonds = 17;
        public const Byte SixDiamonds = 18;
        public const Byte SevenDiamonds = 19;
        public const Byte EightDiamonds = 20;
        public const Byte NineDiamonds = 21;
        public const Byte TenDiamonds = 22;
        public const Byte JackDiamonds = 23;
        public const Byte QueenDiamonds = 24;
        public const Byte KingDiamonds = 25;

        public const Byte AceHearts = 26;
        public const Byte TwoHearts = 27;
        public const Byte ThreeHearts = 28;
        public const Byte FourHearts = 29;
        public const Byte FiveHearts = 30;
        public const Byte SixHearts = 31;
        public const Byte SevenHearts = 32;
        public const Byte EightHearts = 33;
        public const Byte NineHearts = 34;
        public const Byte TenHearts = 35;
        public const Byte JackHearts = 36;
        public const Byte QueenHearts = 37;
        public const Byte KingHearts = 38;

        public const Byte AceSpades = 39;
        public const Byte TwoSpades = 40;
        public const Byte ThreeSpades = 41;
        public const Byte FourSpades = 42;
        public const Byte FiveSpades = 43;
        public const Byte SixSpades = 44;
        public const Byte SevenSpades = 45;
        public const Byte EightSpades = 46;
        public const Byte NineSpades = 47;
        public const Byte TenSpades = 48;
        public const Byte JackSpades = 49;
        public const Byte QueenSpades = 50;
        public const Byte KingSpades = 51;

        public static String[] CardNames;

        static Card()
        {
            CardNames = new String[52];

            FieldInfo[] fieldInfos = typeof(Card).GetFields();
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                FieldInfo fieldInfo = fieldInfos[i];
                if (fieldInfo.FieldType == typeof(Byte))
                {
                    Byte value = (Byte)fieldInfo.GetValue(null);
                    CardNames[value] = fieldInfo.Name;
                }
            }
        }
        public static String CardName(this Byte card)
        {
            return CardNames[card];
        }
        public static Byte CardRank(this Byte card)
        {
            return (Byte)(card % 13);
        }
    }
}