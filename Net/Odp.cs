using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Marler.Net
{
    public static class Odp
    {
        public const Int32 HeaderLength        = 3;
        public static Int32 MaxPacketOverUdp  = 0xFFFF; // This is the maximum length of the odp header plus the odp payload
        public static Int32 MaxPayloadOverUdp = MaxPacketOverUdp - Odp.HeaderLength;

        private static LinearBucketSizeBufferPool bufferPool = null; // Note that this buffer pool is thread safe
        public static void StaticInit()
        {
            StaticInit(MaxPayloadOverUdp);
        }
        public static void StaticInit(Int32 maxPayload)
        {
            if (maxPayload > MaxPacketOverUdp - HeaderLength)
                throw new ArgumentOutOfRangeException(String.Format("You supplied a max payload of {0} but the max is {1}", maxPayload, MaxPacketOverUdp - HeaderLength));

            if (bufferPool != null) throw new InvalidOperationException("Static Initialization for Odp already done");
            bufferPool = new LinearBucketSizeBufferPool(maxPayload + HeaderLength, 64, 256, 4);
        }
        public static LinearBucketSizeBufferPool BufferPool
        {
            get
            {
                if (bufferPool == null) throw new InvalidOperationException("No one has called StaticInit on OdpOverUdp");
                return bufferPool;
            }
        }
        public static void ReceiverLoop(EndPoint endPoint, Byte[] readBytes,
            IOdpConnectionHandler connectionCallback, IOdpErrorHandler errorCallback)
        {
            Dictionary<EndPoint, IOdpPayloadHandler> endPointToOdpPayloadHandler =
                new Dictionary<EndPoint, IOdpPayloadHandler>();

            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.Bind(endPoint);

            while (true)
            {
                int bytesRead;
                try
                {
                    bytesRead = udpSocket.ReceiveFrom(readBytes, ref endPoint);
                }
                catch(SocketException e)
                {
                    if(errorCallback == null) throw e;
                    ServerInstruction instruction = errorCallback.SocketException(e);
                    if((instruction & ServerInstruction.StopServer) != 0) return;
                    continue;
                }

                // Missing Odp Header
                if (bytesRead < 3)
                {
                    OdpMissingHeaderException e = new OdpMissingHeaderException(endPoint);
                    if(errorCallback == null) throw e;
                    ServerInstruction instruction = errorCallback.GotPacketWithNoOdpHeader(e);
                    if((instruction & ServerInstruction.StopServer) != 0) return;
                    continue;
                }

                // No connection
                IOdpPayloadHandler payloadHandler;
                if (!endPointToOdpPayloadHandler.TryGetValue(endPoint, out payloadHandler))
                {
                    connectionCallback.NewConnection(endPoint, out payloadHandler);
                    endPointToOdpPayloadHandler.Add(endPoint, payloadHandler);
                }

                Byte flags = readBytes[0];
                if ((flags & (Byte)OdpFlags.Halt) != 0)
                {
                    ServerInstruction instruction = payloadHandler.ConnectionClosed();
                    if((instruction & ServerInstruction.StopServer) != 0) return;
                    continue;
                }

                if (bytesRead > 3)
                {
                    ServerInstruction instruction;

                    if ((flags & (Byte)OdpFlags.OrderedPayload) == 0)
                    {
                        instruction = payloadHandler.UnorderedPayload(readBytes, 3, bytesRead - 3);
                        if ((instruction & ServerInstruction.StopServer) != 0) return;
                        continue;
                    }

                    instruction = payloadHandler.OrderedPayload(readBytes, 3, bytesRead - 3);
                    if ((instruction & ServerInstruction.StopServer) != 0) return;
                }
            }
        }
    }

    [Flags]
    public enum OdpFlags : byte
    {
        Halt           = 0x80,
        NoMorePayloads = 0x40,
        OrderedPayload = 0x20,
        Resend         = 0x10,
    }
    public interface IOdpErrorHandler
    {
        ServerInstruction SocketException(SocketException e);
        ServerInstruction GotPacketWithNoOdpHeader(OdpMissingHeaderException e);
    }
    public interface IOdpConnectionHandler
    {
        ServerInstruction NewConnection(EndPoint connection, out IOdpPayloadHandler payloadHandler);
    }
    public interface IOdpPayloadHandler
    {
        ServerInstruction OrderedPayload(Byte[] readBytes, Int32 offset, Int32 length);
        ServerInstruction UnorderedPayload(Byte[] readBytes, Int32 offset, Int32 length);
        ServerInstruction ConnectionClosed();
    }

    /// <summary>
    /// Thrown if a halt packet is received without performing a gracefull halt
    /// </summary>
    public class OdpSequenceBadHaltException : Exception
    {
        public OdpSequenceBadHaltException()
            : base()
        {
        }
    }
    public class OdpMissingHeaderException : Exception
    {
        public readonly EndPoint from;
        public OdpMissingHeaderException(EndPoint from)
            : base()
        {
            this.from = from;
        }
    }


    public interface IOdpConnection
    {
        EndPoint RemoteEndPoint { get; }

        Byte[] RequestSendBuffer(Int32 maximumLengthNeeded, out Int32 offset);

        //
        // SendNoAck will send an Odp packet without any flags.
        //    Disadvantage:
        //       The odp protocol cannot determine if the packet becomes lost.
        //    Advantage:
        //       The sender does not have to wait for an Ack from the receiver.
        //       If the packet sent is not intended to have a response, it will reduce the traffic becauses there is no need for an empty ack packet.
        //
        void SendNoAck(Int32 maxOffset);

        //
        // QueueDatagram will send an Odp packet over the connection if the last ack is within the sequence window.
        // Otherwise, it will save the payload in a queue and return.
        //
        void SendOrQueueAndReturn(Int32 maxOffset);

        //
        // This method will block until the last acknowledged datagram is within the sequence window or the timeout is reached.
        // If the timeout is reached it will throw a TimeoutException, otherwise it sends the packet.
        //    throws TimeoutException, OdpSequenceBadHaltException
        //
        void Send(Int32 maxOffset, Int32 timeout);


        // Blocks till all sent packets have been acknowledged
        void Flush(Int32 timeout);

        //
        // returns -1 if connection has gracefully halted
        //    throws TimeoutException, OdpSequenceBadHaltException
        Int32 Receive(Byte[] buffer, UInt32 offset, UInt32 maxLength, UInt32 timeout);

        void GracefulHalt(Int32 timeout);

        void UngracefulHalt();
    }
    

    public class OdpOverUdpForOneThread : IOdpConnection
    {

        // If return value is 1, the payload id is the next in the seqeunce, 
        // If the return value is < 128, the payload is out of order
        // Else, (>= 128), the payload is a resend of a previous payload
        public static Int32 PayloadDiff(Byte currentPayloadID, Byte previousPayloadID)
        {
            Int32 payloadIDDiff = (0xFF & (Int32)currentPayloadID) - (0xFF & (Int32)previousPayloadID);
            if (payloadIDDiff < 0) payloadIDDiff += 256;
            return payloadIDDiff;
        }

        private Socket udpSocket;
        private EndPoint remoteEndPoint;

        Byte[] sendBuffers, recvBuffers;

        Byte lastPayloadIDSent, payloadAck;
        Byte lastPayloadIDReceived;

        Byte[] lastSendBufferRequested;

        public OdpOverUdpForOneThread(AddressFamily addressFamily, EndPoint remoteEndPoint)
        {
            this.udpSocket = new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp);
            this.udpSocket.Connect(remoteEndPoint);

            this.sendBuffers = new Byte[127];
            this.recvBuffers = new Byte[127];

            this.lastPayloadIDSent     = 0xFF;
            this.payloadAck            = 0xFF;
            this.lastPayloadIDReceived = 0xFF;

            this.lastSendBufferRequested = null;
        }

        public EndPoint RemoteEndPoint
        {
            get { return remoteEndPoint; }
        }

        //
        // One buffer can be requested at a time for sending.
        // This function returns the offset into the buffer that the client should put its data
        //
        public Byte[] RequestSendBuffer(Int32 maximumLengthNeeded, out Int32 offset)
        {
            // check if udp socket is null (disconnected)
            if (this.lastSendBufferRequested != null)
                throw new InvalidOperationException("You've already requested a send buffer");

            this.lastSendBufferRequested = Odp.BufferPool.GetBuffer(Odp.HeaderLength + maximumLengthNeeded);

            offset = Odp.HeaderLength;
            return this.lastSendBufferRequested;
        }


        public void SendNoAck(Int32 maxOffset)
        {
            // check if udp socket is null (disconnected)
            if (maxOffset < 3) throw new ArgumentOutOfRangeException(String.Format("The offset you provided ({0}) is out of range (must be >= 3)", maxOffset));

            Byte[] sendBuffer = this.lastSendBufferRequested;
            this.lastSendBufferRequested = null;
            if (sendBuffer == null) throw new InvalidOperationException("You haven't requested a send buffer");

            //
            // Odp Header
            //
            sendBuffer[0] = 0;                     // No flags
            sendBuffer[1] = lastPayloadIDSent;     // Payload ID
            sendBuffer[2] = lastPayloadIDReceived; // Ack Payload ID

            udpSocket.Send(sendBuffer, maxOffset, SocketFlags.None);
        }

        public void SendOrQueueAndReturn(Int32 maxOffset)
        {
            // check if udp socket is null (disconnected)
            throw new NotImplementedException();
        }
        public void Send(Int32 maxOffset, Int32 timeout)
        {
            // check if udp socket is null (disconnected)
            throw new NotImplementedException();
        }
        public void Flush(Int32 timeout)
        {
            // check if udp socket is null (disconnected)
            throw new NotImplementedException();
        }
        public int Receive(Byte[] buffer, UInt32 offset, UInt32 maxLength, UInt32 timeout)
        {
            // check if udp socket is null (disconnected)
            throw new NotImplementedException();
        }
        public void GracefulHalt(Int32 timeout)
        {
            // check if udp socket is null (disconnected)
            throw new NotImplementedException();
        }
        public void UngracefulHalt()
        {
            // check if udp socket is null (disconnected)
            Byte[] haltPacket = Odp.BufferPool.GetBuffer(Odp.HeaderLength);

            //
            // Odp Header
            //
            haltPacket[0] = (Byte)OdpFlags.Halt;   // Halt flag
            haltPacket[1] = lastPayloadIDSent;     // Payload ID
            haltPacket[2] = lastPayloadIDReceived; // Ack Payload ID

            udpSocket.Send(haltPacket, 0, Odp.HeaderLength, SocketFlags.None);
            udpSocket.Close();
            udpSocket = null;
        }
    }

}
