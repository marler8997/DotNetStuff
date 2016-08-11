using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace More
{
    public class OneOrMoreTypes
    {
        public readonly Type firstType;
        public List<Type> otherTypes;
        public OneOrMoreTypes(Type firstType)
        {
            this.firstType = firstType;
        }
    }
    public abstract class NpcExecutor
    {
        public abstract ICollection<NpcInterfaceInfo> Interfaces { get; }
        public abstract ICollection<NpcExecutionObject> ExecutionObjects { get; }
        public abstract IDictionary<String, Type> EnumAndObjectTypes { get; }
        public abstract IDictionary<String, OneOrMoreTypes> EnumAndObjectShortNameTypes { get; }

        public abstract NpcMethodInfo GetNpcMethodInfo(String methodName, UInt16 parameterCount, out NpcExecutionObject executionObject);

        public abstract NpcReturnObjectOrException ExecuteWithObjects(NpcExecutionObject executionObject, NpcMethodInfo npcMethodInfo, params Object[] args);

        public NpcReturnObjectOrException ExecuteWithObjects(String methodName, params Object[] args)
        {
            UInt16 argsLength = (args == null) ? (UInt16)0 : (UInt16)args.Length;
            NpcExecutionObject executionObject;
            NpcMethodInfo npcMethodInfo = GetNpcMethodInfo(methodName, argsLength, out executionObject);

            return ExecuteWithObjects(executionObject, npcMethodInfo, args);
        }
        public NpcReturnObjectOrException ExecuteWithStrings(String methodName, params String[] parameterStrings)
        {
            Int32 parameterStringsLength = (parameterStrings == null) ? 0 : parameterStrings.Length;

            NpcExecutionObject executionObject;
            NpcMethodInfo methodInfo = GetNpcMethodInfo(methodName, (UInt16)parameterStringsLength, out executionObject);

            Object[] parameterObjects = Npc.CreateParameterObjects(methodInfo, parameterStrings);

            return ExecuteWithObjects(executionObject, methodInfo, parameterObjects);
        }
        public NpcReturnObjectOrException ExecuteWithStrings(NpcExecutionObject executionObject, NpcMethodInfo methodInfo, params String[] parameterStrings)
        {
            Object[] parameterObjects = Npc.CreateParameterObjects(methodInfo, parameterStrings);

            return ExecuteWithObjects(executionObject, methodInfo, parameterObjects);
        }
    }
}
