using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace More
{
    public class SelectTunnelsThread
    {
        class SocketPair
        {
            public readonly Socket a, b;
            readonly IDataHandler aToBDataHandler, bToADataHandler;
            public SocketPair(Socket a, Socket b)
                : this(a, null, b, null)
            {
            }
            public SocketPair(Socket a, IDataHandler aToBDataHandler, Socket b, IDataHandler bToADataHandler)
            {
                this.a = a;
                this.b = b;
                this.aToBDataHandler = aToBDataHandler;
                this.bToADataHandler = bToADataHandler;
            }
            public void SendFrom(Socket from, Byte[] data, UInt32 length)
            {
                if (from == a)
                {
                    if (aToBDataHandler == null)
                    {
                        b.Send(data, 0, (Int32)length, SocketFlags.None);
                    }
                    else
                    {
                        aToBDataHandler.HandleData(data, 0, length);
                    }
                }
                else if (from == b)
                {
                    if (bToADataHandler == null)
                    {
                        a.Send(data, 0, (Int32)length, SocketFlags.None);
                    }
                    else
                    {
                        bToADataHandler.HandleData(data, 0, length);
                    }
                }
                else
                {
                    throw new InvalidOperationException("CodeBug: Socket from did not match socket a or b");
                }
            }
        }

        readonly UInt32 receiveBufferLength;

        readonly ManualResetEvent haveTunnels;

        //
        // Any time a piece of code accesses the pairList or pairMap, they
        // should lock on this
        //
        readonly List<SocketPair> pairList = new List<SocketPair>();
        readonly Dictionary<Socket, SocketPair> pairMap = new Dictionary<Socket,SocketPair>();

        public SelectTunnelsThread(UInt32 receiveBufferLength)
        {
            this.receiveBufferLength = receiveBufferLength;
            this.haveTunnels = new ManualResetEvent(false);
            new Thread(Run).Start();
        }

        public void Add(Socket a, Socket b)
        {
            Add(a, null, b, null);
        }
        public void Add(Socket a, IDataHandler aToBDataHandler, Socket b, IDataHandler bToADataHandler)
        {
            SocketPair pair = new SocketPair(a, aToBDataHandler, b, bToADataHandler);
            lock (this)
            {
                pairList.Add(pair);
                pairMap.Add(a, pair);
                pairMap.Add(b, pair);
                haveTunnels.Set();
            }
        }

        //
        // The caller should be in a lock to call this method
        //
        void RemovePair(SocketPair pair)
        {
            if (pair.a.Connected)
            {
                try { pair.a.Shutdown(SocketShutdown.Both); } catch (Exception) { }
                pair.a.Close();
            }
            if (pair.b.Connected)
            {
                try { pair.b.Shutdown(SocketShutdown.Both); } catch (Exception) { }
                pair.b.Close();
            }
            pairList.Remove(pair);
            pairMap.Remove(pair.a);
            pairMap.Remove(pair.b);

            if (pairList.Count <= 0)
            {
                haveTunnels.Reset();
            }
        }
        
        void Run()
        {
            Byte[] receiveBuffer = new Byte[receiveBufferLength];
            List<Socket> selectSockets = new List<Socket>();

            while (true)
            {
                //
                // Wait till there are socket pairs
                //
                while (pairList.Count <= 0)
                {
                    //Console.WriteLine("[Debug] Waiting for tunnels...");
                    haveTunnels.WaitOne();
                    if (pairList.Count > 0)
                    {
                        //Console.WriteLine("[Debug] Now serving tunnels...");
                    }
                }
                
                //
                // Setup select sockets
                //
                selectSockets.Clear();
                lock(this)
                {
                    for (int i = 0; i < pairList.Count; i++)
                    {
                        SocketPair pair = pairList[i];
                        if (!pair.a.Connected || !pair.b.Connected)
                        {
                            RemovePair(pair);
                        }
                        else
                        {
                            selectSockets.Add(pair.a);
                            selectSockets.Add(pair.b);
                        }
                    }
                }

                //
                // Perform the select
                //
                if (selectSockets.Count > 0)
                {
                    Socket.Select(selectSockets, null, null, Int32.MaxValue);

                    for (int i = 0; i < selectSockets.Count; i++)
                    {
                        Socket socket = selectSockets[i];

                        Int32 bytesRead = 0;
                        try
                        {
                            bytesRead = socket.Receive(receiveBuffer);
                        }
                        catch (Exception)
                        {
                        }

                        if (bytesRead <= 0)
                        {
                            lock (this)
                            {
                                RemovePair(pairMap[socket]);
                            }
                        }
                        else
                        {
                            lock (this)
                            {
                                SocketPair pair = pairMap[socket];
                                pair.SendFrom(socket, receiveBuffer, (UInt32)bytesRead);
                            }
                        }
                    }
                }
            }
        }
    }
}
