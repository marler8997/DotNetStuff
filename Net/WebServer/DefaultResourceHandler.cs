using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security;

using More;

using Microsoft.Win32;

namespace More.Net
{
    public class DefaultFileResourceHandler : IResourceHandler
    {
        public readonly IUrlToFileTranslator urlTranslator;
        private readonly String defaultIndexFile;

        public DefaultFileResourceHandler(IUrlToFileTranslator urlTranslator, String defaultIndexFile)
        {
            if (urlTranslator == null) throw new ArgumentNullException("urlTranslator");
            if (String.IsNullOrEmpty(defaultIndexFile)) throw new ArgumentNullException("defaultIndexFile");

            this.urlTranslator = urlTranslator;
            this.defaultIndexFile = defaultIndexFile;
        }

        public void HandleResource(ParsedHttpRequest request, HttpResponse response)
        {
            String urlResource = urlTranslator.UrlToFile(request.url);

            //
            // If it is a directory
            //
            if (Directory.Exists(urlResource))
            {
                String defaultFile = Path.Combine(urlResource, defaultIndexFile);
                if (File.Exists(defaultFile))
                {
                    urlResource = defaultFile;
                }
                else
                {
                    String[] subdirs = Directory.GetDirectories(urlResource);
                    String[] files = Directory.GetFiles(urlResource);

                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">\n");
                    stringBuilder.Append("<HTML><HEAD>\n");
                    stringBuilder.Append("<META http-equiv=Content-Type content=\"text/html; charset=windows-1252\">\n");
                    stringBuilder.Append("</HEAD>\n");
                    stringBuilder.Append("<BODY>\n");
                    //
                    // Print Resource Links List
                    //
                    Stack<String> urlStack = new Stack<String>();
                    String urlIterator = request.url;
                    while (true)
                    {
                        urlIterator = Http.GetParentUrl(urlIterator);
                        if (urlIterator == null) break;
                        urlStack.Push(urlIterator);
                    }
                    Console.WriteLine("DEBUG: URL STACK({0}) :", urlStack.Count);
                    urlStack.Print(Console.Out);

                    stringBuilder.Append(String.Format("<h1>URL: {0}</h1>\n", request.url));
                    if (urlStack.Count > 0)
                    {
                        urlStack.Pop();
                        stringBuilder.Append("<a href=\"/\">[root]/</a>\n");

                        while (urlStack.Count > 0)
                        {
                            String directory = urlStack.Pop();
                            stringBuilder.Append(String.Format("<a href=\"{0}\">{1}/</a>\n", directory, Http.GetUrlFilename(directory)));
                        }
                        stringBuilder.Append(String.Format("{0}/\n", Http.GetUrlFilename(request.url)));
                    }
                    //
                    // Print Directories / Files
                    //
                    stringBuilder.Append(String.Format("<p>Folder listing, to not see this add '{0}' to this directory</p>\n", defaultIndexFile));

                    if ((subdirs == null || subdirs.Length <= 0) && (files == null || files.Length <= 0))
                    {
                        stringBuilder.Append("No Files.");
                    }
                    else
                    {
                        if (subdirs != null)
                        {
                            for (int i = 0; i < subdirs.Length; i++)
                            {
                                stringBuilder.Append(String.Format("<br><a href = \"{0}{1}/\">[{1}]</a>\n", request.url, Path.GetFileName(subdirs[i])));
                            }
                        }
                        if (files != null)
                        {
                            for (int i = 0; i < files.Length; i++)
                            {
                                stringBuilder.Append(String.Format("<br><a href = \"{0}{1}\">{1}</a>\n", request.url, Path.GetFileName(files[i])));
                            }
                        }
                    }
                    stringBuilder.Append("</BODY></HTML>\n");


                    response.Headers.Add("Content-Type", "text/html");
                    Byte[] bodyBytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    response.bodyStream.Write(bodyBytes, 0, bodyBytes.Length);
                    return;
                }
            }

            //
            // It must be a file
            //
            if (File.Exists(urlResource))
            {
                Byte[] bodyBytes = FileExtensions.ReadFile(urlResource);
                response.bodyStream.Write(bodyBytes, 0, bodyBytes.Length);

                // Get the data from a specified item in the key.
                String extenstion = Path.GetExtension(urlResource);
                if (!String.IsNullOrEmpty(extenstion))
                {
                    try
                    {
                        RegistryKey rk = Registry.ClassesRoot.OpenSubKey(Path.GetExtension(urlResource), true);
                        if (rk != null)
                        {
                            String contentType = (String)rk.GetValue("Content Type");
                            response.Headers["Content-Type"] = String.IsNullOrEmpty(contentType) ? "application/octet-stream" :
                                contentType;
                        }
                    }
                    catch (SecurityException se)
                    {
                        Console.WriteLine("WARNING: SecurityException '{0}' while trying to get the ContentType of extension '{0}'",
                            se.Message, extenstion);
                    }
                }
                else
                {
                    Console.WriteLine("WARNING: Cannote get Content-Type from file '{0}' because it has no extension", urlResource);
                }
            }
            else
            {
                response.status = Http.ResponseNotFound;
                String bodyStr = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">\n";
                bodyStr += "<HTML><HEAD>\n";
                bodyStr += "<META http-equiv=Content-Type content=\"text/html; charset=windows-1252\">\n";
                bodyStr += "</HEAD>\n";
                bodyStr += "<BODY>File not found!!</BODY></HTML>\n";

                response.Headers.Add("Content-Type", "text/html");
                Byte[] bodyBytes = Encoding.UTF8.GetBytes(bodyStr);
                response.bodyStream.Write(bodyBytes, 0, bodyBytes.Length);
            }
        }

        public override string ToString()
        {
            return String.Format("DefaultResourceHandler: DefaultIndexFile='{0}'", defaultIndexFile);
        }
    }
}
