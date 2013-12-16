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

        //
        // Gets set when it's methods are found
        //
        public List<NpcMethodInfo> npcMethods;

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
        }
        public void FindNpcMethods(HashSet<Type> hashSetToCheckInterfaces)
        {
            if (npcMethods != null) throw new InvalidOperationException(String.Format(
                "The npc methods for '{0}' have already been found", this));
            this.npcMethods = new List<NpcMethodInfo>();

            //
            // Get Npc Methods
            //
            GetNpcMethods(type, hashSetToCheckInterfaces);

            if (npcMethods.Count <= 0)
                throw new InvalidOperationException(String.Format(
                    "{0} did not implement have any npc methods.  An npc method is a method inside an interface with the [NpcInterface] attribute.",
                    this));
        }
        void GetNpcMethods(Type type, HashSet<Type> reusableHashSet)
        {
            //
            // On static types, just get the methods
            //
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

            //
            // Use recursion to get methods from interfaces and base classes
            //
            reusableHashSet.Clear();
            GetNpcMethodsFromClass(type, reusableHashSet);
        }
        void GetNpcMethodsFromClass(Type type, HashSet<Type> interfacesAlreadyChecked)
        {
            //
            // Check interfaces
            //
            Type[] interfaces = type.GetInterfaces();
            if (interfaces != null && interfaces.Length > 0)
                GetNpcMethodsFromInterfaces(interfaces, interfacesAlreadyChecked);

            //
            // Check Base type
            //
            Type baseType = type.BaseType;
            if (baseType != typeof(Object))
                GetNpcMethodsFromClass(baseType, interfacesAlreadyChecked);
        }
        void GetNpcMethodsFromInterface(Type @interface, HashSet<Type> interfacesAlreadyChecked)
        {
            //
            // Check if this interface is an Npc interface
            //
            Attribute npcInterfaceAttribute = Attribute.GetCustomAttribute(@interface, typeof(NpcInterface));
            if (npcInterfaceAttribute != null)
            {
                MethodInfo[] methods = @interface.GetMethods();
                for (int j = 0; j < methods.Length; j++)
                {
                    MethodInfo methodInfo = methods[j];
                    npcMethods.Add(new NpcMethodInfo(this, methodInfo));
                }
            }

            //
            // Check parent interfaces
            //
            Type[] parentInterfaces = @interface.GetInterfaces();
            if (parentInterfaces != null && parentInterfaces.Length > 0)
                GetNpcMethodsFromInterfaces(parentInterfaces, interfacesAlreadyChecked);
        }
        void GetNpcMethodsFromInterfaces(Type[] interfaces, HashSet<Type> interfacesAlreadyChecked)
        {
            //
            // Check implemented interfaces
            //
            for (int i = 0; i < interfaces.Length; i++)
            {
                Type @interface = interfaces[i];

                if (!interfacesAlreadyChecked.Contains(@interface))
                {
                    interfacesAlreadyChecked.Add(@interface);
                    GetNpcMethodsFromInterface(@interface, interfacesAlreadyChecked);
                }
            }
        }
        public override string ToString()
        {
            return String.Format("NpcExecutionObject(Type='{0}',Name='{1}')", type.Name, objectName);
        }
    }
}
