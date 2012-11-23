using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Marler.NetworkTools
{
    public interface ISerializableReflector
    {
        /// <summary>
        /// A class has a fixed serialization length if and only if it's serialization length is always the same
        /// irregardless of the data it is serializing.
        /// </summary>
        /// <returns>-1 if serialization length is not fixed, otherwise it returns its fixed serialization length</returns>
        Int32 GetFixedSerializationLength();

        Int32 SerializationLength(ISerializableData instance);
        Int32 Serialize(ISerializableData instance, Byte[] array, Int32 offset);

        Int32 Deserialize(ISerializableData instance, Byte[] array, Int32 offset, Int32 maxOffset);

        String ToNiceString(ISerializableData instance);
        void ToNiceString(ISerializableData instance, StringBuilder builder);
        String ToNiceSmallString(ISerializableData instance);
    }
    public class VoidReflectorSerializer : ISerializableReflector
    {
        private static VoidReflectorSerializer instance = null;
        public static VoidReflectorSerializer Instance
        {
            get
            {
                if (instance == null) instance = new VoidReflectorSerializer();
                return instance;
            }
        }
        private VoidReflectorSerializer() { }
        public Int32 GetFixedSerializationLength()                                     { return      0;   }
        public Int32 SerializationLength(ISerializableData instance)                   { return      0;   }
        public Int32 Serialize(ISerializableData instance, Byte[] array, Int32 offset) { return  offset;  }
        public Int32 Deserialize(ISerializableData instance, Byte[] array,
            Int32 offset, Int32 maxOffset)                                             { return  offset;  }
        public String ToNiceString(ISerializableData instance)                         { return "<void>"; }
        public String ToNiceSmallString(ISerializableData instance)                    { return "<void>"; }
        public void ToNiceString(ISerializableData instance, StringBuilder builder)    { builder.Append("<void"); }
    }


    /*
    public class SingleReflectorSerializer : ISerializableData
    {
        protected readonly ISerializableReflector serializer;
        protected SingleReflectorSerializer(ISerializableReflector serializer)
        {
            this.serializer = serializer;
        }
        public Int32 GetFixedSerializationLength()
        {
            return serializer.GetFixedSerializationLength();
        }
        public int SerializationLength()
        {
            return serializer.SerializationLength(this);
        }
        public int Serialize(byte[] array, int offset)
        {
            return serializer.Serialize(this, array, offset);
        }
        public int Deserialize(byte[] array, int offset, int maxOffset)
        {
            return serializer.Deserialize(this, array, offset, maxOffset);
        }
        public String ToNiceString()
        {
            return serializer.ToNiceString(this);
        }
        public void ToNiceString(StringBuilder builder)
        {
            serializer.ToNiceString(this, builder);
        }
        public String ToNiceSmallString()
        {
            return serializer.ToNiceSmallString(this);
        }
    }
    */
    public class ObjectReflectorSerializer : ISerializableData
    {
        protected readonly ISerializableReflector[] serializers;
        protected readonly Int32 fixedSerializationLength;

        protected ObjectReflectorSerializer(params ISerializableReflector[] serializers)
        {
            this.serializers = serializers;

            this.fixedSerializationLength = 0;
            for (int i = 0; i < serializers.Length; i++)
            {
                Int32 fieldFixedSerializationLength = serializers[i].GetFixedSerializationLength();
                if (fieldFixedSerializationLength < 0)
                {
                    this.fixedSerializationLength = -1; // length is not fixed
                    return;
                }
                this.fixedSerializationLength += fieldFixedSerializationLength;
            }
        }
        public Int32 GetFixedSerializationLength()
        {
            return fixedSerializationLength;
        }
        public Int32 SerializationLength()
        {
            if (fixedSerializationLength >= 0) return fixedSerializationLength;

            Int32 length = 0;
            for (int i = 0; i < serializers.Length; i++)
            {
                length += serializers[i].SerializationLength(this);
            }
            return length;
        }
        public Int32 Serialize(Byte[] array, Int32 offset)
        {
            for (int i = 0; i < serializers.Length; i++)
            {
                ISerializableReflector serializer = serializers[i];
                offset = serializer.Serialize(this, array, offset);
            }
            return offset;
        }
        public Int32 Deserialize(Byte[] array, Int32 offset, Int32 maxOffset)
        {
            for (int i = 0; i < serializers.Length; i++)
            {
                ISerializableReflector serializer = serializers[i];
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
            for (int i = 0; i < serializers.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                ISerializableReflector serializer = serializers[i];
                builder.Append(serializer.ToNiceString(this));
            }
            builder.Append("]");
        }
        public String ToNiceSmallString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(GetType().Name);
            builder.Append(":[");
            for (int i = 0; i < serializers.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                ISerializableReflector serializer = serializers[i];
                builder.Append(serializer.ToNiceSmallString(this));
            }
            builder.Append("]");
            return builder.ToString();
        }
    }
    public class ReflectorSerializerList : ISerializableReflector
    {
        private readonly ISerializableReflector[] fields;
        public readonly Int32 fixedSerializationLength;

        public ReflectorSerializerList(params ISerializableReflector[] fields)
        {
            this.fields = fields;

            this.fixedSerializationLength = 0;
            for (int i = 0; i < fields.Length; i++)
            {
                Int32 fieldFixedSerializationLength = fields[i].GetFixedSerializationLength();
                if (fieldFixedSerializationLength < 0)
                {
                    this.fixedSerializationLength = -1; // length is not fixed
                    return;
                }
                this.fixedSerializationLength += fieldFixedSerializationLength;
            }
        }
        public Int32 GetFixedSerializationLength()
        {
            return fixedSerializationLength;
        }
        public Int32 SerializationLength(ISerializableData instance)
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
        public Int32 Serialize(ISerializableData instance, Byte[] array, Int32 offset)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                offset = fields[i].Serialize(instance, array, offset);
            }
            return offset;
        }
        public Int32 Deserialize(ISerializableData instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                ISerializableReflector serializer = fields[i];
                offset = serializer.Deserialize(instance, array, offset, maxOffset);
            }
            return offset;
        }
        public String ToNiceString(ISerializableData instance)
        {
            StringBuilder builder = new StringBuilder();
            ToNiceString(instance, builder);
            return builder.ToString();
        }
        public void ToNiceString(ISerializableData instance, StringBuilder builder)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                ISerializableReflector serializer = fields[i];
                builder.Append(serializer.ToNiceString(instance));
            }
        }
        public String ToNiceSmallString(ISerializableData instance)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < fields.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                ISerializableReflector serializer = fields[i];
                builder.Append(serializer.ToNiceSmallString(instance));
            }
            return builder.ToString();
        }
    }
    public abstract class SingleFieldReflectorSerializer : ISerializableReflector
    {
        public readonly FieldInfo fieldInfo;
        public SingleFieldReflectorSerializer(Type typeThatContainsThisField, String fieldName)
        {
            this.fieldInfo = typeThatContainsThisField.GetField(fieldName);
            if (this.fieldInfo == null)
            {
                throw new InvalidOperationException(String.Format(
                    "The class you provided '{0}' either does not have the field name you provided '{1}' or the field is not public",
                    typeThatContainsThisField.Name, fieldName));
            }
        }
        public SingleFieldReflectorSerializer(Type typeThatContainsThisField, String fieldName, Type expectedFieldType)
        {
            this.fieldInfo = typeThatContainsThisField.GetField(fieldName);
            if (this.fieldInfo == null)
            {
                throw new InvalidOperationException(String.Format(
                    "The class you provided '{0}' either does not have the field name you provided '{1}' or the field is not public",
                    typeThatContainsThisField.Name, fieldName));
            }
            if (fieldInfo.FieldType != expectedFieldType)
            {
                throw new InvalidOperationException(String.Format(
                    "In the class '{0}', you used '{1}' class to wrap the field '{2}' but it's C# type is '{3}'.  It should be '{4}'",
                    typeThatContainsThisField, GetType().Name, fieldName, fieldInfo.FieldType.Name, expectedFieldType));
            }
        }
        public abstract Int32 GetFixedSerializationLength();
        public abstract Int32 SerializationLength(ISerializableData instance);
        public abstract Int32 Serialize(ISerializableData instance, Byte[] array, Int32 offset);
        public abstract Int32 Deserialize(ISerializableData instance, Byte[] array, Int32 offset, Int32 maxOffset);
        public abstract String ToNiceString(ISerializableData instance);
        public virtual void ToNiceString(ISerializableData instance, StringBuilder builder)
        {
            builder.Append(ToNiceString(instance));
        }
        public virtual String ToNiceSmallString(ISerializableData instance)
        {
            return ToNiceString(instance);
        }
    }
    public class SerializableDataFieldReflector : SingleFieldReflectorSerializer
    {
        public SerializableDataFieldReflector(Type typeThatContainsThisField, String fieldName)
            : base(typeThatContainsThisField, fieldName)
        {
            if (!typeof(ISerializableData).IsAssignableFrom(fieldInfo.FieldType))
            {
                throw new InvalidOperationException(String.Format(
                    "A generic reflector serializer can only be used on fields that implement the ISerializableData interface.  The field you are using '{0} {1}' does not implement this interface",
                    fieldInfo.FieldType.Name, fieldInfo.Name));
            }
        }
        public override Int32 GetFixedSerializationLength()
        {
            return -1; // There's not really a way to tell if this type has a fixed length
        }
        private ISerializableData GetValue(ISerializableData instance)
        {
            if (instance == null) throw new InvalidOperationException(String.Format("The Serializer Class '{0}' cannot be null", instance.GetType().Name));

            ISerializableData value = (ISerializableData)fieldInfo.GetValue(instance);

            if (value == null) throw new InvalidOperationException(String.Format("The value of field '{0} {1}' cannot be null for any serialization methods using this serializer",
                fieldInfo.FieldType.Name, fieldInfo.Name));

            return value;
        }
        public override Int32 SerializationLength(ISerializableData instance)
        {
            return GetValue(instance).SerializationLength();
        }
        public override Int32 Serialize(ISerializableData instance, Byte[] array, Int32 offset)
        {
            return GetValue(instance).Serialize(array, offset);
        }
        public override Int32 Deserialize(ISerializableData instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            return GetValue(instance).Deserialize(array, offset, maxOffset);
        }
        public override String ToNiceString(ISerializableData instance)
        {
            return GetValue(instance).ToNiceString();
        }
        public override String ToNiceSmallString(ISerializableData instance)
        {
            return GetValue(instance).ToNiceSmallString();
        }
    }

}