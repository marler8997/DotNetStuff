using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marler.Poker
{
    public class PlayerAtTable
    {
        int moneyInCents;
        int handsPlayed;

        // Table Statistics
        // ...


        // Hand Data
        int cardID1, cardID2;
        int totalMoneyInCurrentPot;  // Note: you should almost never need to use this when caculating decisions to call/fold or raise. Instead, you will use (pot / call amount)
        int currentBet;
    }
}
