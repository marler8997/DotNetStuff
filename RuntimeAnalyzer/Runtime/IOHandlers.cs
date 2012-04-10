using System;

namespace Marler.RuntimeAnalyzer
{
    public static class IOHandlers
    {
        public static UInt64 ConsoleRead()
        {
            return (UInt64)Console.Read();
        }

        public static void ConsoleWrite(UInt64 value)
        {
            Console.Write((Char)value);
        }
    }
}
