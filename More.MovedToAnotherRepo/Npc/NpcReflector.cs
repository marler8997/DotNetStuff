using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace More
{
    public class NpcReflector : NpcExecutor
    {
        readonly NpcExecutionObject[] npcExecutionObjects;

        public override ICollection<NpcInterfaceInfo> Interfaces
        {
            get { return interfaceSet.typeMap.Values; }
        }
        public override ICollection<NpcExecutionObject> ExecutionObjects
        {
            get { return npcExecutionObjects; }
        }
        public override IDictionary<String,Type> EnumAndObjectTypes
        {
            get { return enumAndObjectTypesDictionary; }
        }
        public override IDictionary<String, OneOrMoreTypes> EnumAndObjectShortNameTypes
        {
            get { return enumAndObjectTypesWithUniqueShortNamesDictionary; }
        }

        readonly NpcInterfaceInfo.Set interfaceSet;
        readonly List<NpcMethodOverloadable> methodList;
        readonly Dictionary<String,NpcMethodOverloadable> withObjectMethodDictionary;
        readonly Dictionary<String, List<NpcMethodOverloadable>> noObjectDictionary;
        readonly Dictionary<String, Type> enumAndObjectTypesDictionary;
        readonly Dictionary<String, OneOrMoreTypes> enumAndObjectTypesWithUniqueShortNamesDictionary;

        public NpcReflector(params Object [] executionObjects)
        {
            if (executionObjects == null) throw new ArgumentNullException("executionObjects");
            if (executionObjects.Length <= 0) throw new ArgumentException("exeuctionObjects must have at least one object", "executionObjects");

            this.npcExecutionObjects = new NpcExecutionObject[executionObjects.Length];

            this.interfaceSet                 = new NpcInterfaceInfo.Set(new Dictionary<Type, NpcInterfaceInfo>());
            this.methodList                   = new List<NpcMethodOverloadable>();
            this.withObjectMethodDictionary   = new Dictionary<String, NpcMethodOverloadable>(StringComparer.OrdinalIgnoreCase);
            this.noObjectDictionary           = new Dictionary<String, List<NpcMethodOverloadable>>(StringComparer.OrdinalIgnoreCase);
            this.enumAndObjectTypesDictionary = new Dictionary<String, Type>();
            this.enumAndObjectTypesWithUniqueShortNamesDictionary = new Dictionary<String, OneOrMoreTypes>();

            //
            // Find all methods that are apart of an [NpcInterface]
            //
            SosTypeSerializationVerifier verifier = new SosTypeSerializationVerifier();
            for (int objectIndex = 0; objectIndex < executionObjects.Length; objectIndex++)
            {
                Object executionObject = executionObjects[objectIndex];
                NpcExecutionObject npcExecutionObject = executionObject as NpcExecutionObject;
                if (npcExecutionObject == null)
                {
                    npcExecutionObject = new NpcExecutionObject(executionObject);
                }
                npcExecutionObject.InitializeInterfaces(this.interfaceSet);
                npcExecutionObjects[objectIndex] = npcExecutionObject;

                foreach (NpcInterfaceInfo interfaceInfo in npcExecutionObject.ancestorNpcInterfaces)
                {
                    //
                    // Add all the methods
                    //
                    NpcMethodInfo[] npcMethodInfos = interfaceInfo.npcMethods;
                    for (int methodIndex = 0; methodIndex < npcMethodInfos.Length; methodIndex++)
                    {
                        NpcMethodInfo npcMethodInfo = npcMethodInfos[methodIndex];
                        //Console.WriteLine("   [NpcDebug] Registering types for method '{0}'", npcMethodInfo.methodName);

                        //
                        // Check that all parameter types can be parsed
                        //
                        for (UInt16 k = 0; k < npcMethodInfo.parametersLength; k++)
                        {
                            RegisterType(verifier, npcMethodInfo.parameters[k].ParameterType);
                        }

                        //
                        // Find the appropriate ToString method for the return type
                        //
                        RegisterType(verifier, npcMethodInfo.methodInfo.ReturnType);

                        //
                        // Add method info to dictionary
                        //
                        String objectMethodName = npcExecutionObject.objectName + "." + npcMethodInfo.methodName;
                        NpcMethodOverloadable overloadableMethod;
                        if (withObjectMethodDictionary.TryGetValue(objectMethodName, out overloadableMethod))
                        {
                            overloadableMethod.AddOverload(npcMethodInfo);
                        }
                        else
                        {
                            overloadableMethod = new NpcMethodOverloadable(npcExecutionObject, npcMethodInfo);
                            methodList.Add(overloadableMethod);
                            withObjectMethodDictionary.Add(objectMethodName, overloadableMethod);
                        }

                        List<NpcMethodOverloadable> methodsWithSameShortName;
                        if (!noObjectDictionary.TryGetValue(npcMethodInfo.methodName, out methodsWithSameShortName))
                        {
                            methodsWithSameShortName = new List<NpcMethodOverloadable>();
                            noObjectDictionary.Add(npcMethodInfo.methodName, methodsWithSameShortName);
                        }
                        methodsWithSameShortName.Add(overloadableMethod);
                    }
                }
            }
        }

        //
        // Every function parameter type and return type must be registered.
        // Registration checks that the type can be serialized and also saves the type information to send
        // to the client.
        //
        void RegisterType(SosTypeSerializationVerifier verifier, Type type)
        {
            if (type == typeof(void) || type.IsSosPrimitive()) return;
            if (type.IsArray)
            {
                RegisterType(verifier, type.GetElementType());
                return;
            }

            Type alreadyRegisteredType;
            if (enumAndObjectTypesDictionary.TryGetValue(type.FullName, out alreadyRegisteredType))
            {
                if(alreadyRegisteredType != type)
                    throw new InvalidOperationException(String.Format("Error: there are 2 different types with the same name '{0}'", type.FullName));
                return;
            }

            String because = verifier.CannotBeSerializedBecause(type);
            if (because != null) throw new InvalidOperationException(String.Format(
                 "The type '{0}' cannot be serialized because {1}", type.FullName, because));

            enumAndObjectTypesDictionary.Add(type.FullName, type);

            OneOrMoreTypes shortNameTypeGroup;
            if (enumAndObjectTypesWithUniqueShortNamesDictionary.TryGetValue(type.Name, out shortNameTypeGroup))
            {
                if (shortNameTypeGroup.otherTypes == null) shortNameTypeGroup.otherTypes = new List<Type>();
                shortNameTypeGroup.otherTypes.Add(type);
            }
            else
            {
                enumAndObjectTypesWithUniqueShortNamesDictionary.Add(type.Name, new OneOrMoreTypes(type));
            }

            if(!type.IsEnum)
            {
                //
                // Register the user defined types fields
                //
                FieldInfo[] fieldInfos = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (fieldInfos == null || fieldInfos.Length <= 0) return;
                for (int i = 0; i < fieldInfos.Length; i++)
                {
                    RegisterType(verifier, fieldInfos[i].FieldType);
                }
            }
        }
        public override NpcMethodInfo GetNpcMethodInfo(String methodName, UInt16 parameterCount, out NpcExecutionObject executionObject)
        {
            //
            // Find method with matching name
            //
            NpcMethodOverloadable overloadableMethod;
            if (methodName.Contains("."))
            {
                if (!withObjectMethodDictionary.TryGetValue(methodName, out overloadableMethod))
                {
                    throw new NpcErrorException(NpcErrorCode.UnknownMethodName,
                        String.Format("Method '{0}' was not found", methodName));
                }
            }
            else
            {
                List<NpcMethodOverloadable> overloadableMethodsWithSameName;
                if (!noObjectDictionary.TryGetValue(methodName, out overloadableMethodsWithSameName))
                {
                    throw new NpcErrorException(NpcErrorCode.UnknownMethodName,
                        String.Format("Method '{0}' was not found", methodName));
                }

                if (overloadableMethodsWithSameName.Count == 1)
                {
                    overloadableMethod = overloadableMethodsWithSameName[0];
                }
                else
                {
                    throw new NpcErrorException(NpcErrorCode.AmbiguousMethodName, String.Format(
                        "Method '{0}' exists but there are multiple objects that have a method matching that name, use <object-name>.{0} to indicate which object you would like to call the method on", methodName));
                }
            }

            //
            // Find method with matching name that has the correct number of parameters
            //
            NpcMethodInfo npcMethodInfo = overloadableMethod.GetMethod(parameterCount);
            if (npcMethodInfo == null)
                throw new InvalidOperationException(String.Format("Method '{0}' was found but it does not have {1} parameters", methodName, parameterCount));
            executionObject = overloadableMethod.executionObject;
            return npcMethodInfo;
        }
        public override NpcReturnObjectOrException ExecuteWithObjects(NpcExecutionObject executionObject, NpcMethodInfo npcMethodInfo, params Object[] args)
        {
            //
            // Setup Parameter array of Objects to invoke the method
            //
            MethodInfo methodInfo = npcMethodInfo.methodInfo;

            //
            // Invoke
            //
            try
            {
                //
                // Call pre call if specified
                //
                if(executionObject.preAndPostCall != null)
                {
                    executionObject.preAndPostCall.PreCall(executionObject, npcMethodInfo, args);
                }

                Object returnObject;
                //
                // Make sure execution lock is respected if it was provided
                //
                try
                {
                    if (executionObject.executionLock == null)
                    {
                        returnObject = methodInfo.Invoke(executionObject.invokeObject, args);
                    }
                    else
                    {
                        lock (executionObject.executionLock)
                        {
                            returnObject = methodInfo.Invoke(executionObject.invokeObject, args);
                        }
                    }
                }
                finally
                {
                    //
                    // Call post call if specified
                    //
                    if (executionObject.preAndPostCall != null)
                    {
                        executionObject.preAndPostCall.PostCall(executionObject, npcMethodInfo, args);
                    }
                }

                return new NpcReturnObjectOrException(methodInfo.ReturnType, returnObject, returnObject.SerializeObject());
            }
            catch (TargetInvocationException e)
            {
                return new NpcReturnObjectOrException(e.InnerException, e.SerializeObject());
            }
        }
        public void PrintInformation(TextWriter writer)
        {
            writer.WriteLine("Methods:");
            foreach (NpcMethodOverloadable overloadableMethod in methodList)
            {
                foreach (NpcMethodInfo npcMethodInfo in overloadableMethod)
                {
                    Console.WriteLine("   Name='{0}' {1}", npcMethodInfo.methodName, npcMethodInfo.methodInfo);
                }
            }

            writer.WriteLine("Enum and Object types:");
            foreach (KeyValuePair<String,Type> pair in enumAndObjectTypesDictionary)
            {
                Console.WriteLine("   " + pair.Key);
            }
        }
    }
}
