using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Marler.Poker
{
    public class TablePlayer
    {
        //
        // Public Information
        //
        public String userName;
        public UInt32 chipCount;
        public UInt32 potAmountPlayerCanWin;

        //
        // Private Information
        //
        public Byte highRankCard, lowRankCard;

        public TablePlayer(String userName, UInt32 chipCount)
        {
            this.userName = userName;
            this.chipCount = chipCount;
        }

        public void SetHoleCards(Byte card1, Byte card2)
        {
            Byte card1Rank = card1.CardRank();
            Byte card2Rank = card2.CardRank();

            if (card1Rank >= card2Rank)
            {
                highRankCard = card1;
                lowRankCard = card2;
            }
            else
            {
                highRankCard = card2;
                lowRankCard = card1;
            }
        }
    }
    public class Seat
    {
        public readonly TablePlayer player;
        
        Boolean steppedAway;   // The player has requested to step away from the table
        Boolean unresponsive;  // The player did not respond after some server request and has not responded since
        Boolean inCurrentHand; // 

        public Seat(TablePlayer player)
        {
            this.player = player;
            this.steppedAway = false;
            this.unresponsive = false;
            this.inCurrentHand = false;
        }
        public void NewHand()
        {
            inCurrentHand = steppedAway ? false : (player != null);
            player.potAmountPlayerCanWin = 0;
        }
        public Boolean InCurrentHand()
        {
            return inCurrentHand;
        }
        public void SetIsSittingOut()
        {
            this.steppedAway = true;
        }
        public void SetIsNotSittingOut()
        {
            this.steppedAway = false;
        }
    }

    //
    // Table {
    //    Seat[] // array of seats available for players
    //    Player[] // the players in the current hand
    // }
    //
    public class Table
    {
        readonly Seat[] seats;
        public Int32 playerCountAtTheTable;

        Byte dealerSeatIndex;

        //
        // Current Hand
        //
        public readonly TablePlayer[] playersInCurrentHand; // Ordered left of dealer, to dealer
        public Int32 playerCountInCurrentHand;        

        UInt32 smallBlind;
        UInt32 ante;

        public Table(Byte seatCount)
        {
            this.seats = new Seat[seatCount];
            this.playerCountAtTheTable = 0;

            this.playersInCurrentHand = new TablePlayer[seatCount];
            this.playerCountInCurrentHand = 0;
        }

        public void AddPlayer(TablePlayer player)
        {
            for (int i = 0; i < seats.Length; i++)
            {
                Seat seat = seats[i];
                if (seat == null)
                {
                    seats[i] = new Seat(player);
                    playerCountAtTheTable++;
                    return;
                }
            }
            throw new InvalidOperationException("No seats left");
        }

        //
        // 1. Adds players to the current hand
        // 2. Move the dealer button
        // 3. Set the order of play for the current hand
        //
        public void NewHand()
        {
            Int32 previousPlayerCountInCurrentHand = this.playerCountInCurrentHand;
            //
            // Call NewHand on each seat and count players in this hand
            //
            this.playerCountInCurrentHand = 0;

            for (int i = 0; i < seats.Length; i++)
            {
                Seat seat = seats[i];
                if (seat != null)
                {
                    seat.NewHand();
                    if (seat.InCurrentHand()) playerCountInCurrentHand++;
                }
            }

            if (playerCountAtTheTable < 1) return;

            //
            // Update the dealer button
            //
            Int32 lastDealerSeatIndex = dealerSeatIndex;
            while (true)
            {
                dealerSeatIndex++;
                if (dealerSeatIndex >= seats.Length)
                {
                    dealerSeatIndex = 0;
                }
                if (dealerSeatIndex == lastDealerSeatIndex)
                {
                    break;
                }
                Seat potentialNewDealer = seats[dealerSeatIndex];
                if (potentialNewDealer != null && potentialNewDealer.InCurrentHand()) break;
            }

            //
            // Setup the current hand player array
            //
            Int32 currentPlayerArrayIndex = 0;
            Int32 seatIncrementer = dealerSeatIndex;
            while (true)
            {
                seatIncrementer++;
                if (seatIncrementer >= seats.Length)
                {
                    seatIncrementer = 0;
                }
                if (seatIncrementer == dealerSeatIndex)
                {
                    playersInCurrentHand[currentPlayerArrayIndex++] = seats[dealerSeatIndex].player;
                    break;
                }
                Seat nextPlayer = seats[seatIncrementer];
                if (nextPlayer != null && nextPlayer.InCurrentHand())
                {
                    playersInCurrentHand[currentPlayerArrayIndex++] = nextPlayer.player;
                }
            }

            if (currentPlayerArrayIndex != this.playerCountInCurrentHand)
                throw new InvalidOperationException(String.Format(
                    "InternalBug: Counted {0} players in the hand but then only added {1} players to the hand?",
                    this.playerCountInCurrentHand, currentPlayerArrayIndex));
            
            // Remove references to extra players
            while(currentPlayerArrayIndex < previousPlayerCountInCurrentHand)
            {
                playersInCurrentHand[currentPlayerArrayIndex++] = null;
            }
        }
    }
}
