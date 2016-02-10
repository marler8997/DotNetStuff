using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;

using More;
using More.Net;

namespace More
{
    public interface INpcServerCallback
    {
        void ServerListening(Socket listenSocket);

        void FunctionCall(String clientString, String methodName);
        void FunctionCallThrewException(String clientString, String methodName, Exception e);

        void GotInvalidData(String clientString, String message);

        void ExceptionDuringExecution(String clientString, String methodName, Exception e);
        void ExceptionWhileGeneratingHtml(String clientString, Exception e);

        void UnhandledException(String clientString, Exception e);
    }
    public class NpcSocketHandler : NpcHandler
    {
        readonly LineParser lineParser;
        public NpcSocketHandler(String clientString, INpcServerCallback callback, NpcExecutor npcExecutor, INpcHtmlGenerator npcHtmlGenerator)
            : base(clientString, callback, npcExecutor, npcHtmlGenerator)
        {
            if (callback == null) throw new ArgumentNullException("callback");

            this.lineParser = new LineParser(Encoding.ASCII, Buf.DefaultInitialCapacity, Buf.DefaultExpandLength);
        }

        // Returns false on error
        public Boolean TryReceive(Socket socket, Buf safeBuffer)
        {
            int bytesReceived;
            try
            {
                bytesReceived = socket.Receive(safeBuffer.array);
            }
            catch (SocketException)
            {
                bytesReceived = -1;
            }

            if (bytesReceived <= 0)
            {
                return false; // Error
            }

            lineParser.Add(safeBuffer.array, 0, (uint)bytesReceived);
            return true; // Success
        }


        void HandleNpcLines(ref SelectControl selectControl, Socket socket, Buf safeBuffer, ByteBuilder builder, String line)
        {
            do
            {
                if (!HandleLine(builder, line))
                {
                    selectControl.ShutdownDisposeAndRemoveReceiveSocket(socket);
                    break;
                }
                if (builder.bytes != safeBuffer.array)
                {
                    safeBuffer.array = builder.bytes; // Update the buffer with the bigger buffer
                }

                if (builder.contentLength > 0)
                {
                    socket.Send(builder.bytes, (int)builder.contentLength, SocketFlags.None);
                    builder.Clear();
                }

                line = lineParser.GetLine();

            } while (line != null);
        }

        public void InitialRecvHandler(ref SelectControl selectControl, Socket socket, Buf safeBuffer)
        {
            try
            {
                if (!TryReceive(socket, safeBuffer))
                {
                    selectControl.ShutdownDisposeAndRemoveReceiveSocket(socket);
                    return;
                }

                String line = lineParser.GetLine();
                if (line == null)
                {
                    return;
                }

                ByteBuilder builder = new ByteBuilder(safeBuffer.array);

                // Check if it is an HTTP request
                if (line.StartsWith("GET "))
                {
                    selectControl.RemoveReceiveSocket(socket);
                    uint start = BuildHttpResponseFromFirstLine(builder, line);

                    if (builder.bytes != safeBuffer.array)
                        safeBuffer.array = builder.bytes; // Update the buffer with the bigger buffer

                    if (builder.contentLength > 0)
                        socket.Send(builder.bytes, (int)start, (int)(builder.contentLength - start), SocketFlags.None);

                    selectControl.ShutdownDisposeAndRemoveReceiveSocket(socket);
                    return;
                }

                selectControl.UpdateHandler(socket, AfterFirstLineRecvHandler);
                HandleNpcLines(ref selectControl, socket, safeBuffer, builder, line);
            }
            catch (Exception e)
            {
                callback.UnhandledException(clientString, e);
                selectControl.ShutdownDisposeAndRemoveReceiveSocket(socket);
            }
        }
        public void AfterFirstLineRecvHandler(ref SelectControl selectControl, Socket socket, Buf safeBuffer)
        {
            try
            {
                if (!TryReceive(socket, safeBuffer))
                {
                    selectControl.ShutdownDisposeAndRemoveReceiveSocket(socket);
                    return;
                }

                String line = lineParser.GetLine();
                if (line == null)
                {
                    return;
                }
                
                ByteBuilder builder = new ByteBuilder(safeBuffer.array);
                HandleNpcLines(ref selectControl, socket, safeBuffer, builder, line);
            }
            catch (Exception e)
            {
                callback.UnhandledException(clientString, e);
                selectControl.ShutdownDisposeAndRemoveReceiveSocket(socket);
            }
        }
    }
    public class NpcBlockingThreadHander : NpcHandler
    {
        readonly Socket socket;
        protected SocketLineReader socketLineReader;

