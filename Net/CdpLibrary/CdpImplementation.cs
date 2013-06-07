using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using System.Threading;

using More;

namespace More.Net
{
    enum CdpFlagValue : byte
    {
        PayloadNoAck   =  0,
        PayloadWithAck =  1,
        RandomPayload  =  6,
        Ack            =  8,
        Resend         =  9,
        Halt           = 15,
    }
    enum CdpFlagValueFlag : byte
    {
        ImmediateAck = 0x1,
        GiveControl  = 0x2,
    }
    public static class Cdp
    {
        public const Int32 HeaderLengthWithPayloadID    = 2;
        public const Int32 HeaderLengthWithoutPayloadID = 1;

        public const Int32 MaxDatagramOverUdp           = 0xFFFF;

        public const Int32 MaxPayloadWithIDOverUdp      = MaxDatagramOverUdp - HeaderLengthWithPayloadID;
        public const Int32 MaxPayloadWithoutIDOverUdp   = MaxDatagramOverUdp - HeaderLengthWithoutPayloadID;

        public const Int32 MaxPayloadID                 = 0x0FFF;


        private static LinearBucketSizeBufferPool bufferPool = null; // Note that this buffer pool is thread safe

        public static void StaticInit(Int32 maxPayloadWithID)
        {
            if (maxPayloadWithID > MaxPayloadWithIDOverUdp)
                throw new ArgumentOutOfRangeException(String.Format("You supplied a max payload (with id) of {0} but the max is {1}", maxPayloadWithID, MaxPayloadWithIDOverUdp));
            
            lock(typeof(Cdp))
            {
                if (bufferPool != null) throw new InvalidOperationException("Static Initialization for Cdp already done");
                bufferPool = new LinearBucketSizeBufferPool(maxPayloadWithID + HeaderLengthWithPayloadID, 64, 256, 4);
            }
        }
        public static void TryStaticInit(Int32 maxPayloadWithID)
        {
            if (bufferPool == null)
            {
                try
                {
                    StaticInit(maxPayloadWithID);
                }
                catch (InvalidOperationException e)
                {
                    if (bufferPool == null) throw e;
                }
            }

            if (bufferPool.maxBufferSize < maxPayloadWithID)
                throw new InvalidOperationException(String.Format(
                    "You tried to initialize Cdp with a max payload of {0}, but Cdp was already initialized with max payload of {1}",
                    maxPayloadWithID, bufferPool.maxBufferSize));
        }
        public static LinearBucketSizeBufferPool BufferPool
        {
            get
            {
                if (bufferPool == null) throw new InvalidOperationException("No one has called StaticInit on Cdp");
                return bufferPool;
            }
        }

        // If return value is 1, the payload id is the next in the seqeunce, 
        // If the return value is < 0x800, the payload is out of order
        // Else, (== 0 or >= 0x800), the payload is a resend of a previous payload
        public static Int32 PayloadDiff(Int32 currentPayloadID, Int32 previousPayloadID)
        {
            Int32 payloadIDDiff = (MaxPayloadID & currentPayloadID) - (MaxPayloadID & previousPayloadID);
            if (payloadIDDiff < 0) payloadIDDiff += MaxPayloadID + 1;
            return payloadIDDiff;
        }

