using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace More.Net
{
    public static class SingleAccessor
    {
        public static void Run(String accessorConnectorString, Int32 heartbeatMillis, Int32 reconnectWaitMillis, UInt32 receiveBufferLength)
        {
            SelectTunnelsThread tunnelsThread = new SelectTunnelsThread(receiveBufferLength);
            Accessor accessor = new Accessor(accessorConnectorString);
            
            TunnelControlServer tunnelServer = new TunnelControlServer(tunnelsThread, accessor);
            NpcReflector npcReflector = new NpcReflector(
                new NpcExecutionObject(tunnelServer, "TunnelServer", null, null)
                );

            //
            // TmpHiddenServer Loop
            //
            Byte[] receiveBuffer = new Byte[receiveBufferLength];
            Int64 lastHearbeatTime = 0;
            SingleObjectList singleAccessorSocket = new SingleObjectList();

            while (true)
            {
                //
                // While the accessor is not connected
                //
                while (!accessor.Connected)
                {
                    Console.WriteLine("{0} [{1}] Attempting reconnect...", DateTime.Now, accessor.accessorEndPoint);
                    if (accessor.TryConnect(NpcServerConsoleLoggerCallback.Instance, npcReflector))
                    {
                        Console.WriteLine("{0} [{1}] Connected", DateTime.Now, accessor.accessorEndPoint);
                        lastHearbeatTime = Stopwatch.GetTimestamp();
                    }
                    else
                    {
                        Console.WriteLine("{0} [{1}] Failed to connect", DateTime.Now, accessor.accessorEndPoint);
                        Console.WriteLine("{0} [{1}] Waiting {2} seconds for next reconnect", DateTime.Now, accessor.accessorEndPoint, reconnectWaitMillis / 1000);
                        Thread.Sleep(reconnectWaitMillis);
                    }
                }

                //
                // While the accessor is connected
                //
                while (accessor.Connected)
                {
                    //
                    // Calculate the next heartbeat timeout
                    //
                    Int64 now = Stopwatch.GetTimestamp();
                    Int32 nextHeartbeatTimeoutMillis = heartbeatMillis - StopwatchExtensions.StopwatchTicksAsInt32Milliseconds(now - lastHearbeatTime);
                    if (nextHeartbeatTimeoutMillis <= 0)
                    {
                        Console.WriteLine("{0} [{1}] Sending hearbeat...", DateTime.Now, accessor.accessorEndPoint);

                        accessor.SendHeartbeat();
                        if (!accessor.Connected) break;

                        lastHearbeatTime = now;
                        nextHeartbeatTimeoutMillis = heartbeatMillis;
                    }

                    accessor.ReceiveWithTimeout(receiveBuffer, nextHeartbeatTimeoutMillis);
                }
            }
        }
    }
}
