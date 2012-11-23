using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.NetworkTools
{
    public abstract class ConnectionMessageLogger : MessageLogger
    {
        public ConnectionMessageLogger(String name)
            : base(name)
        {
        }

        public abstract MessageLogger AToBMessageLogger { get; }
        public abstract MessageLogger BToAMessageLogger { get; }

        public abstract void LogAToB(String message);
        public abstract void LogBToA(String message);

        public abstract void LogAToB(String message, params Object[] obj);
        public abstract void LogBToA(String message, params Object[] obj);

        public static readonly ConnectionMessageLogger NullConnectionMessageLogger = new ConnectionMesssageLoggerIgnore();
        private class ConnectionMesssageLoggerIgnore : ConnectionMessageLogger
        {
            public ConnectionMesssageLoggerIgnore() : base(String.Empty) { }
            public override MessageLogger AToBMessageLogger { get { return MessageLogger.NullMessageLogger; } }
            public override MessageLogger BToAMessageLogger { get { return MessageLogger.NullMessageLogger; } }

            public override void LogAToB(string message) {}
            public override void LogBToA(string message) {}
            public override void LogAToB(string message, params object[] obj) {}
            public override void LogBToA(string message, params object[] obj) {}
            public override MessageLogger CreateLoggerSameStreamNewName(string newName)
            {
                return MessageLogger.NullMessageLogger;
            }
            public override void Log(string message) {}
        }
    }

    public class ConnectionMessageLoggerSingleLog : ConnectionMessageLogger
    {
        private readonly MessageLogger messageLogger;
        private readonly String logNameAToB, logNameBToA;
        private readonly String logPrefixFormatString;

        private readonly MessageLogger messageLoggerAToB, messageLoggerBToA;

        public ConnectionMessageLoggerSingleLog(MessageLogger messageLogger, String logNameAToB, String logNameBToA)
            : this(messageLogger, logNameAToB, logNameBToA, "[{0} to {1} Time: {2}]")
        {
        }

        public ConnectionMessageLoggerSingleLog(MessageLogger messageLogger, String logNameAToB, String logNameBToA, String logPrefixFormatString)
            : base(messageLogger.name)
        {
            if (messageLogger == null) throw new ArgumentNullException("messageLogger");
            if (logPrefixFormatString == null) throw new ArgumentNullException("logPrefixFormatString");

            this.messageLogger = messageLogger;
            this.logNameAToB = logNameAToB;
            this.logNameBToA = logNameBToA;
            this.logPrefixFormatString = logPrefixFormatString;

            this.messageLoggerAToB = new CallbackMessageLogger(logNameAToB, Log);
            this.messageLoggerBToA = new CallbackMessageLogger(logNameBToA, Log);
        }

        public override MessageLogger CreateLoggerSameStreamNewName(string newName)
        {
            throw new InvalidOperationException();
        }

        public override MessageLogger AToBMessageLogger { get { return messageLoggerAToB;  } }
        public override MessageLogger BToAMessageLogger { get { return messageLoggerBToA; } }

        public override void Log(String message)
        {
            messageLogger.Log(message);
        }

        private void Log(String name, String message)
        {
            messageLogger.Log(String.Format("{0}: {1}", name, message));
        }

        public override void LogAToB(String message)
        {
            Log(logNameAToB, message);
        }

        public override void LogBToA(String message)
        {
            Log(logNameBToA, message);
        }

        public override void LogAToB(String message, params Object[] obj)
        {
            Log(logNameAToB, String.Format(message, obj));
        }

        public override void LogBToA(String message, params Object[] obj)
        {
            Log(logNameBToA, String.Format(message, obj));
        }
    }
}
