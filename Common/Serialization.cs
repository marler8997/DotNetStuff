using System;
using System.Text;
using System.Reflection;

namespace Marler.Common
{
    public interface ISerializer
    {
        // A class has a fixed serialization length if and only if it's serialization length is always the same
        // irregardless of the data it is serializing.
        // Returns -1 if serialization length is not fixed, otherwise it returns its fixed serialization length
        Int32 FixedSerializationLength();

        Int32 SerializationLength();
        Int32 Serialize(Byte[] array, Int32 offset);

        Int32 Deserialize(Byte[] array, Int32 offset, Int32 maxOffset);

        String ToNiceString();
        void ToNiceString(StringBuilder builder);
        String ToNiceSmallString();
    }
    // An IReflector serailizes to and from a classes fields using reflection
    public interface IReflector
    {
        // A class has a fixed serialization length if and only if it's serialization length is always the same
        // irregardless of the data it is serializing.
        // Returns -1 if serialization length is not fixed, otherwise it returns its fixed serialization length
        Int32 FixedSerializationLength();

        Int32 SerializationLength(ISerializer instance);
        Int32 Serialize(ISerializer instance, Byte[] array, Int32 offset);

        Int32 Deserialize(ISerializer instance, Byte[] array, Int32 offset, Int32 maxOffset);

        String ToNiceString(ISerializer instance);
        void ToNiceString(ISerializer instance, StringBuilder builder);
        String ToNiceSmallString(ISerializer instance);
    }
    public class VoidSerializer: ISerializer
    {
        private static VoidSerializer instance = null;
        public static VoidSerializer Instance
        {
            get
            {
                if (instance == null) instance = new VoidSerializer();
                return instance;
            }
        }
        private VoidSerializer() { }
        public Int32 FixedSerializationLength() { return 0; }

        public Int32 SerializationLength() { return 0; }
        public Int32 Serialize(Byte[] array, Int32 offset) { return offset; }

        public Int32 Deserialize(Byte[] array, Int32 offset, Int32 maxOffset) { return offset; }

        public String ToNiceString() { return "<void>"; }
        public void ToNiceString(StringBuilder builder) { builder.Append("<void>"); }
        public String ToNiceSmallString() { return "<void>"; }
    }
    public class VoidReflector : IReflector
    {
        private static VoidReflector instance = null;
        private static IReflector[] arrayInstance = null;
        public static VoidReflector Instance
        {
            get
            {
                if (instance == null) instance = new VoidReflector();
                return instance;
            }
        }
        public static IReflector[] ArrayInstance
        {
            get
            {
                if(arrayInstance == null) arrayInstance = new IReflector[] {Instance};
                return arrayInstance;
            }
        }
        private VoidReflector() { }
        public Int32 FixedSerializationLength() { return 0; }
        public Int32 SerializationLength(ISerializer instance) { return 0; }
        public Int32 Serialize(ISerializer instance, Byte[] array, Int32 offset) { return offset; }
        public Int32 Deserialize(ISerializer instance, Byte[] array, Int32 offset, Int32 maxOffset) { return offset; }
        public String ToNiceString(ISerializer instance) { return "<void>"; }
        public String ToNiceSmallString(ISerializer instance) { return "<void>"; }
        public void ToNiceString(ISerializer instance, StringBuilder builder) { builder.Append("<void"); }
    }
    public class ClassSerializer : ISerializer
    {
        protected readonly IReflector[] reflectors;
        protected readonly Int32 fixedSerializationLength;

