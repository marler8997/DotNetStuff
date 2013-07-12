using System;
using System.Text;

using More;

namespace More.Net.PdlTestObjects
{
    public class AByte
    {
        public const Int32 FixedSerializationLength = 1;

        static InstanceSerializer serializer = null;
        public static FixedLengthInstanceSerializer<AByte> Serializer
        {
            get
            {
                if(serializer == null) serializer = new InstanceSerializer();
                return serializer;
            }
        }

        class InstanceSerializer : FixedLengthInstanceSerializer<AByte>
        {
            public InstanceSerializer() {}
            public override Int32 FixedSerializationLength() { return AByte.FixedSerializationLength; }
            public override void FixedLengthSerialize(Byte[] bytes, Int32 offset, AByte instance)
            {
                bytes[offset] = instance.value;
                offset += 1;
            }
            public override AByte FixedLengthDeserialize(Byte[] bytes, Int32 offset)
            {
                return new AByte (
                    bytes[offset + 0] // value
                );
            }
            public override void DataString(AByte instance, StringBuilder builder)
            {
                builder.Append("AByte:{");
                builder.Append(instance.value);
                builder.Append("}");
            }
            public override void DataSmallString(AByte instance, StringBuilder builder)
            {
                builder.Append("AByte:{");
                builder.Append(instance.value);
                builder.Append("}");
            }
        }

        public Byte value;
        private AByte() { }
        public AByte(Byte value)
        {
            this.value = value;
        }
        public FixedLengthInstanceSerializerAdapter<AByte> CreateSerializerAdapater()
        {
            return new FixedLengthInstanceSerializerAdapter<AByte>(Serializer, this);
        }
    }
    public class AnSByte
    {
        public const Int32 FixedSerializationLength = 1;

        static InstanceSerializer serializer = null;
        public static FixedLengthInstanceSerializer<AnSByte> Serializer
        {
            get
            {
                if(serializer == null) serializer = new InstanceSerializer();
                return serializer;
            }
        }

        class InstanceSerializer : FixedLengthInstanceSerializer<AnSByte>
        {
            public InstanceSerializer() {}
            public override Int32 FixedSerializationLength() { return AnSByte.FixedSerializationLength; }
            public override void FixedLengthSerialize(Byte[] bytes, Int32 offset, AnSByte instance)
            {
                bytes[offset] = (Byte)instance.value;
                offset += 1;
            }
            public override AnSByte FixedLengthDeserialize(Byte[] bytes, Int32 offset)
            {
                return new AnSByte (
                    (SByte)bytes[offset + 0] // value
                );
            }
            public override void DataString(AnSByte instance, StringBuilder builder)
            {
                builder.Append("AnSByte:{");
                builder.Append(instance.value);
                builder.Append("}");
            }
            public override void DataSmallString(AnSByte instance, StringBuilder builder)
            {
                builder.Append("AnSByte:{");
                builder.Append(instance.value);
                builder.Append("}");
            }
        }

        public SByte value;
        private AnSByte() { }
        public AnSByte(SByte value)
        {
            this.value = value;
        }
        public FixedLengthInstanceSerializerAdapter<AnSByte> CreateSerializerAdapater()
        {
            return new FixedLengthInstanceSerializerAdapter<AnSByte>(Serializer, this);
        }
    }
    public class AUInt16
    {
        public const Int32 FixedSerializationLength = 2;

        static InstanceSerializer serializer = null;
        public static FixedLengthInstanceSerializer<AUInt16> Serializer
        {
            get
            {
                if(serializer == null) serializer = new InstanceSerializer();
                return serializer;
            }
        }

        class InstanceSerializer : FixedLengthInstanceSerializer<AUInt16>
        {
            public InstanceSerializer() {}
            public override Int32 FixedSerializationLength() { return AUInt16.FixedSerializationLength; }
            public override void FixedLengthSerialize(Byte[] bytes, Int32 offset, AUInt16 instance)
            {
                bytes.BigEndianSetUInt16(offset, instance.value);
                offset += 2;
            }
            public override AUInt16 FixedLengthDeserialize(Byte[] bytes, Int32 offset)
            {
                return new AUInt16 (
                    bytes.BigEndianReadUInt16(offset + 0) // value
                );
            }
            public override void DataString(AUInt16 instance, StringBuilder builder)
            {
                builder.Append("AUInt16:{");
                builder.Append(instance.value);
                builder.Append("}");
            }
            public override void DataSmallString(AUInt16 instance, StringBuilder builder)
            {
                builder.Append("AUInt16:{");
                builder.Append(instance.value);
                builder.Append("}");
            }
        }

