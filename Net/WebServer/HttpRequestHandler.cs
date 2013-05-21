using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Marler.Net
{
    class HttpRequestHandler
    {
        private readonly IResourceHandler resourceHandler;
        private readonly NetworkStream stream;
        private readonly MessageLogger messageLogger;
        private readonly IConnectionDataLogger connectionDataLogger;

        public HttpRequestHandler(IResourceHandler resourceHandler, NetworkStream stream,
            MessageLogger messageLogger, IConnectionDataLogger connectionDataLogger)
        {
            if (resourceHandler == null) throw new ArgumentNullException("resourceHandler");
            if (stream == null) throw new ArgumentNullException("null");

            this.resourceHandler = resourceHandler;
            this.stream = stream;

            this.messageLogger = (messageLogger == null) ? MessageLogger.NullMessageLogger : messageLogger;
            this.connectionDataLogger = (connectionDataLogger == null) ? ConnectionDataLogger.Null : connectionDataLogger;
        }

        public void Run()
        {
            try
            {
                //
                // 1. Receive the Request
                //
                MemoryStream requestBuffer = new MemoryStream(1024);

                HttpRequest request = new HttpRequest(stream, messageLogger, connectionDataLogger.AToBDataLogger);

                //
                // 2. Log the Request
                //
                Byte[] requestBytes = requestBuffer.ToArray();
                requestBuffer.Dispose();

                messageLogger.Log("Got Request {0} bytes", requestBytes.Length);

                //
                // 3. Create the Response
                //
                HttpResponse response = new HttpResponse();
                response.version = "HTTP/1.1";
                response.Headers = new Dictionary<String, String>();
                response.Headers.Add("Server", WebServer.Name);
                response.Headers.Add("Date", DateTime.Now.ToString("r"));
                response.Headers.Add("Connection", "close");

                if (!request.ParsedSuccessfully)
                {
                    response.status = HttpProtocol.HttpResponseBadRequest;
                }
                else
                {
                    response.status = HttpProtocol.HttpResponseOK;

                    try
                    {
                        resourceHandler.HandleResource(request, response);
                    }
                    catch (Exception e)
                    {
                        response.bodyStream.Dispose();
                        response.bodyStream = new MemoryStream();
                        Byte[] responseBytes = Encoding.Default.GetBytes(String.Format(
                            "<html><head><title>Exception: {0}</title><body><h1>URL: {1}</h1>\n<h2>Exception: {2}</h2>\n{3}</body></html>",
                            e.GetType().ToString(), request.url, e.Message, e.StackTrace));
                        response.bodyStream.Write(responseBytes, 0, responseBytes.Length);
                        response.Headers.Add("Content-Type", "text/html");
                    }
                }

                //
                // 4. Check Headers
                //
                if (!response.Headers.ContainsKey("Content-Type"))
                {
                    response.Headers.Add("Content-Type", "application/octet-stream");
                }
                Byte[] responseBody = response.bodyStream.ToArray();
                response.Headers.Add("Content-Length", responseBody.Length.ToString());


                //
                // 5. Send the Response
                //

                // Put together the response string
                StringBuilder headerStringBuilder = new StringBuilder(
                    String.Format("{0} {1}\n", response.version, HttpProtocol.responseStatus[response.status]));
                foreach (KeyValuePair<String, String> headerPair in response.Headers)
                {
                    headerStringBuilder.Append(String.Format("{0}: {1}\n", headerPair.Key, headerPair.Value));
                }
                headerStringBuilder.Append('\n');

                String headerString = headerStringBuilder.ToString();
                messageLogger.Log("Sending Header {0} bytes", headerString.Length);

                // Send headers
                byte[] headerByteArray = Encoding.UTF8.GetBytes(headerString);
                connectionDataLogger.LogDataBToA(headerByteArray, 0, headerByteArray.Length);
                stream.Write(headerByteArray, 0, headerByteArray.Length);

                // Send body
                if (responseBody.Length > 0)
                {
                    messageLogger.Log("Sending Body {0} bytes", responseBody.Length);
                    connectionDataLogger.LogDataBToA(responseBody, 0, responseBody.Length);
                    stream.Write(responseBody, 0, responseBody.Length);
                }
            }
            catch (Exception e)
            {
                messageLogger.Log("Exception {0}", e.ToString());
            }
            finally
            {
                messageLogger.Log("Close");
                stream.Close();
            }
        }
    }
}
