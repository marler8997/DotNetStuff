using System;

using Marler.Common;

namespace Marler.Net
{
    public class WebServerOptions : CLParser
    {
        public readonly CLGenericArgument<UInt16> port;
        public readonly CLStringArgument defaultIndexFile;
        public readonly CLInt32Argument socketBackLog;

        public WebServerOptions()
            : base()
        {
            port = new CLGenericArgument<UInt16>(UInt16.Parse, 'p', "Listen Port", "The port number to listen on");
            port.SetDefault(80);
            Add(port);

            defaultIndexFile = new CLStringArgument('i', "Default Index File", "Filename of the default file to send when the client requests a directory");
            defaultIndexFile.SetDefault("index.html");
            Add(defaultIndexFile);

            socketBackLog = new CLInt32Argument('b', "Socket Back Log", "The maximum length of the pending connections queue");
            socketBackLog.SetDefault(32);
            Add(socketBackLog);
        }

        public override void PrintUsageHeader()
        {
            Console.WriteLine("WebServer [options] root-path");
        }
    }
}
