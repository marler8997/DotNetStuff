using System;
using System.Net;
using System.Net.Sockets;

namespace More.Net
{
    public interface IConnectedTransmitterFactory
    {
        IConnectedDatagramTransmitter Create(EndPoint remoteEndPoint);
        IConnectedDatagramTransmitter Create(EndPoint remoteEndPoint, EndPoint localEndPoint);
    }

    /*
    public interface IDatagramTransmitterMultipleEndPoints : IDisposable
    {
        Int32 MaximumDatagramSize { get; }
        void Send(EndPoint remoteEndPoint, Byte[] datagram, Int32 datagramOffset, Int32 datagramLength);
        Boolean DatagramAvailable(EndPoint remoteEndPoint);

        Int32 ReceiveHeaderNonBlocking(EndPoint remoteEndPoint); // returns -1 if none available


        // Returns size of datagram (0 is valid)
        // Will never return a negative number, only works for nonblocking transmitters.
        //  throws InvalidOperationException (if ReceiveIsBlocking is true)
        Int32 ReceiveNonBlocking(EndPoint remoteEndPoint, Byte[] buffer, Int32 offset, Int32 maxLength);

        // Returns size of datagram (0 is valid).
        // Will never return a negative number, only works for blocking transmitters.
        //  throws TimeoutException, InvalidOperationException (if ReceiveIsBlocking is false)
        Int32 ReceiveBlocking(EndPoint remoteEndPoint, Byte[] buffer, Int32 offset, Int32 maxLength, Int32 timeoutMillis);
    }
    */
    public interface IUnroutedDatagramTransmitter : IDisposable
    {
        Boolean DatagramAvailable { get; }
        Int32 MaximumDatagramSize { get; }

        void SendTo(EndPoint endPoint, Byte[] datagram, Int32 datagramOffset, Int32 datagramLength);


        // Returns size of datagram (0 is valid), or -1 if no datagrams available.
        // Will never return a negative number, only works for nonblocking transmitters.
        //  throws InvalidOperationException (if ReceiveIsBlocking is true)
        Int32 ReceiveHeaderNonBlocking(out EndPoint endPoint);

        // Returns size of datagram (0 is valid), or -1 if no datagrams available.
        // Will never return a negative number, only works for nonblocking transmitters.
        //  throws InvalidOperationException (if ReceiveIsBlocking is true)
        Int32 ReceiveNonBlocking(out EndPoint endPoint, Byte[] buffer, Int32 offset, Int32 maxLength);


        // timeoutMillis, set to 0 for no timeout
        // Returns size of datagram (0 is valid), or -1 on timeout.
        // Will never return a negative number, only works for blocking transmitters.
        //  throws InvalidOperationException (if ReceiveIsBlocking is false)
        Int32 ReceiveBlocking(out EndPoint endPoint, Byte[] buffer, Int32 offset, Int32 maxLength, Int32 timeoutMillis);
    }

    public interface IConnectedDatagramTransmitter : IDisposable
    {
        Int32 MaximumDatagramSize { get; }

        EndPoint LocalEndPoint { get; }
        EndPoint RemoteEndPoint { get; }

        void Send(Byte[] datagram, Int32 datagramOffset, Int32 datagramLength);

        Boolean DatagramAvailable { get; }

        // Returns size of datagram (0 is valid), or -1 if no datagrams available.
        // Will never return a negative number, only works for nonblocking transmitters.
        //  throws InvalidOperationException (if ReceiveIsBlocking is true)
        Int32 ReceiveHeaderNonBlocking();

        // Returns size of datagram (0 is valid), or -1 if no datagrams available.
        // Will never return a negative number, only works for nonblocking transmitters.
        //  throws InvalidOperationException (if ReceiveIsBlocking is true)
        Int32 ReceiveNonBlocking(Byte[] buffer, Int32 offset, Int32 maxLength);


        // timeoutMillis, set to 0 for no timeout
        // Returns size of datagram (0 is valid), or -1 on timeout.
        // Will never return a negative number, only works for blocking transmitters.
        //  throws InvalidOperationException (if ReceiveIsBlocking is false)
        Int32 ReceiveBlocking(Byte[] buffer, Int32 offset, Int32 maxLength, Int32 timeoutMillis);
    }

    /*
    public interface ICdpServerController
    {
        Boolean HasControl { get; }
        Int32 AverageLatency { get; }

        //IDatagramControllerTransmitter Transmitter { get; }

        // Returns the payload diff and increments the payload if it is the next id
        Int32 HandlerReceivedPayloadID(Int32 payloadID);

        // Request a buffer to send later
        Byte[] RequestSendBuffer(Int32 maximumLengthNeeded, out Int32 offset);

        // Can be sent by handler or controller
        void SendHeartbeat();

        // Send a Cdp payload without a payload id. Note that this implies its transmission cannot be verified.
        void SendRandom(Int32 offsetLimit);

        // Sends a CDP payload without the ImmediateAck flag
        //    throws CdpTooManyPayloadsInFlightException
        void Send(Int32 offsetLimit, ICdpTimeout timeout);

        // Sends a CDP payload with the WaitForAck flag.
        //    throws TimeoutException
        void SendAndWaitForAck(Int32 offsetLimit, ICdpTimeout timeout);

        void Close(ICdpTimeout timeout);
        void ForceHalt();
    }
    public interface ICdpClientController : ICdpServerController
    {
        // This method will send current payload and wait for payloads from the new controller until control is given back.
        //    This method will wait for packets and call receive handler for each payload until control is given back or the
        //    timeout is reached.
        void SendAndGiveControl(Int32 sendBufferOffsetLimit, Boolean requestImmediateAck, Int32 maxResponsePayload, ICdpClientHandler receiveHandler, ICdpTimeout timeout);
    }
    */

    public interface ICdpServer
    {
        Boolean NewConnection(CdpTransmitter transmitter, out ICdpServerHandler handler, out Int32 maxSendBeforeAck); // return true to close the client
        void ConnectionClosed(EndPoint endPoint);
        Boolean SocketException(SocketException e); // return true to gracefully stop the server, throw exception to to force stop the server
        Boolean HeartbeatFromUnknown(EndPoint endPoint); // return true to gracefully stop the server, throw exception to force stop
    }
    public interface ICdpClientHandler
    {
        Boolean RandomPayload(Byte[] readBytes, Int32 offset, Int32 length);
        Boolean Payload(Byte[] readBytes, Int32 offset, Int32 length);
        Boolean Close(); // return true to acknowledge the close, false to just send a halt
        void Halt();
    }
    public interface ICdpServerHandler : ICdpClientHandler
    {
        //NOTE: if you are the initial controller, this method will not be called because the SendAndGiveControl method will just return instead
        ServerInstruction GotControl(CdpTransmitter transmitter, out Int32 sendBufferOffsetLimit, out Boolean requestImmediateAck); // After it returns the handler calls SendAndGiveControl
    }
    public interface ICdpTimeout
    {
        // return 0 for no receive timeout
        // return positive to indicate initial receive timeout in milliseconds
        Int32 WaitForAckInitialRetryTimeout(Int32 averageLatency);

        // return non positive to indicate the timeout has been reached and no more retries should be attempted
        // return positive to indicate the packet sent should be sent again and the next receive timeout in milliseconds
        Int32 WaitForAckRetryOrTimeout(Int32 retries, Int32 averageLatency, Int32 elapsedMillis, Int32 lastWaitForAckTimeout);


    }
}