        public static void UdpServerLoop(ICdpServer server, Socket udpSocket, EndPoint listenEndPoint, Byte[] maxDatagramBuffer, ICdpTimeout timeout)
        {
            Dictionary<EndPoint, CdpServerDatagramHandler> endPointToHandler =
                new Dictionary<EndPoint, CdpServerDatagramHandler>();

            //Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.Bind(listenEndPoint);

            try
            {

                while (true)
                {
                    EndPoint from = listenEndPoint;
                    int bytesRead;
                    try
                    {
                        bytesRead = udpSocket.ReceiveFrom(maxDatagramBuffer, ref from);
                    }
                    catch (SocketException e)
                    {
                        Boolean stopServerGracefully = server.SocketException(e);
                        if (stopServerGracefully)
                        {
                            throw new NotImplementedException("Stop server gracefully not implemented");
                        }
                        continue;
                    }

                    if (bytesRead <= 0)
                    {
                        // it's just a heartbeat
                        Console.WriteLine("[CdpDebug] Got a heartbeat from '{0}'", from);
                        continue;
                    }

                    CdpServerDatagramHandler handler;

                    //
                    // Handle new connection
                    //
                    if (!endPointToHandler.TryGetValue(from, out handler))
                    {
                        CdpTransmitter transmitter = new CdpTransmitter(new UdpConnectedServerTransmitter(udpSocket, from));

                        ICdpServerHandler serverHandler;
                        Int32 maxSendBeforeAck;
                        Boolean refuseConnection = server.NewConnection(transmitter, out serverHandler, out maxSendBeforeAck);

                        if (refuseConnection)
                        {
                            handler.Closed();
                            server.ConnectionClosed(from);
                            throw new NotImplementedException("Refusing connection is not yet implemented");
                        }

                        if (serverHandler == null)
                        {
                            handler.Closed();
                            server.ConnectionClosed(from);
                            throw new InvalidOperationException("You provided a null payload handler");
                        }

                        handler = new CdpServerDatagramHandler(transmitter, serverHandler, timeout);
                        endPointToHandler.Add(from, handler);
                    }

                    Boolean closeClient = handler.Datagram(maxDatagramBuffer, 0, bytesRead);
                    if (closeClient)
                    {
                        handler.Closed();
                        server.ConnectionClosed(from);
                        endPointToHandler.Remove(from);
                    }
                }

            }
            finally
            {
                if (udpSocket != null) udpSocket.Close();
            }
        }
    }

    public class CdpServerOverUdp
    {
        public readonly EndPoint endPoint;
        public readonly Socket udpSocket;
        public CdpServerOverUdp(EndPoint endPoint)
        {
            this.endPoint = endPoint;
            this.udpSocket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        }
        public void Run(ICdpServer server, Byte[] maxDatagramBuffer, ICdpTimeout timeout)
        {
            Cdp.UdpServerLoop(server, udpSocket, endPoint, maxDatagramBuffer, timeout);
        }
    }

    /// <summary>
    /// Thrown if a halt packet is received without performing a gracefull halt
    /// </summary>
    public class CdpBadHaltException : Exception
    {
        public CdpBadHaltException()
            : base()
        {
        }
    }

    struct CdpBufferPoolDatagram
    {
        public Byte[] datagram;
        public Int32 length;
        /*
        public CdpBufferPoolDatagram(Byte[] datagram, Int32 length)
        {
            this.datagram = datagram;
            this.length = length;
        }
        */
    }
    struct CdpBufferPoolDatagramAndEndPoint
    {
        public EndPoint endPoint;
        public Byte[] datagram;
        public Int32 length;
        /*
        public CdpBufferPoolDatagram(EndPoint endPoint, Byte[] datagram, Int32 length)
        {
            this.endPoint = endPoint;
            this.datagram = datagram;
            this.length = length;
        }
        */
    }
    class DatagramQueue
    {
        readonly Int32 extendLength;

        public CdpBufferPoolDatagram[] queue;
        Int32 queueCount;

        Int32 firstPayloadIDInQueue;

