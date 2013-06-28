using System;
using System.IO;

namespace More.Lfd
{
    public static class WriterExtensions
    {
        public static void WriteLine(this TextWriter writer, UInt32 threeSpaceTabs, String fmt, params Object[] obj)
        {
            writer.Write(String.Format("{{0,{0}}}", 3 * threeSpaceTabs), String.Empty);
            writer.WriteLine(fmt, obj);
        }
        public static void WriteLine(this TextWriter writer, UInt32 threeSpaceTabs, String str)
        {
            writer.Write(String.Format("{{0,{0}}}", 3 * threeSpaceTabs), String.Empty);
            writer.WriteLine(str);
        }

        public static void Write(this TextWriter writer, UInt32 threeSpaceTabs, String fmt, params Object[] obj)
        {
            writer.Write(String.Format("{{0,{0}}}", 3 * threeSpaceTabs), String.Empty);
            writer.Write(fmt, obj);
        }
        public static void Write(this TextWriter writer, UInt32 threeSpaceTabs, String str)
        {
            writer.Write(String.Format("{{0,{0}}}", 3 * threeSpaceTabs), String.Empty);
            writer.Write(str);
        }
    }
}
