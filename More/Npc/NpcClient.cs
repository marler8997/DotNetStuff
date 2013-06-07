using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
        void VerifyMethodDefinitions(Boolean forceMethodUpdateFromServer, SosMethodDefinition[] expectedMethods);
        Object Call(String methodName, params Object[] parameters);
        Object Call(Type returnType, String methodName, params Object[] parameters);
    }
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

        //
        // Sharing NPC Clients
        //
        static Dictionary<EndPoint, NpcClient> sharedClientsByEndPoint;
        static NpcClient SharedClient(EndPoint endPoint)
        {
            if (sharedClientsByEndPoint == null)
            {
                lock (typeof(NpcClient))
                {
                    if (sharedClientsByEndPoint == null)
                    {
                        sharedClientsByEndPoint = new Dictionary<EndPoint, NpcClient>();
                    }
                }
            }

            NpcClient npcClient;
            if (!sharedClientsByEndPoint.TryGetValue(endPoint, out npcClient))
            {
                npcClient = new NpcClient(endPoint);
                sharedClientsByEndPoint.Add(endPoint, npcClient);
            }
            return npcClient;
        }


        public readonly EndPoint serverEndPoint;
        private AsciiSocket asciiSocket;

        readonly List<Type> enumAndObjectTypes;
        List<SosMethodDefinition> methodsFromServer;

        public NpcClient(EndPoint serverEndPoint)
        {
            this.serverEndPoint = serverEndPoint;

            this.asciiSocket = null;

            this.enumAndObjectTypes = new List<Type>();
            InitializeStaticClientTypeFinder();
        }
        public NpcClient(Socket socket)
        {
            this.serverEndPoint = socket.RemoteEndPoint;

            this.asciiSocket = new AsciiSocket(socket);

            this.enumAndObjectTypes = new List<Type>();
            InitializeStaticClientTypeFinder();
        }
        void UnexpectedClose()
        {
            Dispose();
            throw new InvalidOperationException("Server closed unexpectedly");
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
            if (asciiSocket == null || asciiSocket.socket == null || !asciiSocket.socket.Connected)
            {
                if (asciiSocket != null) asciiSocket.Dispose();

                asciiSocket = new AsciiSocket(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
                asciiSocket.socket.Connect(serverEndPoint);
            }
        }
        public void Dispose()
        {
            AsciiSocket cachedAsciiSocket = this.asciiSocket;
            this.asciiSocket = null;
            if (cachedAsciiSocket != null) cachedAsciiSocket.Dispose();
        }
        public void VerifyMethodDefinitions(Boolean forceUpdateMethodsFromServer, SosMethodDefinition[] expectedMethods)
        {
            List<SosMethodDefinition> actualMethods = GetRemoteMethods(forceUpdateMethodsFromServer);
            VerifyMethodDefinitions(actualMethods, expectedMethods);
        }
        public static void VerifyMethodDefinitions(List<SosMethodDefinition> actualMethods, SosMethodDefinition[] expectedMethods)
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
        public List<SosMethodDefinition> GetRemoteMethods(Boolean forceUpdateFromServer)
        {
            if (methodsFromServer == null || forceUpdateFromServer)
            {
                Connect();
                asciiSocket.socket.Send(Encoding.ASCII.GetBytes("methods\n"));

                this.methodsFromServer = new List<SosMethodDefinition>();
                while (true)
                {
                    String methodDefinitionLine = asciiSocket.ReadLine();
                    if (methodDefinitionLine == null) throw new InvalidOperationException("Server closed unexpectedly");
                    if (methodDefinitionLine.Length <= 0) break; // empty line

                    SosMethodDefinition methodDefinition = SosTypes.ParseMethodDefinition(methodDefinitionLine, 0);
                    methodsFromServer.Add(methodDefinition);
                }
            }
            return methodsFromServer;
        }
        public void UpdateAndVerifyEnumAndObjectTypes()
        {
            Connect();
            asciiSocket.socket.Send(Encoding.UTF8.GetBytes("type\n"));

            enumAndObjectTypes.Clear();

            while (true)
            {
                String typeDefinitionLine = asciiSocket.ReadLine();
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
        public Object Call(String methodName, params object[] parameters)
        {
            return Call(null, methodName, parameters);
        }
        public Object Call(Type expectedReturnType, String methodName, params object[] parameters)
        {
            String callString = Npc.CreateCallString(methodName, parameters);
            return PerformCall(expectedReturnType, methodName, callString);
        }

        public Object CallWithRawParameters(String methodName, String rawParameters)
        {
            return CallWithRawParameters(null, methodName, rawParameters);
        }
        public Object CallWithRawParameters(Type expectedReturnType, String methodName, String rawParameters)
        {
            String rawNpcLine;
            if (rawParameters == null)
            {
                rawNpcLine = String.Format("call {0}\n", methodName);
            }
            else
            {
                rawParameters = rawParameters.Trim();
                if (rawParameters.Length <= 0)
                {
                    rawNpcLine = String.Format("call {0}\n", methodName);
                }
                else
                {
                    rawNpcLine = String.Format("call {0} {1}\n", methodName, rawParameters.Replace("\n", "\\n"));
                }
            }
            return PerformCall(expectedReturnType, methodName, rawNpcLine);
        }

        // expectedReturnType can be null, but providing the expected return type makes it unnecessary to search
        // each assembly for the type
        Object PerformCall(Type expectedReturnType, String methodName, String rawNpcLine)
        {
            Connect();

            asciiSocket.socket.Send(Encoding.UTF8.GetBytes(rawNpcLine.ToString()));

            NpcReturnLine returnLine = new NpcReturnLine(asciiSocket.ReadLine());

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
    }
}