        public DatagramQueue(Int32 initialCapacity, Int32 extendLength)
        {
            //if(extendLength < 1) throw new ArgumentOutOfRangeException();
            this.extendLength = extendLength;

            this.queue = new CdpBufferPoolDatagram[initialCapacity];
            this.queueCount = 0;
        }
        public Int32 PayloadIDToIndex(Int32 payloadID)
        {
            return payloadID - firstPayloadIDInQueue;
        }
        public void QueueSend(Int32 payloadID, Byte[] datagram, Int32 length)
        {
            if(queue.Length <= queueCount)
            {
                CdpBufferPoolDatagram[] newQueue = new CdpBufferPoolDatagram[queue.Length + extendLength];
                Array.Copy(queue, newQueue, queue.Length);
                queue = newQueue;
            }

            if(queueCount <= 0)
            {
                queue[0].datagram = datagram;
                queue[0].length = length;
                queueCount = 1;
                firstPayloadIDInQueue = payloadID;
            }
            else
            {
                if(payloadID != firstPayloadIDInQueue + queueCount) throw new InvalidOperationException(
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
    public class CdpTransmitter
    {
        public const Int32 QueueInitialCapacity = 0;
        public const Int32 QueueExtendLength = 1;

        IConnectedDatagramTransmitter connectedDatagramTransmitter;

        Byte[] lastSendBufferRequested;

        internal Int32 nextPayloadID;
        
        readonly DatagramQueue datagramQueue;
        
        private readonly Byte[] headerBuffer;        

        private Int32 averageLatency; // Maybe this should be the max RTT of the last X packets
        private Int32 latencyMeasurementsInAverage;

        public CdpTransmitter(IConnectedDatagramTransmitter connectedDatagramTransmitter)
        {
            this.connectedDatagramTransmitter = connectedDatagramTransmitter;

            this.lastSendBufferRequested = null;

            this.nextPayloadID = 0;

            this.datagramQueue = new DatagramQueue(QueueInitialCapacity, QueueExtendLength);

            this.headerBuffer = new Byte[Cdp.HeaderLengthWithPayloadID];

            this.latencyMeasurementsInAverage = 0;
        }

        public EndPoint RemoteEndPoint { get { return connectedDatagramTransmitter.RemoteEndPoint; } }
        
        //
        // This method must be called to get a buffer before sending
        // This function returns the offset into the buffer that the client should put its data
        //
        public Byte[] RequestSendBuffer(Int32 payloadSize, out Int32 payloadOffset)
        {
            if (this.lastSendBufferRequested != null)
                throw new InvalidOperationException("You've already requested a send buffer");

            
            // NOTE: this buffer is retrieved from the CDP Buffer Pool, it must be freed when it is
            //       no longer needed
            this.lastSendBufferRequested = Cdp.BufferPool.GetBuffer(Cdp.HeaderLengthWithPayloadID + payloadSize);
            

            payloadOffset = Cdp.HeaderLengthWithPayloadID;
            return this.lastSendBufferRequested;
        }

        public void SendHearbeat()
        {
            connectedDatagramTransmitter.Send(headerBuffer, 0, 0);
        }
        public void SendHaltNoPayload()
        {
            headerBuffer[0] = (Byte)CdpFlagValue.Halt << 4;
            connectedDatagramTransmitter.Send(headerBuffer, 0, 1);
        }
        public void HandlerSendHeader(Byte flagValue, Int32 payloadID)
        {
            headerBuffer[0] = (Byte)(         ((Byte)flagValue << 4)  |
                                      (0x0F & (      payloadID >> 8)) );
            headerBuffer[1] = (Byte) payloadID;
            connectedDatagramTransmitter.Send(headerBuffer, 0, 2);
        }
        private Byte[] GetRequestedBuffer(Int32 offsetLimit)
        {
            if (offsetLimit < Cdp.HeaderLengthWithPayloadID)
                throw new ArgumentOutOfRangeException(String.Format("The offset limit you provided ({0}) is out of range (must be >= {1})",
                    offsetLimit, Cdp.HeaderLengthWithPayloadID));

            Byte[] bufferToSend = this.lastSendBufferRequested;
            this.lastSendBufferRequested = null;
            if (bufferToSend == null) throw new InvalidOperationException("You haven't requested a send buffer");
            return bufferToSend;
        }
        public void ControllerSendRandomPayload(Int32 offsetLimit)
        {
            Byte[] bufferToSend = GetRequestedBuffer(offsetLimit);

            // Cdp Header
            bufferToSend[1] = ((Byte)CdpFlagValue.RandomPayload << 4);
            connectedDatagramTransmitter.Send(bufferToSend, 1, offsetLimit);
            Cdp.BufferPool.FreeBuffer(bufferToSend);
        }
        public void ControllerSendPayloadNoAck(Int32 offsetLimit)
        {
            // TODO: Check for acks/resends/halts

            Byte[] bufferToSend = GetRequestedBuffer(offsetLimit);

            // Cdp Header
            Int32 payloadID = nextPayloadID++;
            
            bufferToSend[0] = (Byte)(         ((Byte)CdpFlagValue.PayloadNoAck << 4)  |
                                      (0x0F & (      payloadID                 >> 8)) );
            bufferToSend[1] = (Byte)payloadID;

            // Queue the datagram (because there is no ack
            datagramQueue.QueueSend(payloadID, bufferToSend, offsetLimit);

            connectedDatagramTransmitter.Send(bufferToSend, 0, offsetLimit);            
        }
        public void ControllerSendPayloadWithAck(Int32 offsetLimit, ICdpTimeout timeout)
        {
            // 1. Check for acks/resends/halts


            Byte[] bufferToSend = GetRequestedBuffer(offsetLimit);

            try
            {
                // Cdp Header
                Int32 payloadID = nextPayloadID++;                
                bufferToSend[0] = (Byte)(         ((Byte)CdpFlagValue.PayloadWithAck << 4)  |
                                        (0x0F & (        payloadID                   >> 8)) );
                bufferToSend[1] = (Byte)payloadID;

                //
                // Send the datagram
                //
                connectedDatagramTransmitter.Send(bufferToSend, 0, offsetLimit);

                Int64 stopwatchTicksAfterSend = Stopwatch.GetTimestamp();

                Int32 timeoutMillis = timeout.WaitForAckInitialRetryTimeout(averageLatency);
                if(timeoutMillis < 0) throw new InvalidOperationException(String.Format(
                    "The ICdpTimeout class '{0}' returned negative ({1}) when calling WaitForAckInitialTimeout({2})",
                    timeout.GetType().Name, timeoutMillis, averageLatency));

                Int32 retries = 0;

                // Keep resending the datagram until a header is recevied or timeout is reached
                while (true)
                {
                    Console.WriteLine("Send Retry {0}", retries);
                    Int32 bytesRead = connectedDatagramTransmitter.ReceiveBlocking(headerBuffer, 0, 2, timeoutMillis);

                    if (bytesRead < 0)
                    {
                        Int32 elapsedMillis = (Stopwatch.GetTimestamp() - stopwatchTicksAfterSend).StopwatchTicksAsInt32Milliseconds();
                        timeoutMillis = timeout.WaitForAckRetryOrTimeout(retries, averageLatency, elapsedMillis, timeoutMillis);
                        if (timeoutMillis <= 0) throw new TimeoutException(String.Format("Timed out waiting for ack: {0} retries {1} milliseconds elapsed", retries, elapsedMillis));

                        // Retry sending the packet
                        connectedDatagramTransmitter.Send(bufferToSend, 0, offsetLimit);
                        retries++;
                        continue;
                    }

                    //
                    // Check the datagram
                    //
                    if (bytesRead == 0)
                    {
                        // It's just a heart beat packet
                    }
                    else
                    {
                        Byte receivedFlagValue = (Byte)(headerBuffer[0] >> 4);
                        Int32 receivedPayloadID = (0xF00 & (headerBuffer[0] << 8)) | (0xFF & headerBuffer[1]);
                        if (receivedFlagValue == (Byte)CdpFlagValue.Ack)
                        {
                            if (receivedPayloadID == payloadID)
                            {
                                Console.WriteLine("[CdpDebug] Received ACK for payload id {0}", payloadID);
                                break;
                            }
                            Console.WriteLine("[CdpDebug] Got an ack for an old payload id {0}?", receivedPayloadID);
                        }
                        else if (receivedFlagValue == (Byte)CdpFlagValue.Halt)
                        {
                            throw new CdpBadHaltException();
                        }
                        else if (receivedFlagValue == (Byte)CdpFlagValue.Resend)
                        {
                            if (receivedPayloadID <= payloadID)
                            {
                                Int32 index = datagramQueue.PayloadIDToIndex(receivedPayloadID);
                                if (index >= 0)
                                {
                                    if (receivedPayloadID < payloadID)
                                    {
                                        do
                                        {
                                            Console.WriteLine("[Debug] Queue Index {0} Payload ID {1}", index, receivedPayloadID + index);
                                            CdpBufferPoolDatagram datagram = datagramQueue.queue[index];
                                            connectedDatagramTransmitter.Send(datagram.datagram, 0, datagram.length);
                                            index++;
                                        } while (receivedPayloadID + index < payloadID);
                                    }
                                    connectedDatagramTransmitter.Send(bufferToSend, 0, offsetLimit);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Unknown flag value {0} from '{1}' (Maybe I should throw an exception? TBD)",
                                receivedFlagValue, connectedDatagramTransmitter.RemoteEndPoint);

                        }
                    }

                    //
                    // Get next timeout
                    //
                    {
                        Int32 elapsedMillis = (Stopwatch.GetTimestamp() - stopwatchTicksAfterSend).StopwatchTicksAsInt32Milliseconds();
                        timeoutMillis = timeout.WaitForAckRetryOrTimeout(retries, averageLatency, elapsedMillis, timeoutMillis);
                        if (timeoutMillis <= 0) throw new TimeoutException(String.Format("Timed out waiting for ack: {0} retries {1} milliseconds elapsed", retries, elapsedMillis));
                    }
                }

                datagramQueue.EmptyAndFree(); // free the queue because everything has been acked
            }
            finally
            {
                Cdp.BufferPool.FreeBuffer(bufferToSend);
            }
        }
    }

    public class CdpServerDatagramHandler
    {
        readonly CdpTransmitter transmitter;
        readonly ICdpServerHandler serverHandler;
        readonly ICdpTimeout timeout;

        //Int32 lastPayloadIDReceived;

        public CdpServerDatagramHandler(CdpTransmitter transmitter,
            ICdpServerHandler serverHandler, ICdpTimeout timeout)
        {
            this.transmitter = transmitter;
            this.serverHandler = serverHandler;
            this.timeout = timeout;

            //this.lastPayloadIDReceived = Cdp.MaxPayloadID;
        }
        public void ServerStopped()
        {
        }
        public void Closed()
        {

        }

        //
        // This function implements an algorithm in the CDP protocol
        // specification. See CDP documentation to see the pseudocode for this function.
        //
        // Returns true to close the client
        //
        public Boolean Datagram(Byte[] datagram, Int32 offset, Int32 length)
        {
            /*
            Console.WriteLine("[CdpDebug] Got Datagram {0} Bytes: {1}",
                length, (length <= 0) ? "<null>" : (length < 10) ? BitConverter.ToString(datagram, offset, length) :
                BitConverter.ToString(datagram, offset, 10) + "...");
            */


            if(length <= 0) return false; // just a heartbeat
            
            Byte flagValue = (Byte)(datagram[offset] >> 4);

            //
            // Check if it is a halt or an out of order datagram
            //
            if(flagValue > 7)
            {
                if (flagValue == (Byte)CdpFlagValue.Halt)
                {
                    serverHandler.Halt();
                    return true; // true to close client
                }
                if (flagValue == (Byte)CdpFlagValue.Resend)
                {
                    throw new NotImplementedException();
                }
                // The packet is a handler packet, so it should be ignored
                return false;
            }

            //
            // Check if it contains a payload with an id
            //
            if(flagValue < 6)
            {
                if(length < 2) {
                    // The datagram is not valid CDP, halt
                    Console.WriteLine("[CdpDebug] Received Invalid CDP from client, because it has a flag value of {0}, but the datagram is not long enough for the id", flagValue);
                    transmitter.SendHaltNoPayload();
                    serverHandler.Halt();
                    return true;
                }

                Int32 payloadID = (0xF00 & (datagram[offset] << 8)) | (0xFF & datagram[offset + 1]);

                while(true)
                {
                    Int32 payloadDiff = Cdp.PayloadDiff(payloadID, transmitter.nextPayloadID);
                    if(payloadDiff == 0) break;

                    // Check if it's a resend
                    if (payloadDiff == Cdp.MaxPayloadID)
                    {
                        if (flagValue < 4 && ((flagValue & (Byte)CdpFlagValueFlag.ImmediateAck) != 0))
                        {
                            Console.WriteLine("[CdpDebug] Got a resend of payload {0}. The payload requested an ACK but the ACK must have been lost.", payloadID);
                            // Resend the ack
                            transmitter.HandlerSendHeader((Byte)CdpFlagValue.Ack, payloadID);
                        }
                    }

                    if (payloadDiff >= 0x800)
                    {
                        // The packet is a resend so ignore it
                        Console.WriteLine("[CdpDebug] Received resend of payload id {0}", payloadID);
                        return false;
                    }

                    //throw new NotImplementedException(String.Format("Received payload id of {0} but the diff is {1} so it is out of order. Out of order packets are not yet implemented",
                    //    payloadID, payloadDiff));

                    // Request a resend
                    transmitter.HandlerSendHeader((Byte)CdpFlagValue.Resend, transmitter.nextPayloadID);
                    return false;
                }

                // Update last payload id received
                transmitter.nextPayloadID++;

                //
                // If it's not a close or halt with payload, send immediate ack if requested
                //
                if(flagValue < 4 && ( (flagValue & (Byte)CdpFlagValueFlag.ImmediateAck) != 0))
                {
                    Console.WriteLine("[CdpDebug] (FlagValue={0}) Sending Ack of payload id {1}", flagValue, payloadID);
                    // Ack the payload
                    transmitter.HandlerSendHeader((Byte)CdpFlagValue.Ack, payloadID);
                }

                // Handle the payload
                Boolean closeClient = serverHandler.Payload(datagram, offset + 2, length - 2);
                if(closeClient)
                {
                    // close the connection
                    transmitter.SendHaltNoPayload();
                    serverHandler.Halt();
                    return true; // don't keep client alive
                }

                if(flagValue >= 4) // Either Close or Halt
                {
                    if(flagValue == 5) // It was a close
                    {
                        Boolean acknowledgeClose = serverHandler.Close();
                        if (acknowledgeClose) throw new NotImplementedException();
                        else
                        {
                            transmitter.SendHaltNoPayload();
                        }
                    }

                    serverHandler.Halt();
                    return true;
                }

                if ((flagValue & (Byte)CdpFlagValueFlag.GiveControl) != 0)
                {
                    Int32 sendBufferOffsetLimit;
                    Boolean requestImmediateAck;
                    serverHandler.GotControl(null, out sendBufferOffsetLimit, out requestImmediateAck);
                }

                return false;
            }
            else if (flagValue == (Byte)CdpFlagValue.RandomPayload)
            {
                return serverHandler.RandomPayload(datagram, 1, length - 1);
            }
            else
            {
                // The datagram is not valid CDP, halt
                Console.WriteLine("[CdpDebug] Received Invalid CDP from client, unrecognized flag value of {0}", flagValue);
                transmitter.SendHaltNoPayload();
                serverHandler.Halt();
                return true;
            }
        }
    }

}
