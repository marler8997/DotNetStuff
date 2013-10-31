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
    public class RecordBuilder
    {
        public delegate void RecordHandler(String clientString, Socket socket, Byte[] bytes, UInt32 offset, UInt32 length);
        readonly RecordHandler recordHandler;

        enum State
        {
            Initial,
            PartialLengthReceived,
            LengthReceived
        };

        State state;
        Byte[] copiedFragmentData;
        UInt32 copiedFramentDataLength;

        public RecordBuilder(RecordHandler recordHandler)
        {
            this.state = State.Initial;
            this.recordHandler = recordHandler;
        }
        public void Reset()
        {
            this.copiedFragmentData = null;
            this.copiedFramentDataLength = 0;
        }

        // This function is highly tested
        public void HandleData(String clientString, Socket socket, Byte[] bytes, UInt32 offset, UInt32 offsetLimit)
        {
            switch (state)
            {
                case State.Initial:
                    {
                        while (offset < offsetLimit)
                        {
                            //
                            // Only a few bytes of the length were received
                            //
                            if (offsetLimit - offset < 4)
                            {
                                copiedFramentDataLength = offsetLimit - offset;
                                copiedFragmentData = new Byte[4];
                                for (int i = 0; i < copiedFramentDataLength; i++)
                                {
                                    this.copiedFragmentData[i] = bytes[offset + i];
                                }
                                state = State.PartialLengthReceived;
                                return;
                            }

                            Boolean isLastFragment = (bytes[offset] & 0x80) == 0x80;
                            if (!isLastFragment) throw new NotSupportedException("Multifragment records are not supported");

                            Int32 fragmentLength =
                                (0x7F000000 & (bytes[offset    ] << 24)) |
                                (0x00FF0000 & (bytes[offset + 1] << 16)) |
                                (0x0000FF00 & (bytes[offset + 2] << 8 )) |
                                (0x000000FF & (bytes[offset + 3]      )) ;

                            offset += 4;

                            UInt32 fragmentBytesAvailable = offsetLimit - offset;

                            if (fragmentBytesAvailable < fragmentLength)
                            {
                                this.copiedFragmentData = new Byte[fragmentLength];
                                ArrayCopier.Copy(bytes, offset, copiedFragmentData, 0, fragmentBytesAvailable);
                                this.copiedFramentDataLength = fragmentBytesAvailable;
                                state = State.LengthReceived;
                                return;
                            }

                            recordHandler(clientString, socket, bytes, offset, (UInt32)fragmentLength);
                            offset += (UInt32)fragmentLength;
                        }
                    }
                    return;
                case State.PartialLengthReceived:
                    {
                        while (true)
                        {
                            copiedFragmentData[copiedFramentDataLength] = bytes[offset];
                            offset++;
                            if (copiedFramentDataLength == 3) break;
                            copiedFramentDataLength++;
                            if (offset >= offsetLimit) return;
                        }

                        Boolean isLastFragment = (copiedFragmentData[0] & 0x80) == 0x80;
                        if (!isLastFragment) throw new NotSupportedException("Multifragment records are not supported");

                        Int32 fragmentLength =
                            (0x7F000000 & (copiedFragmentData[0] << 24)) |
                            (0x00FF0000 & (copiedFragmentData[1] << 16)) |
                            (0x0000FF00 & (copiedFragmentData[2] << 8 )) |
                            (0x000000FF & (copiedFragmentData[3]      )) ;

                        UInt32 fragmentBytesAvailable = offsetLimit - offset;

                        if (fragmentBytesAvailable < fragmentLength)
                        {
                            this.copiedFragmentData = new Byte[fragmentLength];
                            ArrayCopier.Copy(bytes, offset, copiedFragmentData, 0, fragmentBytesAvailable);
                            this.copiedFramentDataLength = fragmentBytesAvailable;
                            state = State.LengthReceived;
                            return;
                        }

                        recordHandler(clientString, socket, bytes, offset, (UInt32)fragmentLength);
                        offset += (UInt32)fragmentLength;

                        state = State.Initial;
                        goto case State.Initial;
                    }
                case State.LengthReceived:
                    {
                        UInt32 fragmentBytesAvailable = offsetLimit - offset;
                        UInt32 fragmentBytesNeeded = (UInt32)copiedFragmentData.Length - copiedFramentDataLength;

                        if (fragmentBytesAvailable < fragmentBytesNeeded)
                        {
                            ArrayCopier.Copy(bytes, offset, copiedFragmentData, copiedFramentDataLength, fragmentBytesAvailable);
                            copiedFramentDataLength += fragmentBytesAvailable;
                            return;
                        }
                        else
                        {
                            ArrayCopier.Copy(bytes, offset, copiedFragmentData, copiedFramentDataLength, fragmentBytesNeeded);

                            recordHandler(clientString, socket, copiedFragmentData, 0, (UInt32)copiedFragmentData.Length);
                            offset += fragmentBytesNeeded;

                            this.copiedFragmentData = null;

                            this.state = State.Initial;
                            goto case State.Initial;
                        }
                    }
            }
        }
    }
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
