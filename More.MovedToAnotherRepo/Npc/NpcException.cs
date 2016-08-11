using System;

namespace More
{
    public class NpcException : Exception
    {
        public NpcException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Thrown when the Npc method throws an exception
    /// </summary>
    public class NpcCallException : NpcException
    {
        public readonly Type exceptionType;
        public readonly String stackTrace;

        public NpcCallException(Type exceptionType, String exceptionMessage, String stackTrace)
            : base(exceptionMessage)
        {
            this.exceptionType = exceptionType;
            this.stackTrace = stackTrace;
        }
        public override string StackTrace
        {
            get
            {
                return stackTrace;
            }
        }
    }

    /// <summary>
    /// Thrown when the server returns an NpcError
    /// </summary>
    public class NpcErrorException : NpcException
    {
        public readonly NpcErrorCode errorCode;
        public NpcErrorException(NpcErrorCode errorCode, String npcError)
            : base(npcError)
        {
            this.errorCode = errorCode;
        }
    }

    /// <summary>
    /// Thrown when the server interface is not the expected interface
    /// </summary>
    public class NpcInterfaceMismatch : NpcException
    {
        public NpcInterfaceMismatch(String message)
            : base(message)
        {
        }
    }
}
