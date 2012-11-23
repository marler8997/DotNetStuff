using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Marler.NetworkTools
{
    public class UdpClientTransmitter : IDatagramTransmitter
    {
        EndPoint localEndPoint, connectedRemoteEndPoint;
        Socket udpSocket;
        public UdpClientTransmitter()
        {
            this.localEndPoint = null;
            this.connectedRemoteEndPoint = null;
            this.udpSocket = null;
        }
        public UdpClientTransmitter(Socket udpSocketAlreadyConnected)
        {
            this.udpSocket = udpSocketAlreadyConnected;
            this.connectedRemoteEndPoint = udpSocket.RemoteEndPoint;
        }
        public int MaximumDatagramSize
        {
            get { return 0xFFFF; }
        }
        public EndPoint RemoteEndPoint
        {
            get
            {
                if (connectedRemoteEndPoint == null) throw new InvalidOperationException("This udp transmitter is not connected");
                return connectedRemoteEndPoint;
            }
        }
        public Boolean DatagramAvailable
        {
            get { return udpSocket.Available > 0; }
        }
        public void Bind(EndPoint localEndPoint)
        {
            if (connectedRemoteEndPoint != null) throw new InvalidOperationException("You cannot bind a udp transmitter that is already connected");
            this.localEndPoint = localEndPoint;
            if (this.udpSocket == null)
            {
                this.udpSocket = new Socket(localEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                this.udpSocket.DontFragment = true;
            }
            this.udpSocket.Bind(localEndPoint);
        }

        public void Connect(EndPoint remoteEndPoint)
        {
            if (connectedRemoteEndPoint != null) throw new InvalidOperationException("This udp transmitter is already connected");
            if (this.udpSocket == null)
            {
                this.udpSocket = new Socket(remoteEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                this.udpSocket.DontFragment = true;
            }
            this.udpSocket.Connect(remoteEndPoint);
            this.connectedRemoteEndPoint = remoteEndPoint;
        }
        public void Send(byte[] datagram, int datagramOffset, int datagramLength)
        {
            if (connectedRemoteEndPoint == null) throw new InvalidOperationException("This udp transmitter is not connected");
            this.udpSocket.Send(datagram, datagramOffset, datagramLength, SocketFlags.None);
        }

        public Boolean ReceiveHeaderBlocking(byte[] buffer, int offset, int timeoutMillis)
        {
            if (connectedRemoteEndPoint == null) throw new InvalidOperationException("This udp transmitter is not connected");
            if (udpSocket.Blocking == false) udpSocket.Blocking = true;

            Int64 stopwatchTicks = Stopwatch.GetTimestamp();
            Int32 nextReceiveTimeout = timeoutMillis;
            while (true)
            {
                Int32 bytesRead = this.udpSocket.Receive(buffer, offset, 2, SocketFlags.None); // Use nextReceiveTimeout
                if (bytesRead > 0) return false;
                while (this.udpSocket.Available > 0)
                {
                    bytesRead = this.udpSocket.Receive(buffer, offset, 2, SocketFlags.None);
                    if (bytesRead > 0) return false;
                }

                nextReceiveTimeout = timeoutMillis - (Stopwatch.GetTimestamp() - stopwatchTicks).StopwatchTicksAsInt32Milliseconds();
                if (nextReceiveTimeout <= 0) return true; // Timeout
            }

        }
        public int ReceiveHeaderBlocking(int timeoutMillis)
        {
            throw new NotImplementedException();
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
            if (connectedRemoteEndPoint == null) throw new InvalidOperationException("This udp transmitter is not connected");
            if (udpSocket.Blocking == true) udpSocket.Blocking = false;
            return this.udpSocket.Receive(buffer, offset, maxLength, SocketFlags.None);
        }
        //
        // TODO: Fix this function
        //
        public int ReceiveBlocking(byte[] buffer, int offset, int maxLength, int timeoutMillis)
        {
            if (connectedRemoteEndPoint == null) throw new InvalidOperationException("This udp transmitter is not connected");
            if (udpSocket.Blocking == false) udpSocket.Blocking = true;
            return this.udpSocket.Receive(buffer, offset, maxLength, SocketFlags.None);
        }
        public void Dispose()
        {
            this.connectedRemoteEndPoint = null;
            this.localEndPoint = null;
            Socket udpSocket = this.udpSocket;
            if (udpSocket != null) udpSocket.Close();
        }
    }
    public class UdpServerTransmitter : IDatagramTransmitter
    {
        readonly EndPoint remoteEndPoint;
        readonly Socket udpSocket;
        public UdpServerTransmitter(Socket udpSocket, EndPoint remoteEndPoint)
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
        public EndPoint RemoteEndPoint
        {
            get
            {
                return remoteEndPoint;
            }
        }

        public void Send(Byte[] data, Int32 offset, Int32 length)
        {
            Console.WriteLine("[TransmitterDebug] Sending {0} Bytes: {1}", length, BitConverter.ToString(data, offset, length));
            this.udpSocket.SendTo(data, offset, length, SocketFlags.None, remoteEndPoint);
        }

        public int ReceiveHeaderNonBlocking()
        {
            throw new NotImplementedException();
        }
        public bool ReceiveHeaderBlocking(byte[] buffer, int offset, int timeoutMillis)
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
    public class SharedDatagramTransmitter : IDatagramTransmitter
    {
        readonly Dictionary<EndPoint, IDatagramTransmitter> endPointToTransmitter;
        readonly List<EndPoint> endPointList;

        public SharedDatagramTransmitter()
        {
            this.endPointToTransmitter = new Dictionary<EndPoint, IDatagramTransmitter>();
            this.endPointList = new List<EndPoint>();
        }

        public IDatagramTransmitter NewTransmitter(EndPoint remoteEndPoint, ITransmitterFactory factory)
        {
            IDatagramTransmitter transmitter = factory.Create(remoteEndPoint);
            if (!endPointToTransmitter.ContainsKey(remoteEndPoint))
            {
                endPointList.Add(remoteEndPoint);
                endPointToTransmitter.Add(remoteEndPoint, transmitter);
            }
            else
            {
                endPointToTransmitter[remoteEndPoint] = transmitter;
            }
            return transmitter;
        }
    }
    */





    public class VirtualDatagramTransmitter : IDatagramTransmitter
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
        public int MaximumDatagramSize {get { return 0xFFFF; }}
        public EndPoint RemoteEndPoint
        {
            get { return null; }
        }
        public void Bind(EndPoint localEndPoint)
        {
            throw new NotSupportedException();
        }
        public void Connect(EndPoint remoteEndPoint)
        {
            throw new NotSupportedException();
        }
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
                    if (!lockAcquired) throw new TimeoutException(String.Format("ReceiveBlocking timed out (timeout is {0} milliseconds)", timeoutMillis));
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

        public int ReceiveHeaderBlocking(int timeoutMillis)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDatagramTransmitter Members


        public bool ReceiveHeaderBlocking(byte[] buffer, int offset, int timeoutMillis)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ClumsyTransmitter : IDatagramTransmitter
    {
        readonly IDatagramTransmitter underlyingTransmitter;
        readonly TextWriter debugLog;
        Int64 dropSentDatagramsUntil;

        public ClumsyTransmitter(IDatagramTransmitter underlyingTransmitter, TextWriter debugLog)
        {
            this.underlyingTransmitter = underlyingTransmitter;
            this.debugLog = debugLog;
            this.dropSentDatagramsUntil = 0;
        }
        public void DropAllSentDatagramsForTheNext(Int32 millis)
        {
            dropSentDatagramsUntil = Stopwatch.GetTimestamp() + millis.MillisToStopwatchTicks();
        }
        public int MaximumDatagramSize
        {
            get { return underlyingTransmitter.MaximumDatagramSize; }
        }
        public EndPoint RemoteEndPoint
        {
            get { return underlyingTransmitter.RemoteEndPoint; }
        }
        /*
        public void Bind(EndPoint localEndPoint)
        {
            underlyingTransmitter.Bind(localEndPoint);
        }
        public void Connect(EndPoint remoteEndPoint)
        {
            underlyingTransmitter.Connect(remoteEndPoint);
        }
        */
        public void Send(byte[] data, int offset, int length)
        {
            Int64 now = Stopwatch.GetTimestamp();
            if(now <= dropSentDatagramsUntil)
            {
                if(debugLog != null) debugLog.WriteLine("[ClumsyTransmitter] Send packet dropped (length={0}).  Will drop for another {1} milliseconds",
                    length, (dropSentDatagramsUntil - now).StopwatchTicksAsInt64Milliseconds());
            }
            else
            {
                Console.WriteLine("Sent");
                underlyingTransmitter.Send(data, offset, length);
            }
        }
        public int ReceiveNonBlocking(byte[] buffer, int offset, int maxLength)
        {
            return underlyingTransmitter.ReceiveNonBlocking(buffer, offset, maxLength);
        }
        public int ReceiveBlocking(byte[] buffer, int offset, int maxLength, int timeoutMillis)
        {
            return underlyingTransmitter.ReceiveBlocking(buffer, offset, maxLength, timeoutMillis);
        }
        public void Dispose()
        {
            throw new NotImplementedException();
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

        public int ReceiveHeaderBlocking(int timeoutMillis)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDatagramTransmitter Members


        public bool ReceiveHeaderBlocking(byte[] buffer, int offset, int timeoutMillis)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
