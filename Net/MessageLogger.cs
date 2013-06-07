using System;
using System.IO;
using System.Text;

namespace More.Net
{
    public abstract class MessageLogger
    {
        public readonly String name;

        public MessageLogger(String name)
        {
            this.name = name;
        }

        public abstract MessageLogger CreateLoggerSameStreamNewName(String newName);

        public abstract void Log(String message);
        public void Log(String message, params Object[] obj)
        {
            Log(String.Format(message, obj));
        }

        public static readonly MessageLogger NullMessageLogger = new MessageLoggerIgnore();
        private class MessageLoggerIgnore : MessageLogger
        {
            public MessageLoggerIgnore() : base(String.Empty) {}
            public override MessageLogger CreateLoggerSameStreamNewName(string newName) { return this; }
            public override void Log(string message) {}
        }
    }

    public class ConsoleMessageLogger : MessageLogger
    {
        public ConsoleMessageLogger(String name) : base(name)
        {

        }
        public override MessageLogger CreateLoggerSameStreamNewName(String newName)
        {
            return new ConsoleMessageLogger(newName);
        }

        public override void Log(String message)
        {
            Console.WriteLine("[{0}: {1}]", name, message);
        }
    }

    public delegate void LogMessageDelegate(String name, String message);

    public class CallbackMessageLogger : MessageLogger
    {
        private readonly LogMessageDelegate logMessageDelegate;

        public CallbackMessageLogger(String name, LogMessageDelegate logMessageDelegate)
            : base(name)
        {
            if (logMessageDelegate == null) throw new ArgumentNullException("logMessageDelegate");
            this.logMessageDelegate = logMessageDelegate;
        }

        public override MessageLogger CreateLoggerSameStreamNewName(string newName)
        {
            return new CallbackMessageLogger(newName, logMessageDelegate);
        }

        public override void Log(string message)
        {
            logMessageDelegate(name, message);
        }
    }
}
