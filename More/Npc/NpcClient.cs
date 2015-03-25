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
        void VerifyObject(Boolean forceInterfaceUpdateFromServer);
    }
    public interface INpcDynamicClient : IDisposable
    {
        void VerifyObject(Boolean forceInterfaceUpdateFromServer, String objectName);
    }
    public interface INpcClientCaller : IDisposable
    {
        void UpdateAndVerifyEnumAndObjectTypes();
        void VerifyObject(Boolean forceInterfaceUpdateFromServer, RemoteNpcObject expectedObject);

        Object Call(String methodName, params Object[] parameters);
        Object CallOnObject(String objectName, String methodName, params Object[] parameters);

        Object Call(Type expectedReturnType, String methodName, params Object[] parameters);
        Object CallOnObject(Type expectedReturnType, String objectName, String methodName, params Object[] parameters);
    }
    public class RemoteNpcInterface
    {
        public readonly String name;
        public readonly String[] parentInterfaceNames;
        public readonly SosMethodDefinition[] methods;
        public RemoteNpcInterface(String name, String[] parentInterfaceNames, SosMethodDefinition[] methods)
        {
            this.name = name;
            this.parentInterfaceNames = parentInterfaceNames;
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
        public static List<RemoteNpcObject> GetServerInterface(SocketLineReader socketLineReader,
            out Dictionary<String, RemoteNpcInterface> serverInterfaces)
        {
            socketLineReader.socket.Send(Encoding.ASCII.GetBytes(":interface\n"));

            serverInterfaces = new Dictionary<String, RemoteNpcInterface>();
            List<SosMethodDefinition> methodDefinitionList = new List<SosMethodDefinition>();

            while (true)
            {
                String interfaceName = socketLineReader.ReadLine();
                if (interfaceName == null) throw UnexpectedClose(socketLineReader);
                if (interfaceName.Length <= 0) break;

                // Get parent interfaces
                String[] parentInterfaceNames = null;
                Int32 spaceIndex = interfaceName.IndexOf(' ');
                if (spaceIndex >= 0)
                {
                    parentInterfaceNames = interfaceName.Substring(spaceIndex + 1).Split(' ');
                    interfaceName = interfaceName.Remove(spaceIndex);
                }

                while (true)
                {
                    String methodDefinitionLine = socketLineReader.ReadLine();
                    if (methodDefinitionLine == null) throw UnexpectedClose(socketLineReader);
                    if (methodDefinitionLine.Length <= 0) break;

                    SosMethodDefinition methodDefinition = SosTypes.ParseMethodDefinition(methodDefinitionLine, 0);
                    methodDefinitionList.Add(methodDefinition);
                }
                serverInterfaces.Add(interfaceName, new RemoteNpcInterface(interfaceName, parentInterfaceNames, methodDefinitionList.ToArray()));
                methodDefinitionList.Clear();
            }

            List<RemoteNpcObject> serverObjects = new List<RemoteNpcObject>();
            while (true)
            {
                String objectLine = socketLineReader.ReadLine();
                if (objectLine == null) throw UnexpectedClose(socketLineReader);
                if (objectLine.Length <= 0) break;

                String objectName = objectLine.Peel(out objectLine);
                String[] interfaceNames = objectLine.Split(RemoteNpcObject.SplitChars, StringSplitOptions.RemoveEmptyEntries);
                RemoteNpcInterface[] interfaces = new RemoteNpcInterface[interfaceNames.Length];
                for (int i = 0; i < interfaceNames.Length; i++)
                {
                    String interfaceName = interfaceNames[i];
                    RemoteNpcInterface npcInterface;
                    if (!serverInterfaces.TryGetValue(interfaceName, out npcInterface))
                        throw new FormatException(String.Format("The NPC server returned interface '{0}' in the :objects command but not in the :interfaces command",
                            interfaceName));
                    interfaces[i] = npcInterface;
                }
                serverObjects.Add(new RemoteNpcObject(objectName, interfaces));
            }

            return serverObjects;
        }


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
        public readonly RemoteNpcInterface[] expectedInterfaces;
        public readonly Boolean threadSafe;
        private SocketLineReader socketLineReader;
        public SocketLineReader UnderlyingLineReader
        {
            get { return socketLineReader; }
        }

        readonly List<Type> enumAndObjectTypes;
        Dictionary<String,RemoteNpcInterface> cachedServerInterfaces;
        public Dictionary<String, RemoteNpcInterface> CachedServerInterfaces
        {
            get { return cachedServerInterfaces; }
        }
        List<RemoteNpcObject> cachedServerObjects;

        public NpcClient(EndPoint serverEndPoint, RemoteNpcInterface[] expectedInterfaces, Boolean threadSafe)
        {
            this.serverEndPoint = serverEndPoint;
            this.expectedInterfaces = expectedInterfaces;
            this.threadSafe = threadSafe;
            this.socketLineReader = null;

            this.enumAndObjectTypes = new List<Type>();
            InitializeStaticClientTypeFinder();
        }
        public NpcClient(Socket socket, RemoteNpcInterface[] expectedInterfaces, Boolean threadSafe)
        {
            this.serverEndPoint = socket.RemoteEndPoint;
            this.expectedInterfaces = expectedInterfaces;
            this.threadSafe = threadSafe;

            this.socketLineReader = (socket == null || !socket.Connected) ? null :
                new SocketLineReader(socket, Encoding.ASCII, Buf.DefaultInitialCapacity, Buf.DefaultExpandLength);

            this.enumAndObjectTypes = new List<Type>();
            InitializeStaticClientTypeFinder();
        }
        InvalidOperationException UnexpectedClose()
        {
            Dispose();
            return new InvalidOperationException("Server closed unexpectedly");
        }
        static InvalidOperationException UnexpectedClose(SocketLineReader socketLineReader)
        {
            socketLineReader.Dispose();
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
                    Encoding.ASCII, Buf.DefaultInitialCapacity, Buf.DefaultExpandLength);
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
        // If the interface is updated and expectedInterfaces is not null, then the interfaces will be checked.
        public List<RemoteNpcObject> GetServerInterface(Boolean forceUpdateFromServer)
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

                        cachedServerObjects = GetServerInterface(socketLineReader, out cachedServerInterfaces);

                        if (expectedInterfaces != null)
                        {
                            String serverInterfaceDiff = ServerInterfaceMethodsDiff(cachedServerInterfaces, expectedInterfaces);
                            if (serverInterfaceDiff != null) throw new NpcInterfaceMismatch(serverInterfaceDiff);
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
        public void VerifyObject(Boolean forceInterfaceUpdateFromServer, RemoteNpcObject expectedObject)
        {
            if (expectedInterfaces == null) throw new InvalidOperationException(String.Format(
                 "Cannot call VerifyObject because there were no expectedInterfaces passed to the constructor"));

            GetServerInterface(forceInterfaceUpdateFromServer);

            Boolean foundObject = false;
            for (int serverObjectIndex = 0; serverObjectIndex < cachedServerObjects.Count; serverObjectIndex++)
            {
                RemoteNpcObject serverObject = cachedServerObjects[serverObjectIndex];

                if (expectedObject.name.Equals(serverObject.name))
                {
                    foundObject = true;

                    // Check that the interfaces are the same
                    for (int expectedInterfaceIndex = 0; expectedInterfaceIndex < expectedObject.interfaces.Length; expectedInterfaceIndex++)
                    {
                        RemoteNpcInterface expectedInterface = expectedObject.interfaces[expectedInterfaceIndex];
                        Boolean foundInterface = false;

                        for (int serverInterfaceIndex = 0; serverInterfaceIndex < serverObject.interfaces.Length; serverInterfaceIndex++)
                        {
                            RemoteNpcInterface serverInterface = serverObject.interfaces[serverInterfaceIndex];

                            if (expectedInterface.name.Equals(serverInterface.name))
                            {
                                foundInterface = true;
                                // Note: the interface definition does not need to be checked because it was already checked
                                break;
                            }
                        }

                        if (!foundInterface) throw new InvalidOperationException(String.Format("Server object '{0}' is missing the '{1}' interface", serverObject.name, expectedInterface.name));
                    }

                    break;
                }
            }
            if (!foundObject) throw new InvalidOperationException(String.Format("Server missing '{0}' object", expectedObject.name));
        }
        /*
        public void VerifyObjectsAndInterfaceMethods(Boolean forceInterfaceUpdateFromServer, ICollection<RemoteNpcObject> expectedObjects, ICollection<RemoteNpcInterface> expectedInterfaces)
        {
            GetServerInterface(forceInterfaceUpdateFromServer);
            String interfaceDiff = ServerInterfaceMethodsDiff(cachedServerInterfaces.Values, expectedInterfaces);
            if (interfaceDiff != null) throw new InvalidOperationException(interfaceDiff);

            foreach (RemoteNpcObject expectedObject in expectedObjects)
            {
                Boolean foundObject = false;
                for (int serverObjectIndex = 0; serverObjectIndex < cachedServerObjects.Count; serverObjectIndex++)
                {
                    RemoteNpcObject serverObject = cachedServerObjects[serverObjectIndex];

                    if (expectedObject.name.Equals(serverObject.name))
                    {
                        foundObject = true;

                        // Check that the interfaces are the same
                        for (int expectedInterfaceIndex = 0; expectedInterfaceIndex < expectedObject.interfaces.Length; expectedInterfaceIndex++)
                        {
                            RemoteNpcInterface expectedInterface = expectedObject.interfaces[expectedInterfaceIndex];
                            Boolean foundInterface = false;

                            for(int serverInterfaceIndex = 0; serverInterfaceIndex < serverObject.interfaces.Length; serverInterfaceIndex++)
                            {
                                RemoteNpcInterface serverInterface = serverObject.interfaces[serverInterfaceIndex];

                                if(expectedInterface.name.Equals(serverInterface.name))
                                {
                                    foundInterface = true;
                                    // Note: the interface definition does not need to be checked because it was already checked
                                    break;
                                }

                            }
                            
                            if(!foundInterface) throw new InvalidOperationException(String.Format("Server object '{0}' is missing the '{1}' interface", serverObject.name, expectedInterface.name));
                        }

                        break;
                    }
                }
                if(!foundObject) throw new InvalidOperationException(String.Format("Server missing '{0}' object", expectedObject.name));
            }
        }
        */
        /*
        public void VerifyInterfaceMethods(Boolean forceInterfaceUpdateFromServer, ICollection<RemoteNpcInterface> expectedInterfaces)
        {
            GetServerInterface(forceInterfaceUpdateFromServer);
            String interfaceDiff = ServerInterfaceMethodsDiff(cachedServerInterfaces.Values, expectedInterfaces);
            if (interfaceDiff != null) throw new InvalidOperationException(interfaceDiff);
        }
        */
 
        // Returns null if the client interfaces are contained in the server interfaces, otherwise,
        // returns a message indicating what client interface is missing or out of sync.
        public static String ServerInterfaceMethodsDiff(IDictionary<String, RemoteNpcInterface> serverInterfaces, ICollection<RemoteNpcInterface> expectedInterfaces)
        {
            if (expectedInterfaces.Count > serverInterfaces.Count)
                return String.Format("Expected server to have at least {0} interfaces but server only has {1}", expectedInterfaces.Count, serverInterfaces.Count);

            foreach (RemoteNpcInterface expectedInterface in expectedInterfaces)
            {
                RemoteNpcInterface serverInterface;
                if(!serverInterfaces.TryGetValue(expectedInterface.name, out serverInterface))
                    return String.Format("Server does not have the '{0}' interface", expectedInterface.name);

                if (serverInterface.methods.Length != expectedInterface.methods.Length)
                    return String.Format("Expected server interface '{0}' to have {1} methods but it has {2}",
                        serverInterface.name, expectedInterface.methods.Length, serverInterface.methods.Length);

                //
                // Check that the interfaces are the same
                //
                for (int clientMethodIndex = 0; clientMethodIndex < expectedInterface.methods.Length; clientMethodIndex++)
                {
                    SosMethodDefinition clientMethodDefinition = expectedInterface.methods[clientMethodIndex];
                    Boolean foundMethod = false;
                    for (int serverMethodIndex = 0; serverMethodIndex < serverInterface.methods.Length; serverMethodIndex++)
                    {
                        SosMethodDefinition serverMethodDefinition = serverInterface.methods[serverMethodIndex];
                        if (clientMethodDefinition.Equals(serverMethodDefinition))
                        {
                            foundMethod = true;
                            break;
                        }
                    }
                    if (!foundMethod) return String.Format("Server Interface '{0}' does not have method '{1}'",
                         serverInterface.name, clientMethodDefinition.Definition());
                }
            }

            return null; // The server has all the expected interfaces
        }

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
                    socketLineReader.socket.Send(Encoding.UTF8.GetBytes(":type\n"));

                    enumAndObjectTypes.Clear();

                    while (true)
                    {
                        String typeDefinitionLine = socketLineReader.ReadLine();
                        if (typeDefinitionLine == null) throw UnexpectedClose();
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
        public Object CallOnObject(String objectName, String methodName, params Object[] parameters)
        {
            return Call(null, objectName, methodName, parameters);
        }
        public Object Call(Type expectedReturnType, String methodName, params Object[] parameters)
        {
            String callString = Npc.CreateCallString(methodName, parameters);
            return PerformCall(expectedReturnType, methodName, callString);
        }
        public Object CallOnObject(Type expectedReturnType, String objectName, String methodName, params Object[] parameters)
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
                rawNpcLine = String.Format("{0}\n", methodName);
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
                rawNpcLine = String.Format("{0}.{1}\n", objectName, methodName);
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
                    if (returnLineString == null) throw UnexpectedClose();

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
