using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace More.Net
{
    public interface IConnectionDataLogger
    {
        IDataLogger AToBDataLogger { get; }
        IDataLogger BToADataLogger { get; }

        void LogDataAToB(Byte[] buffer, Int32 offset, Int32 length);
        void LogDataBToA(Byte[] buffer, Int32 offset, Int32 length);
    }

    public static class ConnectionDataLogger
    {
        public static readonly IConnectionDataLogger Null = new ConnectionDataLoggerIgnore();
        private class ConnectionDataLoggerIgnore : IConnectionDataLogger
        {
            public IDataLogger AToBDataLogger
            {
                get { return DataLogger.Null; }
            }
            public IDataLogger BToADataLogger
            {
                get { return DataLogger.Null; }
            }
            public void LogDataAToB(byte[] buffer, int offset, int length)
            {
            }
            public void LogDataBToA(byte[] buffer, int offset, int length)
            {
            }
        }
    }

    public class ConnectionDataLoggerDoubleLog : IConnectionDataLogger
    {
        private readonly IDataLogger loggerAToB, loggerBToA;

        public ConnectionDataLoggerDoubleLog(IDataLogger loggerAToB, IDataLogger loggerBToA)
        {
            if (loggerAToB == null) throw new ArgumentNullException("loggerAToB");
            if (loggerBToA == null) throw new ArgumentNullException("loggerBToA");

            this.loggerAToB = loggerAToB;
            this.loggerBToA = loggerBToA;
        }

        public IDataLogger AToBDataLogger { get { return loggerAToB; } }
        public IDataLogger BToADataLogger { get { return loggerBToA; } }

        public void LogDataAToB(Byte[] buffer, Int32 offset, Int32 length)
        {
            loggerAToB.LogData(buffer, offset, length);
        }

        public void LogDataBToA(Byte[] buffer, Int32 offset, Int32 length)
        {
            loggerBToA.LogData(buffer, offset, length);
        }
    }

    public class ConnectionDataLoggerPrettyLog : IConnectionDataLogger
    {
        private readonly UInt32 socketID;
        private readonly IDataLogger dataLogger;
        private readonly String logNameAToB, logNameBToA;

        private readonly IDataLogger dataLoggerAToB, dataLoggerBToA;

        public ConnectionDataLoggerPrettyLog(UInt32 socketID, IDataLogger dataLogger, String logNameAToB, String logNameBToA)
        {
            if (dataLogger == null) throw new ArgumentNullException("dataLogger");

            this.socketID = socketID;
            this.dataLogger = dataLogger;
            this.logNameAToB = logNameAToB;
            this.logNameBToA = logNameBToA;

            this.dataLoggerAToB = new CallbackDataLogger(LogDataAToB);
            this.dataLoggerBToA = new CallbackDataLogger(LogDataBToA);
        }

        public IDataLogger AToBDataLogger { get { return dataLoggerAToB; } }
        public IDataLogger BToADataLogger { get { return dataLoggerBToA; } }

        public void LogDataAToB(Byte[] buffer, Int32 offset, Int32 length)
        {
            Byte[] header = Encoding.UTF8.GetBytes(
                String.Format("\n[  <<======  Socket {0}: {1} to {2} at {3}]\n", socketID, logNameAToB, logNameBToA, DateTime.Now.ToString("HH:mm:ss tt")));
            lock (dataLogger)
            {
                dataLogger.LogData(header, 0, header.Length);
                dataLogger.LogData(buffer, 0, length);
            }
        }

        public void LogDataBToA(Byte[] buffer, Int32 offset, Int32 length)
        {
            Byte[] header = Encoding.UTF8.GetBytes(
                String.Format("\n[  ======>>  Socket {0}: {1} to {2} at {3}]\n", socketID, logNameBToA, logNameAToB, DateTime.Now.ToString("HH:mm:ss tt")));
            lock (dataLogger)
            {
                dataLogger.LogData(header, 0, header.Length);
                dataLogger.LogData(buffer, 0, length);
            }
        }
    }

    public class ConnectionDataLoggerSingleLog : IConnectionDataLogger
    {
        private readonly IDataLogger dataLogger;
        private readonly String logNameAToB, logNameBToA;
        private readonly String logPrefixFormatString;

        private readonly IDataLogger dataLoggerAToB, dataLoggerBToA;

        public ConnectionDataLoggerSingleLog(IDataLogger dataLogger, String logNameAToB, String logNameBToA)
            : this(dataLogger, logNameAToB, logNameBToA, "[{0} to {1} Time: {2}]")
        {
        }

        public ConnectionDataLoggerSingleLog(IDataLogger dataLogger, String logNameAToB, String logNameBToA,
            String logPrefixFormatString)
        {
            if (dataLogger == null) throw new ArgumentNullException("dataLogger");
            if (logPrefixFormatString == null) throw new ArgumentNullException("logPrefixFormatString");

            this.dataLogger = dataLogger;
            this.logNameAToB = logNameAToB;
            this.logNameBToA = logNameBToA;
            this.logPrefixFormatString = logPrefixFormatString;

            this.dataLoggerAToB = new CallbackDataLogger(LogDataAToB);
            this.dataLoggerBToA = new CallbackDataLogger(LogDataBToA);
        }

        public IDataLogger AToBDataLogger { get { return dataLoggerAToB; } }
        public IDataLogger BToADataLogger { get { return dataLoggerBToA; } }

        public void LogDataAToB(Byte[] buffer, Int32 offset, Int32 length)
        {
            Byte[] header = Encoding.UTF8.GetBytes(
                String.Format(logPrefixFormatString, logNameAToB, logNameBToA, DateTime.Now.ToString("HH:mm:ss tt")));
            lock (dataLogger)
            {
                dataLogger.LogData(header, 0, header.Length);
                dataLogger.LogData(buffer, 0, length);
            }
        }

        public void LogDataBToA(Byte[] buffer, Int32 offset, Int32 length)
        {
            Byte[] header = Encoding.UTF8.GetBytes(
                String.Format(logPrefixFormatString, logNameBToA, logNameAToB, DateTime.Now.ToString("HH:mm:ss tt")));
            lock (dataLogger)
            {
                dataLogger.LogData(header, 0, header.Length);
                dataLogger.LogData(buffer, 0, length);
            }
        }
    }
}