        public UInt16 value;
        private AUInt16() { }
        public AUInt16(UInt16 value)
        {
            this.value = value;
        }
        public FixedLengthInstanceSerializerAdapter<AUInt16> CreateSerializerAdapater()
        {
            return new FixedLengthInstanceSerializerAdapter<AUInt16>(Serializer, this);
        }
    }
    public class AnInt16
    {
        public const Int32 FixedSerializationLength = 2;

        static InstanceSerializer serializer = null;
        public static FixedLengthInstanceSerializer<AnInt16> Serializer
        {
            get
            {
                if(serializer == null) serializer = new InstanceSerializer();
                return serializer;
            }
        }

        class InstanceSerializer : FixedLengthInstanceSerializer<AnInt16>
        {
            public InstanceSerializer() {}
            public override Int32 FixedSerializationLength() { return AnInt16.FixedSerializationLength; }
            public override void FixedLengthSerialize(Byte[] bytes, Int32 offset, AnInt16 instance)
            {
                bytes.BigEndianSetInt16(offset, instance.value);
                offset += 2;
            }
            public override AnInt16 FixedLengthDeserialize(Byte[] bytes, Int32 offset)
            {
                return new AnInt16 (
                    bytes.BigEndianReadInt16(offset + 0) // value
                );
            }
            public override void DataString(AnInt16 instance, StringBuilder builder)
            {
                builder.Append("AnInt16:{");
                builder.Append(instance.value);
                builder.Append("}");
            }
            public override void DataSmallString(AnInt16 instance, StringBuilder builder)
            {
                builder.Append("AnInt16:{");
                builder.Append(instance.value);
                builder.Append("}");
            }
        }

        public Int16 value;
        private AnInt16() { }
        public AnInt16(Int16 value)
        {
            this.value = value;
        }
        public FixedLengthInstanceSerializerAdapter<AnInt16> CreateSerializerAdapater()
        {
            return new FixedLengthInstanceSerializerAdapter<AnInt16>(Serializer, this);
        }
    }
    public class AUInt24
    {
        public const Int32 FixedSerializationLength = 3;

        static InstanceSerializer serializer = null;
        public static FixedLengthInstanceSerializer<AUInt24> Serializer
        {
            get
            {
                if(serializer == null) serializer = new InstanceSerializer();
                return serializer;
            }
        }

        class InstanceSerializer : FixedLengthInstanceSerializer<AUInt24>
        {
            public InstanceSerializer() {}
            public override Int32 FixedSerializationLength() { return AUInt24.FixedSerializationLength; }
            public override void FixedLengthSerialize(Byte[] bytes, Int32 offset, AUInt24 instance)
            {
                bytes.BigEndianSetUInt24(offset, instance.value);
                offset += 3;
            }
            public override AUInt24 FixedLengthDeserialize(Byte[] bytes, Int32 offset)
            {
                return new AUInt24 (
                    bytes.BigEndianReadUInt24(offset + 0) // value
                );
            }
            public override void DataString(AUInt24 instance, StringBuilder builder)
            {
                builder.Append("AUInt24:{");
                builder.Append(instance.value);
                builder.Append("}");
            }
            public override void DataSmallString(AUInt24 instance, StringBuilder builder)
            {
                builder.Append("AUInt24:{");
                builder.Append(instance.value);
                builder.Append("}");
            }
        }

        public UInt32 value;
        private AUInt24() { }
        public AUInt24(UInt32 value)
        {
            this.value = value;
        }
        public FixedLengthInstanceSerializerAdapter<AUInt24> CreateSerializerAdapater()
        {
            return new FixedLengthInstanceSerializerAdapter<AUInt24>(Serializer, this);
        }
    }
}
