using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using More;

namespace More
{
    public interface INpcClient : IDisposable
    {
        void UpdateAndVerifyEnumAndObjectTypes();
        void VerifyMethodDefinitions(Boolean forceMethodUpdateFromServer);
    }
    public interface INpcClientCaller : IDisposable
    {
        void UpdateAndVerifyEnumAndObjectTypes();
        void VerifyInterfaces(Boolean forceMethodUpdateFromServer, RemoteNpcInterface[] expectedInterfaces);
        void VerifyObjects(Boolean forceMethodUpdateFromServer, RemoteNpcObject[] expectedObjects);

        Object Call(String methodName, params Object[] parameters);
        Object Call(String objectName, String methodName, params Object[] parameters);

        Object Call(Type expectedReturnType, String methodName, params Object[] parameters);
        Object Call(Type expectedReturnType, String objectName, String methodName, params Object[] parameters);
    }


    public class RemoteNpcInterface
    {
        public readonly String name;
        public readonly SosMethodDefinition[] methods;
        public RemoteNpcInterface(String name, SosMethodDefinition[] methods)
        {
            this.name = name;
            this.methods = methods;
        }
    }
    public class RemoteNpcObject
    {
        public static readonly Char[] SplitChars = new Char[] {' ', '\n'};

        public readonly String name;
        public readonly RemoteNpcInterface[] interfaces;
        public RemoteNpcObject(String name, RemoteNpcInterface[] interfaces)
        {
            this.name = name;
            this.interfaces = interfaces;
        }
    }


    //
    // Note that this class is NOT thread safe
    //
    public class NpcClient : INpcClientCaller
    {
        //
        // Static Type Finder
        //
        static ContextTypeFinder staticClientTypeFinder = null;
        static void InitializeStaticClientTypeFinder()
        {
            if (staticClientTypeFinder == null)
            {
                lock (typeof(NpcClient))
                {
                    if (staticClientTypeFinder == null)
                    {
                        staticClientTypeFinder = new ContextTypeFinder();
                    }
                }
            }
        }

        public readonly EndPoint serverEndPoint;
        public readonly Boolean threadSafe;
        private SocketLineReader socketLineReader;

        readonly List<Type> enumAndObjectTypes;
        Dictionary<String,RemoteNpcInterface> cachedServerInterfaces;
        List<RemoteNpcObject> cachedServerObjects;

        public NpcClient(EndPoint serverEndPoint, Boolean threadSafe)
        {
            this.serverEndPoint = serverEndPoint;
            this.threadSafe = threadSafe;
            this.socketLineReader = null;

            this.enumAndObjectTypes = new List<Type>();
            InitializeStaticClientTypeFinder();
        }
        public NpcClient(Socket socket, Boolean threadSafe)
        {
            this.serverEndPoint = socket.RemoteEndPoint;
            this.threadSafe = threadSafe;

            this.socketLineReader = (socket == null || !socket.Connected) ? null :
                new SocketLineReader(socket, Encoding.ASCII, ByteBuffer.DefaultInitialCapacity, ByteBuffer.DefaultExpandLength);

            this.enumAndObjectTypes = new List<Type>();
            InitializeStaticClientTypeFinder();
        }
        InvalidOperationException UnexpectedClose()
        {
            Dispose();
            return new InvalidOperationException("Server closed unexpectedly");
        }
        public void ConnectNow()
        {
            lock (serverEndPoint)
            {
                Connect();
            }
        }
        void Connect()
        {
            if (socketLineReader == null || socketLineReader.socket == null || !socketLineReader.socket.Connected)
            {
                if (socketLineReader != null) socketLineReader.Dispose();

                socketLineReader = new SocketLineReader(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                    Encoding.ASCII, ByteBuffer.DefaultInitialCapacity, ByteBuffer.DefaultExpandLength);
                socketLineReader.socket.Connect(serverEndPoint);
            }
        }
        public void Dispose()
        {
            SocketLineReader cachedSocketLineReader = this.socketLineReader;
            this.socketLineReader = null;
            if (cachedSocketLineReader != null) cachedSocketLineReader.Dispose();
        }



