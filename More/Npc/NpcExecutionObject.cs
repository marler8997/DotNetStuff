using System;
using System.Collections.Generic;
using System.Reflection;

namespace More
{
    public class NpcInterfaceInfo
    {
        public struct Set
        {
            public readonly Dictionary<Type, NpcInterfaceInfo> typeMap;
            public Set(Dictionary<Type, NpcInterfaceInfo> typeMap)
            {
                this.typeMap = typeMap;
            }
            public void TryAdd(Type type, NpcInterfaceInfo info)
            {
                typeMap[type] = info;
            }
            public NpcInterfaceInfo LookupOrCreate(Type interfaceType)
            {
                NpcInterfaceInfo info;
                if (typeMap.TryGetValue(interfaceType, out info)) return info;

                info = new NpcInterfaceInfo(interfaceType);
                typeMap.Add(interfaceType, info);

                info.InitializeInterfaces(this);
                return info;
            }
            public void GetParentInterfaces(Type type, List<NpcInterfaceInfo> parentNpcInterfaces, List<NpcInterfaceInfo> ancestorNpcInterfaces)
            {
                Type[] parentInterfaces = type.GetInterfaces();
                if (parentInterfaces != null)
                {
                    for (int i = 0; i < parentInterfaces.Length; i++)
                    {
                        Type parentInterface = parentInterfaces[i];
                        Attribute npcInterfaceAttribute = Attribute.GetCustomAttribute(parentInterface, typeof(NpcInterface));
                        if (npcInterfaceAttribute == null)
                        {
                            GetParentInterfaces(parentInterface, parentNpcInterfaces, ancestorNpcInterfaces);
                        }
                        else
                        {
                            NpcInterfaceInfo parentNpcInterface = LookupOrCreate(parentInterface);
                            if (parentNpcInterfaces != null && !parentNpcInterfaces.Contains(parentNpcInterface))
                            {
                                parentNpcInterfaces.Add(parentNpcInterface);
                            }
                            if (!ancestorNpcInterfaces.Contains(parentNpcInterface))
                            {
                                ancestorNpcInterfaces.Add(parentNpcInterface);
                            }
                            // Any more parent interfaces shouldn't be added so null is passed
                            GetParentInterfaces(parentInterface, null, ancestorNpcInterfaces);
                        }

                    }
                }
            }
        }
        public readonly Type interfaceType;
        public readonly String name;

        public List<NpcInterfaceInfo> parentNpcInterfaces;
        public List<NpcInterfaceInfo> ancestorNpcInterfaces;

        public readonly NpcMethodInfo[] npcMethods;

        private NpcInterfaceInfo(Type interfaceType)
        {
            this.interfaceType = interfaceType;
            this.name = interfaceType.Name;

            // Get Methods
            MethodInfo[] methods = interfaceType.GetMethods();
            if (methods == null || methods.Length <= 0)
                throw new InvalidOperationException(String.Format("NpcInterface type '{0}' has no methods", interfaceType.Name));

            npcMethods = new NpcMethodInfo[methods.Length];
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo methodInfo = methods[i];
                npcMethods[i] = new NpcMethodInfo(methodInfo);
            }
        }
        public void InitializeInterfaces(NpcInterfaceInfo.Set interfaceSet)
        {
            if (parentNpcInterfaces != null) throw new InvalidOperationException("Interfaces already initialized");

            this.parentNpcInterfaces = new List<NpcInterfaceInfo>();
            this.ancestorNpcInterfaces = new List<NpcInterfaceInfo>();
            interfaceSet.GetParentInterfaces(interfaceType, parentNpcInterfaces, ancestorNpcInterfaces);
        }
    }
    public interface INpcPreAndPostCalls
    {
        void PreCall(NpcExecutionObject executionObject, NpcMethodInfo npcMethodInfo, params Object[] args);
        void PostCall(NpcExecutionObject executionObject, NpcMethodInfo npcMethodInfo, params Object[] args);
    }
    public class NpcStaticExecutionObject
    {
        public readonly Type type;
        public readonly String objectName;
        public Object executionLock;
        internal readonly INpcPreAndPostCalls preAndPostCall;
        public NpcStaticExecutionObject(Type staticNpcObject)
            : this(staticNpcObject, null, null, null)
        {
        }
        public NpcStaticExecutionObject(Type staticNpcObject, String objectName, Object executionLock, INpcPreAndPostCalls preAndPostCall)
        {
            this.type = staticNpcObject;
            this.objectName = (objectName == null) ? type.Name : objectName;
            this.executionLock = executionLock;
            this.preAndPostCall = preAndPostCall;

            throw new NotImplementedException();
            //
            // On static types, just get the methods
            //
            /*
            if (type.IsAbstract && type.IsSealed)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                for (int i = 0; i < methods.Length; i++)
                {
                    MethodInfo methodInfo = methods[i];
                    npcMethods.Add(new NpcMethodInfo(this, methodInfo));
                }
                return;
            }
            */
        }
    }
    public class NpcExecutionObject
    {
        public readonly Type type;
        public readonly Object invokeObject;

        public readonly String objectName;

        public Object executionLock;
        internal readonly INpcPreAndPostCalls preAndPostCall;

        public List<NpcInterfaceInfo> parentNpcInterfaces;
        public List<NpcInterfaceInfo> ancestorNpcInterfaces;

        public NpcExecutionObject(Object invokeObject)
            : this(invokeObject, null, null, null)
        {
        }
        public NpcExecutionObject(Object invokeObject, String objectName)
            : this(invokeObject, objectName, null, null)
        {
        }
        public NpcExecutionObject(Object invokeObject, String objectName, Object executionLock, INpcPreAndPostCalls preAndPostCall)
        {
            this.type = invokeObject.GetType();
            this.invokeObject = invokeObject;
            this.objectName = (objectName == null) ? type.Name : objectName;
            this.executionLock = executionLock;
            this.preAndPostCall = preAndPostCall;
        }
        public void InitializeInterfaces(NpcInterfaceInfo.Set interfaceSet)
        {
            if (parentNpcInterfaces == null)
            {
                this.parentNpcInterfaces = new List<NpcInterfaceInfo>();
                this.ancestorNpcInterfaces = new List<NpcInterfaceInfo>();
                interfaceSet.GetParentInterfaces(type, parentNpcInterfaces, ancestorNpcInterfaces);

                if (parentNpcInterfaces.Count <= 0) throw new InvalidOperationException(String.Format(
                    "Class {0} did not implement any NpcInterfaces (An NpcInterface is an interface with the [NpcInterface] attribute)", type.Name));
            }
            else
            {
                foreach (var @interface in parentNpcInterfaces)
                {
                    interfaceSet.TryAdd(@interface.interfaceType, @interface);
                }
                foreach (var @interface in ancestorNpcInterfaces)
                {
                    interfaceSet.TryAdd(@interface.interfaceType, @interface);
                }
            }
        }
        public override string ToString()
        {
            return String.Format("NpcExecutionObject(Type='{0}',Name='{1}')", type.Name, objectName);
        }
    }
}
