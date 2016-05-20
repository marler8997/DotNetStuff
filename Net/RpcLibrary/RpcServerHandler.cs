using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;

using More;


namespace More.Net
{
    public class RpcServerConnectionHandler
    {
        public readonly RpcServerHandler server;
        public readonly Socket socket;
        public readonly RecordBuilder recordBuilder;
        public RpcServerConnectionHandler(RpcServerHandler server, Socket socket)
        {
            this.server = server;
            this.socket = socket;
            this.recordBuilder = new RecordBuilder(socket.SafeRemoteEndPointString(), server.HandleTcpRecord);
        }
        public void HandleData(ref SelectControl control, Socket sock, Buf safeBuffer)
        {
            int bytesReceived;
            try
            {
                bytesReceived = sock.Receive(safeBuffer.array);
            }
            catch (SocketException)
            {
                bytesReceived = -1;
            }
            if (bytesReceived <= 0)
            {
                sock.ShutdownSafe();
                control.DisposeAndRemoveReceiveSocket(sock);
                return;
            }

            recordBuilder.HandleData(socket, safeBuffer.array, 0, (uint)bytesReceived);
        }
    }

    public abstract class RpcServerHandler
    {
        public readonly String serviceName;
        public readonly Buf sendBuffer;

        public RpcServerHandler(String serviceName, Buf sendBuffer)
        {
            this.serviceName = serviceName;
            this.sendBuffer = sendBuffer;
        }

        public abstract Boolean ProgramHeaderSupported(RpcProgramHeader programHeader);

        public abstract RpcReply Call(String clientString, RpcCall call,
            Byte[] callParameters, UInt32 callOffset, UInt32 callMaxOffset,
            out ISerializer replyParameters);

        public void AcceptCallback(ref SelectControl control, Socket listenSock, Buf safeBuffer)
        {
            Socket newSocket = listenSock.Accept();
            RpcServerConnectionHandler connection = new RpcServerConnectionHandler(this, newSocket);
            control.AddReceiveSocket(newSocket, connection.HandleData);
        }

        EndPoint from = new IPEndPoint(IPAddress.Any, 0);
        public void DatagramRecvHandler(ref SelectControl control, Socket sock, Buf safeBuffer)
        {
            int bytesReceived = sock.ReceiveFrom(safeBuffer.array, ref from);
            if (bytesReceived <= 0)
            {
                if (bytesReceived < 0)
                {
                    throw new InvalidOperationException(String.Format("ReceiveFrom on UDP socket returned {0}", bytesReceived));
                }
                return; // TODO: how to handle neg
            }

            String clientString = "?";
            try
            {
                clientString = from.ToString();
            }
            catch (Exception) { }

            UInt32 parametersOffset;
            RpcMessage callMessage = new RpcMessage(safeBuffer.array, 0, (uint)bytesReceived, out parametersOffset);

            if (callMessage.messageType != RpcMessageType.Call)
            {
                throw new InvalidOperationException(String.Format("Received an Rpc reply from '{0}' but only expecting Rpc calls", clientString));
            }
            if (!ProgramHeaderSupported(callMessage.call.programHeader))
            {
                new RpcMessage(callMessage.transmissionID, new RpcReply(RpcVerifier.None, RpcAcceptStatus.ProgramUnavailable)).SendUdp(from, sock, safeBuffer, null);
                return;
            }

            ISerializer replyParameters;
            RpcReply reply = Call(clientString, callMessage.call, safeBuffer.array, parametersOffset, (uint)bytesReceived, out replyParameters);

            if (reply != null)
            {
                new RpcMessage(callMessage.transmissionID, reply).SendUdp(from, sock, safeBuffer, replyParameters);
            }
        }
        public void HandleTcpRecord(String clientString, Socket socket, Byte[] record, UInt32 recordOffset, UInt32 recordOffsetLimit)
        {
            UInt32 parametersOffset;
            RpcMessage callMessage = new RpcMessage(record, recordOffset, recordOffsetLimit, out parametersOffset);

            if (callMessage.messageType != RpcMessageType.Call)
            {
                throw new InvalidOperationException(String.Format("Received an Rpc reply from '{0}' but only expecting Rpc calls", clientString));
            }

            if (!ProgramHeaderSupported(callMessage.call.programHeader))
            {
                new RpcMessage(callMessage.transmissionID, new RpcReply(RpcVerifier.None, RpcAcceptStatus.ProgramUnavailable)).SendTcp(socket, sendBuffer, null);
            }

            ISerializer replyParameters;
            RpcReply reply = Call(clientString, callMessage.call, record, parametersOffset, recordOffsetLimit, out replyParameters);

            if (reply != null)
            {
                new RpcMessage(callMessage.transmissionID, reply).SendTcp(socket, sendBuffer, replyParameters);
            }
        }
    }
}
