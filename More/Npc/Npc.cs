using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

//
// TODO: Finish implementing table syntax
// TODO: Allow subclasses to be passed as parameters (means extra fields should be ignored)

namespace More
{
    public class NpcInterface : Attribute { }

    public enum NpcErrorCode
    {
        UnhandledException    = 0,
        InvalidCallSyntax     = 1,
        InvalidCallParameters = 2,
        UnknownMethodName     = 3,
        AmbiguousMethodName   = 4,
        RequestedUnknownType  = 5,
    }

    public static class Npc
    {
        static readonly Char[] InvalidCharsForNonQuotedStrings = new Char[]{' ', '\n', '\r', '\t', '\\','"'};

        public static Object[] CreateParameterObjects(NpcMethodInfo npcMethodInfo, params String[] parameterStrings)
        {
            int parameterStringsLength = (parameterStrings == null) ? 0 : parameterStrings.Length;
            int parameterInfosLength = (npcMethodInfo.parameters == null) ? 0 : npcMethodInfo.parameters.Length;

            if (parameterInfosLength != parameterStringsLength)
                throw new InvalidOperationException(String.Format("Expected {0} arguments but got {1}",
                    parameterInfosLength, parameterStringsLength));

            if (parameterStringsLength <= 0) return null;

            ParameterInfo[] parameterInfos = npcMethodInfo.parameters;
            Object[] parameterObjects = new Object[parameterStringsLength];

            for (int i = 0; i < parameterStringsLength; i++)
            {
                String parameterString = parameterStrings[i];

                ParameterInfo parameterInfo = parameterInfos[i];
                Type parameterType = parameterInfos[i].ParameterType;

                //Console.WriteLine("Parameter {0} Type={1}", parameterInfo.Name, parameterType);

                //
                // Add quotes if parameter is string type and it is missing quotes
                //
                if (parameterType == typeof(String) && !parameterString.Equals("null"))
                {
                    if (parameterString.Length <= 0)
                    {
                        parameterString = "null"; // default to null for empty string
                    }
                    else
                    {
                        if (parameterString[0] != '"')
                        {
                            //
                            // make sure the string does not contain any whitespace or quotes
                            //
                            if (parameterString.IndexOfAny(InvalidCharsForNonQuotedStrings) >= 0)
                                throw new InvalidOperationException(String.Format(
                                    "You provided a non-quoted string with invalid characters '{0}' (invalid characters include whitespace, backslashes or quotes)", parameterString)); 
                            parameterString = "\"" + parameterString + "\"";
                        }
                    }
                }

                Int32 deserialationIndex;
                try
                {
                    deserialationIndex = Sos.Deserialize(
                        out parameterObjects[i], parameterType, parameterString, 0, parameterString.Length);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(String.Format("Failed to deserialize argument {0} ({1}) of type {2}: {3}",
                        i + 1, parameterInfo.Name, parameterType.SosTypeName(), e.Message));
                }
                if (deserialationIndex != parameterString.Length) throw new InvalidOperationException(String.Format(
                     "Argument {0} (type={1}) has {2} characters but deserializtion only used {3} characters",
                     i, parameterType, parameterString.Length, deserialationIndex));
            }

            return parameterObjects;
        }
        public static String CreateCallString(String methodName, Object[] args)
        {
            if (args == null || args.Length == 0)
                return String.Format("call {0}\n", methodName);

            StringBuilder callStringBuilder = new StringBuilder("call ");
            callStringBuilder.Append(methodName);

            for (int i = 0; i < args.Length; i++)
            {
                callStringBuilder.Append(' ');
                args[i].SerializeObject(callStringBuilder);
            }
            return callStringBuilder.ToString().Replace("\n", "\\n") + "\n";
        }
        public static void ParseParameters(String parametersString, List<String> parameterList)
        {
            if (parametersString == null) return;
            if (parametersString.Length <= 0) return;

            Int32 offset = 0;

            while (true)
            {
                while(true)
                {
                    Char c = parametersString[offset];
                    if(Sos.IsValidStartOfSosString(c)) break;
                    if(!Char.IsWhiteSpace(c)) throw new FormatException(String.Format(
                        "Every parameter string must start with 0-9,a-z,A-Z,\",[,{{ or ', but parameter {0} started with '{1}' (charcode={2})",
                        parameterList.Count, c, (UInt32)c));
                    offset++;
                    if(offset >= parametersString.Length) return;
                }

                Int32 nextSpace = Sos.NextNonQuotedWhitespace(parametersString, offset);
                parameterList.Add(parametersString.Substring(offset, nextSpace - offset));

                offset = nextSpace + 1;
                if (offset >= parametersString.Length) return;
            }
        }

        public static Boolean IsValidNpcObjectName(String objectName)
        {
            for (int i = 0; i < objectName.Length; i++)
            {
                Char c = objectName[i];
                if (
                    (c < 'a' || c > 'z') &&
                    (c < 'A' || c > 'Z') &&
                    (c != '.') &&
                    (c < '0' || c > '9') &&
                    (c != '_')) return false;
            }
            return true;
        }
    }
}
