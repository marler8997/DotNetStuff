using System;
using System.IO;

namespace Marler.Hmd
{
    public static class WriterExtensions
    {
        public static void WriteLine(this TextWriter writer, Int32 tabs, String fmt, params Object[] obj)
        {
            writer.Write(String.Format("{{0,{0}}}", 3 * tabs), String.Empty);
            writer.WriteLine(fmt, obj);
        }
        public static void WriteLine(this TextWriter writer, Int32 tabs, String str)
        {
            writer.Write(String.Format("{{0,{0}}}", 3 * tabs), String.Empty);
            writer.WriteLine(str);
        }

        public static void Write(this TextWriter writer, Int32 tabs, String fmt, params Object[] obj)
        {
            writer.Write(String.Format("{{0,{0}}}", 3 * tabs), String.Empty);
            writer.Write(fmt, obj);
        }
        public static void Write(this TextWriter writer, Int32 tabs, String str)
        {
            writer.Write(String.Format("{{0,{0}}}", 3 * tabs), String.Empty);
            writer.Write(str);
        }
    }
}
