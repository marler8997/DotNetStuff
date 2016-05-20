using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using More;
using More.Net;
using More.Net.TmpCommand;

namespace More.Net
{
    class Options : CLParser
    {
        public readonly CLGenericArgument<UInt16> heartbeatSeconds;
        public readonly CLGenericArgument<UInt16> reconnectWaitSeconds;

        public readonly CLStringArgument authenticationFile;

        public readonly CLGenericArgument<UInt32> receiveBufferLength;

        public Options()
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
        }
    }
    class TmpServerMain
    {
        static void Main(string[] args)
        {
            Options options = new Options();
            List<String> nonOptionArgs = options.Parse(args);

            if (nonOptionArgs.Count != 2)
            {
                options.ErrorAndUsage("Expected 2 non option argument but got none");
                return;
            }

            String serverName = nonOptionArgs[0];
            String accessorConnectorString = nonOptionArgs[1];

            TlsSettings tlsSettings;
            if (!options.authenticationFile.set)
            {
                tlsSettings = new TlsSettings(false);
            }
            else
            {
                throw new NotImplementedException("Authentication file not yet implemented");
            }
            /*
            Run(tlsSettings, serverName, accessorConnector,
                options.heartbeatSeconds.ArgValue,
                options.reconnectWaitSeconds.ArgValue, options.receiveBufferLength.ArgValue);
        }
        public static void Run(TlsSettings tlsSettings, String serverName, String accessorConnectorString,
            UInt16 heartbeatSeconds, UInt16 reconnectWaitSeconds, UInt32 receiveBufferLength)
        {*/

            ServerInfo serverInfo = new ServerInfo(
                Encoding.ASCII.GetBytes(serverName),
                options.heartbeatSeconds.ArgValue,
                options.reconnectWaitSeconds.ArgValue);

            //SelectTunnelsThread tunnelsThread = new SelectTunnelsThread(receiveBufferLength);
            AccessorConnection accessorConnection = new AccessorConnection(accessorConnectorString);

            //TunnelControlServer tunnelServer = new TunnelControlServer(tunnelsThread, accessor);
            //NpcReflector npcReflector = new NpcReflector(
            //    new NpcExecutionObject(tunnelServer, "TunnelServer", null, null)
            //    );

            Int32 heartbeatMillis = serverInfo.SecondsPerHeartbeat * 1000;
            Int32 reconnectWaitMillis = serverInfo.SecondsPerReconnect * 1000;

            Buf safeBuffer = new Buf(options.receiveBufferLength.ArgValue);
            SelectServer selectServer = new SelectServer(true, safeBuffer);
            accessorConnection.StartConnect(ref selectServer.control, safeBuffer);
            selectServer.Run();


            /*
            //
            // TmpHiddenServer Loop
            //
            Byte[] receiveBuffer = new Byte[receiveBufferLength];
            Buf sendBuffer = new Buf(256, 256);
            Int64 lastHearbeatTime = 0;
            SingleObjectList singleAccessorSocket = new SingleObjectList();

            while (true)
            {
                //
                // While the accessor is not connected
                //
                while (!accessorConnection.Connected)
                {
                    Console.WriteLine("{0} [{1}] Attempting reconnect...", DateTime.Now, accessorConnection.accessorHostString);
                    if (accessorConnection.TryConnectAndInitialize(tlsSettings, sendBuffer, serverInfo))
                    {
                        Console.WriteLine("{0} [{1}] Connected", DateTime.Now, accessorConnection.accessorHostString);
                        lastHearbeatTime = Stopwatch.GetTimestamp();
                    }
                    else
                    {
                        Console.WriteLine("{0} [{1}] Failed to connect, waiting {2} seconds for reconnect",
                            DateTime.Now, accessorConnection.accessorHostString, reconnectWaitMillis / 1000);
                        Thread.Sleep(reconnectWaitMillis);
                    }
                }

                //
                // While the accessor is connected
                //
                while (accessorConnection.Connected)
                {
                    //
                    // Calculate the next heartbeat timeout
                    //
                    Int64 now = Stopwatch.GetTimestamp();
                    Int32 nextHeartbeatTimeoutMillis = heartbeatMillis - StopwatchExtensions.StopwatchTicksAsInt32Milliseconds(now - lastHearbeatTime);
                    if (nextHeartbeatTimeoutMillis <= 0)
                    {
                        Console.WriteLine("{0} [{1}] Sending hearbeat...", DateTime.Now, accessorConnection.accessorHostString);

                        accessorConnection.SendHeartbeat();
                        if (!accessorConnection.Connected) break;

                        lastHearbeatTime = now;
                        nextHeartbeatTimeoutMillis = heartbeatMillis;
                    }

                    accessorConnection.ReceiveWithTimeout(receiveBuffer, nextHeartbeatTimeoutMillis);
                }
            }
            */
        }
    }
}