        //
        // Methods Definitions
        //
        public List<RemoteNpcObject> GetRemoteMethods(Boolean forceUpdateFromServer)
        {
            if (threadSafe) Monitor.Enter(serverEndPoint);
            try
            {
                if (cachedServerInterfaces == null || forceUpdateFromServer)
                {
                    //
                    // The reason for the retry logic is because if the underlying socket is disconnected, it may not
                    // fail until after a send and a receive...so the socket should be reconnected and the request should
                    // be repeated only once.
                    //
                    Boolean retryOnSocketException = true;
                RETRY_LOCATION:
                    try
                    {
                        Connect();
                        socketLineReader.socket.Send(Encoding.ASCII.GetBytes(":interface\n"));

                        cachedServerInterfaces = new Dictionary<String, RemoteNpcInterface>();
                        List<SosMethodDefinition> methodDefinitionList = new List<SosMethodDefinition>();

                        while (true)
                        {
                            String interfaceName = socketLineReader.ReadLine();
                            if (interfaceName == null) UnexpectedClose();
                            if (interfaceName.Length <= 0) break;

                            while (true)
                            {
                                String methodDefinitionLine = socketLineReader.ReadLine();
                                if (methodDefinitionLine == null) UnexpectedClose();
                                if (methodDefinitionLine.Length <= 0) break;

                                SosMethodDefinition methodDefinition = SosTypes.ParseMethodDefinition(methodDefinitionLine, 0);
                                methodDefinitionList.Add(methodDefinition);
                            }
                            cachedServerInterfaces.Add(interfaceName, new RemoteNpcInterface(interfaceName, methodDefinitionList.ToArray()));
                            methodDefinitionList.Clear();
                        }

                        cachedServerObjects = new List<RemoteNpcObject>();
                        while (true)
                        {
                            String objectLine = socketLineReader.ReadLine();
                            if (objectLine == null) UnexpectedClose();
                            if (objectLine.Length <= 0) break;
                            
                            String objectName = objectLine.Peel(out objectLine);
                            String[] interfaceNames = objectLine.Split(RemoteNpcObject.SplitChars, StringSplitOptions.RemoveEmptyEntries);
                            RemoteNpcInterface[] interfaces = new RemoteNpcInterface[interfaceNames.Length];
                            for (int i = 0; i < interfaceNames.Length; i++)
                            {
                                String interfaceName = interfaceNames[i];
                                RemoteNpcInterface npcInterface;
                                if (!cachedServerInterfaces.TryGetValue(interfaceName, out npcInterface))
                                    throw new FormatException(String.Format("The NPC server returned interface '{0}' in the :objects command but not in the :interfaces command",
                                        interfaceName));
                                interfaces[i] = npcInterface;
                            }
                            cachedServerObjects.Add(new RemoteNpcObject(objectName, interfaces));
                        }
                    }
                    catch (SocketException)
                    {
                        if (socketLineReader != null)
                        {
                            socketLineReader.Dispose();
                            socketLineReader = null;
                        }
                        if (retryOnSocketException)
                        {
                            retryOnSocketException = false;
                            goto RETRY_LOCATION;
                        }
                        throw;
                    }
                }
                return cachedServerObjects;
            }
            catch(Exception)
            {
                cachedServerInterfaces = null;
                cachedServerObjects = null;
                throw;
            }
            finally
            {
                if (threadSafe) Monitor.Exit(serverEndPoint);
            }
        }
        public void VerifyInterfaces(Boolean forceMethodUpdateFromServer, RemoteNpcInterface[] expectedInterfaces)
        {
            throw new NotImplementedException();
            //VerifyMethodDefinitions(...,...);
        }
        public void VerifyObjects(Boolean forceMethodUpdateFromServer, RemoteNpcObject[] expectedObjects)
        {
            throw new NotImplementedException();
            //VerifyMethodDefinitions(...,...);
        }
        /*
        public void VerifyMethodDefinitions(Boolean forceUpdateMethodsFromServer, List<RemoteNpcObject> expectedObjects)
        {
            throw new NotImplementedException();
            //List<RemoteNpcObject> serverObjects = GetRemoteMethods(forceUpdateMethodsFromServer);
            //VerifyMethodDefinitions(serverObjects, expectedMethods);
        }
        public static void VerifyMethodDefinitions(List<RemoteNpcObject> actualObjects, RemoteNpcObject[] expectedMethods)
        {
            
            if (actualMethods.Count < expectedMethods.Length)
                throw new InvalidOperationException(String.Format(
                    "Server has {0} methods but you expected at least {1} methods", actualMethods.Count, expectedMethods.Length));
            
 
            //
            // Count how many types are expected
            //
            List<String> expectedRemoteTypes = new List<String>();
            for (int i = 0; i < expectedMethods.Length; i++)
            {
                SosMethodDefinition expectedMethod = expectedMethods[i];
                if (!expectedRemoteTypes.Contains(expectedMethod.methodPrefix))
                {
                    expectedRemoteTypes.Add(expectedMethod.methodPrefix);
                }
            }

            //
            // Check that all expected methods are present in the actual methods
            //
            for (int i = 0; i < expectedMethods.Length; i++)
            {
                SosMethodDefinition expectedMethod = expectedMethods[i];

                for (int j = 0; j < actualMethods.Count; j++)
                {
                    SosMethodDefinition actualMethod = actualMethods[j];
                    if (expectedMethod.fullMethodName.Equals(actualMethod.fullMethodName))
                    {
                        if (!expectedMethod.Equals(actualMethod))
                            throw new InvalidOperationException(String.Format(
                                "Expected Method '{0}' differs from actual Method '{1}'", expectedMethod.Definition(), actualMethod.Definition()));
                        break;
                    }
                }
            }

            //
            // Check if the any actual methods from the server are not expected for each expected type
            //
            for (int i = 0; i < actualMethods.Count; i++)
            {
                SosMethodDefinition actualMethod = actualMethods[i];

                if (expectedRemoteTypes.Contains(actualMethod.methodPrefix))
                {
                    Boolean actualMethodIsExpected = false;
                    for (int j = 0; j < expectedMethods.Length; j++)
                    {
                        SosMethodDefinition expectedMethod = expectedMethods[j];
                        if (actualMethod.Equals(expectedMethod))
                        {
                            actualMethodIsExpected = true;
                            break;
                        }
                    }
                    if (!actualMethodIsExpected)
                    {
                        throw new InvalidOperationException(String.Format("For remote type '{0}' the server reported a method that was not expected '{1}'", actualMethod.methodPrefix, actualMethod.Definition()));
                    }
                }
            }
        }
        */


