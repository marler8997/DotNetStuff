using System;
using System.Collections.Generic;
using System.Reflection;

namespace More
{
    public interface INpcPreAndPostCalls
    {
        void PreCall(NpcExecutionObject executionObject, NpcMethodInfo npcMethodInfo, params Object[] args);
        void PostCall(NpcExecutionObject executionObject, NpcMethodInfo npcMethodInfo, params Object[] args);
    }

    public class NpcExecutionObject
    {
        public readonly Type type;
        public readonly Object invokeObject;

        public readonly Boolean isStatic;

        public readonly String objectName;
        public readonly String objectNameLowerInvariant;

        public Object executionLock;
        internal readonly INpcPreAndPostCalls preAndPostCall;

        public readonly List<NpcMethodInfo> npcMethods;

        public NpcExecutionObject(Object invokeObject)
            : this(invokeObject, null, null, null)
        {
        }
        public NpcExecutionObject(Object invokeObject, String objectName, Object executionLock, INpcPreAndPostCalls preAndPostCall)
        {
            this.type = invokeObject as Type;
            if (this.type == null)
            {
                this.type = invokeObject.GetType();
                this.isStatic = false;
            }
            else
            {
                this.isStatic = true;
            }

            this.invokeObject = invokeObject;

            this.objectName = (objectName == null) ? type.FullName : objectName;
            this.objectNameLowerInvariant = this.objectName.ToLowerInvariant();

            this.executionLock = executionLock;
            this.preAndPostCall = preAndPostCall;

            this.npcMethods = new List<NpcMethodInfo>();        
        }
    }
}
