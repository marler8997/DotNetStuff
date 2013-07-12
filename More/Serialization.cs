using System;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;

namespace More
{

    // The FixedSerializationLength is -1 if the object's serialization length can change,
    // otherwise, it represents the objects fixed serialization length
    public interface ISerializer
    {
        Int32 FixedSerializationLength();

        Int32 SerializationLength();
        Int32 Serialize(Byte[] bytes, Int32 offset);

        Int32 Deserialize(Byte[] bytes, Int32 offset, Int32 offsetLimit);

        void DataString(StringBuilder builder);
        void DataSmallString(StringBuilder builder);
    }
    public delegate ISerializer Deserializer(Byte[] bytes, Int32 offset, Int32 offsetLimit);

    // An IReflector is a serializer that serializes values directly to and from CSharp fields using reflection
    public interface IReflector
    {
        Int32 FixedSerializationLength();

        Int32 SerializationLength(Object instance);
        Int32 Serialize(Object instance, Byte[] array, Int32 offset);

        Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 offsetLimit);

        void DataString(Object instance, StringBuilder builder);
        void DataSmallString(Object instance, StringBuilder builder);
    }
    /*
    public interface IGenericReflector<T>
    {
        Int32 FixedSerializationLength();

        Int32 SerializationLength(T instance);
        Int32 Serialize(T instance, Byte[] array, Int32 offset);

        Int32 Deserialize(T instance, Byte[] array, Int32 offset, Int32 offsetLimit);

        void DataString(T instance, StringBuilder builder);
        void DataSmallString(T instance, StringBuilder builder);
    }
    */

    public interface IInstanceSerializer<T>
    {
        Int32 SerializationLength(T instance);
        Int32 Serialize(Byte[] bytes, Int32 offset, T instance);
        Int32 Deserialize(Byte[] bytes, Int32 offset, Int32 offsetLimit, out T instance);
        void DataString(T instance, StringBuilder builder);
        void DataSmallString(T instance, StringBuilder builder);
    }
    public abstract class FixedLengthInstanceSerializer<T> : IInstanceSerializer<T>
    {
        public readonly Int32 fixedSerializationLength;
        protected FixedLengthInstanceSerializer()
        {
            this.fixedSerializationLength = FixedSerializationLength();
        }

        public abstract Int32 FixedSerializationLength();
        public abstract void FixedLengthSerialize(Byte[] bytes, Int32 offset, T instance);
        public abstract T FixedLengthDeserialize(Byte[] bytes, Int32 offset);

        public Int32 SerializationLength(T instance)
        {
            return fixedSerializationLength;
        }
        public Int32 Serialize(byte[] bytes, int offset, T instance)
        {
            FixedLengthSerialize(bytes, offset, instance);
            return offset + fixedSerializationLength;
        }
        public Int32 Deserialize(byte[] bytes, int offset, int offsetLimit, out T instance)
        {
            instance = FixedLengthDeserialize(bytes, offset);
            return offset + fixedSerializationLength;
        }
        public abstract void DataString(T instance, StringBuilder builder);
        public abstract void DataSmallString(T instance, StringBuilder builder);
    }


    public class InstanceSerializerAdapter<T> : ISerializer
    {
        readonly IInstanceSerializer<T> serializer;
        public T instance;
        public InstanceSerializerAdapter(IInstanceSerializer<T> serializer, T instance)
        {
            this.serializer = serializer;
            this.instance = instance;
        }
        public int FixedSerializationLength()              { return -1; }
        public int SerializationLength()                   { return serializer.SerializationLength(instance); }
        public int Serialize(byte[] bytes, int offset)     { return serializer.Serialize(bytes, offset, instance); }
        public int Deserialize(byte[] bytes, int offset, int offsetLimit)
                                                           { return serializer.Deserialize(bytes, offset, offsetLimit, out instance); }
        public void DataString(StringBuilder builder)      { serializer.DataString(instance, builder); }
        public void DataSmallString(StringBuilder builder) { serializer.DataSmallString(instance, builder); }
    }

    public class FixedLengthInstanceSerializerAdapter<T> : ISerializer
    {
        readonly FixedLengthInstanceSerializer<T> serializer;
        public T instance;
        public FixedLengthInstanceSerializerAdapter(FixedLengthInstanceSerializer<T> serializer, T instance)
        {
            this.serializer = serializer;
            this.instance = instance;
        }
        public int FixedSerializationLength()          { return serializer.fixedSerializationLength; }
        public int SerializationLength()               { return serializer.fixedSerializationLength; }
        public int Serialize(byte[] bytes, int offset)
        {
            serializer.FixedLengthSerialize(bytes, offset, instance);
            return offset + serializer.fixedSerializationLength;
        }
        public int Deserialize(byte[] bytes, int offset, int offsetLimit)
        {
            instance = serializer.FixedLengthDeserialize(bytes, offset);
            return offset + serializer.fixedSerializationLength;
        }
        public void DataString(StringBuilder builder)      { serializer.DataString(instance, builder); }
        public void DataSmallString(StringBuilder builder) { serializer.DataSmallString(instance, builder); }
    }


    
    //public delegate T FixedLengthDeserializer<T>(Byte[] bytes, Int32 offset);
    //public delegate T DynamicLengthDeserializer<T>(Byte[] bytes, Int32 offset, Int32 offsetLimit, out Int32 newOffset);

    /*
    public class FixedLengthReflectorInstanceSerializer<T> : FixedLengthInstanceSerializer<T>
    {
        readonly IReflector reflector;
        FixedLengthDeserializer<T> deserializer;
        readonly Int32 fixedSerializationLength;
        public FixedLengthReflectorInstanceSerializer(IReflector reflector, FixedLengthDeserializer<T> deserializer)
        {
            this.reflector = reflector;
            this.deserializer = deserializer;
            fixedSerializationLength = reflector.FixedSerializationLength();
            if (fixedSerializationLength < 0) throw new InvalidOperationException(
                 "This class is only for reflectors with FixedSerializationLength");
        }
        public int FixedSerializationLength()                          { return fixedSerializationLength; }
        public void Serialize(byte[] bytes, int offset, T instance)    { reflector.Serialize(instance, bytes, offset); }
        public void DataString(T instance, StringBuilder builder)      { reflector.DataString(instance, builder); }
        public void DataSmallString(T instance, StringBuilder builder) { reflector.DataSmallString(instance, builder); }
        public T Deserialize(byte[] array, int offset)
        {
            return deserializer(array, offset);
        }
    }
    public abstract class DynamicLengthSerializer<T> : IDynamicLengthSerializer<T>
    {
        readonly IReflector reflector;
        public DynamicLengthSerializer(IReflector reflector)
        {
            this.reflector = reflector;
        }
        public int SerializationLength(T instance)                 { return reflector.SerializationLength(instance); }
        public int Serialize(byte[] array, int offset, T instance) { return reflector.Serialize(instance, array, offset); }
        public void DataString(T instance, StringBuilder builder)
        {
            reflector.DataString(instance, builder);
        }
        public void DataSmallString(T instance, StringBuilder builder)
        {
            reflector.DataSmallString(instance, builder);
        }

        public abstract int Deserialize(byte[] array, int offset, int offsetLimit, out T outInstance);
    }
    */

    public static class ISerializerString
    {
        public static String DataString(ISerializer reflector)
        {
            StringBuilder builder = new StringBuilder();
            reflector.DataString(builder);
            return builder.ToString();
        }
        public static String DataSmallString(ISerializer reflector)
        {
            StringBuilder builder = new StringBuilder();
            reflector.DataSmallString(builder);
            return builder.ToString();
        }
        public static String DataString(IReflector reflector, Object instance)
        {
            StringBuilder builder = new StringBuilder();
            reflector.DataString(instance, builder);
            return builder.ToString();
        }
        public static String DataSmallString(IReflector reflector, Object instance)
        {
            StringBuilder builder = new StringBuilder();
            reflector.DataSmallString(instance, builder);
            return builder.ToString();
        }
        /*
        public static String DataString<T>(IGenericReflector<T> reflector, T instance)
        {
            StringBuilder builder = new StringBuilder();
            reflector.DataString(instance, builder);
            return builder.ToString();
        }
        public static String DataSmallString<T>(IGenericReflector<T> reflector, T instance)
        {
            StringBuilder builder = new StringBuilder();
            reflector.DataSmallString(instance, builder);
            return builder.ToString();
        }
        */
    }

    public class ReflectorToSerializerAdapater : ISerializer
    {
        protected readonly IReflector reflector;
        readonly Object instance;
        readonly Int32 fixedSerializationLength;

        public ReflectorToSerializerAdapater(IReflector reflector, Object instance)
        {
            this.reflector = reflector;
            this.instance = instance;
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
        public Int32 Deserialize(Byte[] array, Int32 offset, Int32 offsetLimit)
        {
            return reflector.Deserialize(instance, array, offset, offsetLimit);
        }
        public void DataString(StringBuilder builder)      { reflector.DataString(instance, builder); }
        public void DataSmallString(StringBuilder builder) { reflector.DataSmallString(instance, builder); }
    }

    public class Reflectors : IReflector
    {
        public readonly IReflector[] reflectors;
        public readonly Int32 fixedSerializationLength;

        public Reflectors(params IReflector[] reflectors)
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
        public delegate IReflector ReflectorCreator(Reflectors theseReflectors);
        public Reflectors(IReflector[] reflectors, ReflectorCreator nullReflectorCreator)
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
        public int SerializationLength(Object instance)
        {
            if(fixedSerializationLength >= 0) return fixedSerializationLength;

            Int32 length = 0;
            for(int i = 0; i < reflectors.Length; i++)
            {
                length += reflectors[i].SerializationLength(instance);
            }
            return length;
        }
        public int Serialize(Object instance, byte[] array, int offset)
        {
            for(int i = 0; i < reflectors.Length; i++)
            {
                offset = reflectors[i].Serialize(instance, array, offset);
            }
            return offset;
        }
        public int Deserialize(Object instance, byte[] array, int offset, int offsetLimit)
        {
            for(int i = 0; i < reflectors.Length; i++)
            {
                offset = reflectors[i].Deserialize(instance, array, offset, offsetLimit);
            }
            return offset;
        }
        public void DataString(Object instance, StringBuilder builder)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                reflectors[i].DataString(instance, builder);
            }
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

        public Int32 Deserialize(Byte[] array, Int32 offset, Int32 offsetLimit) { return offset; }

        public void DataString(StringBuilder builder)      { builder.Append("<void>"); }
        public void DataSmallString(StringBuilder builder) { builder.Append("<void>"); }
    }
    public class VoidReflector : IReflector
    {
        private static VoidReflector instance = null;
        private static IReflector[] reflectorsArrayInstance = null;
        private static Reflectors reflectorsInstance = null;
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
                if (reflectorsArrayInstance == null) reflectorsArrayInstance = new IReflector[] { Instance };
                return reflectorsArrayInstance;
            }
        }
        public static Reflectors Reflectors
        {
            get
            {
                if (reflectorsInstance == null) reflectorsInstance = new Reflectors(ReflectorsArray);
                return reflectorsInstance;
            }
        }
        private VoidReflector() { }
        public Int32 FixedSerializationLength() { return 0; }
        public Int32 SerializationLength(Object instance) { return 0; }
        public Int32 Serialize(Object instance, Byte[] array, Int32 offset) { return offset; }
        public Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 offsetLimit) { return offset; }
        public void DataString(Object instance, StringBuilder builder)      { builder.Append("<void>"); }
        public void DataSmallString(Object instance, StringBuilder builder) { builder.Append("<void>"); }
    }
    public class SubclassSerializer : ISerializer
    {
        protected readonly IReflector[] reflectors;
        protected readonly Int32 fixedSerializationLength;

        public SubclassSerializer(Reflectors reflectors)
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
        public Int32 Deserialize(Byte[] array, Int32 offset, Int32 offsetLimit)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                IReflector serializer = reflectors[i];
                offset = serializer.Deserialize(this, array, offset, offsetLimit);
            }
            return offset;
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

        public SerializerFromObjectAndReflectors(Object instance, Reflectors reflectors)
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
        public Int32 Deserialize(Byte[] array, Int32 offset, Int32 offsetLimit)
        {
            for (int i = 0; i < reflectors.Length; i++)
            {
                IReflector serializer = reflectors[i];
                offset = serializer.Deserialize(instance, array, offset, offsetLimit);
            }
            return offset;
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
                    "In the class '{0}' field '{1}', you specified the expected type to be '{2}' but it is actually '{3}",
                    classThatHasThisField.Name, fieldName, expectedFieldType.FullName, fieldInfo.FieldType.FullName));
        }
        public abstract Int32 FixedSerializationLength();
        public abstract Int32 SerializationLength(Object instance);
        public abstract Int32 Serialize(Object instance, Byte[] array, Int32 offset);
        public abstract Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 offsetLimit);
        public abstract void DataString(Object instance, StringBuilder builder);
        public virtual void DataSmallString(Object instance, StringBuilder builder)
        {
            DataString(instance, builder);
        }
    }
    public class ClassFieldReflectors<FieldType> : ClassFieldReflector /* where FieldType : new() */
    {
        private IReflector[] fieldReflectors;
        private Int32 fixedSerializationLength;

        public ClassFieldReflectors(Type classThatHasThisField, String fieldName, Reflectors fieldReflectors)
            : base(classThatHasThisField, fieldName)
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
        public override Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 offsetLimit)
        {
            if (fieldReflectors == null) return offset;

            Object structObject = FormatterServices.GetUninitializedObject(typeof(FieldType));
            //FieldType structObject = new FieldType();

            for (int i = 0; i < fieldReflectors.Length; i++)
            {
                IReflector serializer = fieldReflectors[i];
                offset = serializer.Deserialize(structObject, array, offset, offsetLimit);
            }

            fieldInfo.SetValue(instance, structObject);

            return offset;
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
        public Int32 Deserialize(Byte[] array, Int32 offset, Int32 offsetLimit)
        {
            Int32 length = offsetLimit - offset;

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
        public void DataString(StringBuilder builder)
        {
            builder.Append((bytes == null) ? "<null>" : BitConverter.ToString(bytes, offset, length));
        }
        public void DataSmallString(StringBuilder builder)
        {
            builder.Append((bytes == null) ? "<null>" : ((bytes.Length <= 10) ?
                BitConverter.ToString(bytes) : String.Format("[{0} bytes]", length)));
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
