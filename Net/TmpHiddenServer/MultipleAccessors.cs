using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace More.Net
{
    /*
    public static class MultipleAccessors
    {
        public static void Run(List<String> accessorConnectorStrings, Int32 heartbeatMillis, Int32 reconnectWaitMillis)
        {
            Accessor[] accessors = new Accessor[accessorConnectorStrings.Count];
            Int32 connectedSockets = 0;
            for (int i = 0; i < accessorConnectorStrings.Count; i++)
            {
                Accessor accessor = new Accessor(accessorConnectorStrings[i]);
                accessors[i] = accessor;
                accessor.TryConnect(ref connectedSockets);
            }

            //
            // TmpHiddenServer Loop
            //
            List<Socket> selectSockets = new List<Socket>(accessors.Length);
            Byte[] receiveBuffer = new Byte[16];
            Int64 lastHearbeatTime = Stopwatch.GetTimestamp();
            Int64 lastReconnectTime = Stopwatch.GetTimestamp();

            while (true)
            {
                Int64 now = Stopwatch.GetTimestamp();


                //
                // Check for heartbeats
                //
                Int32 nextHeartbeatTimeoutMillis = heartbeatMillis - StopwatchExtensions.StopwatchTicksAsInt32Milliseconds(now - lastHearbeatTime);
                if (nextHeartbeatTimeoutMillis <= 0)
                {
                    for (int i = 0; i < accessors.Length; i++)
                    {
                        Accessor accessor = accessors[i];
                        if (accessor.accessorSocket != null)
                        {
                            Console.WriteLine("{0} [{1}] Sending hearbeat...", DateTime.Now, accessor.accessorEndPoint);
                            try
                            {
                                accessor.accessorSocket.Send(Tmp.HeartBeatPacket);
                            }
                            catch (Exception)
                            {
                                accessor.accessorSocket = null;
                                connectedSockets--;
                            }
                        }
                    }

                    lastHearbeatTime = Stopwatch.GetTimestamp();
                    nextHeartbeatTimeoutMillis = heartbeatMillis;
                }

                //
                // Check for reconnects
                //
                Int32 nextReconnectTimeoutMillis = Int32.MaxValue;
                if (connectedSockets < accessors.Length)
                {
                    nextReconnectTimeoutMillis = reconnectWaitMillis - StopwatchExtensions.StopwatchTicksAsInt32Milliseconds(now - lastReconnectTime);
                    if (nextReconnectTimeoutMillis <= 0)
                    {
                        // Attempt reconnects
                        for (int i = 0; i < accessors.Length; i++)
                        {
                            Accessor accessor = accessors[i];
                            if (accessor.accessorSocket == null)
                            {
                                Console.WriteLine("{0} [{1}] Attempting reconnect...", DateTime.Now, accessor.accessorEndPoint);
                                if (accessor.TryConnect(ref connectedSockets))
                                {
                                    Console.WriteLine("{0} [{1}] Connected", DateTime.Now, accessor.accessorEndPoint);
                                }
                                else
                                {
                                    Console.WriteLine("{0} [{1}] Failed to connect", DateTime.Now, accessor.accessorEndPoint);
                                }
                            }
                        }

                        lastReconnectTime = Stopwatch.GetTimestamp();
                        nextReconnectTimeoutMillis = (connectedSockets < accessors.Length) ? reconnectWaitMillis : Int32.MaxValue;
                    }
                }


                Int32 timeoutMillis = (nextHeartbeatTimeoutMillis <= nextReconnectTimeoutMillis) ? nextHeartbeatTimeoutMillis : nextReconnectTimeoutMillis;


                selectSockets.Clear();
                for (int i = 0; i < accessors.Length; i++)
                {
                    Accessor accessor = accessors[i];
                    if (accessor.accessorSocket != null)
                    {
                        selectSockets.Add(accessor.accessorSocket);
                    }
                }

                if (selectSockets.Count != connectedSockets) throw new InvalidOperationException(String.Format(
                     "CodeBug: The number of connected sockets {0} did not match the number of sockets added to the Select Socket list {1}",
                     connectedSockets, selectSockets.Count));

                if (connectedSockets <= 0)
                {
                    Console.WriteLine("{0} No connected sockets...sleeping for {1} seconds", DateTime.Now, timeoutMillis / 1000);
                    Thread.Sleep(timeoutMillis);
                }
                else
                {
                    Console.WriteLine("{0} {1} connected sockets, timeout is {2} seconds", DateTime.Now, connectedSockets, timeoutMillis / 1000);
                    Socket.Select(selectSockets, null, null, timeoutMillis * 1000);

                    //Console.WriteLine("{0} Sockets Popped", selectSockets.Count);

                    for (int i = 0; i < selectSockets.Count; i++)
                    {
                        Socket accessorSocket = selectSockets[i];

                        Int32 bytesRead = 0;
                        try
                        {
                            bytesRead = accessorSocket.Receive(receiveBuffer);
                        }
                        catch (Exception)
                        {
                        }

                        if (bytesRead <= 0)
                        {

                            //
                            // Remove the disconnected socket
                            //
                            Boolean removed = false;
                            for (int accessorIndex = 0; accessorIndex < accessors.Length; accessorIndex++)
                            {
                                Accessor accessor = accessors[accessorIndex];
                                if (accessor.accessorSocket == accessorSocket)
                                {
                                    Console.WriteLine("{0} [{1}] Disconnected", DateTime.Now, accessor.accessorEndPoint);
                                    accessor.accessorSocket = null;
                                    accessor.
                                    connectedSockets--;
                                    removed = true;
                                    break;
                                }
                            }

                            if (!removed) throw new InvalidOperationException(String.Format(
                                 "CodeBug: Socket '{0}' got exception but it did not match any accessor sockets", accessorSocket.SafeRemoteEndPointString()));
                        }
                        else
                        {
                            Console.WriteLine("{0} [{1}] Received {0} bytes ", DateTime.Now, "???", bytesRead);
                        }
                    }
                }
            }

        }
    }
    */
}
