using System;
using System.Text;

using More;

namespace More.Net
{
    public class SimpleInt32Reflector : ClassFieldReflector
    {
        public SimpleInt32Reflector(Type typeThatContainsThisField, String fieldName)
            : base(typeThatContainsThisField, fieldName, typeof(Int32))
        {
        }
        public override Int32 FixedSerializationLength()
        {
            return 4;
        }
        public override Int32 SerializationLength(Object instance)
        {
            return 4;
        }
        public override Int32 Serialize(Object instance, Byte[] array, Int32 offset)
        {
            Int32 value = (Int32)fieldInfo.GetValue(instance);
            array[offset    ] = (Byte)(value >> 24);
            array[offset + 1] = (Byte)(value >> 16);
            array[offset + 2] = (Byte)(value >>  8);
            array[offset + 3] = (Byte)(value      );
            return offset + 4;
        }
        public override Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            Int32 value = (Int32)(
                (Int32)(0xFF000000 & (array[offset    ] << 24)) |
                       (0x00FF0000 & (array[offset + 1] << 16)) |
                       (0x0000FF00 & (array[offset + 2] <<  8)) |
                       (0x000000FF & (array[offset + 3]      )) );
            fieldInfo.SetValue(instance, value);
            return offset + 4;
        }
        public override String DataString(Object instance)
        {
            return String.Format("{0}:{1}", fieldInfo.Name, (Int32)fieldInfo.GetValue(instance));
        }
    }
    public class SimpleUInt16Reflector : ClassFieldReflector
    {
        public SimpleUInt16Reflector(Type typeThatContainsThisField, String fieldName)
            : base(typeThatContainsThisField, fieldName, typeof(UInt16))
        {
        }
        public override Int32 FixedSerializationLength()
        {
            return 2;
        }
        public override Int32 SerializationLength(Object instance)
        {
            return 2;
        }
        public override Int32 Serialize(Object instance, Byte[] array, Int32 offset)
        {
            UInt16 value = (UInt16)fieldInfo.GetValue(instance);
            array[offset    ] = (Byte)(value >>  8);
            array[offset + 1] = (Byte)(value      );
            return offset + 2;

        }
        public override Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            UInt16 value = (UInt16)(
                       (0xFF00 & (array[offset    ] << 8)) |
                       (0x00FF & (array[offset + 1]     )) );
            fieldInfo.SetValue(instance, value);
            return offset + 2;
        }
        public override String DataString(Object instance)
        {
            return String.Format("{0}:{1}", fieldInfo.Name, (UInt16)fieldInfo.GetValue(instance));
        }
    }
    public class SimpleUInt32Reflector : ClassFieldReflector
    {
        public SimpleUInt32Reflector(Type typeThatContainsThisField, String fieldName)
            : base(typeThatContainsThisField, fieldName, typeof(UInt32))
        {
        }
        public override Int32 FixedSerializationLength()
        {
            return 4;
        }
        public override Int32 SerializationLength(Object instance)
        {
            return 4;
        }
        public override Int32 Serialize(Object instance, Byte[] array, Int32 offset)
        {
            UInt32 value = (UInt32)fieldInfo.GetValue(instance);
            array[offset    ] = (Byte)(value >> 24);
            array[offset + 1] = (Byte)(value >> 16);
            array[offset + 2] = (Byte)(value >>  8);
            array[offset + 3] = (Byte)(value      );
            return offset + 4;

        }
        public override Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            UInt32 value = (UInt32)(
                (Int32)(0xFF000000 & (array[offset    ] << 24)) |
                       (0x00FF0000 & (array[offset + 1] << 16)) |
                       (0x0000FF00 & (array[offset + 2] <<  8)) |
                       (0x000000FF & (array[offset + 3]      )) );
            fieldInfo.SetValue(instance, value);
            return offset + 4;
        }
        public override String DataString(Object instance)
        {
            return String.Format("{0}:{1}", fieldInfo.Name, (UInt32)fieldInfo.GetValue(instance));
        }
    }
    public class SimpleEnumReflector : ClassFieldReflector
    {
        //Type enumType;
        public SimpleEnumReflector(Type typeThatContainsThisField, String fieldName, Type enumType)
            : base(typeThatContainsThisField, fieldName, enumType)
        {
            //this.enumType = enumType;
        }
        public override Int32 FixedSerializationLength()
        {
            return 4;
        }
        public override Int32 SerializationLength(Object instance)
        {
            return 4;
        }
        public override Int32 Serialize(Object instance, Byte[] array, Int32 offset)
        {
            Enum valueAsEnum = (Enum)fieldInfo.GetValue(instance);
            Int32 value = Convert.ToInt32(valueAsEnum);
            array[offset    ] = (Byte)(value >> 24);
            array[offset + 1] = (Byte)(value >> 16);
            array[offset + 2] = (Byte)(value >>  8);
            array[offset + 3] = (Byte)(value      );
            return offset + 4;
        }
        public override Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            Int32 value = (Int32)(
                (Int32)(0xFF000000 & (array[offset    ] << 24)) |
                       (0x00FF0000 & (array[offset + 1] << 16)) |
                       (0x0000FF00 & (array[offset + 2] <<  8)) |
                       (0x000000FF & (array[offset + 3]      )) );
            fieldInfo.SetValue(instance, value);
            return offset + 4;
        }
        public override String DataString(Object instance)
        {
            return String.Format("{0}:{1}", fieldInfo.Name, fieldInfo.GetValue(instance));
        }
    }
}