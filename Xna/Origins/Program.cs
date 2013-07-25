using System;

namespace Marler.Xna.Origins
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Origins3DGame game = new Origins3DGame())
            {
                game.Run();
            }
        }
    }
#endif
}

