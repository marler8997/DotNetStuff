using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net;

using More;

namespace More.Net
{
    public enum RpcVersion
    {
        One = 1,
        Two = 2,
    }
    public class RpcProgramHeader : SubclassSerializer
    {
        public static readonly IReflectors memberSerializers = new IReflectors(new IReflector[] {
            new XdrUInt32Reflector(typeof(RpcProgramHeader), "rpcVersion"),
            new XdrUInt32Reflector(typeof(RpcProgramHeader), "program"),
            new XdrUInt32Reflector(typeof(RpcProgramHeader), "programVersion"),
        });

        public readonly UInt32 rpcVersion;
        public readonly UInt32 program;
        public readonly UInt32 programVersion;

        public RpcProgramHeader()
            : base(memberSerializers)
        {
        }
        public RpcProgramHeader(RpcVersion rpcVersion, UInt32 program, UInt32 programVersion)
            : base(memberSerializers)
        {
            this.rpcVersion = (UInt32)rpcVersion;
            this.program = program;
            this.programVersion = programVersion;
        }
    }
    public enum RpcMessageType
    {
        Call  = 0,
        Reply = 1,
    };
    public class RpcMessage : SubclassSerializer
    {
        public static readonly IReflectors memberSerializers = new IReflectors(new IReflector[] {
            new XdrUInt32Reflector(typeof(RpcMessage), "transmissionID"),

            new XdrDescriminatedUnionReflector<RpcMessageType>(
                new XdrEnumReflector(typeof(RpcMessage), "messageType", typeof(RpcMessageType)),
                null,                
                new XdrDescriminatedUnionReflector<RpcMessageType>.KeyAndSerializer(RpcMessageType.Call, new IReflector[] {
                    new ClassFieldReflectors<RpcCall>(typeof(RpcMessage), "call", RpcCall.memberSerializers)}),
                new XdrDescriminatedUnionReflector<RpcMessageType>.KeyAndSerializer(RpcMessageType.Reply, new IReflector[] {
                    new ClassFieldReflectors<RpcReply>(typeof(RpcMessage), "reply", RpcReply.memberSerializers)})
            ),
        });

        public UInt32 transmissionID;
        public RpcMessageType messageType;
        public RpcCall call;
        public RpcReply reply;

        public RpcMessage()
            : base(memberSerializers)
        {
        }

        public RpcMessage(UInt32 transmissionID, RpcCall call)
            : base(memberSerializers)
        {
            this.transmissionID = transmissionID;
            this.messageType = RpcMessageType.Call;
            this.call = call;
        }

        public RpcMessage(UInt32 transmissionID, RpcReply reply)
            : base(memberSerializers)
        {
            this.transmissionID = transmissionID;
            this.messageType = RpcMessageType.Reply;
            this.reply = reply;
        }
        public RpcMessage(Socket socket, ByteBuffer buffer, out Int32 contentOffset, out Int32 contentMaxOffset)
            : base(memberSerializers)
        {
            //
            // TODO: catch socket exception to prevent server from failing
            //

            buffer.EnsureCapacity(12);
            socket.ReadFullSize(buffer.array, 0, 4); // read the size
            Int32 rpcMessageSize =
                (Int32)(
                (Int32)(0x7F000000 & (buffer.array[0] << 24)) |
                       (0x00FF0000 & (buffer.array[1] << 16)) |
                       (0x0000FF00 & (buffer.array[2] <<  8)) |
                       (0x000000FF & (buffer.array[3]      )) );

            if ((buffer.array[0] & 0x80) != 0x80)
                throw new NotImplementedException(String.Format("Records with multiple fragments it not currently implemented"));

            buffer.EnsureCapacity(rpcMessageSize);
            socket.ReadFullSize(buffer.array, 0, rpcMessageSize);

            //
            // Deserialize
            //
            contentMaxOffset = rpcMessageSize;

            if (RpcPerformanceLog.rpcMessageSerializationLogger != null) RpcPerformanceLog.StartSerialize();
            contentOffset = Deserialize(buffer.array, 0, rpcMessageSize);
            if (RpcPerformanceLog.rpcMessageSerializationLogger != null) RpcPerformanceLog.StopSerializationAndLog("RpcDeserializationTime");
        }
        public RpcMessage(Byte[] data, Int32 offset, Int32 maxOffset, out Int32 contentOffset)
            : base(memberSerializers)
        {
            if (RpcPerformanceLog.rpcMessageSerializationLogger != null) RpcPerformanceLog.StartSerialize();
            contentOffset = Deserialize(data, offset, maxOffset);
            if (RpcPerformanceLog.rpcMessageSerializationLogger != null) RpcPerformanceLog.StopSerializationAndLog("RpcDeserializationTime");
        }
        public void SendTcp(Socket socket, ByteBuffer buffer, ISerializer messageContents)
        {
            Int32 messageContentLength = (messageContents == null) ? 0 : messageContents.SerializationLength();
            Int32 totalMessageLength = SerializationLength() + messageContentLength;

            buffer.EnsureCapacity(4 + totalMessageLength); // Extra 4 bytes for the record header

            if (RpcPerformanceLog.rpcMessageSerializationLogger != null) RpcPerformanceLog.StartSerialize();
            Int32 offset = Serialize(buffer.array, 4);
            if (messageContents != null)
            {
                offset = messageContents.Serialize(buffer.array, offset);
            }
            if (RpcPerformanceLog.rpcMessageSerializationLogger != null) RpcPerformanceLog.StopSerializationAndLog("RpcSerializationTime");

            if (offset != totalMessageLength + 4)
                throw new InvalidOperationException(String.Format("[CodeBug] The caclulated serialization length of RpcMessage '{0}' was {1} but actual size was {2}",
                    DataString(), totalMessageLength, offset));

            //
            // Insert the record header
            //
            buffer.array[0] = (Byte)(0x80 | (totalMessageLength >> 24));
            buffer.array[1] = (Byte)(        totalMessageLength >> 16 );
            buffer.array[2] = (Byte)(        totalMessageLength >>  8 );
            buffer.array[3] = (Byte)(        totalMessageLength       );

            socket.Send(buffer.array, 0, totalMessageLength + 4, SocketFlags.None);
        }
        public void SendUdp(EndPoint endPoint, Socket socket, ByteBuffer buffer, ISerializer messageContents)
        {

            Int32 messageContentLength = (messageContents == null) ? 0 : messageContents.SerializationLength();
            Int32 totalMessageLength = SerializationLength() + messageContentLength;

            buffer.EnsureCapacity(totalMessageLength); // Extra 4 bytes for the record header

            if (RpcPerformanceLog.rpcMessageSerializationLogger != null) RpcPerformanceLog.StartSerialize();
            Int32 offset = Serialize(buffer.array, 0);
            if (messageContents != null)
            {
                offset = messageContents.Serialize(buffer.array, offset);
            }
            if (RpcPerformanceLog.rpcMessageSerializationLogger != null) RpcPerformanceLog.StopSerializationAndLog("RpcSerializationTime");

            if (offset != totalMessageLength)
                throw new InvalidOperationException(String.Format("[CodeBug] The caclulated serialization length of RpcMessage '{0}' was {1} but actual size was {2}",
                    DataString(), totalMessageLength, offset));

            socket.SendTo(buffer.array, 0, totalMessageLength, SocketFlags.None, endPoint);
        }
    }
}
