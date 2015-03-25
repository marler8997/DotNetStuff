using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using More.Net.TmpCommand;

namespace More.Net
{
    public class TunnelManipulationReverseServer
    {
        public static void Run(TlsSettings tlsSettings, String serverName, String accessorConnectorString, UInt16 heartbeatSeconds,
            UInt16 reconnectWaitSeconds, UInt32 receiveBufferLength)
        {
            ServerInfo serverInfo = new ServerInfo(
                Encoding.ASCII.GetBytes(serverName),
                heartbeatSeconds,
                reconnectWaitSeconds);

            SelectTunnelsThread tunnelsThread = new SelectTunnelsThread(receiveBufferLength);
            AccessorConnection accessorConnection = new AccessorConnection(accessorConnectorString);
            
            //TunnelControlServer tunnelServer = new TunnelControlServer(tunnelsThread, accessor);
            //NpcReflector npcReflector = new NpcReflector(
            //    new NpcExecutionObject(tunnelServer, "TunnelServer", null, null)
            //    );

            Int32 heartbeatMillis = heartbeatSeconds * 1000;
            Int32 reconnectWaitMillis = reconnectWaitSeconds * 1000;

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
                    Console.WriteLine("{0} [{1}] Attempting reconnect...", DateTime.Now, accessorConnection.accessorEndPoint);
                    if (accessorConnection.TryConnectAndInitialize(tlsSettings, sendBuffer, serverInfo, tunnelsThread))
                    {
                        Console.WriteLine("{0} [{1}] Connected", DateTime.Now, accessorConnection.accessorEndPoint);
                        lastHearbeatTime = Stopwatch.GetTimestamp();
                    }
                    else
                    {
                        Console.WriteLine("{0} [{1}] Failed to connect, waiting {2} seconds for reconnect",
                            DateTime.Now, accessorConnection.accessorEndPoint, reconnectWaitMillis / 1000);
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
                        Console.WriteLine("{0} [{1}] Sending hearbeat...", DateTime.Now, accessorConnection.accessorEndPoint);

                        accessorConnection.SendHeartbeat();
                        if (!accessorConnection.Connected) break;

                        lastHearbeatTime = now;
                        nextHeartbeatTimeoutMillis = heartbeatMillis;
                    }

                    accessorConnection.ReceiveWithTimeout(receiveBuffer, nextHeartbeatTimeoutMillis);
                }
            }
        }
    }
}
