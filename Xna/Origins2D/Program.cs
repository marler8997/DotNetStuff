using System;

namespace Marler.Xna.Origins2D
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Origins2DGame game = new Origins2DGame())
            {
                game.Run();
            }
        }
    }
#endif
}

