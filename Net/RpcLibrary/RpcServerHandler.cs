using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;

using More;


namespace More.Net
{
    public abstract class RpcServerHandler : StreamSelectServerCallback, DatagramSelectServerCallback
    {
        public readonly String serviceName;
        readonly Dictionary<Socket, RecordBuilder> socketToRecordParser;
        
        private readonly ByteBuffer sendBuffer;

        public RpcServerHandler(String serviceName, ByteBuffer sendBuffer)
        {
            this.serviceName = serviceName;
            this.socketToRecordParser = new Dictionary<Socket, RecordBuilder>();
            this.sendBuffer = sendBuffer;
        }

        public abstract Boolean ProgramHeaderSupported(RpcProgramHeader programHeader);

        public abstract RpcReply Call(String clientString, RpcCall call,
            Byte[] callParameters, UInt32 callOffset, UInt32 callMaxOffset,
            out ISerializer replyParameters);

        public void ServerListening(Socket listenSocket)
        {
        }
        public void ServerStopped()
        {
            Console.WriteLine("[{0}] The server has stopped", serviceName);
        }
        public ServerInstruction ListenSocketClosed(UInt32 clientCount)
        {
            Console.WriteLine("[{0}] The listen socket closed", serviceName);
            return ServerInstruction.StopServer;
        }
        public ServerInstruction ClientOpenCallback(UInt32 clientCount, Socket socket)
        {
            //Console.WriteLine("[{0}] New Client '{1}'", serviceName, socket.RemoteEndPoint);
            socketToRecordParser.Add(socket, new RecordBuilder(HandleTcpRecord));
            return ServerInstruction.NoInstruction;
        }
        public ServerInstruction ClientCloseCallback(UInt32 clientCount, Socket socket)
        {
            //Console.WriteLine("[{0}] Close Client", serviceName);
            socketToRecordParser.Remove(socket);
            return ServerInstruction.NoInstruction;
        }
        public ServerInstruction ClientDataCallback(Socket socket, Byte[] bytes, UInt32 bytesRead)
        {
            String clientString = String.Format("TCP:{0}", socket.SafeRemoteEndPointString());

            RecordBuilder recordBuilder;
            if (!socketToRecordParser.TryGetValue(socket, out recordBuilder))
            {
                Console.WriteLine("[{0}] No entry in dictionary for client '{1}'", serviceName, clientString);
                return ServerInstruction.CloseClient;
            }

            recordBuilder.HandleData(clientString, socket, bytes, 0, bytesRead);
            return ServerInstruction.NoInstruction;
        }
        public ServerInstruction DatagramPacket(System.Net.EndPoint endPoint, Socket socket, Byte[] bytes, UInt32 bytesRead)
        {
            String clientString = String.Format("UDP:{0}", endPoint);

            UInt32 parametersOffset;
            RpcMessage callMessage = new RpcMessage(bytes, 0, bytesRead, out parametersOffset);

            if (callMessage.messageType != RpcMessageType.Call)
                throw new InvalidOperationException(String.Format("Received an Rpc reply from '{0}' but only expecting Rpc calls", clientString));
            if (!ProgramHeaderSupported(callMessage.call.programHeader))
            {
                new RpcMessage(callMessage.transmissionID, new RpcReply(RpcVerifier.None, RpcAcceptStatus.ProgramUnavailable)).SendUdp(endPoint, socket, sendBuffer, null);
                return ServerInstruction.NoInstruction;
            }

            ISerializer replyParameters;
            RpcReply reply = Call(clientString, callMessage.call, bytes, parametersOffset, bytesRead, out replyParameters);

            if (reply != null)
            {
                new RpcMessage(callMessage.transmissionID, reply).SendUdp(endPoint, socket, sendBuffer, replyParameters);
            }

            return ServerInstruction.NoInstruction;
        }

        void HandleTcpRecord(String clientString, Socket socket, Byte[] record, UInt32 recordOffset, UInt32 recordOffsetLimit)
        {
            UInt32 parametersOffset;
            RpcMessage callMessage = new RpcMessage(record, recordOffset, recordOffsetLimit, out parametersOffset);

            if (callMessage.messageType != RpcMessageType.Call)
                throw new InvalidOperationException(String.Format("Received an Rpc reply from '{0}' but only expecting Rpc calls", clientString));

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
