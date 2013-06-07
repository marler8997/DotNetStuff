using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

using More;

namespace More.Net
{
    public class UdpConnectedClientTransmitter : IConnectedDatagramTransmitter
    {
        readonly Socket udpSocket;
        readonly SingleObjectList list;

        public UdpConnectedClientTransmitter(EndPoint serverEndPoint)
        {
            this.udpSocket = new Socket(serverEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            this.udpSocket.Connect(serverEndPoint);
            this.list = new SingleObjectList();
        }
        public int MaximumDatagramSize
        {
            get { return 0xFFFF; }
        }
        public EndPoint LocalEndPoint { get { return udpSocket.LocalEndPoint; } }
        public EndPoint RemoteEndPoint { get { return udpSocket.RemoteEndPoint; } }
        public Boolean DatagramAvailable
        {
            get { return udpSocket.Available > 0; }
        }

        public void Send(byte[] datagram, int datagramOffset, int datagramLength)
        {
            this.udpSocket.Send(datagram, datagramOffset, datagramLength, SocketFlags.None);
        }

        public int ReceiveHeaderNonBlocking()
        {
            throw new NotImplementedException();
        }

        //
        // TODO: Fix this function
        //
        public int ReceiveNonBlocking(byte[] buffer, int offset, int maxLength)
        {
            if (udpSocket.Available <= 0) return -1;
            if (udpSocket.Blocking == true) udpSocket.Blocking = false;
            return this.udpSocket.Receive(buffer, offset, maxLength, SocketFlags.None);
        }
        //
        // TODO: Fix this function
        //
        public int ReceiveBlocking(byte[] buffer, int offset, int maxLength, int timeoutMillis)
        {
            /*
            if (udpSocket.Blocking == false)
            {
                Console.WriteLine("[UdpDebug] UdpSocket was not blocking but now it should be");
                udpSocket.Blocking = true;
            }
            */
            if (timeoutMillis <= 0)
            {
                this.udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 0);
                if (udpSocket.Blocking == false)
                {
                    Console.WriteLine("[UdpDebug] UdpSocket was not blocking but now it should be");
                    udpSocket.Blocking = true;
                }
                return this.udpSocket.Receive(buffer, offset, offset + maxLength, SocketFlags.None);
            }

            list.obj = udpSocket;
            Socket.Select(list, null, null, timeoutMillis * 1000);
            if (list.obj == null) return -1; // Timeout

            Int32 bytesRead = this.udpSocket.Receive(buffer, offset, offset + maxLength, SocketFlags.None);
            if (bytesRead < 0) return -1; // This should never happen
            return bytesRead;
        }
        public void Dispose()
        {
            if (udpSocket != null) udpSocket.Close();
        }
    }


    public class UdpConnectedServerTransmitter : IConnectedDatagramTransmitter
    {
        readonly EndPoint remoteEndPoint;
        readonly Socket udpSocket;
        public UdpConnectedServerTransmitter(Socket udpSocket, EndPoint remoteEndPoint)
        {
            if (udpSocket == null || remoteEndPoint == null)
                throw new ArgumentNullException();

            this.udpSocket = udpSocket;
            this.remoteEndPoint = remoteEndPoint;
        }
        public Boolean DatagramAvailable
        {
            get { throw new NotImplementedException(); }
        }

        public int MaximumDatagramSize
        {
            get { return 0xFFFF; }
        }
        public EndPoint LocalEndPoint { get { return udpSocket.LocalEndPoint; } }
        public EndPoint RemoteEndPoint { get { return remoteEndPoint; } }

        public void Send(Byte[] data, Int32 offset, Int32 length)
        {
            Console.WriteLine("[TransmitterDebug] Sending {0} Bytes: {1}", length, BitConverter.ToString(data, offset, length));
            this.udpSocket.SendTo(data, offset, length, SocketFlags.None, remoteEndPoint);
        }

        public int ReceiveHeaderNonBlocking()
        {
            throw new NotImplementedException();
        }
        public Int32 ReceiveHeaderBlocking(byte[] buffer, int offset, int timeoutMillis)
        {
            throw new NotImplementedException();
        }
        public int ReceiveNonBlocking(byte[] buffer, int offset, int maxLength)
        {
            throw new InvalidOperationException("You cannot call receive on this type of udp transmitter");
        }

        public int ReceiveBlocking(byte[] buffer, int offset, int maxLength, int timeoutMillis)
        {
            throw new InvalidOperationException("You cannot call receive on this type of udp transmitter");
        }
        public void Dispose()
        {
        }
    }




