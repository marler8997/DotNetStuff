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
            this.lineParser = new LineParser(Encoding.ASCII, ByteBuffer.DefaultInitialCapacity, ByteBuffer.DefaultExpandLength);
            this.atFirstLine = true;
            this.done = false;
        }
        public void Handle(Byte[] buffer, Int32 bytesRead)
        {
            Handle(buffer, 0, bytesRead);
        }
        public void Handle(Byte[] buffer, Int32 offset, Int32 bytesRead)
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
            this.socketLineReader = new SocketLineReader(socket, Encoding.ASCII, ByteBuffer.DefaultInitialCapacity, ByteBuffer.DefaultExpandLength);
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
    public abstract class NpcHandler : IDisposable
    {
        protected readonly String clientString;
        protected readonly INpcServerCallback callback;
        protected readonly IDataHandler responseHandler;
        protected readonly NpcExecutor npcExecutor;
        protected readonly INpcHtmlGenerator npcHtmlGenerator;

        protected NpcHandler(String clientString, INpcServerCallback callback, IDataHandler responseHandler, NpcExecutor npcExecutor, INpcHtmlGenerator npcHtmlGenerator)
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
                responseHandler.HandleData(response404, 0, response404.Length);
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
            responseHandler.HandleData(headerBytes, 0, headerBytes.Length);
            responseHandler.HandleData(contents, 0, contents.Length);

            return;
        }

        protected void HandleLine(String line)
        {
            Byte[] response = null;

            String commandArguments;
            String command = line.Peel(out commandArguments);

            if (command.Equals("call", StringComparison.InvariantCultureIgnoreCase))
            {
                response = CallCommandHandler(commandArguments);
            }
            else if (command.Equals("methods", StringComparison.InvariantCultureIgnoreCase))
            {
                response = MethodsCommandHandler();
            }
            else if (command.Equals("type", StringComparison.InvariantCultureIgnoreCase))
            {
                response = TypeCommandHandler(commandArguments);
            }
            else if (command.Equals("help", StringComparison.InvariantCultureIgnoreCase))
            {
                response = GenerateHelpMessage(null);
            }
            else if (command.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
            {
                responseHandler.Dispose();
                return;
            }
            else
            {
                callback.GotInvalidData(clientString, String.Format("Unknown Command from line '{0}'", line));
                response = GenerateHelpMessage(String.Format("Unknown Command '{0}', expected 'help', 'exit', 'methods', 'type' or 'call'", command));
            }
            if (response != null) responseHandler.HandleData(response, 0, response.Length);
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
            builder.Append("Commands:\n");
            builder.Append("   call method-name [args...] Call given method with given arguments\n");
            builder.Append("   methods                    Print available methods (use 'parsable' for more detailed)\n");
            builder.Append("   type [type]                No argument: print all types, 1 argument: print given type information\n");
            builder.Append("   exit                       Exit\n");
            builder.Append("   help                       Show this help\n");
            return Encoding.UTF8.GetBytes(builder.ToString());
        }
        private Byte[] CallCommandHandler(String callArguments)
        {
            //
            // This method call will always return a specifically formatted string.
            //    1. On Success   "Success <ReturnType> [<ReturnValue>]"
            //    3. On Exception "Exception <ExceptionMessage> <ExceptionType> <SerializedException>
            //    2. On NpcError  "NpcError <ErrorCode> <Error Message>
            //
            if (callArguments == null || callArguments.Length <= 0)
                return Encoding.ASCII.GetBytes(NpcError(NpcErrorCode.InvalidCallSyntax, "missing method name"));

            //
            // Get method name
            //
            String methodName;
            String parametersString;

            Int32 spaceIndex = callArguments.IndexOf(' ');
            if (spaceIndex <= 0 || spaceIndex >= callArguments.Length - 1)
            {
                if (spaceIndex == 0) return Encoding.ASCII.GetBytes(NpcError(NpcErrorCode.InvalidCallSyntax, "found 2 spaces after 'call'"));
                methodName = callArguments.Trim();
                parametersString = null;
            }
            else
            {
                methodName = callArguments.Remove(spaceIndex);
                parametersString = callArguments.Substring(spaceIndex + 1);
            }

            //
            // Parse Parameters
            //
            List<String> parametersList;
            try
            {
                parametersList = Npc.ParseParameters(parametersString);
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
            catch (Exception e)
            {
                callback.ExceptionDuringExecution(clientString, methodName, e);
                return Encoding.ASCII.GetBytes(NpcError(NpcErrorCode.UnhandledException, e.GetType().Name + ": " + e.Message));
            }
        }
        private Byte[] MethodsCommandHandler()
        {
            StringBuilder listBuilder = new StringBuilder();

            foreach(NpcExecutionObject executionObject in npcExecutor.ExecutionObjects)
            {
                foreach (NpcMethodInfo npcMethodInfo in executionObject.npcMethods)
                {
#if WindowsCE
                    listBuilder.Append(npcMethodInfo.methodInfo.ReturnType.SosTypeName());
#else
                    listBuilder.Append(npcMethodInfo.methodInfo.ReturnParameter.ParameterType.SosTypeName());
#endif
                    listBuilder.Append(' ');
                    listBuilder.Append(npcMethodInfo.npcMethodName);

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
            }
            listBuilder.Append('\n'); // Add blank line to end (to mark end of data)
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

            Type enumOrObjectType;
            if(npcExecutor.EnumAndObjectTypes.TryGetValue(requestedTypeString, out enumOrObjectType))
            {
                return Encoding.UTF8.GetBytes(enumOrObjectType.SosTypeName() + " " + enumOrObjectType.SosTypeDefinition() + "\n");
            }

            if(requestedTypeString.IsSosPrimitive())
            {
                return Encoding.UTF8.GetBytes(requestedTypeString + " primitive type\n");
            }

            return Encoding.UTF8.GetBytes(requestedTypeString + " unknown type\n");
        }
    }
}