        public NpcBlockingThreadHander(String clientString, INpcServerCallback callback, Socket socket,
            NpcExecutor npcExecutor, INpcHtmlGenerator npcHtmlGenerator)
            : base(clientString, callback, npcExecutor, npcHtmlGenerator)
        {
            this.socket = socket;
            this.socketLineReader = new SocketLineReader(socket, Encoding.ASCII, Buf.DefaultInitialCapacity, Buf.DefaultExpandLength);
        }
        public void Run()
        {
            try
            {
                ByteBuilder builder = new ByteBuilder(2048);

                //
                // Get first line
                //
                String line = socketLineReader.ReadLine();
                if (line == null) return;

                if (line.StartsWith("GET "))
                {
                    uint start = BuildHttpResponseFromFirstLine(builder, line);
                    if (builder.contentLength > 0)
                    {
                        socket.Send(builder.bytes, (int)start, (int)(builder.contentLength - start), SocketFlags.None);
                    }

                    socket.ShutdownAndDispose();
                }
                else
                {
                    //
                    // Tcp Mode
                    //
                    do
                    {
                        if (!HandleLine(builder, line))
                        {
                            socket.ShutdownAndDispose();
                            break;
                        }
                        if (builder.contentLength > 0)
                        {
                            socket.Send(builder.bytes, (int)builder.contentLength, SocketFlags.None);
                            builder.Clear();
                        }

                        line = socketLineReader.ReadLine();
                    } while (line != null);

                    socket.ShutdownAndDispose();
                }
            }
            catch (Exception e)
            {
                callback.UnhandledException(clientString, e);
                socket.ShutdownAndDispose();
            }
        }
    }
    public class NpcHandler
    {
        protected readonly String clientString;

        protected readonly INpcServerCallback callback;
        protected readonly NpcExecutor npcExecutor;
        protected readonly INpcHtmlGenerator htmlGenerator;

        public NpcHandler(String clientString, INpcServerCallback callback, NpcExecutor npcExecutor, INpcHtmlGenerator npcHtmlGenerator)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            if (npcExecutor == null) throw new ArgumentNullException("npcExecutor");
            if (npcHtmlGenerator == null) throw new ArgumentNullException("npcHtmlGenerator");

            this.clientString = clientString;

