using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marler.Poker
{
    public class PascalsTriangle
    {
        public readonly int max;

        // n C k = pascalsTriangle[n][k]
        //
        //  n\k  | 1  2  3  4
        //  ---------------------
        //   1   | 1 (1)
        //   2   | 1  2
        //   3   | 1  3  (3)
        //   4   | 1  4   6
        //   5   | 1  5  10  (10)
        //   6   | 1  6  15   20

        private readonly Int64[][] pascalsTriangle;

        public PascalsTriangle(int max)
        {
            this.max = max;
            pascalsTriangle = new Int64[max][];


            pascalsTriangle[0] = new Int64[1];
            pascalsTriangle[0][0] = 1;


            for (Int64 n = 2; n <= max; n++)
            {
                Int64[] row = new Int64[1 + (n >> 1)];
                this.pascalsTriangle[n - 1] = row;
                Int64[] previousRow = this.pascalsTriangle[n - 2];

                row[0    ]   = 1;
                row[1    ]   = n;
                row[row.Length-1] = 2*previousRow[row.Length-2];
                for (int k = 2; k < row.Length - 1; k++)
                {
                    row[k] = previousRow[k - 1] + previousRow[k];
                }

                n++;
                if (n > max) break;

                previousRow = row;
                row = new Int64[previousRow.Length];
                this.pascalsTriangle[n - 1] = row;

                row[0] = 1;
                row[1] = n;
                for (int k = 2; k <= previousRow.Length - 1; k++)
                {
                    row[k] = previousRow[k - 1] + previousRow[k];
                }
            }
        }

        public Int64 NChooseK(int n, int k)
        {
            int nOver2 = (n >> 1);
            if (k > nOver2)
            {
                k = n - k;
            }
            return pascalsTriangle[n-1][k];
        }

        public void Print()
        {
            for (int n = 1; n <= max; n++)
            {
                Int64[] row = pascalsTriangle[n - 1];
                for(int i = 0; i < row.Length; i++) {
                    Console.Write(" {0}", pascalsTriangle[n-1][i]);
                }
                Console.WriteLine();
            }
        }
    }

}
