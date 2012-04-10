using System;
using System.Collections.Generic;

namespace Marler.NetworkTools
{
    public class WebServerProgram
    {
        public static void Main(String[] args)
        {
            WebServerOptions optionsParser = new WebServerOptions();
            List<String> nonOptionArgs = optionsParser.Parse(args);

            if (nonOptionArgs.Count < 1)
            {
                Console.WriteLine("Please give the path");
                optionsParser.PrintUsage();
            }
            else if (nonOptionArgs.Count > 1)
            {
                Console.WriteLine("Expected 1 non-option argument, you gave {0}", nonOptionArgs.Count);
                optionsParser.PrintUsage();
            }

            String rootDirectory = nonOptionArgs[0];

            DefaultUrlToFileTranslator urlTranslator = new DefaultUrlToFileTranslator(rootDirectory);

            DefaultFileResourceHandler defaultFileHandler = new DefaultFileResourceHandler(
                urlTranslator, optionsParser.defaultIndexFile.ArgValue);

            /*
            ExtensionFilteredResourceHandler extensionHandler =
                new ExtensionFilteredResourceHandler(defaultFileHandler);

            extensionHandler.AddExtensionHandler("bat", new BatchResourceHandler(
                urlTranslator, TimeSpan.FromSeconds(20)));
            */

            WebServer webServer = new WebServer(defaultFileHandler, optionsParser.port.ArgValue,
                optionsParser.socketBackLog.ArgValue);

            webServer.Run();
        }
    }
}
