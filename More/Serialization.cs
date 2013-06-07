using System;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;

namespace More
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

        String DataString();
        void DataString(StringBuilder builder);
        String DataSmallString();
        void DataSmallString(StringBuilder builder);
    }
    // An IReflector is a serializer that serializes values directly to and from CSharp fields using reflection
    public interface IReflector
    {
        // A class has a fixed serialization length if and only if it's serialization length is always the same
        // irregardless of the data it is serializing.
        // Returns -1 if serialization length is not fixed, otherwise it returns its fixed serialization length
        Int32 FixedSerializationLength();

        Int32 SerializationLength(Object instance);
        Int32 Serialize(Object instance, Byte[] array, Int32 offset);

        Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset);

        String DataString(Object instance);
        void DataString(Object instance, StringBuilder builder);
        String DataSmallString(Object instance);
        void DataSmallString(Object instance, StringBuilder builder);
    }


    public class ReflectorToSerializerAdapater : ISerializer
    {
        readonly Object instance;
        protected readonly IReflector reflector;
        readonly Int32 fixedSerializationLength;

        public ReflectorToSerializerAdapater(Object instance, IReflector reflector)
        {
            this.instance = instance;
            this.reflector = reflector;
            this.fixedSerializationLength = reflector.FixedSerializationLength();
        }
        public Int32 FixedSerializationLength()
        {
            return fixedSerializationLength;
        }
        public Int32 SerializationLength()
        {
            return reflector.SerializationLength(instance);
        }
        public Int32 Serialize(Byte[] array, Int32 offset)
        {
            return reflector.Serialize(instance, array, offset);
        }
        public Int32 Deserialize(Byte[] array, Int32 offset, Int32 maxOffset)
        {
            return reflector.Deserialize(instance, array, offset, maxOffset);
        }
        public String DataString()                         { return reflector.DataString(instance); }
        public void DataString(StringBuilder builder)      { reflector.DataString(instance, builder); }
        public String DataSmallString()                    { return reflector.DataSmallString(instance); }
        public void DataSmallString(StringBuilder builder) { reflector.DataSmallString(instance, builder); }
    }

    public class IReflectors : IReflector
    {
        public readonly IReflector[] reflectors;
        public readonly Int32 fixedSerializationLength;

        public IReflectors(params IReflector[] reflectors)
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
        //
        // Create Reflectors with circular references
        //
        public delegate IReflector ReflectorCreator(IReflectors theseReflectors);
        public IReflectors(IReflector[] reflectors, ReflectorCreator nullReflectorCreator)
        {
            this.reflectors = reflectors;
            this.fixedSerializationLength = -1;

            Boolean foundNull = false;

            for (int i = 0; i < reflectors.Length; i++)
            {
                IReflector reflector = reflectors[i];
                if (reflector == null)
                {
                    if (foundNull) throw new InvalidOperationException("This constructor requires one and only one IReflector to be null but found more than one");
                    foundNull = true;
                    reflector = nullReflectorCreator(this);
                    if (reflector == null) throw new InvalidOperationException("The null reflector creator you provided returned null");
                    reflectors[i] = reflector;
                }
            }

            if (!foundNull) throw new InvalidOperationException("This constructor requires that one of the reflectors is null but none were null");
        }

        public int FixedSerializationLength()
        {
            return fixedSerializationLength;
        }
        public int SerializationLength(object instance)
        {
            if(fixedSerializationLength >= 0) return fixedSerializationLength;

            Int32 length = 0;
            for(int i = 0; i < reflectors.Length; i++)
            {
                length += reflectors[i].SerializationLength(instance);
            }
            return length;
        }
        public int Serialize(object instance, byte[] array, int offset)
        {
            for(int i = 0; i < reflectors.Length; i++)
            {
                offset = reflectors[i].Serialize(instance, array, offset);
            }
            return offset;
        }
        public int Deserialize(object instance, byte[] array, int offset, int maxOffset)
        {
            for(int i = 0; i < reflectors.Length; i++)
            {
                offset = reflectors[i].Deserialize(instance, array, offset, maxOffset);
            }
            return offset;
        }
        public String DataString(Object instance)
        {
            StringBuilder builder = new StringBuilder();
            DataString(instance, builder);
            return builder.ToString();
        }
        public void DataString(Object instance, StringBuilder builder)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                reflectors[i].DataString(instance, builder);
            }
        }
        public String DataSmallString(Object instance)
        {
            StringBuilder builder = new StringBuilder();
            DataSmallString(instance, builder);
            return builder.ToString();
        }
        public void DataSmallString(Object instance, StringBuilder builder)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                reflectors[i].DataSmallString(instance, builder);
            }
        }
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

        public String DataString()                         { return "<void>"; }
        public void DataString(StringBuilder builder)      { builder.Append("<void>"); }
        public String DataSmallString()                    { return "<void>"; }
        public void DataSmallString(StringBuilder builder) { builder.Append("<void>"); }
    }
    public class VoidReflector : IReflector
    {
        private static VoidReflector instance = null;
        private static IReflector[] reflectorsArrayInstance = null;
        private static IReflectors reflectorsInstance = null;
        public static VoidReflector Instance
        {
            get
            {
                if (instance == null) instance = new VoidReflector();
                return instance;
            }
        }
        public static IReflector[] ReflectorsArray
        {
            get
            {
                if (reflectorsArrayInstance == null) reflectorsArrayInstance = new IReflector[]{Instance};
                return reflectorsArrayInstance;
            }
        }
        public static IReflectors Reflectors
        {
            get
            {
                if (reflectorsInstance == null) reflectorsInstance = new IReflectors(ReflectorsArray);
                return reflectorsInstance;
            }
        }
        private VoidReflector() { }
        public Int32 FixedSerializationLength() { return 0; }
        public Int32 SerializationLength(Object instance) { return 0; }
        public Int32 Serialize(Object instance, Byte[] array, Int32 offset) { return offset; }
        public Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset) { return offset; }
        public String DataString(Object instance)                           { return "<void>"; }
        public void DataString(Object instance, StringBuilder builder)      { builder.Append("<void>"); }
        public String DataSmallString(Object instance)                      { return "<void>"; }
        public void DataSmallString(Object instance, StringBuilder builder) { builder.Append("<void>"); }
    }
    public class SubclassSerializer : ISerializer
    {
        protected readonly IReflector[] reflectors;
        protected readonly Int32 fixedSerializationLength;

        public SubclassSerializer(IReflectors reflectors)
        {
            this.reflectors = reflectors.reflectors;
            this.fixedSerializationLength = reflectors.fixedSerializationLength;
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
        public String DataString()
        {
            StringBuilder builder = new StringBuilder();
            DataString(builder);
            return builder.ToString();
        }
        public void DataString(StringBuilder builder)
        {
            builder.Append(GetType().Name);
            builder.Append(":[");
            for (int i = 0; i < reflectors.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                reflectors[i].DataString(this, builder);
            }
            builder.Append("]");
        }
        public String DataSmallString()
        {
            StringBuilder builder = new StringBuilder();
            DataSmallString(builder);
            return builder.ToString();
        }
        public void DataSmallString(StringBuilder builder)
        {
            builder.Append(GetType().Name);
            builder.Append(":[");
            for (int i = 0; i < reflectors.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                reflectors[i].DataSmallString(this, builder);
            }
            builder.Append("]");
        }
    }


    public interface ISerializerCreator
    {
        ISerializer CreateSerializer();
    }
    public class SerializerFromObjectAndReflectors : ISerializer
    {
        readonly Object instance;
        protected readonly IReflector[] reflectors;
        protected readonly Int32 fixedSerializationLength;

        public SerializerFromObjectAndReflectors(Object instance, IReflectors reflectors)
        {
            this.instance = instance;
            this.reflectors = reflectors.reflectors;
            this.fixedSerializationLength = reflectors.fixedSerializationLength;
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
                length += reflectors[i].SerializationLength(instance);
            }
            return length;
        }
        public Int32 Serialize(Byte[] array, Int32 offset)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                IReflector serializer = reflectors[i];
                offset = serializer.Serialize(instance, array, offset);
            }
            return offset;
        }
        public Int32 Deserialize(Byte[] array, Int32 offset, Int32 maxOffset)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                IReflector serializer = reflectors[i];
                offset = serializer.Deserialize(instance, array, offset, maxOffset);
            }
            return offset;
        }
        public String DataString()
        {
            StringBuilder builder = new StringBuilder();
            DataString(builder);
            return builder.ToString();
        }
        public void DataString(StringBuilder builder)
        {
            builder.Append(instance.GetType().Name);
            builder.Append(":[");
            for (int i = 0; i < reflectors.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                reflectors[i].DataString(instance, builder);
            }
            builder.Append("]");
        }
        public String DataSmallString()
        {
            StringBuilder builder = new StringBuilder();
            DataSmallString(builder);
            return builder.ToString();
        }
        public void DataSmallString(StringBuilder builder)
        {
            builder.Append(instance.GetType().Name);
            builder.Append(":[");
            for (int i = 0; i < reflectors.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                reflectors[i].DataSmallString(instance, builder);
            }
            builder.Append("]");
        }
    }

    public abstract class ClassFieldReflector : IReflector
    {
        public readonly FieldInfo fieldInfo;
        protected ClassFieldReflector(Type classThatHasThisField, String fieldName)
        {
            this.fieldInfo = classThatHasThisField.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (this.fieldInfo == null) throw new InvalidOperationException(String.Format(
                    "The class you provided '{0}' does not have the field name you provided '{1}'",
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
        public abstract Int32 SerializationLength(Object instance);
        public abstract Int32 Serialize(Object instance, Byte[] array, Int32 offset);
        public abstract Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset);
        public abstract String DataString(Object instance);
        public virtual void DataString(Object instance, StringBuilder builder)
        {
            builder.Append(DataString(instance));
        }
        public virtual String DataSmallString(Object instance)
        {
            return DataString(instance);
        }
        public virtual void DataSmallString(Object instance, StringBuilder builder)
        {
            builder.Append(DataSmallString(instance));
        }
    }

    public class ClassFieldReflectors<FieldType> : ClassFieldReflector /* where FieldType : new() */
    {
        private IReflector[] fieldReflectors;
        private Int32 fixedSerializationLength;

        public ClassFieldReflectors(Type typeThatContainsThisField, String fieldName, IReflectors fieldReflectors)
            : base(typeThatContainsThisField, fieldName)
        {
            this.fieldReflectors = fieldReflectors.reflectors;
            this.fixedSerializationLength = fieldReflectors.fixedSerializationLength;
        }
        public override Int32 FixedSerializationLength()
        {
            return fixedSerializationLength;
        }
        public override Int32 SerializationLength(Object instance)
        {
            if (fieldReflectors == null) return 0;
            if (fixedSerializationLength >= 0) return fixedSerializationLength;

            Object structInstance = fieldInfo.GetValue(instance);

            Int32 length = 0;
            for (int i = 0; i < fieldReflectors.Length; i++)
            {
                length += fieldReflectors[i].SerializationLength(structInstance);
            }
            return length;
        }
        public override Int32 Serialize(Object instance, Byte[] array, Int32 offset)
        {
            if (fieldReflectors == null) return offset;

            Object structInstance = (instance == null) ? null : fieldInfo.GetValue(instance);

            for (int i = 0; i < fieldReflectors.Length; i++)
            {
                IReflector serializer = fieldReflectors[i];
                offset = serializer.Serialize(structInstance, array, offset);
            }
            return offset;
        }
        public override Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            if (fieldReflectors == null) return offset;

            Object structObject = FormatterServices.GetUninitializedObject(typeof(FieldType));
            //FieldType structObject = new FieldType();

            for (int i = 0; i < fieldReflectors.Length; i++)
            {
                IReflector serializer = fieldReflectors[i];
                offset = serializer.Deserialize(structObject, array, offset, maxOffset);
            }

            fieldInfo.SetValue(instance, structObject);

            return offset;
        }
        public override String DataString(Object instance)
        {
            StringBuilder builder = new StringBuilder();
            DataString(instance, builder);
            return builder.ToString();
        }
        public override void DataString(Object instance, StringBuilder builder)
        {
            builder.Append(fieldInfo.Name);
            builder.Append(':');

            Object structInstance = fieldInfo.GetValue(instance);

            if (structInstance == null)
            {
                builder.Append("<null>");
                return;
            }
            builder.Append("{");

            if (fieldReflectors != null)
            {
                for (int i = 0; i < fieldReflectors.Length; i++)
                {
                    if (i > 0) builder.Append(", ");

                    fieldReflectors[i].DataString(structInstance, builder);
                }
            }
            builder.Append("}");
        }
        public override String DataSmallString(Object instance)
        {
            StringBuilder builder = new StringBuilder();
            DataSmallString(instance, builder);
            return builder.ToString();
        }
        public override void DataSmallString(Object instance, StringBuilder builder)
        {
            builder.Append(fieldInfo.Name);
            builder.Append(':');

            Object structInstance = fieldInfo.GetValue(instance);

            if (structInstance == null)
            {
                builder.Append("<null>");
                return;
            }
            builder.Append("{");

            if (fieldReflectors != null)
            {
                for (int i = 0; i < fieldReflectors.Length; i++)
                {
                    if (i > 0) builder.Append(", ");

                    fieldReflectors[i].DataSmallString(structInstance, builder);
                }
            }
            builder.Append("}");
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
        public String DataString()
        {
            return (bytes == null) ? "<null>" : BitConverter.ToString(bytes, offset, length);
        }
        public void DataString(StringBuilder builder)
        {
            builder.Append(DataString());
        }
        public String DataSmallString()
        {
            return (bytes == null) ? "<null>" : ((bytes.Length <= 10) ?
                BitConverter.ToString(bytes) : String.Format("[{0} bytes]", length));
        }
        public void DataSmallString(StringBuilder builder)
        {
            builder.Append(DataSmallString());
        }
    }
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
        public override Int32 SerializationLength(Object instance)
        {
            return GetValue(instance).SerializationLength();
        }
        public override Int32 Serialize(Object instance, Byte[] array, Int32 offset)
        {
            return GetValue(instance).Serialize(array, offset);
        }
        public override Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            return GetValue(instance).Deserialize(array, offset, maxOffset);
        }
        public override String ToNiceString(Object instance)
        {
            return GetValue(instance).ToNiceString();
        }
        public override String ToNiceSmallString(Object instance)
        {
            return GetValue(instance).ToNiceSmallString();
        }
    }
    */
}