            this.callback = callback;
            this.npcExecutor = npcExecutor;
            this.htmlGenerator = npcHtmlGenerator;
        }
        public UInt32 BuildHttpResponseFromFirstLine(ByteBuilder httpBuilder, String firstLineOfHttpRequest)
        {
            String[] httpStrings = firstLineOfHttpRequest.Split(new Char[] { ' ' }, 3);

            String resourceString = HttpUtility.UrlDecode(httpStrings[1]);

            httpBuilder.AppendAscii(httpStrings[2]);
            httpBuilder.AppendAscii(' ');

            if (resourceString.Equals("/favicon.ico"))
            {
                httpBuilder.AppendAscii("200 OK\r\nConnection: close\r\nAccess-Control-Allow-Origin: *\r\nContent-Type: text/html\r\nContent-Length: ");
                httpBuilder.AppendNumber(DefaultFavIcon.Length);
                httpBuilder.AppendAscii(Http.DoubleNewline);
                httpBuilder.AppendAscii(DefaultFavIcon);
                return 0;
            }
            else
            {
                return BuildHttpResponseFromResource(httpBuilder, resourceString);
            }
        }
        // Returns the offset of the http response in the text builder
        public UInt32 BuildHttpResponseFromResource(ByteBuilder httpBuilder, String resourceString)
        {
            // Append the headers (leave space for content length)
            httpBuilder.AppendAscii("200 OK\r\nConnection: close\r\nAccess-Control-Allow-Origin: *\r\nContent-Type: text/html\r\nContent-Length: ??????????\r\n\r\n");

            uint contentOffset = httpBuilder.contentLength;
            BuildHtmlResponse(httpBuilder, resourceString);
            uint contentLength = httpBuilder.contentLength - contentOffset;

            // Insert the content length
            String contentLengthString = contentLength.ToString();
            if (contentLengthString.Length > 10)
            {
                throw new InvalidOperationException(String.Format("CodeBug: content length {0} is too big", contentLengthString));
            }
            for (int i = 0; i < contentLengthString.Length; i++)
            {
                httpBuilder.bytes[contentOffset - 4 - contentLengthString.Length + i] = (byte)contentLengthString[i];
            }

            // Shift the headers
            UInt32 shift = 10 - (uint)contentLengthString.Length; // 10 characters were reserved for the content length
            if (shift > 0)
            {
                for (int i = 0; i <= (contentOffset - 15); i++)
                {
                    httpBuilder.bytes[contentOffset - 5 - contentLengthString.Length - i] =
                        httpBuilder.bytes[contentOffset - 5 - contentLengthString.Length - i - shift];
                }
            }

            return shift;
        }
        public void BuildHtmlResponse(ITextBuilder builder, String resourceString)
        {
            //
            // Generate HTML Headers
            //
            builder.AppendAscii("<html><head>");
            try
            {
                htmlGenerator.GenerateHtmlHeaders(builder, resourceString);
            }
            catch (Exception) { }

            //
            // Add CSS
            //
            builder.AppendAscii("<style type=\"text/css\">");
            htmlGenerator.GenerateCss(builder);
            builder.AppendAscii("</style>");

            builder.AppendAscii("</head><body><div id=\"PageDiv\">");

            //
            // Check page type
            //
            Boolean methodsPage;
            Boolean startsWithType;
            Boolean typesPage;
            Boolean call;
            if (resourceString.Equals("/") || resourceString.StartsWith("/methods"))
            {
                methodsPage = true;
                startsWithType = false;
                typesPage = false;
                call = false;
            }
            else if (resourceString.StartsWith("/type"))
            {
                methodsPage = false;
                startsWithType = true;
                typesPage = "/type".Length + 1 >= resourceString.Length;
                call = false;
            }
            else if (resourceString.StartsWith("/call"))
            {
                methodsPage = false;
                startsWithType = false;
                typesPage = false;
                call = true;
            }
            else
            {
                methodsPage = false;
                startsWithType = false;
                typesPage = false;
                call = false;
            }

            //
            // Generate Page Links
            //
            builder.AppendAscii("<div id=\"Nav\"><div id=\"NavLinkWrapper\">");

            //htmlContentBuilder.AppendFormat("<a href=\"/methods\" class=\"NavLink\"{0}>Methods</a>", methodsPage ? " id=\"CurrentNav\"" : "");
            builder.AppendAscii("<a href=\"/methods\" class=\"NavLink\"");
            if (methodsPage)
                builder.AppendAscii(" id=\"CurrentNav\"");
            builder.AppendAscii(">Methods</a>");
            //htmlContentBuilder.AppendFormat("<a href=\"/type\" class=\"NavLink\"{0}>Types</a>", typesPage ? " id=\"CurrentNav\"" : "");
            builder.AppendAscii("<a href=\"/type\" class=\"NavLink\"");
            if (typesPage)
                builder.AppendAscii(" id=\"CurrentNav\"");
            builder.AppendAscii(">Types</a>");

            builder.AppendAscii("</div></div>");
            builder.AppendAscii("<div id=\"ContentDiv\">");

            //
            // Generate HTML Body
            //
            try
            {
                UInt32 lengthBeforeBody = builder.Length;

                Boolean success;
                if (methodsPage)
                {
                    htmlGenerator.GenerateMethodsPage(builder);
                    success = true;
                }
                else if (typesPage)
                {
                    htmlGenerator.GenerateTypesPage(builder);
                    success = true;
                }
                else if (startsWithType)
                {
                    resourceString = resourceString.Substring("/type".Length + 1);
                    htmlGenerator.GenerateTypePage(builder, resourceString);
                    success = true;
                }
                else if (call)
                {
                    if ("/call".Length + 1 >= resourceString.Length)
                    {
                        throw new InvalidOperationException("no method name was supplied in the url (should be /call/&lt;method-name&gt;?&lt;parameters&gt;)");
                    }
                    resourceString = resourceString.Substring("/call".Length + 1);
                    htmlGenerator.GenerateCallPage(builder, resourceString);
                    success = true;
                }
                else
                {
                    throw new InvalidOperationException(String.Format("Unknown resource '{0}'", resourceString));
                }

                if (!success)
                {
                    String message = String.Format("Error Processing HTTP Resource '{0}'", resourceString);

                    if (builder.Length <= lengthBeforeBody) builder.AppendAscii(message);
                    callback.GotInvalidData(clientString, message);
                }
                else
                {
                    callback.FunctionCall(clientString, String.Format("HTTP '{0}'", resourceString));
                }
            }
            catch (Exception e)
            {
                htmlGenerator.GenerateExceptionHtml(builder, e);
                callback.ExceptionWhileGeneratingHtml(clientString, e);
            }
            builder.AppendAscii("</div></div></body></html>");
        }