    /*
    class DatagramAndEndPointQueue
    {
        readonly Int32 extendLength;

        CdpBufferPoolDatagramAndEndPoint[] queue;
        Int32 queueCount;

        Int32 firstPayloadIDInQueue;

        public DatagramAndEndPointQueue(Int32 initialCapacity, Int32 extendLength)
        {
            //if(extendLength < 1) throw new ArgumentOutOfRangeException();
            this.extendLength = extendLength;

            this.queue = new CdpBufferPoolDatagramAndEndPoint[initialCapacity];
            this.queueCount = 0;
        }

        public void QueueSend(Int32 payloadID, Byte[] datagram, Int32 length)
        {
            if (queue.Length <= queueCount)
            {
                CdpBufferPoolDatagramAndEndPoint[] newQueue = new CdpBufferPoolDatagramAndEndPoint[queue.Length + extendLength];
                Array.Copy(queue, newQueue, queue.Length);
                queue = newQueue;
            }

            if (queueCount <= 0)
            {
                queue[0].datagram = datagram;
                queue[0].length = length;
                queueCount = 1;
                firstPayloadIDInQueue = payloadID;
            }
            else
            {
                if (payloadID != firstPayloadIDInQueue + queueCount) throw new InvalidOperationException(
                     String.Format("You queued datagram with payload ID '{0}' but the queue has {1} datagram(s) and the first payload id in the queue is {2}",
                     payloadID, queueCount, firstPayloadIDInQueue));
                queue[queueCount].datagram = datagram;
                queue[queueCount].length = length;
                queueCount++;
            }
        }

        public void EmptyAndFree()
        {
            for (int i = 0; i < queueCount; i++)
            {
                Cdp.BufferPool.FreeBuffer(queue[i].datagram);
                queue[i].datagram = null; // get rid of the reference
            }
            queueCount = 0;
        }
    }

    public class UdpServerSocket
    {
        readonly IUnroutedDatagramTransmitter unroutedTransmitter;
        readonly Int32 maximumReceiveDatagram;

        readonly DatagramQueue datagramQueue;
        
        readonly Dictionary<EndPoint, CdpServerDatagramHandler> endPointToHandler;

        public UdpServerSocket(IUnroutedDatagramTransmitter unroutedTransmitter, Int32 maximumReceiveDatagram)
        {
            this.unroutedTransmitter = unroutedTransmitter;
            this.maximumReceiveDatagram = maximumReceiveDatagram;

            this.datagramQueue = new DatagramQueue(0, 1); // TODO: change this

            this.endPointToHandler = new Dictionary<EndPoint, CdpServerDatagramHandler>();
        }
        void ReceiveNonBlockingFrom(EndPoint endPoint, Byte[] buffer, Int32 offset, Int32 maxLength)
        {
            while (unroutedTransmitter.DatagramAvailable)
            {
                //Int32 bytesRead = unroutedTransmitter.ReceiveNonBlocking(buffer, offset, maxLength);

            }
        }
    }


    */





    public class VirtualDatagramTransmitter : IConnectedDatagramTransmitter
    {
        public Int32 DefaultInitialCapacity = 1024;

        class Datagram
        {
            public readonly Byte[] array;
            public readonly Int32 length;
            public Datagram(Byte[] array, Int32 length)
            {
                this.array = array;
                this.length = length;
            }
        }

        public readonly VirtualDatagramTransmitter otherTransmitter;

        readonly Queue<Datagram> sendQueue;

        public VirtualDatagramTransmitter()
        {
            this.otherTransmitter = new VirtualDatagramTransmitter(this);
            this.sendQueue = new Queue<Datagram>();
        }
        private VirtualDatagramTransmitter(VirtualDatagramTransmitter otherTransmitter)
        {
            this.otherTransmitter = otherTransmitter;
            this.sendQueue = new Queue<Datagram>();
        }
        public int MaximumDatagramSize { get { return 0xFFFF; } }
        public EndPoint LocalEndPoint { get { return null; } }
        public EndPoint RemoteEndPoint { get { return null; } }
        public void Send(Byte[] data, Int32 offset, Int32 length)
        {
            Byte[] sendBytes = Cdp.BufferPool.GetBuffer(length);
            Array.Copy(data, offset, sendBytes, 0, length);

            lock (sendQueue)
            {
                sendQueue.Enqueue(new Datagram(sendBytes, length));
                // If there could be someone waiting for this queue
                if(sendQueue.Count <= 1)
                {
                    Monitor.Pulse(sendQueue);
                }
            }
            // TODO: Notify other transmitter blocking receive
        }
        public Int32 ReceiveNonBlocking(Byte[] buffer, Int32 offset, Int32 maxLength)
        {
            Datagram datagram;
            lock (otherTransmitter.sendQueue)
            {
                if (otherTransmitter.sendQueue.Count <= 0) return -1;
                datagram = otherTransmitter.sendQueue.Dequeue();
            }

            Cdp.BufferPool.FreeBuffer(datagram.array);
            if (datagram.length > maxLength)
            {
                throw new InvalidOperationException(String.Format("The datagram is {0} bytes but your max length is {1}", datagram.length, maxLength));
            }
            Array.Copy(datagram.array, 0, buffer, offset, datagram.length);
            return datagram.length;
        }

