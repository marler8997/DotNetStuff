using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;

namespace Marler.NetworkTools
{
    public abstract class RpcServerHandler : StreamSelectServerCallback, DatagramSelectServerCallback
    {
        public readonly String serviceName;
        private readonly ByteBuffer sendBuffer;

        public RpcServerHandler(String serviceName, ByteBuffer sendBuffer)
        {
            this.serviceName = serviceName;
            this.sendBuffer = sendBuffer;
        }

        public abstract Boolean ProgramHeaderSupported(RpcProgramHeader programHeader);

        public abstract RpcReply Call(String clientString, RpcCall call,
            Byte[] callParameters, Int32 callOffset, Int32 callMaxOffset,
            out ISerializableData replyParameters);  

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
            return ServerInstruction.NoInstruction;
        }
        public ServerInstruction ClientCloseCallback(int clientCount, Socket socket)
        {
            //Console.WriteLine("[{0}] Close Client", serviceName);
            return ServerInstruction.NoInstruction;
        }
        public ServerInstruction ClientDataCallback(Socket socket, byte[] bytes, int bytesRead)
        {
            //
            // TODO: In the future I will need to save the data from every packet
            //       into a buffer which I will then process
            //       For now, to keep things simple I will assume every packet is exactly one record
            //
            /*
            RecordHandler recordHandler = nfsServer.socketRecordDictionary[socket];

            recordHandler.AddBytes(bytes, 0, bytesRead);
            */


            String clientString = String.Format("TCP:{0}", socket.RemoteEndPoint);

            Int32 parametersOffset;
            RpcMessage callMessage = new RpcMessage(bytes, 4, bytesRead, out parametersOffset);

            if (callMessage.messageType != RpcMessageType.Call)
                throw new InvalidOperationException(String.Format("Received an Rpc reply from '{0}' but only expecting Rpc calls", clientString));
            if (!ProgramHeaderSupported(callMessage.call.programHeader))
            {
                new RpcMessage(callMessage.transmissionID, new RpcReply(RpcVerifier.None, RpcAcceptStatus.ProgramUnavailable)).SendTcp(socket, sendBuffer, null);
                return ServerInstruction.NoInstruction;
            }

            ISerializableData replyParameters;
            RpcReply reply = Call(clientString, callMessage.call, bytes, parametersOffset, bytesRead, out replyParameters);

            if (reply != null)
            {
                new RpcMessage(callMessage.transmissionID, reply).SendTcp(socket, sendBuffer, replyParameters);
            }

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

            ISerializableData replyParameters;
            RpcReply reply = Call(clientString, callMessage.call, bytes, parametersOffset, bytesRead, out replyParameters);

            if (reply != null)
            {
                new RpcMessage(callMessage.transmissionID, reply).SendUdp(endPoint, socket, sendBuffer, replyParameters);
            }

            return ServerInstruction.NoInstruction;
        }      
    }
}