        public void UpdateAndVerifyEnumAndObjectTypes()
        {
            if (threadSafe) Monitor.Enter(serverEndPoint);
            try
            {
                //
                // The reason for the retry logic is because if the underlying socket is disconnected, it may not
                // fail until after a send and a receive...so the socket should be reconnected and the request should
                // be repeated only once.
                //
                Boolean retryOnSocketException = true;
            RETRY_LOCATION:
                try
                {
                    Connect();
                    socketLineReader.socket.Send(Encoding.UTF8.GetBytes("type\n"));

                    enumAndObjectTypes.Clear();

                    while (true)
                    {
                        String typeDefinitionLine = socketLineReader.ReadLine();
                        if (typeDefinitionLine == null) UnexpectedClose();
                        if (typeDefinitionLine.Length == 0) break; // empty line

                        Int32 spaceIndex = typeDefinitionLine.IndexOf(' ');
                        String sosTypeName = typeDefinitionLine.Remove(spaceIndex);
                        String typeDefinition = typeDefinitionLine.Substring(spaceIndex + 1);

                        Type type = GetTypeFromSosTypeName(sosTypeName);

                        if (typeDefinition.StartsWith("Enum"))
                        {
                            SosEnumDefinition enumDefinition = SosTypes.ParseSosEnumTypeDefinition(typeDefinition, 4);
                            enumDefinition.VerifyType(type);
                        }
                        else
                        {
                            SosObjectDefinition objectDefinition = SosTypes.ParseSosObjectTypeDefinition(typeDefinition, 0);
                            objectDefinition.VerifyType(type);
                        }
                        enumAndObjectTypes.Add(type);
                    }
                }
                catch (SocketException)
                {
                    if (socketLineReader != null)
                    {
                        socketLineReader.Dispose();
                        socketLineReader = null;
                    }
                    if (retryOnSocketException)
                    {
                        retryOnSocketException = false;
                        goto RETRY_LOCATION;
                    }
                    throw;
                }
            }
            finally
            {
                if (threadSafe) Monitor.Exit(serverEndPoint);
            }
        }
        Type GetTypeFromSosTypeName(String typeName)
        {
            //
            // Check if the type is an array
            //
            if (typeName[typeName.Length - 1] == ']')
            {
                return GetTypeFromSosTypeName(typeName.Remove(typeName.Length - 2)).MakeArrayType();
            }
            //
            // Check if it is an Sos primitive type
            //
            Type sosPrimitiveType = typeName.TryGetSosPrimitive();
            if (sosPrimitiveType != null) return sosPrimitiveType;
            //
            // Check if it is in the ContextTypeFinder
            //
            return staticClientTypeFinder.FindType(typeName);
        }
        void ThrowExceptionFromCall(String methodName, NpcReturnLine returnLine)
        {
            Type exceptionType;
            try
            {
                exceptionType = staticClientTypeFinder.FindType(returnLine.sosTypeName);
            }
            catch (InvalidOperationException)
            {
                //Console.WriteLine("Could not find type '{0}'", returnLine.sosTypeName);
                goto INVALID_EXCEPTION;
            }

            Object exceptionObject;
            try
            {
                Int32 offset = Sos.Deserialize(out exceptionObject, exceptionType,
                    returnLine.sosSerializationString, 0, returnLine.sosSerializationString.Length);
                if (offset != returnLine.sosSerializationString.Length) goto INVALID_EXCEPTION;
            }
            catch (Exception)
            {
                //Console.WriteLine("Faild to deserialize exception '{0}'", returnLine.sosTypeName);
                goto INVALID_EXCEPTION;
            }

            try
            {
                Exception e = (Exception)exceptionObject;
                throw e;
            }
            catch (InvalidCastException)
            {
                //Console.WriteLine("Could not cast '{0}' to Exception", exceptionObject.GetType().Name);
                goto INVALID_EXCEPTION;
            }

        INVALID_EXCEPTION:
            throw new Exception(String.Format("Method '{0}' threw exception {1}: '{2}'",
                methodName, returnLine.sosTypeName, returnLine.exceptionMessage));
        }
        public Object Call(String methodName, params Object[] parameters)
        {
            return Call((Type)null, methodName, parameters);
        }
        public Object Call(String objectName, String methodName, params Object[] parameters)
        {
            return Call(null, objectName, methodName, parameters);
        }
        public Object Call(Type expectedReturnType, String methodName, params Object[] parameters)
        {
            String callString = Npc.CreateCallString(methodName, parameters);
            return PerformCall(expectedReturnType, methodName, callString);
        }
        public Object Call(Type expectedReturnType, String objectName, String methodName, params Object[] parameters)
        {
            String callString = Npc.CreateCallString(objectName, methodName, parameters);
            return PerformCall(expectedReturnType, methodName, callString);
        }

