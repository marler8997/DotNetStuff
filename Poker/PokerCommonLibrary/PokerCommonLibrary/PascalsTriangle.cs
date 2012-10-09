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

        /*
 1
 1 2
 1 3
 1 4 4
 1 5 0
 1 6 5 1
 1 7 11 0
 1 8 18 11 1
 1 9 26 29 0
 1 10 35 55 29 1
 1 11 45 90 84 0
 1 12 56 135 174 84 1
 1 13 68 191 309 258 0
 1 14 81 259 500 567 258 1
 1 15 95 340 759 1067 825 0
 1 16 110 435 1099 1826 1892 825 1
 1 17 126 545 1534 2925 3718 2717 0
 1 18 143 671 2079 4459 6643 6435 2717 1
 1 19 161 814 2750 6538 11102 13078 9152 0
 1 20 180 975 3564 9288 17640 24180 22230 9152 1
 1 21 200 1155 4539 12852 26928 41820 46410 31382 0
 1 22 221 1355 5694 17391 39780 68748 88230 77792 31382 1
 1 23 243 1576 7049 23085 57171 108528 156978 166022 109174 0
 1 24 266 1819 8625 30134 80256 165699 265506 323000 275196 109174 1
 1 25 290 2085 10444 38759 110390 245955 431205 588506 598196 384370 0
 1 26 315 2375 12529 49203 149149 356345 677160 1019711 1186702 982566 384370 1
         */

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
