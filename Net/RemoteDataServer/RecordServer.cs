using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using More.Net;

namespace More
{
    public abstract class RecordServerHandler : StreamSelectServerCallback, DatagramSelectServerCallback
    {
        public readonly String serviceName;
        readonly Dictionary<Socket, RecordBuilder> socketToRecordParser;

        private readonly ByteBuffer sendBuffer;

        public RecordServerHandler(String serviceName, ByteBuffer sendBuffer)
        {
            this.serviceName = serviceName;
            this.socketToRecordParser = new Dictionary<Socket, RecordBuilder>();
            this.sendBuffer = sendBuffer;
        }

        // returns the number of bytes to respond with
        public abstract UInt32 HandleRecord(String clientString,
            Byte[] record, UInt32 offset, UInt32 offsetLimit,
            ByteBuffer sendBuffer, UInt32 sendOffset);

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
        void HandleTcpRecord(String clientString, Socket socket, Byte[] record, UInt32 recordOffset, UInt32 recordOffsetLimit)
        {
            UInt32 responseLength = HandleRecord(clientString, record, recordOffset, recordOffsetLimit, sendBuffer, 4);

            if (responseLength > 0)
            {
                Byte[] array = sendBuffer.array;
                array.BigEndianSetUInt32(0, responseLength);
                socket.Send(array, 0, (Int32)(responseLength + 4), SocketFlags.None);
            }
        }
        public ServerInstruction DatagramPacket(EndPoint endPoint, Socket socket, Byte[] bytes, UInt32 bytesRead)
        {
            String clientString = String.Format("UDP:{0}", endPoint);

            UInt32 responseLength = HandleRecord(clientString, bytes, 0, bytesRead, sendBuffer, 0);
            if (responseLength > 0)
            {
                socket.SendTo(sendBuffer.array, 0, (Int32)responseLength, SocketFlags.None, endPoint);
            }
            return ServerInstruction.NoInstruction;
        }
    }
}