        public Object CallWithRawParameters(String methodName, String rawParameters)
        {
            return CallWithRawParameters(null, methodName, rawParameters);
        }
        public Object CallWithRawParameters(Type expectedReturnType, String methodName, String rawParameters)
        {
            if (rawParameters != null)
            {
                rawParameters = rawParameters.Trim();
            }

            String rawNpcLine;
            if (String.IsNullOrEmpty(rawParameters))
            {
                rawNpcLine = String.Format("{0}", methodName);
            }
            else
            {
                rawNpcLine = String.Format("{0} {1}\n", methodName, rawParameters.Replace("\n", "\\n"));
            }
            return PerformCall(expectedReturnType, methodName, rawNpcLine);
        }
        public Object CallWithRawParameters(Type expectedReturnType, String objectName, String methodName, String rawParameters)
        {
            if (rawParameters != null)
            {
                rawParameters = rawParameters.Trim();
            }

            String rawNpcLine;
            if (String.IsNullOrEmpty(rawParameters))
            {
                rawNpcLine = String.Format("{0}.{1}", objectName, methodName);
            }
            else
            {
                rawNpcLine = String.Format("{0}.{1} {2}\n", objectName, methodName, rawParameters.Replace("\n", "\\n"));
            }
            return PerformCall(expectedReturnType, methodName, rawNpcLine);
        }