        // Returns false if the npc stream is done
        public Boolean HandleLine(ITextBuilder responseBuilder, String line)
        {
            String commandArguments;
            String command = line.Peel(out commandArguments);

            if (String.IsNullOrEmpty(command))
            {
                AppendProtocolHelp(responseBuilder);
            }
            else
            {
                if (commandArguments != null) commandArguments = commandArguments.Trim();

                if (command[0] != ':')
                {
                    CallCommandHandler(responseBuilder, command, commandArguments);
                }
                else if (command.Equals(":call", StringComparison.OrdinalIgnoreCase))
                {
                    CallCommandHandler(responseBuilder, commandArguments);
                }
                else if (command.Equals(":methods", StringComparison.OrdinalIgnoreCase))
                {
                    MethodsCommandHandler(responseBuilder, commandArguments);
                }
                else if (command.Equals(":type", StringComparison.OrdinalIgnoreCase))
                {
                    TypeCommandHandler(responseBuilder, commandArguments);
                }
                else if (command.Equals(":interface", StringComparison.OrdinalIgnoreCase))
                {
                    InterfaceCommandHandler(responseBuilder);
                }
                else if (command.Equals(":help", StringComparison.OrdinalIgnoreCase) || command.Equals("", StringComparison.OrdinalIgnoreCase))
                {
                    AppendProtocolHelp(responseBuilder);
                }
                else if (command.Equals(":exit", StringComparison.OrdinalIgnoreCase))
                {
                    return false; // Close the stream
                }
                else
                {
                    callback.GotInvalidData(clientString, String.Format("Unknown Command from line '{0}'", line));
                    responseBuilder.AppendAscii("Unknown command '");
                    responseBuilder.AppendAscii(command);
                    responseBuilder.AppendAscii("'\n");
                    AppendProtocolHelp(responseBuilder);
                }
            }
            return true; // Stay connected
        }
        static void AppendNpcError(ITextBuilder responseBuilder, NpcErrorCode errorCode, String errorMessage)
        {
            responseBuilder.AppendAscii(NpcReturnObject.NpcReturnLineNpcErrorPrefix);
            responseBuilder.AppendNumber((byte)errorCode);
            responseBuilder.AppendAscii(' ');
            responseBuilder.AppendAscii(errorCode.ToString());
            responseBuilder.AppendAscii(errorMessage.Replace("\n", "\\n"));
            responseBuilder.AppendAscii('\n');
        }
        static void AppendProtocolHelp(ITextBuilder responseBuilder)
        {
            responseBuilder.AppendAscii("Npc Protocol:\n");
            responseBuilder.AppendAscii("   method-name [args...]       Call method-name with the given arguments\n");
            responseBuilder.AppendAscii("   :methods [object] [verbose] Print all objects and their methods or just the given objects methods\n");
            responseBuilder.AppendAscii("   :type [type]                No argument: print all types, 1 argument: print given type information\n");
            responseBuilder.AppendAscii("   :interface                  Print all interfaces then the objects\n");
            responseBuilder.AppendAscii("   :exit                       Exit\n");
            responseBuilder.AppendAscii("   :help                       Show this help\n");
        }
        public void CallCommandHandler(ITextBuilder responseBuilder, String call)
        {
            //
            // This method always returns a specifically formatted string.
            //    1. On Success   "Success <ReturnType> [<ReturnValue>]"
            //    3. On Exception "Exception <ExceptionMessage> <ExceptionType> <SerializedException>
            //    2. On NpcError  "NpcError <ErrorCode> <ErrorMessage>
            //
            if (String.IsNullOrEmpty(call))
            {
                AppendNpcError(responseBuilder, NpcErrorCode.InvalidCallSyntax, "missing method name");
                return;
            }

            //
            // Get method name
            //
            String methodName;
            String parametersString;
            methodName = call.Peel(out parametersString);

            CallCommandHandler(responseBuilder, methodName, parametersString);
        }
        public void CallCommandHandler(ITextBuilder responseBuilder, String methodName, String arguments)
        {
            //
            // Parse Parameters
            //
            List<String> parametersList = new List<String>();
            try
            {
                Npc.ParseParameters(arguments, parametersList);
            }
            catch (FormatException e)
            {
                callback.GotInvalidData(clientString, e.Message);
                AppendNpcError(responseBuilder, NpcErrorCode.InvalidCallParameters, e.Message);
                return;
            }

            String[] parameters = (parametersList == null) ? null : parametersList.ToArray();
            try
            {
                NpcReturnObjectOrException returnObject = npcExecutor.ExecuteWithStrings(methodName, parameters);
                if (returnObject.exception != null)
                {
                    callback.FunctionCallThrewException(clientString, methodName, returnObject.exception);
                }
                else
                {
                    callback.FunctionCall(clientString, methodName);
                }

                returnObject.AppendNpcReturnLine(responseBuilder);
                return;
            }
            catch (NpcErrorException ne)
            {
                callback.ExceptionDuringExecution(clientString, methodName, ne);
                responseBuilder.Clear();
                AppendNpcError(responseBuilder, ne.errorCode, ne.Message);
            }
            catch (Exception e)
            {
                callback.ExceptionDuringExecution(clientString, methodName, e);
                responseBuilder.Clear();
                AppendNpcError(responseBuilder, NpcErrorCode.UnhandledException, e.GetType().Name + ": " + e.Message);
            }
        }
        static void AddVerboseObjectMethods(ITextBuilder builder, NpcExecutionObject executionObject)
        {
            for (int interfaceIndex = 0; interfaceIndex < executionObject.ancestorNpcInterfaces.Count; interfaceIndex++)
            {
                NpcInterfaceInfo npcInterfaceInfo = executionObject.ancestorNpcInterfaces[interfaceIndex];
                for (int methodIndex = 0; methodIndex < npcInterfaceInfo.npcMethods.Length; methodIndex++)
                {
                    NpcMethodInfo npcMethodInfo = npcInterfaceInfo.npcMethods[methodIndex];
#if WindowsCE
                    builder.AppendAscii(npcMethodInfo.methodInfo.ReturnType.SosTypeName());
#else
                    builder.AppendAscii(npcMethodInfo.methodInfo.ReturnParameter.ParameterType.SosTypeName());
#endif
                    builder.AppendAscii(' ');
                    builder.AppendAscii(npcMethodInfo.methodName);

                    builder.AppendAscii('(');

                    ParameterInfo[] parameters = npcMethodInfo.parameters;
                    for (UInt16 j = 0; j < npcMethodInfo.parametersLength; j++)
                    {
                        ParameterInfo parameterInfo = parameters[j];
                        if (j > 0) builder.AppendAscii(',');
                        builder.AppendAscii(parameterInfo.ParameterType.SosTypeName());
                        builder.AppendAscii(' ');
                        builder.AppendAscii(parameterInfo.Name);
                    }
                    builder.AppendAscii(")\n");
                }
            }
        }
        static void AddShortObjectMethods(ITextBuilder builder, NpcExecutionObject executionObject)
        {
            for (int interfaceIndex = 0; interfaceIndex < executionObject.ancestorNpcInterfaces.Count; interfaceIndex++)
            {
                NpcInterfaceInfo npcInterfaceInfo = executionObject.ancestorNpcInterfaces[interfaceIndex];
                for (int methodIndex = 0; methodIndex < npcInterfaceInfo.npcMethods.Length; methodIndex++)
                {
                    NpcMethodInfo npcMethodInfo = npcInterfaceInfo.npcMethods[methodIndex];
                    builder.AppendAscii(npcMethodInfo.methodName);

                    ParameterInfo[] parameters = npcMethodInfo.parameters;
                    for (UInt16 argIndex = 0; argIndex < npcMethodInfo.parametersLength; argIndex++)
                    {
                        ParameterInfo parameterInfo = parameters[argIndex];
                        builder.AppendAscii(' ');
                        builder.AppendAscii(parameterInfo.ParameterType.SosShortTypeName());
                        builder.AppendAscii(':');
                        builder.AppendAscii(parameterInfo.Name);
                    }

                    builder.AppendAscii(" returns ");
#if WindowsCE
                    builder.AppendAscii(npcMethodInfo.methodInfo.ReturnType.SosTypeName());
#else
                    builder.AppendAscii(npcMethodInfo.methodInfo.ReturnParameter.ParameterType.SosShortTypeName());
#endif
                    builder.AppendAscii("\n");
                }
            }
        }
        void MethodsCommandHandler(ITextBuilder responseBuilder, String args)
        {
            String originalArgs = args;
            if(String.IsNullOrEmpty(args))
            {
                args = null;
            }

            //
            // Parse Arguments
            //
            Boolean verbose = false;
            String objectName = null;
            while (args != null)
            {
                String arg = args.Peel(out args);
                if (arg.Equals("verbose", StringComparison.OrdinalIgnoreCase))
                {
                    verbose = true;
                }
                else
                {
                    if (objectName != null)
                    {
                        //String.Format("Invalid arguments '{0}'\n", originalArgs);
                        responseBuilder.AppendAscii("Invalid arguments '");
                        responseBuilder.AppendAscii(originalArgs);
                        responseBuilder.AppendAscii("'\n");
                        return;
                    }
                    objectName = arg;
                }
            }

            //
            // Build Response
            //
            if (objectName == null)
            {
                if (verbose)
                {
                    foreach (NpcExecutionObject executionObject in npcExecutor.ExecutionObjects)
                    {
                        responseBuilder.AppendAscii(executionObject.objectName);
                        responseBuilder.AppendAscii('\n');
                        AddVerboseObjectMethods(responseBuilder, executionObject);
                        responseBuilder.AppendAscii("\n"); // Indicates end of object methods
                    }
                    responseBuilder.AppendAscii("\n"); // Indicates end of all objects
                }
                else
                {
                    foreach (NpcExecutionObject executionObject in npcExecutor.ExecutionObjects)
                    {
                        responseBuilder.AppendAscii(executionObject.objectName);
                        responseBuilder.AppendAscii('\n');
                        AddShortObjectMethods(responseBuilder, executionObject);
                        responseBuilder.AppendAscii("\n"); // Indicates end of object methods
                    }
                    responseBuilder.AppendAscii("\n"); // Indicates end of all objects
                }
            }
            else
            {
                Boolean foundObject = false;
                foreach (NpcExecutionObject executionObject in npcExecutor.ExecutionObjects)
                {
                    if(executionObject.objectName.Equals(objectName, StringComparison.OrdinalIgnoreCase))
                    {
                        if(verbose)
                        {
                            AddVerboseObjectMethods(responseBuilder, executionObject);
                        }
                        else
                        {
                            AddShortObjectMethods(responseBuilder, executionObject);
                        }
                        responseBuilder.AppendAscii("\n"); // Indicates end of object methods
                        foundObject = true;
                        break;
                    }
                }
                if(!foundObject)
                {
                    responseBuilder.AppendAscii("Error: Could not find object '");
                    responseBuilder.AppendAscii(objectName);
                    responseBuilder.AppendAscii("'\n");
                }
            }
        }
        void InterfaceCommandHandler(ITextBuilder responseBuilder)
        {
            //
            // Add Interface Definitions
            //
            foreach (NpcInterfaceInfo interfaceInfo in npcExecutor.Interfaces)
            {
                responseBuilder.AppendAscii(interfaceInfo.name);
                for (int i = 0; i < interfaceInfo.parentNpcInterfaces.Count; i++)
                {
                    NpcInterfaceInfo parentNpcInterface = interfaceInfo.parentNpcInterfaces[i];
                    responseBuilder.AppendAscii(' ');
                    responseBuilder.AppendAscii(parentNpcInterface.name);
                }
                responseBuilder.AppendAscii('\n');

                for (int i = 0; i < interfaceInfo.npcMethods.Length; i++)
                {
                    NpcMethodInfo npcMethodInfo = interfaceInfo.npcMethods[i];
#if WindowsCE
                    responseBuilder.Append(npcMethodInfo.methodInfo.ReturnType.SosTypeName());
#else
                    responseBuilder.AppendAscii(npcMethodInfo.methodInfo.ReturnParameter.ParameterType.SosTypeName());
#endif
                    responseBuilder.AppendAscii(' ');
                    responseBuilder.AppendAscii(npcMethodInfo.methodName);

                    responseBuilder.AppendAscii('(');

                    ParameterInfo[] parameters = npcMethodInfo.parameters;
                    for (UInt16 j = 0; j < npcMethodInfo.parametersLength; j++)
                    {
                        ParameterInfo parameterInfo = parameters[j];
                        if (j > 0) responseBuilder.AppendAscii(',');
                        responseBuilder.AppendAscii(parameterInfo.ParameterType.SosTypeName());
                        responseBuilder.AppendAscii(' ');
                        responseBuilder.AppendAscii(parameterInfo.Name);
                    }
                    responseBuilder.AppendAscii(")\n");
                }
                responseBuilder.AppendAscii('\n');
            }
            responseBuilder.AppendAscii('\n'); // Add blank line to end (to mark end of interfaces)

            //
            // Add Object Names and their Interfaces
            //
            foreach (NpcExecutionObject executionObject in npcExecutor.ExecutionObjects)
            {
                responseBuilder.AppendAscii(executionObject.objectName);
                for (int i = 0; i < executionObject.parentNpcInterfaces.Count; i++)
                {
                    NpcInterfaceInfo npcInterface = executionObject.parentNpcInterfaces[i];
                    responseBuilder.AppendAscii(' ');
                    responseBuilder.AppendAscii(npcInterface.name);
                }
                responseBuilder.AppendAscii('\n');
            }
            responseBuilder.AppendAscii('\n'); // Add blank line to end (to mark end of objects
        }
        void TypeCommandHandler(ITextBuilder responseBuilder, String arguments)
        {
            if (arguments == null || arguments.Length <= 0)
            {
                foreach (KeyValuePair<String,Type> pair in npcExecutor.EnumAndObjectTypes)
                {
                    Type type = pair.Value;
                    responseBuilder.AppendAscii(type.SosTypeName());
                    responseBuilder.AppendAscii(' ');
                    responseBuilder.AppendAscii(type.SosTypeDefinition());
                    responseBuilder.AppendAscii('\n');
                }
                responseBuilder.AppendAscii('\n');
                return;
            }

            String requestedTypeString = arguments.Trim();

            if (requestedTypeString.IsSosPrimitive())
            {
                responseBuilder.AppendAscii(requestedTypeString);
                responseBuilder.AppendAscii(" primitive type\n");
                return;
            }

            Type enumOrObjectType;
            if(npcExecutor.EnumAndObjectTypes.TryGetValue(requestedTypeString, out enumOrObjectType))
            {
                responseBuilder.AppendAscii(enumOrObjectType.SosTypeName());
                responseBuilder.AppendAscii(' ');
                responseBuilder.AppendAscii(enumOrObjectType.SosTypeDefinition());
                responseBuilder.AppendAscii('\n');
                return;
            }
            OneOrMoreTypes oneOrMoreEnumOrObjectType;
            if (npcExecutor.EnumAndObjectShortNameTypes.TryGetValue(requestedTypeString, out oneOrMoreEnumOrObjectType))
            {
                enumOrObjectType = oneOrMoreEnumOrObjectType.firstType;
                if (oneOrMoreEnumOrObjectType.otherTypes == null)
                {
                    responseBuilder.AppendAscii(enumOrObjectType.SosTypeName());
                    responseBuilder.AppendAscii(' ');
                    responseBuilder.AppendAscii(enumOrObjectType.SosTypeDefinition());
                    responseBuilder.AppendAscii('\n');
                    return;
                }
                else
                {
                    responseBuilder.AppendAscii("Error: '");
                    responseBuilder.AppendAscii(requestedTypeString);
                    responseBuilder.AppendAscii("' is ambiguous, include namespace to find the correct type");
                    return;
                }
            }

            responseBuilder.AppendAscii(requestedTypeString);
            responseBuilder.AppendAscii(" unknown type\n");
        }

