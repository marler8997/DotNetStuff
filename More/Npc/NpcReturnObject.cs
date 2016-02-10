using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace More
{
    public class NpcReturnLine
    {
        //
        // An NPC return line is intended to be human readable but has a very strict format.
        //
        // There are 3 types of return lines, and each starts with it's own prefix:
        //
        // Success Return Line: "Success <ReturnType> [<ReturnValue>]"
        //
        //    The <ReturnType> is always the Sos type name with the namespace included (Case is sensitive).
        //    So an int type would be "Int32".
        //    If the Return type is "Void" then no return value is included, otherwise,
        //    the <ReturnValue> is the Sos serialized return value.
        //
        // Exception Return Line: "Exception <ExceptionMessage> <ExceptionType> <ExceptionValue>"
        //    The <ExceptionMessage> is the exception message. It is delimited by quotes, and escapes '\n', '"', and '\'.
        //    The <ExceptionType> is the Sos type name of the exception.
        //    The <ExceptionValue> is the sos serialized exception.
        //
        //    Note: The reason the ExceptionMessage is also included is just in case the client receiving the return line
        //          does not have the actual exception type in it's libraries, or if it's version of the exception is different
        //          from the server's version. This way, the client can still have a valid exception message even if it doesn't
        //          have the exception type code.
        //
        // An NpcError return line: "NpcError <ErrorCode> <ErrorMessage>"
        //
        public const String NpcReturnLineSuccessPrefix   = "Success ";
        public const String NpcReturnLineExceptionPrefix = "Exception ";
        public const String NpcReturnLineNpcErrorPrefix = "NpcError ";

        public readonly String exceptionMessage;

        public readonly String sosTypeName;
        public readonly String sosSerializationString;

        public NpcReturnLine(String sosTypeName, String sosSerializationString)
        {
            this.exceptionMessage = null;
            this.sosTypeName = sosTypeName;
            this.sosSerializationString = sosSerializationString;
        }

        //
        // Throws NpcErrorException for an npc error
        //
        public NpcReturnLine(String returnLine)
        {
            Boolean success = returnLine.StartsWith(NpcReturnObject.NpcReturnLineSuccessPrefix);

            if (success || returnLine.StartsWith(NpcReturnObject.NpcReturnLineExceptionPrefix))
            {
                String noPrefixReturnLine;
                if (success)
                {
                    noPrefixReturnLine = returnLine.Substring(NpcReturnLineSuccessPrefix.Length);

                    if (noPrefixReturnLine.Equals("Void"))
                    {
                        this.exceptionMessage = null;
                        this.sosTypeName = "Void";
                        this.sosSerializationString = null;
                        return;
                    }
                }
                else
                {
                    noPrefixReturnLine = returnLine.Substring(NpcReturnLineExceptionPrefix.Length);

                    //
                    // Exception Message
                    //
                    Object exceptionMessageObject;
                    Int32 offset = Sos.Deserialize(out exceptionMessageObject, typeof(String),
                        noPrefixReturnLine, 0, noPrefixReturnLine.Length);
                    this.exceptionMessage = (String)exceptionMessageObject;

                    if(offset >= noPrefixReturnLine.Length - 1) InvalidReturnLine(returnLine, "Missing exception type and serialized exception");
                    noPrefixReturnLine = noPrefixReturnLine.Substring(offset + 1);
                }

                //
                // Get the return type
                //
                Int32 spaceIndex = noPrefixReturnLine.IndexOf(' ');
                if (spaceIndex < 0 || spaceIndex >= noPrefixReturnLine.Length - 1) InvalidReturnLine(returnLine, "missing the return value");
                if (spaceIndex == 0) InvalidReturnLine(returnLine, "After 'Success' prefix there were 2 spaces in a row");

                this.sosTypeName = noPrefixReturnLine.Remove(spaceIndex);
                this.sosSerializationString = noPrefixReturnLine.Substring(spaceIndex + 1);

                return;
            }

            if (returnLine.StartsWith(NpcReturnObject.NpcReturnLineNpcErrorPrefix))
            {
                String errorCode;
                returnLine.Peel(out errorCode);
                errorCode = errorCode.Peel(out returnLine);
                throw new NpcErrorException((NpcErrorCode)Enum.Parse(typeof(NpcErrorCode), errorCode), returnLine);
            }

            InvalidReturnLine(returnLine, String.Format("does not start with '{0}','{1}' or '{2}'",
                NpcReturnObject.NpcReturnLineSuccessPrefix, NpcReturnObject.NpcReturnLineNpcErrorPrefix,
                NpcReturnObject.NpcReturnLineExceptionPrefix));

        }
        void InvalidReturnLine(String returnLine, String message)
        {
            throw new FormatException(String.Format("Invalid NPC return line '{0}': {1}", returnLine, message));
        }
    }
    public class NpcReturnObject
    {
        public const String NpcReturnLineSuccessPrefix = "Success ";
        public const String NpcReturnLineExceptionPrefix = "Exception ";
        public const String NpcReturnLineNpcErrorPrefix = "NpcError ";

        /// <summary>The return type of the method that was called.</summary>
        public readonly Type type;

        /// <summary>A reference to the object that was returned (could be null).</summary>
        public Object value;

        /// <summary>The npc string representation of the object that was returned.</summary>
        public String valueSosSerializationString;

        public NpcReturnObject(Type returnType, Object returnValue, String returnValueSosSerializationString)
        {
            if (returnType == null) throw new ArgumentNullException("returnType");

            this.type = returnType;
            this.value = returnValue;
            this.valueSosSerializationString = returnValueSosSerializationString.Replace("\n","\\n");
        }

        public virtual void AppendNpcReturnLine(ITextBuilder responseBuilder)
        {
            //
            // This method call will always return a specifically formatted string.
            //    1. On Success "Success <ReturnType> <ReturnValue>"
            //    2. On Exception "Exception <ExceptionTypeName> <ExceptionMessage> <StackTrace>
            //
            responseBuilder.AppendAscii(NpcReturnLineSuccessPrefix);
            if (type == typeof(void))
            {
                responseBuilder.AppendAscii("Void\n");
            }
            else
            {
                responseBuilder.AppendAscii(type.SosTypeName());
                responseBuilder.AppendAscii(' ');
                responseBuilder.AppendAscii(valueSosSerializationString);
                responseBuilder.AppendAscii('\n');
            }
        }
    }
    public class NpcReturnObjectOrException : NpcReturnObject
    {
        /// <summary>
        /// The message of the exception that was thrown during the method call
        /// </summary>
        public readonly Exception exception;

        public NpcReturnObjectOrException(Exception thrownException, String exceptionNpcString)
            : base(thrownException.GetType(), thrownException, exceptionNpcString)
        {
            this.exception = thrownException;
        }
        public NpcReturnObjectOrException(Type returnType, Object returnValue, String returnValueNpcString)
            : base(returnType, returnValue, returnValueNpcString)
        {
            if (returnType == null) throw new ArgumentNullException("returnType");
            this.exception = null;
        }
        public override void AppendNpcReturnLine(ITextBuilder responseBuilder)
        {
            if (exception == null)
            {
                base.AppendNpcReturnLine(responseBuilder);
            }
            else
            {
                String exceptionMessage = exception.Message.SerializeString().Replace("\n", "\\n");
                String exceptionAsNpcString = exception.SerializeObject().Replace("\n", "\\n");

                responseBuilder.AppendAscii(NpcReturnObject.NpcReturnLineExceptionPrefix);
                responseBuilder.AppendAscii(exceptionMessage);
                responseBuilder.AppendAscii(' ');
                responseBuilder.AppendAscii(exception.GetType().SosTypeName());
                responseBuilder.AppendAscii(' ');
                responseBuilder.AppendAscii(exceptionAsNpcString);
                responseBuilder.AppendAscii('\n');
            }
        }
    }
}
