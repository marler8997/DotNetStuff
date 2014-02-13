using System;
using System.Collections.Generic;
using System.Reflection;

namespace More
{
    public class NpcInterfaceInfo
    {
        static readonly Dictionary<Type, NpcInterfaceInfo> staticNpcInterfaceMap = new Dictionary<Type, NpcInterfaceInfo>();

        internal static NpcInterfaceInfo Create(Type newInterfaceType)
        {
            lock (staticNpcInterfaceMap)
            {
                NpcInterfaceInfo info = TryLookup(newInterfaceType);
                if (info != null) return info;
                info = new NpcInterfaceInfo(newInterfaceType);
                staticNpcInterfaceMap.Add(newInterfaceType, info);
                return info;
            }
        }
        public static NpcInterfaceInfo TryLookup(Type @interface)
        {
            lock (staticNpcInterfaceMap)
            {
                NpcInterfaceInfo info;
                if (staticNpcInterfaceMap.TryGetValue(@interface, out info)) return info;
                return null;
            }
        }

        public readonly String name;
        public readonly String nameLowerInvariant;

        public readonly Type interfaceType;

        public readonly NpcMethodInfo[] npcMethods;

        private NpcInterfaceInfo(Type interfaceType)
        {
            this.name = interfaceType.Name;
            this.nameLowerInvariant = interfaceType.Name.ToLowerInvariant();
            this.interfaceType = interfaceType;

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
        public readonly String objectNameLowerInvariant;
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
            this.objectNameLowerInvariant = this.objectName.ToLowerInvariant();
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
        public readonly String objectNameLowerInvariant;

        public Object executionLock;
        internal readonly INpcPreAndPostCalls preAndPostCall;

        public readonly List<NpcInterfaceInfo> npcInterfaces;

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
            this.objectNameLowerInvariant = this.objectName.ToLowerInvariant();
            this.executionLock = executionLock;
            this.preAndPostCall = preAndPostCall;

            this.npcInterfaces = new List<NpcInterfaceInfo>();

            Type[] interfaces = type.GetInterfaces();
            if (interfaces == null || interfaces.Length <= 0) throw new InvalidOperationException(String.Format(
                "Class '{0}' does not inherit any interfaces", type.Name));

            //Console.WriteLine("[NpcDebug] '{0}' has {1} interfaces:", type.Name, interfaces.Length);
            for(int i = 0; i < interfaces.Length; i++)
            {
                Type @interface = interfaces[i];
                Attribute npcInterfaceAttribute = Attribute.GetCustomAttribute(@interface, typeof(NpcInterface));
                if (npcInterfaceAttribute != null)
                {
                    AddInterface(@interface);
                }
            }

            if (npcInterfaces.Count <= 0) throw new InvalidOperationException(String.Format(
                "Class {0} did not implement any NpcInterfaces (An NpcInterface is an interface with the [NpcInterface] attribute)", this.type.Name));
        }        
        // Note: staticNpcInterfaceMap must be locked on before this method is called
        void AddInterfaces(Type[] interfaces)
        {
            for (int i = 0; i < interfaces.Length; i++)
            {
                Type @interface = interfaces[i];
                NpcInterfaceInfo npcInterface = NpcInterfaceInfo.TryLookup(@interface);
                if(npcInterface != null)
                {
                    npcInterfaces.Add(npcInterface);
                }
                else
                {
                    //
                    // Check if this interface is an Npc interface
                    //
                    Attribute npcInterfaceAttribute = Attribute.GetCustomAttribute(@interface, typeof(NpcInterface));
                    if (npcInterfaceAttribute != null)
                    {
                        AddInterface(@interface);
                    }
                }
            }
        }
        void AddInterface(Type newInterfaceType)
        {
            for (int i = 0; i < npcInterfaces.Count; i++)
            {
                NpcInterfaceInfo existingLeafInterface = npcInterfaces[i];
                if (newInterfaceType == existingLeafInterface.interfaceType)
                {
                    //Console.WriteLine("[NpcDebug] NpcInterface '{0}' has already been added to '{1}'",
                    //    newInterfaceType.Name, objectName);
                    return;
                }
                /*
                if (newInterfaceType.IsAssignableFrom(existingLeafInterface.interfaceType))
                {
                    Console.WriteLine("[NpcDebug] NpcInterface '{0}' is a parent interface of '{1}'",
                        newInterfaceType.Name, existingLeafInterface.name);
                    return;
                }
                if (existingLeafInterface.interfaceType.IsAssignableFrom(newInterfaceType))
                {
                    Console.WriteLine("[NpcDebug] NpcInterface '{0}' is a parent interface of '{1}'",
                        existingLeafInterface.name, newInterfaceType.Name);
                    newNpcInterface = new NpcInterfaceInfo(newInterfaceType);
                    //staticNpcInterfaceMap.Add(newInterfaceType, newNpcInterface);
                    leafNpcInterfaces[i] = newNpcInterface;
                    return;
                }
                */
            }

            //Console.WriteLine("[NpcDebug] Adding NpcInterface '{0}' to '{1}'",
            //    newInterfaceType.Name, objectName);
            //staticNpcInterfaceMap.Add(newInterfaceType, newNpcInterface);
            npcInterfaces.Add(NpcInterfaceInfo.Create(newInterfaceType));
        }
        public override string ToString()
        {
            return String.Format("NpcExecutionObject(Type='{0}',Name='{1}')", type.Name, objectName);
        }
    }
}
