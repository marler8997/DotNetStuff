using System;

using More;

namespace More.Net
{
    class FtpServerOptions : CLParser
    {
        public readonly CLGenericArgument<UInt16> port;
        public readonly CLInt32Argument socketBackLog;

        public FtpServerOptions()
            : base()
        {
            port = new CLGenericArgument<UInt16>(UInt16.Parse, 'p', "Listen Port", "The port number to listen on");
            port.SetDefault(21);
            Add(port);

            socketBackLog = new CLInt32Argument('b', "Socket Back Log", "The maximum length of the pending connections queue");
            socketBackLog.SetDefault(32);
            Add(socketBackLog);
        }

        public override void PrintUsageHeader()
        {
            Console.WriteLine("FtpServer [options] root-path");
        }
    }
}
