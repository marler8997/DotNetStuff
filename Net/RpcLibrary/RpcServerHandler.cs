using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;

using Marler.Common;

namespace Marler.Net
{
    public class RecordParser
    {
        public delegate void RecordHandler(String clientString, Socket socket, Byte[] bytes, Int32 offset, Int32 maxOffset);
        readonly RecordHandler recordHandler;

        Byte[] copiedFragmentData;
        Int32 copiedFramentDataLength;

        public RecordParser(RecordHandler recordHandler)
        {
            this.recordHandler = recordHandler;
        }
        public void ParseAndHandleRecords(String clientString, Socket socket,  Byte[] bytes, Int32 offset, Int32 maxOffset)
        {
            while(offset < maxOffset)
            {
                if (copiedFragmentData == null)
                {
                    //
                    // TODO: fix this corner case
                    //
                    if (maxOffset - offset < 4) throw new NotImplementedException("You have run into a corner case where the fragment length was not sent in a single packet...this corner case is not yet implemented");

                    Boolean isLastFragment = (bytes[offset] & 0x80) == 0x80;
                    if(!isLastFragment) throw new NotSupportedException("Multifragment records are not supported");

                    Int32 fragmentLength =
                        (0x7F000000 & (bytes[offset    ] << 24)) |
                        (0x00FF0000 & (bytes[offset + 1] << 16)) |
                        (0x0000FF00 & (bytes[offset + 2] <<  8)) |
                        (0x000000FF & (bytes[offset + 3]      )) ;

                    offset += 4;

                    Int32 fragmentBytesAvailable = maxOffset - offset;

                    if (fragmentBytesAvailable >= fragmentLength)
                    {
                        recordHandler(clientString, socket, bytes, offset, fragmentLength);
                        offset += fragmentLength;
                        continue;
                    }

                    this.copiedFragmentData = new Byte[fragmentLength];

                    Array.Copy(bytes, offset, copiedFragmentData, 0, fragmentBytesAvailable);
                    this.copiedFramentDataLength = fragmentBytesAvailable;

                    return;
                }
                else
                {
                    Int32 fragmentBytesAvailable = maxOffset - offset;
                    Int32 fragmentBytesNeeded = copiedFragmentData.Length - copiedFramentDataLength;

                    if(fragmentBytesAvailable >= fragmentBytesNeeded)
                    {
                        Array.Copy(bytes, offset, copiedFragmentData, copiedFramentDataLength, fragmentBytesNeeded);

                        recordHandler(clientString, socket, copiedFragmentData, 0, copiedFramentDataLength);
                        offset += fragmentBytesNeeded;

                        this.copiedFragmentData = null;

                        continue;
                    }

                    Array.Copy(bytes, offset, copiedFragmentData, copiedFramentDataLength, fragmentBytesAvailable);
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
            Byte[] callParameters, Int32 callOffset, Int32 callMaxOffset,
            out ISerializer replyParameters);  

        public void ServerStopped()
        {
            Console.WriteLine("[{0}] The server has stopped", serviceName);
        }
        public ServerInstruction ListenSocketClosed(int clientCount)
        {
            Console.WriteLine("[{0}] The listen socket closed", serviceName);
            return ServerInstruction.StopServer;
        }
        public ServerInstruction ClientOpenCallback(int clientCount, Socket socket)
        {
            //Console.WriteLine("[{0}] New Client '{1}'", serviceName, socket.RemoteEndPoint);
            socketToRecordBuilder.Add(socket, new RecordParser(HandleTcpRecord));
            return ServerInstruction.NoInstruction;
        }
        public ServerInstruction ClientCloseCallback(int clientCount, Socket socket)
        {
            //Console.WriteLine("[{0}] Close Client", serviceName);
            socketToRecordBuilder.Remove(socket);
            return ServerInstruction.NoInstruction;
        }
        public ServerInstruction ClientDataCallback(Socket socket, byte[] bytes, int bytesRead)
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
        public ServerInstruction DatagramPacket(System.Net.EndPoint endPoint, Socket socket, byte[] bytes, int bytesRead)
        {
            String clientString = String.Format("UDP:{0}", endPoint);

            Int32 parametersOffset;
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

        void HandleTcpRecord(String clientString, Socket socket, byte[] record, Int32 recordOffset, Int32 recordMaxOffset)
        {
            Int32 parametersOffset;
            RpcMessage callMessage = new RpcMessage(record, recordOffset, recordMaxOffset, out parametersOffset);

            if (callMessage.messageType != RpcMessageType.Call)
                throw new InvalidOperationException(String.Format("Received an Rpc reply from '{0}' but only expecting Rpc calls", clientString));

            if (!ProgramHeaderSupported(callMessage.call.programHeader))
            {
                new RpcMessage(callMessage.transmissionID, new RpcReply(RpcVerifier.None, RpcAcceptStatus.ProgramUnavailable)).SendTcp(socket, sendBuffer, null);
            }

            ISerializer replyParameters;
            RpcReply reply = Call(clientString, callMessage.call, record, parametersOffset, recordMaxOffset, out replyParameters);

            if (reply != null)
            {
                new RpcMessage(callMessage.transmissionID, reply).SendTcp(socket, sendBuffer, replyParameters);
            }
        }
    }
}
