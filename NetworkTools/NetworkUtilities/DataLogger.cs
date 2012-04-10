using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marler.NetworkTools
{
    public interface IDataLogger
    {
        void LogData(Byte[] buffer, Int32 offset, Int32 length);

    }

    public static class DataLogger
    {
        public static readonly IDataLogger Null = new DataLoggerIgnore();
        private class DataLoggerIgnore : IDataLogger
        {
            void IDataLogger.LogData(byte[] buffer, int offset, int length)
            {
            }
        }
    }


    public class ConsoleDataLoggerWithLabels : IDataLogger
    {
        public readonly String prefix, postfix;

        public ConsoleDataLoggerWithLabels(String prefix, String postfix)
        {
            this.prefix = prefix;
            this.postfix = postfix;
        }

        public void LogData(Byte[] buffer, Int32 offset, Int32 length)
        {
            Console.WriteLine(prefix);
            Console.Write(Encoding.UTF8.GetString(buffer, 0, length));
            Console.WriteLine(postfix);
        }
    }

    public class ConsoleDataLogger : IDataLogger
    {
        private static ConsoleDataLogger instance;
        public static ConsoleDataLogger Instance { get { if (instance == null) instance = new ConsoleDataLogger(); return instance; } }

        private ConsoleDataLogger() { }

        public void LogData(Byte[] buffer, Int32 offset, Int32 length)
        {
            Console.Write(Encoding.UTF8.GetString(buffer, 0, length));
        }
    }

    public delegate void LogDataDelegate(Byte[] buffer, Int32 offset, Int32 length);

    public class CallbackDataLogger : IDataLogger
    {
        private readonly LogDataDelegate logDataDelegate;

        public CallbackDataLogger(LogDataDelegate logDataDelegate)
        {
            if (logDataDelegate == null) throw new ArgumentNullException("logDataDelegate");

            this.logDataDelegate = logDataDelegate;
        }

        public void LogData(Byte[] buffer, Int32 offset, Int32 length)
        {
            logDataDelegate(buffer, offset, length);
        }
    }
}
