using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace Marler.NetworkTools
{
    /*
    public class CdpSingleSendController : ICdpClientController
    {
        private readonly Byte[] headerBuffer;
        public readonly IDatagramTransmitter transmitter;
        Byte[] sendBuffer;
        Int32 lastPayloadID;
        private Boolean hasControl;
        private Int32 averageLatency;

        public static CdpSingleSendController ConnectOverUdp(AddressFamily addressFamily, EndPoint remoteEndPoint, Boolean haveControl)
        {
            UdpClientTransmitter udpTransmitter = new UdpClientTransmitter();
            udpTransmitter.Connect(remoteEndPoint);
            return new CdpSingleSendController(udpTransmitter, haveControl);
        }
        public CdpSingleSendController(IDatagramTransmitter transmitter, Boolean haveControl)
        {
            this.headerBuffer = new Byte[Cdp.HeaderLengthWithPayloadID];
            this.transmitter = transmitter;
            this.sendBuffer = null;
            this.lastPayloadID = Cdp.MaxPayloadID;
            this.hasControl = haveControl;
            this.averageLatency = 0;
        }

        public Boolean HasControl
        {
            get { return hasControl; }
        }
        public int AverageLatency
        {
            get { return averageLatency; }
        }

        public Int32 HandlerReceivedPayloadID(Int32 payloadID)
        {
            Int32 payloadDiff = Cdp.PayloadDiff(payloadID, lastPayloadID);
            if (payloadDiff == 1)
            {
                lastPayloadID++;
                return 1;
            }
            return payloadDiff;
        }

        public void SendHeartbeat()
        {
            transmitter.SendHeartbeat();
        }

        //
        // This method must be called to get a buffer before sending
        // This function returns the offset into the buffer that the client should put its data
        //
        public Byte[] RequestSendBuffer(Int32 payloadSize, out Int32 offset)
        {
            if (this.sendBuffer != null) throw new InvalidOperationException("You've already requested a send buffer");
            this.sendBuffer = Cdp.BufferPool.GetBuffer(Cdp.HeaderLengthWithPayloadID + payloadSize);
            offset = Cdp.HeaderLengthWithPayloadID;
            return this.sendBuffer;
        }
        public void SendRandom(Int32 offsetLimit)
        {
            if (offsetLimit < 3) throw new ArgumentOutOfRangeException(String.Format("The offset you provided ({0}) is out of range (must be >= 3)", offsetLimit));

            Byte[] bufferToSend = this.sendBuffer;
            this.sendBuffer = null;
            if (bufferToSend == null) throw new InvalidOperationException("You haven't requested a send buffer");

            // Cdp Header
            bufferToSend[1] = 0; // No flags

            transmitter.Send(bufferToSend, 1, offsetLimit - 1);
        }
        private Int32 NextPayloadIDForSend()
        {
            if (lastPayloadID >= Cdp.MaxPayloadID)
            {
                this.lastPayloadID = 0;
            }
            else
            {
                this.lastPayloadID++;
            }
            return this.lastPayloadID;
        }

        private Byte[] PrepareSendBufferForPayloadWithID(CdpControllerSendFlags extraFlags, Int32 offsetLimit)
        {
            if (!hasControl) throw new InvalidOperationException("Cannot send beacuse right now you are not the controller");
            if (offsetLimit < 3) throw new ArgumentOutOfRangeException(String.Format("The offset you provided ({0}) is out of range (must be >= 3)", offsetLimit));

            Byte[] bufferToSend = this.sendBuffer;
            this.sendBuffer = null;
            if (bufferToSend == null) throw new InvalidOperationException("You haven't requested a send buffer");

            // Cdp Header
            Int32 payloadID = NextPayloadIDForSend();
            bufferToSend[0] = (Byte)((0xF8 & (Byte)(CdpControllerSendFlags.PayloadWithID | extraFlags)) |
                                     (0x07 & (Byte)(payloadID >> 8)));
            bufferToSend[1] = (Byte)payloadID;
            return bufferToSend;
        }

        private void WaitForAck(ICdpTimeout timeout)
        {
            while (true)
            {
                Int32 bytesReceived = transmitter.ReceiveBlocking(headerBuffer, 0, 2,
                    timeout.RetryTimeoutWhenWaitingForAck(0, averageLatency));
                Console.WriteLine("[TransmitterDebug] Recevied {0} bytes", bytesReceived);

                return;
            }
        }
        public void Send(int offsetLimit, ICdpTimeout timeout)
        {
            throw new InvalidOperationException(String.Format("This function is invalid for this controller '{0}'", GetType().Name));
        }
        public void SendAndWaitForAck(Int32 offsetLimit, ICdpTimeout timeout)
        {
            Byte[] sendBuffer = PrepareSendBufferForPayloadWithID(CdpControllerSendFlags.ImmediateAck, offsetLimit);
            while (true)
            {
                transmitter.Send(sendBuffer, 0, offsetLimit);

                Int32 bytesReceived = transmitter.ReceiveBlocking(headerBuffer, 0, 2,
                    timeout.RetryTimeoutWhenWaitingForAck(0, averageLatency));
                Console.WriteLine("[TransmitterDebug] Recevied {0} bytes", bytesReceived);

                return;
            }




            Send(CdpControllerSendFlags.ImmediateAckOrResend, offsetLimit, timeout);
            WaitForAck(timeout);

            Cdp.BufferPool.FreeBuffer(this.sendBuffer);
            this.sendBuffer = null;
        }
        public void SendAndGiveControl(int sendBufferOffsetLimit, bool requestImmediateAck, int maxResponsePayload, ICdpClientHandler receiveHandler, ICdpTimeout timeout)
        {
            Send(CdpControllerSendFlags.GiveControl, sendBufferOffsetLimit, timeout);

            Byte[] recvBuffer = Cdp.BufferPool.GetBuffer(Cdp.HeaderLengthWithPayloadID + maxResponsePayload);

            Int32 retries = 0;
            Int64 receiveStartTime = Stopwatch.GetTimestamp();
            while (true)
            {
                Int32 receiveTimeout = timeout.RetryTimeoutAfterGiveControl(retries, averageLatency);
                Int32 datagramSize = transmitter.ReceiveBlocking(recvBuffer, 0,
                    Cdp.HeaderLengthWithPayloadID + maxResponsePayload, receiveTimeout);

                if (datagramSize < 0)
                {
                    Int32 diffMillis = (Int32)StopwatchExtensions.StopwatchTicksAsInt64Milliseconds(Stopwatch.GetTimestamp() - receiveStartTime);
                    if (!timeout.KeepTryingToReceiveAfterGiveControl(retries, averageLatency, diffMillis, receiveTimeout))
                        throw new TimeoutException(String.Format("No reponse after for give control {0} milliseconds", diffMillis));
                    retries++;
                }
            }

            if (!hasControl) throw new InvalidOperationException("Cannott send beacuse right now you are not the controller");


            throw new NotImplementedException();
        }
        public void Close(ICdpTimeout timeout)
        {
            throw new NotImplementedException();
        }

        public void ForceHalt()
        {
            CdpFlags flags = CdpFlags.Halt | (hasControl ? 0 : CdpFlags.Controller) | CdpFlags.Close;
            Int32 payloadID = lastPayloadID;

            // Cdp Header
            headerBuffer[0] = (Byte)((0xF8 & (Byte)flags) |
                                   (0x07 & (Byte)(payloadID)));
            headerBuffer[1] = (Byte)payloadID;

            connectedTransmitter.Send(headerBuffer, 0, 2);
        }

        #region ICdpClientController Members

        void ICdpClientController.SendAndGiveControl(int sendBufferOffsetLimit, bool requestImmediateAck, int maxResponsePayload, ICdpClientHandler receiveHandler, ICdpTimeout timeout)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICdpServerController Members

        bool ICdpServerController.HasControl
        {
            get { throw new NotImplementedException(); }
        }

        int ICdpServerController.AverageLatency
        {
            get { throw new NotImplementedException(); }
        }

        int ICdpServerController.HandlerReceivedPayloadID(int payloadID)
        {
            throw new NotImplementedException();
        }

        byte[] ICdpServerController.RequestSendBuffer(int maximumLengthNeeded, out int offset)
        {
            throw new NotImplementedException();
        }

        void ICdpServerController.SendHeartbeat()
        {
            throw new NotImplementedException();
        }

        void ICdpServerController.SendRandom(int offsetLimit)
        {
            throw new NotImplementedException();
        }

        void ICdpServerController.Send(int offsetLimit, ICdpTimeout timeout)
        {
            throw new NotImplementedException();
        }

        void ICdpServerController.SendAndWaitForAck(int offsetLimit, ICdpTimeout timeout)
        {
            throw new NotImplementedException();
        }

        void ICdpServerController.Close(ICdpTimeout timeout)
        {
            throw new NotImplementedException();
        }

        void ICdpServerController.ForceHalt()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    */

    /*
    public class CdpController : ICdpClientController
    {
        public readonly IDatagramTransmitter transmitter;

        Byte[][] payloadSendQueue;
        Int32 queuePosition;
        Byte[] lastSendBufferRequested;

        Int32 lastPayloadID;

        private Boolean hasControl;

        private readonly Byte[] headerBuffer;
        private Int32 averageLatency;

        public static CdpController ConnectOverUdp(AddressFamily addressFamily, EndPoint remoteEndPoint, Boolean haveControl, Int32 maxSendsBeforeAck)
        {
            UdpClientTransmitter udpTransmitter = new UdpClientTransmitter();
            udpTransmitter.Connect(remoteEndPoint);
            return new CdpController(udpTransmitter, haveControl, maxSendsBeforeAck);
        }
        public CdpController(IDatagramTransmitter transmitter, Boolean haveControl, Int32 maxSendBeforeAck)
        {
            if (maxSendBeforeAck >= 0x8000)
                throw new ArgumentOutOfRangeException(String.Format("MaxSendBeforeAck must be less than 0x8000 but you gave {0}", maxSendBeforeAck));

            this.transmitter = transmitter;

            this.payloadSendQueue = new Byte[maxSendBeforeAck][];
            this.queuePosition = 0;
            this.lastSendBufferRequested = null;

            this.lastPayloadID = Cdp.MaxPayloadID;

            this.hasControl = haveControl;

            this.headerBuffer = new Byte[Cdp.HeaderLengthWithPayloadID];
            this.averageLatency = 0;
        }

        public Boolean HasControl
        {
            get { return hasControl; }
        }
        public int AverageLatency
        {
            get { return averageLatency; }
        }

        public Int32 HandlerReceivedPayloadID(Int32 payloadID)
        {
            Int32 payloadDiff = Cdp.PayloadDiff(payloadID, lastPayloadID);
            if (payloadDiff == 1)
            {
                lastPayloadID++;
                return 1;
            }
            return payloadDiff;
        }

        //
        // This method must be called to get a buffer before sending
        // This function returns the offset into the buffer that the client should put its data
        //
        public Byte[] RequestSendBuffer(Int32 payloadSize, out Int32 offset)
        {
            // check if udp socket is null (disconnected)
            if (this.lastSendBufferRequested != null)
                throw new InvalidOperationException("You've already requested a send buffer");

            this.lastSendBufferRequested = Cdp.BufferPool.GetBuffer(Cdp.HeaderLengthWithPayloadID + payloadSize);

            offset = Cdp.HeaderLengthWithPayloadID;
            return this.lastSendBufferRequested;
        }
        public void SendRandom(Int32 offsetLimit)
        {
            if (offsetLimit < 3) throw new ArgumentOutOfRangeException(String.Format("The offset you provided ({0}) is out of range (must be >= 3)", offsetLimit));

            Byte[] sendBuffer = this.lastSendBufferRequested;
            this.lastSendBufferRequested = null;
            if (sendBuffer == null) throw new InvalidOperationException("You haven't requested a send buffer");

            //
            // Cdp Header
            //
            sendBuffer[1] = 0;                     // No flags

            connectedTransmitter.Send(sendBuffer, 1, offsetLimit - 1);
        }
        private Int32 NextPayloadIDForSend()
        {
            if (lastPayloadID >= Cdp.MaxPayloadID)
            {
                this.lastPayloadID = 0;
            }
            else
            {
                this.lastPayloadID++;
            }
            return this.lastPayloadID;
        }
        private Byte[] PrepareSendBuffer(CdpControllerSendFlags extraFlags, Int32 offsetLimit)
        {
            if (!hasControl) throw new InvalidOperationException("Cannot send beacuse right now you are not the controller");
            if (offsetLimit < 3) throw new ArgumentOutOfRangeException(String.Format("The offset you provided ({0}) is out of range (must be >= 3)", offsetLimit));

            Byte[] bufferToSend = this.lastSendBufferRequested;
            this.lastSendBufferRequested = null;
            if (bufferToSend == null) throw new InvalidOperationException("You haven't requested a send buffer");

            // Cdp Header
            Int32 payloadID = NextPayloadIDForSend();
            bufferToSend[0] = (Byte)((0xF8 & (Byte)(CdpFlags.Controller | extraFlags)) |
                                     (0x07 & (Byte)(payloadID >> 8)));
            bufferToSend[1] = (Byte)payloadID;
            return bufferToSend;
        }

        private void WaitForAck(ICdpTimeout timeout)
        {
            while (true)
            {
                Int32 bytesReceived = connectedTransmitter.ReceiveBlocking(headerBuffer, 0, 2,
                    timeout.RetryTimeoutWhenWaitingForAck(0, averageLatency));
                if (bytesReceived < 0)
                {
                    throw new TimeoutException();
                }

                Console.WriteLine("[TransmitterDebug] Recevied {0} bytes", bytesReceived);

                return;


            }
        }

        public void Send(int offsetLimit, ICdpTimeout timeout)
        {
            Send(0, offsetLimit, timeout);
        }

        public void SendAndWaitForAck(Int32 offsetLimit, ICdpTimeout timeout)
        {
            Send(CdpFlags.ImmediateAckOrResend, offsetLimit, timeout);
            WaitForAck(timeout);
        }

        public void SendAndGiveControl(int sendBufferOffsetLimit, int maxResponsePayload, ICdpPayloadHandler receiveHandler, ICdpTimeout timeout)
        {
            if (!hasControl) throw new InvalidOperationException("Cannott send beacuse right now you are not the controller");


            throw new NotImplementedException();
        }

        public void Close(ICdpTimeout timeout)
        {
            throw new NotImplementedException();
        }

        public void ForceHalt()
        {
            CdpFlags flags = CdpFlags.Halt | (hasControl ? 0 : CdpFlags.Controller) | CdpFlags.Close;
            Int32 payloadID = lastPayloadID;

            // Cdp Header
            headerBuffer[0] = (Byte)((0xF8 & (Byte)flags) |
                                   (0x07 & (Byte)(payloadID)));
            headerBuffer[1] = (Byte)payloadID;

            connectedTransmitter.Send(headerBuffer, 0, 2);
        }

        #region ICdpControllerWithoutGive Members


        public void SendHeartbeat()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    */
}