        public Int32 ReceiveBlocking(Byte[] buffer, Int32 offset, Int32 maxLength, Int32 timeoutMillis)
        {
            Datagram datagram = null;
            while (true)
            {
                lock(otherTransmitter.sendQueue)
                {
                    if (otherTransmitter.sendQueue.Count > 0)
                    {
                        datagram = otherTransmitter.sendQueue.Dequeue();
                        break;
                    }
                    Console.WriteLine("[Debug] Monitor.wait on other transmitter's send queue");
                    Boolean lockAcquired = Monitor.Wait(otherTransmitter.sendQueue, timeoutMillis);
                    if (!lockAcquired)
                    {
                        Console.WriteLine("[Debug] ReceiveBlocking timed out (timeout is {0} milliseconds)", timeoutMillis);
                        return -1;
                    }
                    Console.WriteLine("[Debug] LockAcquired = {0}", lockAcquired);
                }
            }

            Cdp.BufferPool.FreeBuffer(datagram.array);
            if (datagram.length > maxLength)
            {
                throw new InvalidOperationException(String.Format("The datagram is {0} bytes but your max length is {1}", datagram.length, maxLength));
            }
            Array.Copy(datagram.array, 0, buffer, offset, datagram.length);
            return datagram.length;
        }
        public void Dispose()
        {
        }

        public Int32 DatagramsInSendQueue
        {
            get
            {
                return sendQueue.Count;
            }
        }

        public void Print(TextWriter writer)
        {
            lock (sendQueue)
            {
                lock (otherTransmitter.sendQueue)
                {
                    Console.WriteLine("----------------------------------------------");
                    Console.WriteLine("{0} buffers in send queue", sendQueue.Count);
                    Int32 i = 0;
                    foreach(Datagram datagram in sendQueue)
                    {
                        Console.WriteLine("   [{0}] {1} bytes", i, datagram.length);
                        i++;
                    }
                    Console.WriteLine("{0} buffers in receive queue", otherTransmitter.sendQueue.Count);
                    i = 0;
                    foreach (Datagram datagram in otherTransmitter.sendQueue)
                    {
                        Console.WriteLine("   [{0}] {1} bytes", i, datagram.length);
                        i++;
                    }
                    Console.WriteLine("----------------------------------------------");
                }
            }
        }

        #region IDatagramTransmitter Members


        public Boolean DatagramAvailable
        {
            get { throw new NotImplementedException(); }
        }

