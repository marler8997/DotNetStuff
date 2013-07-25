using System;

namespace Marler.Xna
{
    static class Program
    {
        static void Main(string[] args)
        {
            using (HexagonGame game = new HexagonGame())
            {
                game.Run();
            }
        }
    }
}

