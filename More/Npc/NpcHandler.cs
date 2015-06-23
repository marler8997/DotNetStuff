using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;

using More;

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
    public class NpcDataHandler : NpcHandler
    {
        readonly LineParser lineParser;
        Boolean atFirstLine;
        Boolean done;
        public Boolean Done { get { return done; } }

        public NpcDataHandler(String clientString, INpcServerCallback callback, IDataHandler responseHandler, NpcExecutor npcExecutor, INpcHtmlGenerator npcHtmlGenerator)
            : base(clientString, callback, responseHandler, npcExecutor, npcHtmlGenerator)
        {
            this.lineParser = new LineParser(Encoding.ASCII, Buf.DefaultInitialCapacity, Buf.DefaultExpandLength);
            this.atFirstLine = true;
            this.done = false;
        }
        public void Handle(Byte[] buffer, UInt32 bytesRead)
        {
            Handle(buffer, 0, bytesRead);
        }
        public void Handle(Byte[] buffer, UInt32 offset, UInt32 bytesRead)
        {
            if (done == true) return;

            lineParser.Add(buffer, offset, bytesRead);

            String line = lineParser.GetLine();
            if(line == null) return;

            if (atFirstLine)
            {
                atFirstLine = false;
                if (line.StartsWith("GET"))
                {
                    HandleHttpRequest(line);
                    done = true;
                    return;
                }  
            }

            do
            {

                HandleLine(line);
                line = lineParser.GetLine();

            } while(line != null);
        }
    }
    public class NpcBlockingThreadHander : NpcHandler
    {
        protected SocketLineReader socketLineReader;

        public NpcBlockingThreadHander(String clientString, INpcServerCallback callback, Socket socket,
            NpcExecutor npcExecutor, INpcHtmlGenerator npcHtmlGenerator)
            : base(clientString, callback, new SocketSendDataHandler(socket), npcExecutor, npcHtmlGenerator)
        {
            this.socketLineReader = new SocketLineReader(socket, Encoding.ASCII, Buf.DefaultInitialCapacity, Buf.DefaultExpandLength);
        }
        public void Run()
        {
            try
            {
                //
                // Get first line
                //
                String line = socketLineReader.ReadLine();
                if (line == null) return;

                if (line.StartsWith("GET"))
                {
                    HandleHttpRequest(line);
                }
                else
                {
                    //
                    // Tcp Mode
                    //
                    while (true)
                    {
                        HandleLine(line);

                        line = socketLineReader.ReadLine();
                        if (line == null) return;
                    }
                }
            }
            catch (Exception e)
            {
                callback.UnhandledException(clientString, e);
            }
            finally
            {
                if (socketLineReader.socket.Connected)
                {
                    Thread.Sleep(500);
                    Dispose();
                }
            }
        }
    }
    public class NpcHandler : IDisposable
    {
        protected readonly String clientString;
        protected readonly INpcServerCallback callback;
        protected readonly IDataHandler responseHandler;
        protected readonly NpcExecutor npcExecutor;
        protected readonly INpcHtmlGenerator npcHtmlGenerator;

        public NpcHandler(String clientString, INpcServerCallback callback, IDataHandler responseHandler, NpcExecutor npcExecutor, INpcHtmlGenerator npcHtmlGenerator)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            if (responseHandler == null) throw new ArgumentNullException("responseHandler");
            if (npcExecutor == null) throw new ArgumentNullException("npcExecutor");
            if (npcHtmlGenerator == null) throw new ArgumentNullException("npcHtmlGenerator");

            this.clientString = clientString;
            this.callback = callback;
            this.responseHandler = responseHandler;
            this.npcExecutor = npcExecutor;
            this.npcHtmlGenerator = npcHtmlGenerator;
        }
        public void Dispose()
        {
            responseHandler.Dispose();
        }
        protected void HandleHttpRequest(String firstLineOfHttpRequest)
        {
            //
            // HTTP MODE
            //
            String[] httpStrings = firstLineOfHttpRequest.Split(new Char[] { ' ' }, 3);

            String resourceString = HttpUtility.UrlDecode(httpStrings[1]);
            String httpVersionString = httpStrings[2];

            if (resourceString.Equals("/favicon.ico"))
            {
                Byte[] response404 = Encoding.UTF8.GetBytes(String.Format("{0} 404 Not Found\r\n\r\n", httpVersionString));
                responseHandler.HandleData(response404, 0, (UInt32)response404.Length);
                responseHandler.Dispose();
                return;
            }

            StringBuilder htmlContentBuilder = new StringBuilder();

            //
            // Generate HTML Headers
            //
            htmlContentBuilder.Append("<html><head>");
            try
            {
                npcHtmlGenerator.GenerateHtmlHeaders(htmlContentBuilder, resourceString);
            }
            catch (Exception) { }

            //
            // Add CSS
            //
            htmlContentBuilder.Append("<style type=\"text/css\">");
            npcHtmlGenerator.GenerateCss(htmlContentBuilder);
            htmlContentBuilder.Append("</style>");

            htmlContentBuilder.Append("</head><body><div id=\"PageDiv\">");

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
            htmlContentBuilder.Append("<div id=\"Nav\"><div id=\"NavLinkWrapper\">");

            htmlContentBuilder.Append(String.Format("<a href=\"/methods\" class=\"NavLink\"{0}>Methods</a>", methodsPage ? " id=\"CurrentNav\"" : ""));
            htmlContentBuilder.Append(String.Format("<a href=\"/type\" class=\"NavLink\"{0}>Types</a>", typesPage ? " id=\"CurrentNav\"" : ""));

            htmlContentBuilder.Append("</div></div>");
            htmlContentBuilder.Append("<div id=\"ContentDiv\">");

            //
            // Generate HTML Body
            //
            try
            {
                Int32 lengthBeforeBody = htmlContentBuilder.Length;

                Boolean success;
                if (methodsPage)
                {
                    npcHtmlGenerator.GenerateMethodsPage(htmlContentBuilder);
                    success = true;
                }
                else if (typesPage)
                {
                    npcHtmlGenerator.GenerateTypesPage(htmlContentBuilder);
                    success = true;
                }
                else if (startsWithType)
                {
                    resourceString = resourceString.Substring("/type".Length + 1);
                    npcHtmlGenerator.GenerateTypePage(htmlContentBuilder, resourceString);
                    success = true;
                }
                else if (call)
                {
                    if ("/call".Length + 1 >= resourceString.Length)
                    {
                        throw new InvalidOperationException("no method name was supplied in the url (should be /call/&lt;method-name&gt;?&lt;parameters&gt;)");
                    }
                    resourceString = resourceString.Substring("/call".Length + 1);
                    npcHtmlGenerator.GenerateCallPage(htmlContentBuilder, resourceString);
                    success = true;
                }
                else
                {
                    throw new InvalidOperationException(String.Format("Unknown resource '{0}'", resourceString));
                }

                if (!success)
                {
                    String message = String.Format("Error Processing HTTP Resource '{0}'", resourceString);

                    if (htmlContentBuilder.Length <= lengthBeforeBody) htmlContentBuilder.Append(message);
                    callback.GotInvalidData(clientString, message);
                }
                else
                {
                    callback.FunctionCall(clientString, String.Format("HTTP '{0}'", resourceString));
                }
            }
            catch (Exception e)
            {
                npcHtmlGenerator.GenerateExceptionHtml(htmlContentBuilder, e);
                callback.ExceptionWhileGeneratingHtml(clientString, e);
            }
            htmlContentBuilder.Append("</div></div></body></html>");

            //
            // Generate HTTP Headers
            //
            Byte[] contents = Encoding.UTF8.GetBytes(htmlContentBuilder.ToString());

            StringBuilder httpHeadersBuilder = new StringBuilder();
            httpHeadersBuilder.Append(httpVersionString);
            httpHeadersBuilder.Append(" 200 OK\r\nConnection: close\r\nAccess-Control-Allow-Origin: *\r\nContent-Type: text/html\r\n");
            httpHeadersBuilder.Append(String.Format("Content-Length: {0}\r\n", contents.Length));
            httpHeadersBuilder.Append("\r\n");
            Byte[] headerBytes = Encoding.UTF8.GetBytes(httpHeadersBuilder.ToString());

            //
            // Send Response
            //
            responseHandler.HandleData(headerBytes, 0, (UInt32)headerBytes.Length);
            responseHandler.HandleData(contents, 0, (UInt32)contents.Length);

            return;
        }
        public void HandleLine(String line)
        {
            Byte[] response = null;

            String commandArguments;
            String command = line.Peel(out commandArguments);

            if (String.IsNullOrEmpty(command))
            {
                response = GenerateHelpMessage(null);
            }
            else
            {
                Boolean isColonCommand;
                if (command[0] == ':')
                {
                    isColonCommand = true;
                    command = command.Substring(1);
                }
                else
                {
                    isColonCommand = false;
                }
                if (commandArguments != null) commandArguments = commandArguments.Trim();

                if (command.Equals("call", StringComparison.OrdinalIgnoreCase))
                {
                    response = CallCommandHandler(commandArguments);
                }
                else if (command.Equals("methods", StringComparison.OrdinalIgnoreCase))
                {
                    response = MethodsCommandHandler(commandArguments);
                }
                else if (command.Equals("type", StringComparison.OrdinalIgnoreCase))
                {
                    response = TypeCommandHandler(commandArguments);
                }
                else if (command.Equals("interface", StringComparison.OrdinalIgnoreCase))
                {
                    response = InterfaceCommandHandler();
                }
                else if (command.Equals("help", StringComparison.OrdinalIgnoreCase) || command.Equals("", StringComparison.OrdinalIgnoreCase))
                {
                    response = GenerateHelpMessage(null);
                }
                else if (command.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    responseHandler.Dispose();
                    return;
                }
                else
                {
                    if (isColonCommand)
                    {
                        callback.GotInvalidData(clientString, String.Format("Unknown Command from line '{0}'", line));
                        response = GenerateHelpMessage(String.Format("Unknown Command '{0}', expected 'help', 'exit', 'methods', 'type' or 'call'", command));
                    }
                    else
                    {
                        response = CallCommandHandler(command, commandArguments);
                    }
                }
            }
            if (response != null) responseHandler.HandleData(response, 0, (UInt32)response.Length);
        }
        private String NpcError(NpcErrorCode errorCode, String errorMessage)
        {
            return String.Format("{0}{1} {2}: {3}\n", NpcReturnObject.NpcReturnLineNpcErrorPrefix,
                (Int32)errorCode, errorCode, errorMessage.Replace("\n", "\\n"));
        }
        private Byte[] GenerateHelpMessage(String prefixMessage)
        {
            StringBuilder builder = new StringBuilder();
            if (prefixMessage != null)
            {
                builder.Append(prefixMessage);
                builder.Append('\n');
            }
            builder.Append("Npc Protocol:\n");
            builder.Append("   method-name [args...]       Call method-name with the given arguments\n");
            builder.Append("   :methods [object] [verbose] Print all objects and their methods or just the given objects methods\n");
            builder.Append("   :type [type]                No argument: print all types, 1 argument: print given type information\n");
            builder.Append("   :interface                  Print all interfaces then the objects\n");
            builder.Append("   :exit                       Exit\n");
            builder.Append("   :help                       Show this help\n");
            return Encoding.UTF8.GetBytes(builder.ToString());
        }
        Byte[] CallCommandHandler(String call)
        {
            //
            // This method always returns a specifically formatted string.
            //    1. On Success   "Success <ReturnType> [<ReturnValue>]"
            //    3. On Exception "Exception <ExceptionMessage> <ExceptionType> <SerializedException>
            //    2. On NpcError  "NpcError <ErrorCode> <ErrorMessage>
            //
            if (String.IsNullOrEmpty(call))
                return Encoding.ASCII.GetBytes(NpcError(NpcErrorCode.InvalidCallSyntax, "missing method name"));

            //
            // Get method name
            //
            String methodName;
            String parametersString;
            methodName = call.Peel(out parametersString);

            return CallCommandHandler(methodName, parametersString);
        }
        Byte[] CallCommandHandler(String methodName, String arguments)
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
                String message = NpcError(NpcErrorCode.InvalidCallParameters, e.Message);
                callback.GotInvalidData(clientString, message);
                return Encoding.ASCII.GetBytes(message);
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

                return Encoding.ASCII.GetBytes(returnObject.ToNpcReturnLineString());
            }
            catch (NpcErrorException ne)
            {
                callback.ExceptionDuringExecution(clientString, methodName, ne);
                return Encoding.ASCII.GetBytes(NpcError(ne.errorCode, ne.Message));
            }
            catch (Exception e)
            {
                callback.ExceptionDuringExecution(clientString, methodName, e);
                return Encoding.ASCII.GetBytes(NpcError(NpcErrorCode.UnhandledException, e.GetType().Name + ": " + e.Message));
            }
        }
        void AddVerboseObjectMethods(StringBuilder builder, NpcExecutionObject executionObject)
        {
            for (int interfaceIndex = 0; interfaceIndex < executionObject.ancestorNpcInterfaces.Count; interfaceIndex++)
            {
                NpcInterfaceInfo npcInterfaceInfo = executionObject.ancestorNpcInterfaces[interfaceIndex];
                for (int methodIndex = 0; methodIndex < npcInterfaceInfo.npcMethods.Length; methodIndex++)
                {
                    NpcMethodInfo npcMethodInfo = npcInterfaceInfo.npcMethods[methodIndex];
#if WindowsCE
                    listBuilder.Append(npcMethodInfo.methodInfo.ReturnType.SosTypeName());
#else
                    builder.Append(npcMethodInfo.methodInfo.ReturnParameter.ParameterType.SosTypeName());
#endif
                    builder.Append(' ');
                    builder.Append(npcMethodInfo.methodName);

                    builder.Append('(');

                    ParameterInfo[] parameters = npcMethodInfo.parameters;
                    for (UInt16 j = 0; j < npcMethodInfo.parametersLength; j++)
                    {
                        ParameterInfo parameterInfo = parameters[j];
                        if (j > 0) builder.Append(',');
                        builder.Append(parameterInfo.ParameterType.SosTypeName());
                        builder.Append(' ');
                        builder.Append(parameterInfo.Name);
                    }
                    builder.Append(")\n");
                }
            }
        }
        void AddShortObjectMethods(StringBuilder builder, NpcExecutionObject executionObject)
        {
            for (int interfaceIndex = 0; interfaceIndex < executionObject.ancestorNpcInterfaces.Count; interfaceIndex++)
            {
                NpcInterfaceInfo npcInterfaceInfo = executionObject.ancestorNpcInterfaces[interfaceIndex];
                for (int methodIndex = 0; methodIndex < npcInterfaceInfo.npcMethods.Length; methodIndex++)
                {
                    NpcMethodInfo npcMethodInfo = npcInterfaceInfo.npcMethods[methodIndex];
                    builder.Append(npcMethodInfo.methodName);

                    ParameterInfo[] parameters = npcMethodInfo.parameters;
                    for (UInt16 argIndex = 0; argIndex < npcMethodInfo.parametersLength; argIndex++)
                    {
                        ParameterInfo parameterInfo = parameters[argIndex];
                        builder.Append(' ');
                        builder.Append(parameterInfo.ParameterType.SosShortTypeName());
                        builder.Append(':');
                        builder.Append(parameterInfo.Name);
                    }

                    builder.Append(" returns ");
#if WindowsCE
                    listBuilder.Append(npcMethodInfo.methodInfo.ReturnType.SosTypeName());
#else
                    builder.Append(npcMethodInfo.methodInfo.ReturnParameter.ParameterType.SosShortTypeName());
#endif
                    builder.Append("\n");
                }
            }
        }
        private Byte[] MethodsCommandHandler(String args)
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
                        return Encoding.ASCII.GetBytes(String.Format("Invalid arguments '{0}'\n", originalArgs));
                    }
                    objectName = arg;
                }
            }

            //
            // Build Response
            //
            StringBuilder listBuilder = new StringBuilder();

            if (objectName == null)
            {
                if (verbose)
                {
                    foreach (NpcExecutionObject executionObject in npcExecutor.ExecutionObjects)
                    {
                        listBuilder.Append(executionObject.objectName);
                        listBuilder.Append('\n');
                        AddVerboseObjectMethods(listBuilder, executionObject);
                        listBuilder.Append("\n"); // Indicates end of object methods
                    }
                    listBuilder.Append("\n"); // Indicates end of all objects
                }
                else
                {
                    foreach (NpcExecutionObject executionObject in npcExecutor.ExecutionObjects)
                    {
                        listBuilder.Append(executionObject.objectName);
                        listBuilder.Append('\n');
                        AddShortObjectMethods(listBuilder, executionObject);
                        listBuilder.Append("\n"); // Indicates end of object methods
                    }
                    listBuilder.Append("\n"); // Indicates end of all objects
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
                            AddVerboseObjectMethods(listBuilder, executionObject);
                        }
                        else
                        {
                            AddShortObjectMethods(listBuilder, executionObject);
                        }
                        listBuilder.Append("\n"); // Indicates end of object methods
                        foundObject = true;
                        break;
                    }
                }
                if(!foundObject)
                {
                    listBuilder.Append("Error: Could not find object '");
                    listBuilder.Append(objectName);
                    listBuilder.Append("'\n");
                }
            }
            return Encoding.ASCII.GetBytes(listBuilder.ToString());
        }
        private Byte[] InterfaceCommandHandler()
        {
            StringBuilder listBuilder = new StringBuilder();

            //
            // Add Interface Definitions
            //
            foreach (NpcInterfaceInfo interfaceInfo in npcExecutor.Interfaces)
            {
                listBuilder.Append(interfaceInfo.name);
                for (int i = 0; i < interfaceInfo.parentNpcInterfaces.Count; i++)
                {
                    NpcInterfaceInfo parentNpcInterface = interfaceInfo.parentNpcInterfaces[i];
                    listBuilder.Append(' ');
                    listBuilder.Append(parentNpcInterface.name);
                }
                listBuilder.Append('\n');

                for (int i = 0; i < interfaceInfo.npcMethods.Length; i++)
                {
                    NpcMethodInfo npcMethodInfo = interfaceInfo.npcMethods[i];
#if WindowsCE
                    listBuilder.Append(npcMethodInfo.methodInfo.ReturnType.SosTypeName());
#else
                    listBuilder.Append(npcMethodInfo.methodInfo.ReturnParameter.ParameterType.SosTypeName());
#endif
                    listBuilder.Append(' ');
                    listBuilder.Append(npcMethodInfo.methodName);

                    listBuilder.Append('(');

                    ParameterInfo[] parameters = npcMethodInfo.parameters;
                    for (UInt16 j = 0; j < npcMethodInfo.parametersLength; j++)
                    {
                        ParameterInfo parameterInfo = parameters[j];
                        if (j > 0) listBuilder.Append(',');
                        listBuilder.Append(parameterInfo.ParameterType.SosTypeName());
                        listBuilder.Append(' ');
                        listBuilder.Append(parameterInfo.Name);
                    }
                    listBuilder.Append(")\n");
                }
                listBuilder.Append('\n');
            }
            listBuilder.Append('\n'); // Add blank line to end (to mark end of interfaces)

            //
            // Add Object Names and their Interfaces
            //
            foreach (NpcExecutionObject executionObject in npcExecutor.ExecutionObjects)
            {
                listBuilder.Append(executionObject.objectName);
                for (int i = 0; i < executionObject.parentNpcInterfaces.Count; i++)
                {
                    NpcInterfaceInfo npcInterface = executionObject.parentNpcInterfaces[i];
                    listBuilder.Append(' ');
                    listBuilder.Append(npcInterface.name);
                }
                listBuilder.Append('\n');
            }
            listBuilder.Append('\n'); // Add blank line to end (to mark end of objects

            return Encoding.UTF8.GetBytes(listBuilder.ToString());
        }
        private Byte[] TypeCommandHandler(String arguments)
        {
            if (arguments == null || arguments.Length <= 0)
            {
                StringBuilder returnString = new StringBuilder();

                foreach (KeyValuePair<String,Type> pair in npcExecutor.EnumAndObjectTypes)
                {
                    Type type = pair.Value;
                    returnString.Append(type.SosTypeName());
                    returnString.Append(' ');
                    returnString.Append(type.SosTypeDefinition());
                    returnString.Append('\n');
                }
                returnString.Append('\n');
                return Encoding.UTF8.GetBytes(returnString.ToString());
            }

            String requestedTypeString = arguments.Trim();

            if (requestedTypeString.IsSosPrimitive())
            {
                return Encoding.UTF8.GetBytes(requestedTypeString + " primitive type\n");
            }

            Type enumOrObjectType;
            if(npcExecutor.EnumAndObjectTypes.TryGetValue(requestedTypeString, out enumOrObjectType))
            {
                return Encoding.UTF8.GetBytes(enumOrObjectType.SosTypeName() + " " + enumOrObjectType.SosTypeDefinition() + "\n");
            }
            OneOrMoreTypes oneOrMoreEnumOrObjectType;
            if (npcExecutor.EnumAndObjectShortNameTypes.TryGetValue(requestedTypeString, out oneOrMoreEnumOrObjectType))
            {
                enumOrObjectType = oneOrMoreEnumOrObjectType.firstType;
                if (oneOrMoreEnumOrObjectType.otherTypes == null)
                {
                    return Encoding.UTF8.GetBytes(enumOrObjectType.SosTypeName() + " " + enumOrObjectType.SosTypeDefinition() + "\n");
                }
                else
                {
                    return Encoding.UTF8.GetBytes(String.Format("Error: '{0}' is ambiguous, include namespace to find the correct type", requestedTypeString));
                }
            }

            return Encoding.UTF8.GetBytes(requestedTypeString + " unknown type\n");
        }
    }
}