        protected ClassSerializer(params IReflector[] reflectors)
        {
            this.reflectors = reflectors;

            this.fixedSerializationLength = 0;
            for (int i = 0; i < reflectors.Length; i++)
            {
                Int32 fieldFixedSerializationLength = reflectors[i].FixedSerializationLength();
                if (fieldFixedSerializationLength < 0)
                {
                    this.fixedSerializationLength = -1; // length is not fixed
                    return;
                }
                this.fixedSerializationLength += fieldFixedSerializationLength;
            }
        }
        public Int32 FixedSerializationLength()
        {
            return fixedSerializationLength;
        }
        public Int32 SerializationLength()
        {
            if (fixedSerializationLength >= 0) return fixedSerializationLength;

            Int32 length = 0;
            for (int i = 0; i < reflectors.Length; i++)
            {
                length += reflectors[i].SerializationLength(this);
            }
            return length;
        }
        public Int32 Serialize(Byte[] array, Int32 offset)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                IReflector serializer = reflectors[i];
                offset = serializer.Serialize(this, array, offset);
            }
            return offset;
        }
        public Int32 Deserialize(Byte[] array, Int32 offset, Int32 maxOffset)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                IReflector serializer = reflectors[i];
                offset = serializer.Deserialize(this, array, offset, maxOffset);
            }
            return offset;
        }
        public String ToNiceString()
        {
            StringBuilder builder = new StringBuilder();
            ToNiceString(builder);
            return builder.ToString();
        }
        public void ToNiceString(StringBuilder builder)
        {
            builder.Append(GetType().Name);
            builder.Append(":[");
            for (int i = 0; i < reflectors.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                IReflector serializer = reflectors[i];
                builder.Append(serializer.ToNiceString(this));
            }
            builder.Append("]");
        }
        public String ToNiceSmallString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(GetType().Name);
            builder.Append(":[");
            for (int i = 0; i < reflectors.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                IReflector serializer = reflectors[i];
                builder.Append(serializer.ToNiceSmallString(this));
            }
            builder.Append("]");
            return builder.ToString();
        }
    }
    public abstract class ClassFieldReflector : IReflector
    {
        public readonly FieldInfo fieldInfo;
        protected ClassFieldReflector(Type classThatHasThisField, String fieldName)
        {
            this.fieldInfo = classThatHasThisField.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (this.fieldInfo == null) throw new InvalidOperationException(String.Format(
                    "The class you provided '{0}' either does not have the field name you provided '{1}'",
                    classThatHasThisField.Name, fieldName));
        }
        protected ClassFieldReflector(Type classThatHasThisField, String fieldName, Type expectedFieldType)
            : this(classThatHasThisField, fieldName)
        {
            if (fieldInfo.FieldType != expectedFieldType)
                throw new InvalidOperationException(String.Format(
                    "In the class '{0}' field '{1}', you specified that the expected the type to be '{2}' but it is actually '{3}",
                    classThatHasThisField, fieldName, expectedFieldType.FullName, fieldInfo.FieldType.FullName));
        }
        public abstract Int32 FixedSerializationLength();
        public abstract Int32 SerializationLength(ISerializer instance);
        public abstract Int32 Serialize(ISerializer instance, Byte[] array, Int32 offset);
        public abstract Int32 Deserialize(ISerializer instance, Byte[] array, Int32 offset, Int32 maxOffset);
        public abstract String ToNiceString(ISerializer instance);
        public virtual void ToNiceString(ISerializer instance, StringBuilder builder)
        {
            builder.Append(ToNiceString(instance));
        }
        public virtual String ToNiceSmallString(ISerializer instance)
        {
            return ToNiceString(instance);
        }
    }

    public class PartialByteArraySerializer : ISerializer
    {
        private static PartialByteArraySerializer nullInstance = null;
        public static PartialByteArraySerializer Null
        {
            get
            {
                if (nullInstance == null) nullInstance = new PartialByteArraySerializer(null, 0, 0);
                return nullInstance;
            }
        }

        public Byte[] bytes;
        public Int32 offset, length;
        public PartialByteArraySerializer()
        {
        }
        public PartialByteArraySerializer(Byte[] bytes, Int32 offset, Int32 length)
        {
            this.bytes = bytes;
            this.offset = offset;
            this.length = length;
            if (offset + length > bytes.Length) throw new ArgumentOutOfRangeException();
        }
        public Int32 FixedSerializationLength()
        {
            return -1; // length is 
        }
        public Int32 SerializationLength()
        {
            if (bytes == null) return 0;
            return length;
        }
        public Int32 Serialize(Byte[] array, Int32 offset)
        {
            if (this.bytes == null) return offset;
            Array.Copy(this.bytes, this.offset, array, offset, this.length);
            return offset + this.length;
        }
        public Int32 Deserialize(Byte[] array, Int32 offset, Int32 maxOffset)
        {
            Int32 length = maxOffset - offset;

            if (length <= 0)
            {
                this.bytes = null;
                this.offset = 0;
                this.length = 0;
                return offset;
            }

            this.bytes = new Byte[length];
            Array.Copy(array, offset, this.bytes, 0, length);

            this.offset = 0;
            this.length = length;

            return offset + length;
        }
        public String ToNiceString()
        {
            return (bytes == null) ? "<null>" : BitConverter.ToString(bytes, offset, length);
        }
        public void ToNiceString(StringBuilder builder)
        {
            builder.Append(ToNiceString());
        }
        public String ToNiceSmallString()
        {
            return (bytes == null) ? "<null>" : ((bytes.Length <= 10) ?
                BitConverter.ToString(bytes) : String.Format("[{0} bytes]", length));
        }
    }
    /*
    public class ReflectorList : IReflector
    {
        private readonly IReflector[] fields;
        public readonly Int32 fixedSerializationLength;

        public ReflectorList(params IReflector[] fields)
        {
            this.fields = fields;

            this.fixedSerializationLength = 0;
            for (int i = 0; i < fields.Length; i++)
            {
                Int32 fieldFixedSerializationLength = fields[i].FixedSerializationLength();
                if (fieldFixedSerializationLength < 0)
                {
                    this.fixedSerializationLength = -1; // length is not fixed
                    return;
                }
                this.fixedSerializationLength += fieldFixedSerializationLength;
            }
        }
        public Int32 FixedSerializationLength()
        {
            return fixedSerializationLength;
        }
        public Int32 SerializationLength(ISerializer instance)
        {
            if (fixedSerializationLength >= 0)
            {
                return fixedSerializationLength;
            }
            Int32 length = 0;
            for (int i = 0; i < fields.Length; i++)
            {
                length += fields[i].SerializationLength(instance);
            }
            return length;
        }
        public Int32 Serialize(ISerializer instance, Byte[] array, Int32 offset)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                offset = fields[i].Serialize(instance, array, offset);
            }
            return offset;
        }
        public Int32 Deserialize(ISerializer instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                IReflector serializer = fields[i];
                offset = serializer.Deserialize(instance, array, offset, maxOffset);
            }
            return offset;
        }
        public String ToNiceString(ISerializer instance)
        {
            StringBuilder builder = new StringBuilder();
            ToNiceString(instance, builder);
            return builder.ToString();
        }
        public void ToNiceString(ISerializer instance, StringBuilder builder)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                IReflector serializer = fields[i];
                builder.Append(serializer.ToNiceString(instance));
            }
        }
        public String ToNiceSmallString(ISerializer instance)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < fields.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                IReflector serializer = fields[i];
                builder.Append(serializer.ToNiceSmallString(instance));
            }
            return builder.ToString();
        }
    }
     */
    /*
    public class SerializableDataFieldReflector : ClassFieldReflector
    {
        public SerializableDataFieldReflector(Type typeThatContainsThisField, String fieldName)
            : base(typeThatContainsThisField, fieldName)
        {
            if (!typeof(ISerializer).IsAssignableFrom(fieldInfo.FieldType))
            {
                throw new InvalidOperationException(String.Format(
                    "A generic reflector serializer can only be used on fields that implement the ISerializer interface.  The field you are using '{0} {1}' does not implement this interface",
                    fieldInfo.FieldType.Name, fieldInfo.Name));
            }
        }
        public override Int32 FixedSerializationLength()
        {
            return -1; // There's not really a way to tell if this type has a fixed length
        }
        private ISerializer GetValue(ISerializer instance)
        {
            if (instance == null) throw new InvalidOperationException(String.Format("The Serializer Class '{0}' cannot be null", instance.GetType().Name));

            ISerializer value = (ISerializer)fieldInfo.GetValue(instance);

            if (value == null) throw new InvalidOperationException(String.Format("The value of field '{0} {1}' cannot be null for any serialization methods using this serializer",
                fieldInfo.FieldType.Name, fieldInfo.Name));

            return value;
        }
        public override Int32 SerializationLength(ISerializer instance)
        {
            return GetValue(instance).SerializationLength();
        }
        public override Int32 Serialize(ISerializer instance, Byte[] array, Int32 offset)
        {
            return GetValue(instance).Serialize(array, offset);
        }
        public override Int32 Deserialize(ISerializer instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            return GetValue(instance).Deserialize(array, offset, maxOffset);
        }
        public override String ToNiceString(ISerializer instance)
        {
            return GetValue(instance).ToNiceString();
        }
        public override String ToNiceSmallString(ISerializer instance)
        {
            return GetValue(instance).ToNiceSmallString();
        }
    }
    */
}
