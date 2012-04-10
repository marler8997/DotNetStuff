using System;

using Marler.OptionsParser;

namespace Marler.NetworkTools
{
    class FtpServerOptions : Options
    {
        public readonly OptionGenericArg<UInt16> port;
        public readonly OptionIntArg socketBackLog;

        public FtpServerOptions()
            : base()
        {
            port = new OptionGenericArg<UInt16>(UInt16.Parse, 'p', "Listen Port", "The port number to listen on");
            port.SetDefault(21);

            socketBackLog = new OptionIntArg('b', "Socket Back Log", "The maximum length of the pending connections queue");
            socketBackLog.SetDefault(32);

            AddOption(port);
            AddOption(socketBackLog);
        }

        public override void PrintHeader()
        {
            Console.WriteLine("FtpServer [options] root-path");
        }
    }
}
