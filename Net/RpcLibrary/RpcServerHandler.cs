using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;

using More;

#if WindowsCE
using ArrayCopier = System.MissingInCEArrayCopier;
#else
using ArrayCopier = System.Array;
#endif

namespace More.Net
{
    public class RecordParser
    {
        public delegate void RecordHandler(String clientString, Socket socket, Byte[] bytes, UInt32 offset, UInt32 maxOffset);
        readonly RecordHandler recordHandler;

        Byte[] copiedFragmentData;
        UInt32 copiedFramentDataLength;

        public RecordParser(RecordHandler recordHandler)
        {
            this.recordHandler = recordHandler;
        }
        public void ParseAndHandleRecords(String clientString, Socket socket, Byte[] bytes, UInt32 offset, UInt32 offsetLimit)
        {
            while (offset < offsetLimit)
            {
                if (copiedFragmentData == null)
                {
                    //
                    // TODO: fix this corner case
                    //
                    if (offsetLimit - offset < 4) throw new NotImplementedException("You have run into a corner case where the fragment length was not sent in a single packet...this corner case is not yet implemented");

                    Boolean isLastFragment = (bytes[offset] & 0x80) == 0x80;
                    if(!isLastFragment) throw new NotSupportedException("Multifragment records are not supported");

                    Int32 fragmentLength =
                        (0x7F000000 & (bytes[offset    ] << 24)) |
                        (0x00FF0000 & (bytes[offset + 1] << 16)) |
                        (0x0000FF00 & (bytes[offset + 2] <<  8)) |
                        (0x000000FF & (bytes[offset + 3]      )) ;

                    offset += 4;

                    UInt32 fragmentBytesAvailable = offsetLimit - offset;

                    if (fragmentBytesAvailable >= fragmentLength)
                    {
                        recordHandler(clientString, socket, bytes, offset, (UInt32)fragmentLength);
                        offset += (UInt32)fragmentLength;
                        continue;
                    }

                    this.copiedFragmentData = new Byte[fragmentLength];

                    ArrayCopier.Copy(bytes, offset, copiedFragmentData, 0, fragmentBytesAvailable);
                    this.copiedFramentDataLength = fragmentBytesAvailable;

                    return;
                }
                else
                {
                    UInt32 fragmentBytesAvailable = offsetLimit - offset;
                    UInt32 fragmentBytesNeeded = (UInt32)copiedFragmentData.Length - copiedFramentDataLength;

                    if(fragmentBytesAvailable >= fragmentBytesNeeded)
                    {
                        ArrayCopier.Copy(bytes, offset, copiedFragmentData, copiedFramentDataLength, fragmentBytesNeeded);

                        recordHandler(clientString, socket, copiedFragmentData, 0, copiedFramentDataLength);
                        offset += fragmentBytesNeeded;

                        this.copiedFragmentData = null;

                        continue;
                    }

                    ArrayCopier.Copy(bytes, offset, copiedFragmentData, copiedFramentDataLength, fragmentBytesAvailable);
                    this.copiedFramentDataLength += fragmentBytesAvailable;

                    return;
                }
            }
        }
    }

    public abstract class RpcServerHandler : StreamSelectServerCallback, DatagramSelectServerCallback
    {
        public readonly String serviceName;
        readonly Dictionary<Socket, RecordParser> socketToRecordBuilder;
        
        private readonly ByteBuffer sendBuffer;

        public RpcServerHandler(String serviceName, ByteBuffer sendBuffer)
        {
            this.serviceName = serviceName;
            this.socketToRecordBuilder = new Dictionary<Socket, RecordParser>();
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
            socketToRecordBuilder.Add(socket, new RecordParser(HandleTcpRecord));
            return ServerInstruction.NoInstruction;
        }
        public ServerInstruction ClientCloseCallback(UInt32 clientCount, Socket socket)
        {
            //Console.WriteLine("[{0}] Close Client", serviceName);
            socketToRecordBuilder.Remove(socket);
            return ServerInstruction.NoInstruction;
        }
        public ServerInstruction ClientDataCallback(Socket socket, Byte[] bytes, UInt32 bytesRead)
        {
            String clientString = String.Format("TCP:{0}", socket.SafeRemoteEndPointString());

            RecordParser recordParser;
            if (!socketToRecordBuilder.TryGetValue(socket, out recordParser))
            {
                Console.WriteLine("[{0}] No entry in dictionary for client '{1}'", serviceName, clientString);
                return ServerInstruction.CloseClient;
            }

            recordParser.ParseAndHandleRecords(clientString, socket, bytes, 0, bytesRead);
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