        // To load a new favicon, use the ConvertFavIconToByteArrayCode function in the ManualTests.cs file
        public static readonly Byte[] DefaultFavIcon = new Byte[] {
            0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x10, 0x10, 0x00, 0x00, 0x01, 0x00, 0x20, 0x00, 0x68, 0x04,
            0x00, 0x00, 0x16, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x20, 0x00,
            0x00, 0x00, 0x01, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA1, 0x69, 0x2D, 0x14, 0xA1, 0x69, 0x2D, 0xFF, 0xA1, 0x69,
            0x2D, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA1, 0x69, 0x2D, 0x58, 0xA1, 0x69,
            0x2D, 0xFF, 0xA1, 0x69, 0x2D, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA1, 0x69, 0x2D, 0xFF, 0xA1, 0x69, 0x2D, 0x58, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA1, 0x69,
            0x2D, 0x58, 0xA1, 0x69, 0x2D, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0xA1, 0x69, 0x2D, 0xFF, 0xA1, 0x69, 0x2D, 0xFF, 0xA1, 0x69, 0x2D, 0xFF, 0xA1, 0x69,
            0x2D, 0xFF, 0xA1, 0x69, 0x2D, 0xFF, 0xA1, 0x69, 0x2D, 0xFF, 0xA1, 0x69, 0x2D, 0xFF, 0xA1, 0x69,
            0x2D, 0xFF, 0xA1, 0x69, 0x2D, 0xFF, 0xA1, 0x69, 0x2D, 0xFF, 0xA1, 0x69, 0x2D, 0xFF, 0xA1, 0x69,
            0x2D, 0xFF, 0xA1, 0x69, 0x2D, 0xFF, 0xA1, 0x69, 0x2D, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xA1, 0x69,
            0x2D, 0x58, 0xA1, 0x69, 0x2D, 0x58, 0xA1, 0x69, 0x2D, 0xFF, 0xA1, 0x69, 0x2D, 0x58, 0xA1, 0x69,
            0x2D, 0x58, 0xA1, 0x69, 0x2D, 0x58, 0xA1, 0x69, 0x2D, 0x58, 0xA1, 0x69, 0x2D, 0x58, 0xA1, 0x69,
            0x2D, 0x58, 0xA1, 0x69, 0x2D, 0x58, 0xA1, 0x69, 0x2D, 0x58, 0xA1, 0x69, 0x2D, 0x58, 0xA1, 0x69,
            0x2D, 0x58, 0xA1, 0x69, 0x2D, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0xA1, 0x69, 0x2D, 0x58, 0xA1, 0x69, 0x2D, 0x58, 0xA1, 0x69, 0x2D, 0xFF, 0xA1, 0x69,
            0x2D, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA1, 0x69, 0x2D, 0x58, 0xA1, 0x69,
            0x2D, 0xFF, 0xA1, 0x69, 0x2D, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0xA1, 0x69, 0x2D, 0x58, 0x00, 0x00, 0x00, 0x16, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA1, 0x69, 0x2D, 0x58, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0x33, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00,
            0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00,
            0x00, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00,
            0x00, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00,
            0x00, 0x33, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00,
            0x00, 0x33, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0x00, 0xEF, 0xF7,
            0x00, 0x00, 0xDF, 0xFB, 0x00, 0x00, 0x80, 0x01, 0x00, 0x00, 0xDF, 0xFB, 0x00, 0x00, 0xEF, 0xF7,
            0x00, 0x00, 0x32, 0x70, 0x00, 0x00, 0x32, 0x70, 0x00, 0x00, 0x22, 0x73, 0x00, 0x00, 0x02, 0x73,
            0x00, 0x00, 0x02, 0x73, 0x00, 0x00, 0x02, 0x13, 0x00, 0x00, 0x02, 0x13, 0x00, 0x00, 0x12, 0xD3,
            0x00, 0x00, 0x32, 0x10, 0x00, 0x00, 0x32, 0x10, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00,
        };
    }
}
