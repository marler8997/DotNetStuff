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
        public readonly CLGenericArgument<UInt32> receiveBufferLength;

        public TmpHiddenServerOptions()
        {
            heartbeatSeconds = new CLGenericArgument<UInt16>(UInt16.Parse, 'h', "heartbeat-time", "The number of seconds between heartbeats");
            heartbeatSeconds.SetDefault(Tmp.DefaultHeartbeatSeconds);
            Add(heartbeatSeconds);

            reconnectWaitSeconds = new CLGenericArgument<UInt16>(UInt16.Parse, 'w', "reconnect-wait-seconds", "The time between reconnect attempts");
            reconnectWaitSeconds.SetDefault(Tmp.DefaultReconnectWaitSeconds);
            Add(reconnectWaitSeconds);

            receiveBufferLength = new CLGenericArgument<UInt32>(UInt32.Parse, 'r', "receive-buffer-length", "Receive buffer length");
            receiveBufferLength.SetDefault(4096);
            Add(receiveBufferLength);
        }
        public override void PrintUsageHeader()
        {
            Console.WriteLine("TmpHiddenServer.exe [options] <TmpAccessorConnector1> <TmpAccessorConnect2> ...");
        }
    }
    class TmpHiddenServerMain
    {
        static void Main(string[] args)
        {
            TmpHiddenServerOptions options = new TmpHiddenServerOptions();

            List<String> nonOptionArgs = options.Parse(args);

            if (nonOptionArgs.Count < 1)
            {
                options.ErrorAndUsage("Expected at least 1 non option argument but got non");
                return;
            }
            
            Int32 heartbeatMillis = options.heartbeatSeconds.ArgValue * 1000;
            Int32 reconnectWaitMillis = options.reconnectWaitSeconds.ArgValue * 1000;

            if (nonOptionArgs.Count == 1)
            {
                SingleAccessor.Run(nonOptionArgs[0], heartbeatMillis, reconnectWaitMillis, options.receiveBufferLength.ArgValue);
            }
            else
            {
                throw new NotImplementedException("Multiple accessors not yet implemented");
                //MultipleAccessors.Run(nonOptionArgs, heartbeatMillis, reconnectWaitMillis);
            }
        }
    }
}