        // expectedReturnType can be null, but providing the expected return type makes it unnecessary to search
        // each assembly for the type
        Object PerformCall(Type expectedReturnType, String methodName, String rawNpcLine)
        {
            if (threadSafe) Monitor.Enter(serverEndPoint);
            try
            {

                //
                // The reason for the retry logic is because if the underlying socket is disconnected, it may not
                // fail until after a send and a receive...so the socket should be reconnected and the request should
                // be repeated only once.
                //
                Boolean retryOnSocketException = true;
            RETRY_LOCATION:
                try
                {
                    Connect();

                    socketLineReader.socket.Send(Encoding.UTF8.GetBytes(rawNpcLine.ToString()));

                    String returnLineString = socketLineReader.ReadLine();
                    if (returnLineString == null) UnexpectedClose();

                    NpcReturnLine returnLine = new NpcReturnLine(returnLineString);

                    if (returnLine.exceptionMessage != null) ThrowExceptionFromCall(methodName, returnLine);

                    if (expectedReturnType == null)
                    {
                        if (returnLine.sosTypeName.Equals("Void")) return null;
                        expectedReturnType = GetTypeFromSosTypeName(returnLine.sosTypeName);
                    }
                    else
                    {
                        if (!returnLine.sosTypeName.Equals(expectedReturnType.SosTypeName()))
                            throw new InvalidOperationException(String.Format("Expected return type to be {0} but was {1}",
                                expectedReturnType.SosTypeName(), returnLine.sosTypeName));
                    }

                    if (expectedReturnType == typeof(void)) return null;

                    Object returnObject;
                    Int32 valueStringOffset = Sos.Deserialize(out returnObject, expectedReturnType,
                        returnLine.sosSerializationString, 0, returnLine.sosSerializationString.Length);

                    if (valueStringOffset != returnLine.sosSerializationString.Length)
                        throw new InvalidOperationException(String.Format(
                            "Used {0} characters to deserialize object of type '{1}' but the serialization string had {2} characters",
                            valueStringOffset, expectedReturnType.SosTypeName(), returnLine.sosSerializationString.Length));

                    return returnObject;
                }
                catch (SocketException)
                {
                    if (socketLineReader != null)
                    {
                        socketLineReader.Dispose();
                        socketLineReader = null;
                    }
                    if (retryOnSocketException)
                    {
                        retryOnSocketException = false;
                        goto RETRY_LOCATION;
                    }
                    throw;
                }
            }
            finally
            {
                if (threadSafe) Monitor.Exit(serverEndPoint);
            }
        }
    }
}