        public int ReceiveHeaderNonBlocking()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ClumsyTransmitter : IConnectedDatagramTransmitter
    {
        readonly IConnectedDatagramTransmitter underlyingTransmitter;
        readonly TextWriter debugLog;

        Int64 dropSentDatagramsUntil;
        Int64 dropReceivedDatagramsUntil;

        readonly Byte[] receiveBuffer;

        public ClumsyTransmitter(IConnectedDatagramTransmitter underlyingTransmitter, TextWriter debugLog)
        {
            this.underlyingTransmitter = underlyingTransmitter;
            this.debugLog = debugLog;

            this.dropSentDatagramsUntil = 0;
            this.dropReceivedDatagramsUntil = 0;

            this.receiveBuffer = new Byte[underlyingTransmitter.MaximumDatagramSize];
        }
        public EndPoint LocalEndPoint { get { return underlyingTransmitter.LocalEndPoint; } }
        public EndPoint RemoteEndPoint { get { return underlyingTransmitter.RemoteEndPoint; } }
        public void DropAllSentDatagramsForTheNext(Int32 millis)
        {
            dropSentDatagramsUntil = Stopwatch.GetTimestamp() + millis.MillisToStopwatchTicks();
        }
        Boolean DropSend()
        {
            Int64 now = Stopwatch.GetTimestamp();
            if (now <= dropSentDatagramsUntil)
            {
                if (debugLog != null) debugLog.WriteLine("[ClumsyTransmitter] Send packet dropped. Will drop for another {1} millisecond(s)",
                     (dropSentDatagramsUntil - now).StopwatchTicksAsInt64Milliseconds());
                return true;
            }
            return false;
        }
        public void DropAllReceivedDatagramsForTheNext(Int32 millis)
        {
            dropReceivedDatagramsUntil = Stopwatch.GetTimestamp() + millis.MillisToStopwatchTicks();
        }
        Boolean DropReceive()
        {
            return (Stopwatch.GetTimestamp() <= dropSentDatagramsUntil);
        }
        void DropAllAvailableDatagrams()
        {
            Int32 droppedDatagrams = 0;
            while (underlyingTransmitter.DatagramAvailable)
            {
                Int32 length = underlyingTransmitter.ReceiveNonBlocking(receiveBuffer, 0, receiveBuffer.Length);
                if (length < 0) throw new InvalidOperationException(String.Format(
                    "Underlying transmitter said datagram was available but  ReceiveNoBlocking returned {0}", length));

                if (debugLog != null) debugLog.WriteLine("[ClumsyTransmitter] Receive datagram dropped (length={0}).", length);
                droppedDatagrams++;
            }
            if (droppedDatagrams > 1)
            {
                if (debugLog != null) debugLog.WriteLine("[ClumsyTransmitter] Dropped {0} datagrams.", droppedDatagrams);
            }
        }

        public int MaximumDatagramSize
        {
            get { return underlyingTransmitter.MaximumDatagramSize; }
        }

        public void Send(byte[] data, int offset, int length)
        {
            Int64 now = Stopwatch.GetTimestamp();
            if (now <= dropSentDatagramsUntil)
            {
                if (debugLog != null) debugLog.WriteLine("[ClumsyTransmitter] Send packet dropped (length={0}).  Will drop for another {1} milliseconds",
                     length, (dropSentDatagramsUntil - now).StopwatchTicksAsInt64Milliseconds());
            }
            else
            {
                underlyingTransmitter.Send(data, offset, length);
            }
        }
        public Boolean DatagramAvailable
        {
            get
            {
                Boolean available = underlyingTransmitter.DatagramAvailable;
                if (available && DropReceive())
                {
                    DropAllAvailableDatagrams();
                    return false;
                }

                return available;
            }
        }
        public int ReceiveHeaderNonBlocking()
        {
            if (DropReceive())
            {
                DropAllAvailableDatagrams();
                return -1;
            }

            return underlyingTransmitter.ReceiveHeaderNonBlocking();
        }
        public int ReceiveNonBlocking(byte[] buffer, int offset, int maxLength)
        {
            if (DropReceive())
            {
                DropAllAvailableDatagrams();
                return -1;
            }

            return underlyingTransmitter.ReceiveNonBlocking(buffer, offset, maxLength);
        }
        public int ReceiveBlocking(byte[] buffer, int offset, int maxLength, int timeoutMillis)
        {
            Int64 diffStopwatchTicks = dropReceivedDatagramsUntil - Stopwatch.GetTimestamp();
            if (diffStopwatchTicks > 0)
            {
                Int32 waitTimeMillis = diffStopwatchTicks.StopwatchTicksAsInt32Milliseconds();
                DropAllAvailableDatagrams();

                if (timeoutMillis < waitTimeMillis)
                {
                    Console.WriteLine("[ClumsyTransmitter] Dropping all packets (timeout {0}) drop time {1} is larger than timeout", timeoutMillis, waitTimeMillis);
                    Thread.Sleep(timeoutMillis);
                    DropAllAvailableDatagrams();
                    return -1;
                }

                if (debugLog != null)
                    debugLog.WriteLine("[ClumsyTransmitter] Sleeping for {0} milliseconds until clumsy transmitter can receive again", waitTimeMillis);
                Thread.Sleep(waitTimeMillis);
                timeoutMillis -= waitTimeMillis;
                Console.WriteLine("[ClumsyTransmitter] Old Timeout {0} New Timeout After Drop {1}", timeoutMillis + waitTimeMillis, timeoutMillis);
            }

            return underlyingTransmitter.ReceiveBlocking(buffer, offset, maxLength, timeoutMillis);
        }
        public void Dispose()
        {
            underlyingTransmitter.Dispose();
        }
    }
}
