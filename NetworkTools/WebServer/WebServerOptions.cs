using System;
using Marler.OptionsParser;

namespace Marler.NetworkTools
{
    public class WebServerOptions : Options
    {
        public readonly OptionGenericArg<UInt16> port;
        public readonly OptionStringArg defaultIndexFile;
        public readonly OptionIntArg socketBackLog;

        public WebServerOptions()
            : base()
        {
            port = new OptionGenericArg<UInt16>(UInt16.Parse, 'p', "Listen Port", "The port number to listen on");
            port.SetDefault(80);

            defaultIndexFile = new OptionStringArg('i', "Default Index File", "Filename of the default file to send when the client requests a directory");
            defaultIndexFile.SetDefault("index.html");

            socketBackLog = new OptionIntArg('b', "Socket Back Log", "The maximum length of the pending connections queue");
            socketBackLog.SetDefault(32);

            AddOption(port);
            AddOption(defaultIndexFile);
            AddOption(socketBackLog);
        }

        public override void PrintHeader()
        {
            Console.WriteLine("WebServer [options] root-path");
        }
    }
}
