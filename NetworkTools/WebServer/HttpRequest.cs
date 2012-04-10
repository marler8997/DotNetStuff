using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Web;

namespace Marler.NetworkTools
{
    public class HttpRequest
    {
        public const Int32 RequestBufferInitialCapacity = 1024;

        public const Int32 InitialCapacityForMethod = 8;
        public const Int32 InitialCapacityForUrl = 128;

        public enum RequestParserState
        {
            Method, Url, UrlParam, UrlParamValue, Version,
            HeaderKey, HeaderValue, Body, Done
        };

        private readonly RequestParserState parserState;

        public readonly String method;
        public readonly String url;
        public readonly String httpVersion;

        public readonly Dictionary<String, String> urlArguments;
        public readonly Dictionary<String, String> headers;

        public readonly Byte[] body;

        public Boolean ParsedSuccessfully { get { return parserState == RequestParserState.Done; } }

        public HttpRequest(NetworkStream stream, MessageLogger messageLogger, IDataLogger dataLogger)
        {
            int bytesRead;
            byte[] readBuffer = new byte[2048];

            parserState = RequestParserState.Method;

            StringBuilder stringBuilder = new StringBuilder(InitialCapacityForMethod); ;
            String hValue = String.Empty;
            String hKey = String.Empty;
            String temp;
            Int32 bodyIndex = 0;

            do
            {
                switch (parserState)
                {
                    case RequestParserState.Method:
                        messageLogger.Log("Reading Request");
                        break;
                    case RequestParserState.Body:
                        messageLogger.Log("Waiting for {0} bytes for the body", body.Length - bodyIndex);
                        break;
                    default:
                        messageLogger.Log("Waiting for more data (ParserState={0})...", parserState);
                        break;
                }
                bytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                if (bytesRead <= 0) break;

                if (dataLogger != null) dataLogger.LogData(readBuffer, 0, bytesRead);

                int offset = 0;
                do
                {
                    switch (parserState)
                    {
                        case RequestParserState.Method:
                            if (readBuffer[offset] != ' ')
                            {
                                stringBuilder.Append((char)readBuffer[offset]);
                            }
                            else
                            {
                                method = stringBuilder.ToString();
                                stringBuilder = new StringBuilder(InitialCapacityForUrl);
                                parserState = RequestParserState.Url;
                            }
                            offset++;
                            break;
                        case RequestParserState.Url:
                            if (readBuffer[offset] == '?')
                            {
                                url = HttpUtility.UrlDecode(stringBuilder.ToString());

                                hKey = String.Empty;
                                urlArguments = new Dictionary<String, String>();
                                parserState = RequestParserState.UrlParam;
                            }
                            else if (readBuffer[offset] != ' ')
                            {
                                stringBuilder.Append((char)readBuffer[offset]);
                            }
                            else
                            {
                                url = HttpUtility.UrlDecode(stringBuilder.ToString());
                                parserState = RequestParserState.Version;
                            }
                            offset++;
                            break;
                        case RequestParserState.UrlParam:
                            if (readBuffer[offset] == '=')
                            {
                                offset++;
                                hValue = String.Empty;
                                parserState = RequestParserState.UrlParamValue;
                            }
                            else if (readBuffer[offset] == ' ')
                            {
                                offset++;

                                url = HttpUtility.UrlDecode(url);
                                parserState = RequestParserState.Version;
                            }
                            else
                            {
                                hKey += (char)readBuffer[offset++];
                            }
                            break;
                        case RequestParserState.UrlParamValue:
                            if (readBuffer[offset] == '&')
                            {
                                offset++;
                                hKey = HttpUtility.UrlDecode(hKey);
                                hValue = HttpUtility.UrlDecode(hValue);
                                if (urlArguments.TryGetValue(hKey, out temp))
                                    urlArguments[hKey] = String.Format("{0},{1}", temp, hValue);
                                else
                                    urlArguments[hKey] = hValue;

                                hKey = String.Empty;
                                parserState = RequestParserState.UrlParam;
                            }
                            else if (readBuffer[offset] == ' ')
                            {
                                offset++;
                                hKey = HttpUtility.UrlDecode(hKey);
                                hValue = HttpUtility.UrlDecode(hValue);
                                if (urlArguments.TryGetValue(hKey, out temp))
                                    urlArguments[hKey] = String.Format("{0},{1}", temp, hValue);
                                else
                                    urlArguments[hKey] = hValue;

                                parserState = RequestParserState.Version;
                            }
                            else
                            {
                                hValue += (char)readBuffer[offset++];
                            }
                            break;
                        case RequestParserState.Version:
                            if (readBuffer[offset] == '\r') offset++;

                            if (readBuffer[offset] != '\n')
                            {
                                httpVersion += (char)readBuffer[offset++];
                            }
                            else
                            {
                                offset++;
                                hKey = String.Empty;
                                headers = new Dictionary<String, String>();
                                parserState = RequestParserState.HeaderKey;
                            }
                            break;
                        case RequestParserState.HeaderKey:
                            if (readBuffer[offset] == '\r') offset++;

                            if (readBuffer[offset] == '\n')
                            {
                                offset++;
                                if (headers.TryGetValue("Content-Length", out temp))
                                {
                                    body = new byte[Int32.Parse(temp)];
                                    parserState = RequestParserState.Body;
                                }
                                else
                                {
                                    parserState = RequestParserState.Done;
                                }
                            }
                            else if (readBuffer[offset] == ':')
                            {
                                offset++;

                            }
                            else if (readBuffer[offset] != ' ')
                            {
                                hKey += (char)readBuffer[offset++];
                            }
                            else
                            {
                                offset++;
                                hValue = "";
                                parserState = RequestParserState.HeaderValue;
                            }
                            break;
                        case RequestParserState.HeaderValue:
                            if (readBuffer[offset] == '\r') offset++;

                            if (readBuffer[offset] == '\n')
                            {
                                offset++;
                                headers.Add(hKey, hValue);
                                hKey = String.Empty;
                                parserState = RequestParserState.HeaderKey;
                            }
                            else
                            {
                                hValue += (char)readBuffer[offset++];
                            }
                            break;
                        case RequestParserState.Body:
                            // Append to request BodyData
                            Array.Copy(readBuffer, offset, body, bodyIndex, bytesRead - offset);
                            bodyIndex += bytesRead - offset;
                            offset = bytesRead;
                            if (bodyIndex >= ((body == null) ? 0 : body.Length))
                            {
                                parserState = RequestParserState.Done;
                            }
                            break;
                        default:
                            throw new InvalidOperationException(String.Format("Unrecognized Parser State '{0}' ({1})", parserState, (int)parserState));
                    }
                }
                while (offset < bytesRead);

            } while ((parserState != RequestParserState.Done) && stream.DataAvailable);
        }

        public override String ToString()
        {
            return String.Format("[HttpRequest method='{0}' url='{1}' httpVersion='{2}' headers='{3}' bodyLength='{4}']",
                method, url, httpVersion, (headers == null) ? "<null>" : headers.GetString(), (body == null) ? 0 : body.Length);
        }
    }
}
