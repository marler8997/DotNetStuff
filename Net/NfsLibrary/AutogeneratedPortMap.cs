// InputSha CD572DD569EA8ABF2763128758E4A153BD751A03
//
// This file was autogenerated using the PdlCodeGenerator
//     GenerationDateTime : 9/20/2013 4:48:14 PM
//
using System;
using System.Text;

using More;

namespace More.Net
{
    public class Mapping
    {
        public const UInt32 FixedSerializationLength = 16;

        static InstanceSerializer serializer = null;
        public static InstanceSerializer Serializer
        {
            get
            {
                if(serializer == null) serializer = new InstanceSerializer();
                return serializer;
            }
        }

        public class InstanceSerializer : FixedLengthInstanceSerializer<Mapping>
        {
            public InstanceSerializer() {}
            public override UInt32 FixedSerializationLength() { return Mapping.FixedSerializationLength; }
            public override void FixedLengthSerialize(Byte[] bytes, UInt32 offset, Mapping instance)
            {
                bytes.BigEndianSetUInt32(offset, instance.program);
                offset += 4;
                bytes.BigEndianSetUInt32(offset, instance.version);
                offset += 4;
                bytes.BigEndianSetUInt32(offset, instance.protocol);
                offset += 4;
                bytes.BigEndianSetUInt32(offset, instance.port);
                offset += 4;
            }
            public override Mapping FixedLengthDeserialize(Byte[] bytes, UInt32 offset)
            {
                return new Mapping (
                    bytes.BigEndianReadUInt32(offset + 0), // program
                    bytes.BigEndianReadUInt32(offset + 4), // version
                    bytes.BigEndianReadUInt32(offset + 8), // protocol
                    bytes.BigEndianReadUInt32(offset + 12) // port
                );
            }
            public override void DataString(Mapping instance, StringBuilder builder)
            {
                builder.Append("Mapping:{");
                builder.Append(instance.program);
                builder.Append(',');
                builder.Append(instance.version);
                builder.Append(',');
                builder.Append(instance.protocol);
                builder.Append(',');
                builder.Append(instance.port);
                builder.Append("}");
            }
            public override void DataSmallString(Mapping instance, StringBuilder builder)
            {
                builder.Append("Mapping:{");
                builder.Append(instance.program);
                builder.Append(',');
                builder.Append(instance.version);
                builder.Append(',');
                builder.Append(instance.protocol);
                builder.Append(',');
                builder.Append(instance.port);
                builder.Append("}");
            }
        }

        public UInt32 program;
        public UInt32 version;
        public UInt32 protocol;
        public UInt32 port;
        private Mapping() { }
        public Mapping(UInt32 program, UInt32 version, UInt32 protocol, UInt32 port)
        {
            this.program = program;
            this.version = version;
            this.protocol = protocol;
            this.port = port;
        }
        public FixedLengthInstanceSerializerAdapter<Mapping> CreateSerializerAdapater()
        {
            return new FixedLengthInstanceSerializerAdapter<Mapping>(Serializer, this);
        }
    }
    public class GetPortReply
    {
        public const UInt32 FixedSerializationLength = 4;

        static InstanceSerializer serializer = null;
        public static InstanceSerializer Serializer
        {
            get
            {
                if(serializer == null) serializer = new InstanceSerializer();
                return serializer;
            }
        }

        public class InstanceSerializer : FixedLengthInstanceSerializer<GetPortReply>
        {
            public InstanceSerializer() {}
            public override UInt32 FixedSerializationLength() { return GetPortReply.FixedSerializationLength; }
            public override void FixedLengthSerialize(Byte[] bytes, UInt32 offset, GetPortReply instance)
            {
                bytes.BigEndianSetUInt32(offset, instance.port);
                offset += 4;
            }
            public override GetPortReply FixedLengthDeserialize(Byte[] bytes, UInt32 offset)
            {
                return new GetPortReply (
                    bytes.BigEndianReadUInt32(offset + 0) // port
                );
            }
            public override void DataString(GetPortReply instance, StringBuilder builder)
            {
                builder.Append("GetPortReply:{");
                builder.Append(instance.port);
                builder.Append("}");
            }
            public override void DataSmallString(GetPortReply instance, StringBuilder builder)
            {
                builder.Append("GetPortReply:{");
                builder.Append(instance.port);
                builder.Append("}");
            }
        }

        public UInt32 port;
        private GetPortReply() { }
        public GetPortReply(UInt32 port)
        {
            this.port = port;
        }
        public FixedLengthInstanceSerializerAdapter<GetPortReply> CreateSerializerAdapater()
        {
            return new FixedLengthInstanceSerializerAdapter<GetPortReply>(Serializer, this);
        }
    }
}
