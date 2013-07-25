using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Marler.Poker
{
    //
    // Notes about communication
    // 1. Clients will want to verify that packets come from the actual server (no one is spoofing packets from the server),
    //    otherwise, malicious hosts could send fake packets to confuse a client and make them have bad information
    // 2. The server should be able to verify a packet came from a client, otherwise, malicious attackers could spoof decisions from
    //    another client.
    // 3. Much of the information in the packet can be public (except for things like hole cards), but the packets must either be signed
    //    or encrypted so the server/client can verify the sender.
    //

    //
    // Holdem Sequence
    //
    // 1. Server sends Deal info to every client
    //      Hole Cards (encrypted)
    //      Players/Positions
    //      Blinds/Antes
    //
    // While(true) {
    //    if(betting finished) {
    //        ...
    //    }
    //    Wait for next player to act
    // }
    //
    //
    // Information Transfer Sequence
    //
    // State: Dealing
    //
    // [0] Server > AllClients
    //      1. hole cards (encrypted)
    //      2. dealer button position
    //      3. changes to seats (new players, players leaving)
    //      4. Changes to player stack sizes
    //      5. Changes to antes or blinds
    //
    //
    // State: Betting (SubState: Preflop)
    //
    // [*] Client > Server: Check, Check/Fold or Call X or Cancel
    //
    //
    // Variables: Player actionPlayer; UInt32 amountToPlay;
    //
    // Action action;
    // if(actionPlayer.preAction != null) {
    //   action = actionPlayer.preAction;
    // } else {
    //   wait for action...
    //   if(timeout) {
    //     handle timeout;
    //     Server > AllClients: player check folds
    //   }
    // }
    // 
    // 
    // 
    //
    //
    //
    //   [0] Server > AllClients: Time, next player to act, last players action
    class HoldemWaitEvent
    {
        Int32 timeout;
    }

    public class HoldemTableStateMachine
    {
        Table table;
        Deck deck;
        Byte board1, board2, board3, board4, board5;

        UInt32 pot;

        public HoldemTableStateMachine(Table table, Random random)
        {
            this.table = table;
            this.deck = new Deck(random, 52);
        }
        public void PrintStatus()
        {
            for (int i = 0; i < table.playerCountInCurrentHand; i++)
            {
                TablePlayer player = table.playersInCurrentHand[i];
                Console.WriteLine("Player {0,2} '{1,20}' {2,13} {3,13}", i, player.userName,
                    player.highRankCard.CardName(), player.lowRankCard.CardName());
            }
            Console.WriteLine("Board: {0} {1} {2} {3} {4}",
                board1.CardName(),
                board2.CardName(),
                board3.CardName(),
                board4.CardName(),
                board5.CardName());
        }
        public void Hand()
        {
            table.NewHand();

            if(table.playerCountAtTheTable < 1)
            {
                // Can't play with less than 2 players at the table
                return;
            }

            if(table.playerCountInCurrentHand < 1)
            {
                // can't play with no players in the hand
                return;
            }

            //
            // Get the blinds
            //






            deck.Shuffle();

            //
            // Deal
            //
            Int32 deckIndex = 0;
            for (int i = 0; i < table.playerCountInCurrentHand; i++)
            {
                TablePlayer player = table.playersInCurrentHand[i];
                player.SetHoleCards(deck.cards[deckIndex], deck.cards[deckIndex + 1]);
                deckIndex += 2;
            }
            this.board1 = deck.cards[deckIndex++];
            this.board2 = deck.cards[deckIndex++];
            this.board3 = deck.cards[deckIndex++];
            this.board4 = deck.cards[deckIndex++];
            this.board5 = deck.cards[deckIndex++];

            //
            // Get Blinds
            //
            if(table.playerCountInCurrentHand < 2)
        }
    }
}
