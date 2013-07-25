using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Marler.Poker
{
    class RandomNames
    {
        readonly Random random;
        readonly List<String> randomNames;
        public RandomNames(Random random, String[] randomNames)
        {
            this.random = random;
            this.randomNames = new List<String>(randomNames);
        }
        public String GetRandomName()
        {
            if (randomNames.Count <= 0) throw new InvalidOperationException("Out of names");
            Int32 randomIndex = random.Next(randomNames.Count);
            String name = randomNames[randomIndex];
            randomNames.RemoveAt(randomIndex);
            return name;
        }
    }
    class Program
    {
        static readonly Random StaticRandom = new Random((Int32)Stopwatch.GetTimestamp());
        static readonly String[] RandomNames = new String[] {
            "Justin",
            "Eric",
            "Kevin",
            "Jason",
            "Daniel",
            "Mathew",
            "Sophia",
            "Emma",
            "Olivia",
            "Lily",
            "Madison",
        };
        static void Usage()
        {
            Console.WriteLine("Usage: LibraryTester.exe <seat-count> <player-count>");
        }
        static void Main(string[] args)
        {
            if(args.Length != 2)
            {
                Console.WriteLine("Expected {0} arguments but got {1}", 2, args.Length);
                Usage();
                return;
            }

            Byte seatCount = Byte.Parse(args[0]);
            Byte playerCount = Byte.Parse(args[1]);

            RandomNames randomNames = new RandomNames(StaticRandom, RandomNames);
            
            Table table = new Table(seatCount);
            for (int i = 0; i < playerCount; i++)
            {
                table.AddPlayer(new TablePlayer(randomNames.GetRandomName(), (UInt32)(StaticRandom.Next() % 2000 + 500)));
            }

            HoldemTableStateMachine stateMachine = new HoldemTableStateMachine(table, StaticRandom);

            while (true)
            {
                Console.Write("->");
                String command = Console.ReadLine();
                
                if (command.Equals("exit", StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }
                else if (command.Equals("hand", StringComparison.CurrentCultureIgnoreCase))
                {
                    stateMachine.Hand();
                }
                else if (command.Equals("status", StringComparison.CurrentCultureIgnoreCase))
                {
                    stateMachine.PrintStatus();
                }
                else
                {
                    Console.WriteLine("Unknown Command '{0}'", command);
                }
            }
        }
    }
}
