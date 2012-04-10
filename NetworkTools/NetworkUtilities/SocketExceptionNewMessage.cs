using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Marler.NetworkTools
{
    public class SocketExceptionWithExtraMessage : SocketException
    {
        public readonly String extraMessage;
        private readonly SocketException socketException;

        public SocketExceptionWithExtraMessage(SocketException socketException, String extraMessage)
            : base(socketException.ErrorCode)
        {
            this.extraMessage = extraMessage;
            this.socketException = socketException;
        }
        public override string Message
        {
            get
            {
                return String.Format("{0}: {1}", socketException.Message, extraMessage);
            }
        }
        public override IDictionary Data
        {
            get
            {
                return socketException.Data;
            }
        }
        public override bool Equals(object obj)
        {
            return socketException.Equals(obj);
        }
        public override Exception GetBaseException()
        {
            return socketException.GetBaseException();
        }
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            socketException.GetObjectData(info, context);
        }
        public override int ErrorCode
        {
            get
            {
                return socketException.ErrorCode;
            }
        }
        public override int GetHashCode()
        {
            return socketException.GetHashCode();
        }
        public override string HelpLink
        {
            get
            {
                return socketException.HelpLink;
            }
            set
            {
                socketException.HelpLink = value;
            }
        }
        public override string Source
        {
            get
            {
                return socketException.Source;
            }
            set
            {
                socketException.Source = value;
            }
        }
        public override string StackTrace
        {
            get
            {
                return socketException.StackTrace;
            }
        }
    }
}
