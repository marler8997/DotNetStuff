using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace More
{
    public class NpcMethodInfo
    {
        public readonly String methodName;
        public readonly String methodNameLowerInvariant;
        public readonly MethodInfo methodInfo;
        public readonly ParameterInfo[] parameters;
        public readonly UInt16 parametersLength;

        public NpcMethodInfo(MethodInfo methodInfo)
        {
            this.methodName = methodInfo.Name;
            this.methodNameLowerInvariant = this.methodName.ToLowerInvariant();
            this.methodInfo = methodInfo;
            this.parameters = methodInfo.GetParameters();
            this.parametersLength = (this.parameters == null) ? (UInt16)0 :
                (UInt16)this.parameters.Length;
        }
        /*
        public void ToParsableString(StringBuilder builder)
        {
            builder.Append(methodInfo.ReturnParameter.ParameterType.FullName);
            builder.Append(' ');
            builder.Append(npcMethodName);

            for (UInt16 j = 0; j < parametersLength; j++)
            {
                ParameterInfo parameterInfo = parameters[j];
                builder.Append(' ');
                builder.Append(parameterInfo.ParameterType.FullName);
                builder.Append(':');
                builder.Append(parameterInfo.Name);
            }
            builder.Append('\n');
        }
        */
    }
    public class NpcMethodOverloadable : IEnumerable<NpcMethodInfo>
    {
        public readonly NpcExecutionObject executionObject;

        public readonly NpcMethodInfo npcMethodInfo;
        private SortedList<UInt16, NpcMethodInfo> overloads;

        public NpcMethodOverloadable(NpcExecutionObject executionObject, NpcMethodInfo npcMethodInfo)
        {
            this.executionObject = executionObject;
            this.npcMethodInfo = npcMethodInfo;
            this.overloads = null;
        }
        public void AddOverload(NpcMethodInfo overloadMethodInfo)
        {
            if (overloadMethodInfo.parametersLength == this.npcMethodInfo.parametersLength)
            {
                throw new NotSupportedException(String.Format("{0} Method {1}, adding overloads with the same parameter count is currently not supported",
                    executionObject, overloadMethodInfo.methodInfo.Name));
            }

            if (overloads == null)
            {
                overloads = new SortedList<UInt16, NpcMethodInfo>(4);
                overloads.Add(overloadMethodInfo.parametersLength, overloadMethodInfo);
            }
            else
            {
                if (overloads.ContainsKey(overloadMethodInfo.parametersLength))
                {
                    throw new NotSupportedException(String.Format("Method {0}.{1}, adding overloads with the same parameter count is currently not supported",
                        executionObject.type.Name, overloadMethodInfo.methodInfo.Name));
                }
                overloads.Add(overloadMethodInfo.parametersLength, overloadMethodInfo);
            }
        }
        public NpcMethodInfo GetMethod(UInt16 parameterCount)
        {
            if (this.npcMethodInfo.parametersLength == parameterCount) return this.npcMethodInfo;

            if (overloads != null)
            {
                NpcMethodInfo overloadMethodInfo;
                if (overloads.TryGetValue(parameterCount, out overloadMethodInfo))
                {
                    return overloadMethodInfo;
                }
            }
            return null;
        }
        public IEnumerator<NpcMethodInfo> GetEnumerator()
        {
            return new OneOrMoreEnumerator<NpcMethodInfo>(npcMethodInfo, (overloads == null) ? null : overloads.Values);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new OneOrMoreEnumerator<NpcMethodInfo>(npcMethodInfo, (overloads == null) ? null : overloads.Values);
        }
    }
}
