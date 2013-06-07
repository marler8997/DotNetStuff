using System;
using System.Collections.Generic;
using System.Reflection;

namespace More
{
    public static class NpcReflection
    {
        public static void GetNpcMethods(this Type type, List<MethodInfo> npcMethods, HashSet<Type> reusableHashSet)
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
                    npcMethods.Add(methodInfo);
                }
                return;
            }


            //
            // Use recursion to get methods from interfaces and base classes
            //
            reusableHashSet.Clear();
            GetNpcMethodsFromClass(type, npcMethods, reusableHashSet);
        }
        static void GetNpcMethodsFromClass(Type type, List<MethodInfo> npcMethods, HashSet<Type> interfacesAlreadyChecked)
        {
            //
            // Check interfaces
            //
            Type[] interfaces = type.GetInterfaces();
            if(interfaces != null && interfaces.Length > 0)
                GetNpcMethodsFromInterfaces(interfaces, npcMethods, interfacesAlreadyChecked);

            //
            // Check Base type
            //
            Type baseType = type.BaseType;
            if (baseType != typeof(Object))
                GetNpcMethodsFromClass(baseType, npcMethods, interfacesAlreadyChecked);
        }
        static void GetNpcMethodsFromInterface(Type @interface, List<MethodInfo> npcMethods, HashSet<Type> interfacesAlreadyChecked)
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
                    npcMethods.Add(methodInfo);
                }
            }

            //
            // Check parent interfaces
            //
            Type[] parentInterfaces = @interface.GetInterfaces();
            if (parentInterfaces != null && parentInterfaces.Length > 0)
                GetNpcMethodsFromInterfaces(parentInterfaces, npcMethods, interfacesAlreadyChecked);
        }
        static void GetNpcMethodsFromInterfaces(Type[] interfaces, List<MethodInfo> npcMethods, HashSet<Type> interfacesAlreadyChecked)
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
                    GetNpcMethodsFromInterface(@interface, npcMethods, interfacesAlreadyChecked);
                }
            }
        }
    }
}
