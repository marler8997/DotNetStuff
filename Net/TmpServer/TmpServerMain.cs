using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using More;
using More.Net;

namespace More.Net
{
    class TmpHiddenServerOptions : CLParser
    {
        public readonly CLGenericArgument<UInt16> heartbeatSeconds;
        public readonly CLGenericArgument<UInt16> reconnectWaitSeconds;

        public readonly CLStringArgument authenticationFile;

        public readonly CLGenericArgument<UInt32> receiveBufferLength;

        public TmpHiddenServerOptions()
        {
            heartbeatSeconds = new CLGenericArgument<UInt16>(UInt16.Parse, 'h', "heartbeat-time", "The number of seconds between heartbeats");
            heartbeatSeconds.SetDefault(Tmp.DefaultHeartbeatSeconds);
            Add(heartbeatSeconds);

            reconnectWaitSeconds = new CLGenericArgument<UInt16>(UInt16.Parse, 'w', "reconnect-wait-seconds", "The time between reconnect attempts");
            reconnectWaitSeconds.SetDefault(Tmp.DefaultReconnectWaitSeconds);
            Add(reconnectWaitSeconds);

            authenticationFile = new CLStringArgument('a', "auth", "Authentication file");
            Add(authenticationFile);

            receiveBufferLength = new CLGenericArgument<UInt32>(UInt32.Parse, 'r', "receive-buffer-length", "Receive buffer length");
            receiveBufferLength.SetDefault(4096);
            Add(receiveBufferLength);
        }
        public override void PrintUsageHeader()
        {
            Console.WriteLine("Connect Mode: TmpHiddenServer.exe [options] <ServerName> <TmpAccessorConnector>");
            Console.WriteLine("Listen Mode : TmpHiddenServer.exe [options] <ServerName> <ListenPort>");
        }
    }
    class TmpServerMain
    {
        static void Main(string[] args)
        {
            TmpHiddenServerOptions options = new TmpHiddenServerOptions();

            List<String> nonOptionArgs = options.Parse(args);

            if (nonOptionArgs.Count != 2)
            {
                options.ErrorAndUsage("Expected 2 non option argument but got none");
                return;
            }

            String serverName = nonOptionArgs[0];
            String accessorConnector = nonOptionArgs[1];

            TlsSettings tlsSettings;
            if (!options.authenticationFile.set)
            {
                tlsSettings = new TlsSettings(false);
            }
            else
            {
                throw new NotImplementedException("Authentication file not yet implemented");
            }

            TunnelManipulationReverseServer.Run(tlsSettings, serverName, accessorConnector,
                options.heartbeatSeconds.ArgValue,
                options.reconnectWaitSeconds.ArgValue, options.receiveBufferLength.ArgValue);
        }
    }
}
