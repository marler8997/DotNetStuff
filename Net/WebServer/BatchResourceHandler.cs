using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

using More;

namespace More.Net
{
    /*
    public class BatchResourceHandler : IResourceHandler
    {
        public readonly IUrlToFileTranslator urlTranslator;
        public readonly TimeSpan timeout;

        public BatchResourceHandler(IUrlToFileTranslator urlTranslator, TimeSpan timeout)
        {
            this.urlTranslator = urlTranslator;
            this.timeout = timeout;
        }
        public void HandleResource(ParsedHttpRequest request, HttpResponse response)
        {
            StringWriter stdout = new StringWriter();
            StringWriter stderr = new StringWriter();

            Process process = new Process();
            process.StartInfo.FileName = urlTranslator.UrlToFile(request.url);

            //
            // TODO: setup environment variabes with http headers
            //
            if (process.RunWithReadersAndWriters(timeout, null, stdout, stderr))
            {
                Console.WriteLine("StdError=\"{0}\"", stderr.ToString());

                //
                // Process stdout
                //
                Int32 lastNewline = 0;
                String stdoutString = stdout.ToString();
                Console.WriteLine("StdOut=\"{0}\"", stdoutString);
                for(int offset = 0; offset < stdoutString.Length; offset++)
                {
                    if (offset == '\n')
                    {
                        Console.WriteLine("HEADER: {0}", stdoutString.Substring(lastNewline, offset - lastNewline));
                        lastNewline = offset + 1;
                    }
                }
            }
            else
            {
                response.Headers["Content-Type"] = "text/plain";
                Byte [] responseBytes = Encoding.UTF8.GetBytes("Process Timed Out");
                response.bodyStream.Write(responseBytes, 0, responseBytes.Length);
                Console.WriteLine("Process timed out");
            }
        }
    }
    */
}
