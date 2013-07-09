using System;
using System.Text;

namespace More
{
    public enum PdlType
    {
        // Primitive Types

        //   Unsigned
        Byte = 0,
        UInt16 = 1,
        UInt32 = 2,
        UInt64 = 3,

        //   Signed
        SByte = 4,
        Int16 = 5,
        Int32 = 6,
        Int64 = 7,

        // Enum/Flags Types
        Enum = 8,
        Flags = 9,

        // The Object Type
        Object = 10,
    }
    public enum PdlArraySizeType
    {
        NotAnArray = 0,
        BasedOnCommandSize = 1,
        Byte = 2,
        UInt16 = 3,
        UInt32 = 4,
        UInt64 = 5,
    }
    public static class PdlTypeExtensions
    {
        public static Boolean IsValidUnderlyingEnumIntegerType(this PdlType type)
        {
            return type <= PdlType.UInt64;
        }
        static Boolean IsIntegerType(this PdlType type)
        {
            return type <= PdlType.Int64;
        }
        public static PdlType ParseIntegerType(String typeString)
        {
            PdlType type;
            try { type = (PdlType)Enum.Parse(typeof(PdlType), typeString, true); }
            catch (ArgumentException) { throw new FormatException(String.Format("Unknown Pdl Type '{0}'", typeString)); }
            if (!type.IsIntegerType()) throw new InvalidOperationException(String.Format(
                 "Expected an integer type but got type '{0}'", type));
            return type;
        }
        public static String GetPdlArraySizeString(this PdlArraySizeType arraySizeType)
        {
            if (arraySizeType == PdlArraySizeType.NotAnArray)
            {
                return null;
            }
            if (arraySizeType == PdlArraySizeType.BasedOnCommandSize)
            {
                return "[]";
            }
            return "[" + arraySizeType + "]";
        }
    }


    //
    // Integer Reflector types
    //
    public class PdlByteReflector : ClassFieldReflector
    {
        public PdlByteReflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(Byte))
        {
        }
        public override Int32 FixedSerializationLength()           { return 1; }
        public override Int32 SerializationLength(Object instance) { return 1; }
        public override Int32 Serialize(Object instance, Byte[] array, Int32 offset)
        {
            array[offset++] = (Byte)fieldInfo.GetValue(instance);
            return offset;
        }
        public override Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            fieldInfo.SetValue(instance, array[offset]);
            return offset + 1;
        }
        public override String DataString(Object instance)
        {
            return ((Byte)fieldInfo.GetValue(instance)).ToString();
        }
    }
    public class PdlSByteReflector : ClassFieldReflector
    {
        public PdlSByteReflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(SByte))
        {
        }
        public override Int32 FixedSerializationLength()           { return 1; }
        public override Int32 SerializationLength(Object instance) { return 1; }
        public override Int32 Serialize(Object instance, Byte[] array, Int32 offset)
        {
            array[offset++] = (Byte)fieldInfo.GetValue(instance);
            return offset;
        }
        public override Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            fieldInfo.SetValue(instance, (SByte)array[offset]);
            return offset + 1;
        }
        public override String DataString(Object instance)
        {
            return ((SByte)fieldInfo.GetValue(instance)).ToString();
        }
    }
    public class PdlUInt16Reflector : ClassFieldReflector
    {
        public PdlUInt16Reflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(UInt16))
        {
        }
        public override Int32 FixedSerializationLength()           { return 2; }
        public override Int32 SerializationLength(Object instance) { return 2; }
        public override Int32 Serialize(Object instance, Byte[] array, Int32 offset)
        {
            UInt16 value = (UInt16)fieldInfo.GetValue(instance);
            array[offset++] = (Byte)(value >> 8);
            array[offset++] = (Byte)(value     );
            return offset;
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
            return ((UInt16)fieldInfo.GetValue(instance)).ToString();
        }
    }
    public class PdlInt16Reflector : ClassFieldReflector
    {
        public PdlInt16Reflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(Int16))
        {
        }
        public override Int32 FixedSerializationLength()           { return 2; }
        public override Int32 SerializationLength(Object instance) { return 2; }
        public override Int32 Serialize(Object instance, Byte[] array, Int32 offset)
        {
            Int16 value = (Int16)fieldInfo.GetValue(instance);
            array[offset++] = (Byte)(value >> 8);
            array[offset++] = (Byte)(value     );
            return offset;
        }
        public override Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            Int16 value = (Int16)(
                (0xFF00 & (array[offset    ] << 8)) |
                (0x00FF & (array[offset + 1]     )) );

            fieldInfo.SetValue(instance, value);
            return offset + 2;
        }
        public override String DataString(Object instance)
        {
            return ((Int16)fieldInfo.GetValue(instance)).ToString();
        }
    }
    public class PdlUInt32Reflector : ClassFieldReflector
    {
        public PdlUInt32Reflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(UInt32))
        {
        }
        public override Int32 FixedSerializationLength()           { return 4; }
        public override Int32 SerializationLength(Object instance) { return 4; }
        public override Int32 Serialize(Object instance, Byte[] array, Int32 offset)
        {
            UInt32 value = (UInt32)fieldInfo.GetValue(instance);
            array[offset++] = (Byte)(value >> 24);
            array[offset++] = (Byte)(value >> 16);
            array[offset++] = (Byte)(value >>  8);
            array[offset++] = (Byte)(value      );
            return offset;
        }
        public override Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            UInt32 value = (UInt32)(
                (0xFF000000U & (array[offset    ] << 24)) |
                (0x00FF0000U & (array[offset + 1] << 16)) |
                (0x0000FF00U & (array[offset + 2] <<  8)) |
                (0x000000FFU & (array[offset + 3]      )) );

            fieldInfo.SetValue(instance, value);
            return offset + 4;
        }
        public override String DataString(Object instance)
        {
            return ((UInt32)fieldInfo.GetValue(instance)).ToString();
        }
    }
    public class PdlInt32Reflector : ClassFieldReflector
    {
        public PdlInt32Reflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(Int32))
        {
        }
        public override Int32 FixedSerializationLength()           { return 4; }
        public override Int32 SerializationLength(Object instance) { return 4; }
        public override Int32 Serialize(Object instance, Byte[] array, Int32 offset)
        {
            Int32 value = (Int32)fieldInfo.GetValue(instance);
            array[offset++] = (Byte)(value >> 24);
            array[offset++] = (Byte)(value >> 16);
            array[offset++] = (Byte)(value >>  8);
            array[offset++] = (Byte)(value      );
            return offset;
        }
        public override Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            Int32 value = (Int32)(
                (0xFF000000U & (array[offset    ] << 24)) |
                (0x00FF0000U & (array[offset + 1] << 16)) |
                (0x0000FF00U & (array[offset + 2] <<  8)) |
                (0x000000FFU & (array[offset + 3]      )) );

            fieldInfo.SetValue(instance, value);
            return offset + 4;
        }
        public override String DataString(Object instance)
        {
            return ((Int32)fieldInfo.GetValue(instance)).ToString();
        }
    }
    public class PdlUInt64Reflector : ClassFieldReflector
    {
        public PdlUInt64Reflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(UInt64))
        {
        }
        public override Int32 FixedSerializationLength()           { return 8; }
        public override Int32 SerializationLength(Object instance) { return 8; }
        public override Int32 Serialize(Object instance, Byte[] array, Int32 offset)
        {
            UInt64 value = (UInt64)fieldInfo.GetValue(instance);
            array[offset++] = (Byte)(value >> 56);
            array[offset++] = (Byte)(value >> 48);
            array[offset++] = (Byte)(value >> 40);
            array[offset++] = (Byte)(value >> 32);
            array[offset++] = (Byte)(value >> 24);
            array[offset++] = (Byte)(value >> 16);
            array[offset++] = (Byte)(value >>  8);
            array[offset++] = (Byte)(value      );
            return offset;
        }
        public override Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            UInt64 value = (UInt64)(
                (0xFF00000000000000UL & (UInt64)(array[offset    ] << 56)) |
                (0x00FF000000000000UL & (UInt64)(array[offset + 1] << 48)) |
                (0x0000FF0000000000UL & (UInt64)(array[offset + 2] << 40)) |
                (0x000000FF00000000UL & (UInt64)(array[offset + 3] << 32)) |
                (0x00000000FF000000UL & (UInt64)(array[offset + 4] << 24)) |
                (0x0000000000FF0000UL & (UInt64)(array[offset + 5] << 16)) |
                (0x000000000000FF00UL & (UInt64)(array[offset + 6] <<  8)) |
                (0x00000000000000FFUL & (UInt64)(array[offset + 7]      )) );

            fieldInfo.SetValue(instance, value);
            return offset + 8;
        }
        public override String DataString(Object instance)
        {
            return ((UInt64)fieldInfo.GetValue(instance)).ToString();
        }
    }
    public class PdlInt64Reflector : ClassFieldReflector
    {
        public PdlInt64Reflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(Int64))
        {
        }
        public override Int32 FixedSerializationLength()           { return 8; }
        public override Int32 SerializationLength(Object instance) { return 8; }
        public override Int32 Serialize(Object instance, Byte[] array, Int32 offset)
        {
            Int64 value = (Int64)fieldInfo.GetValue(instance);
            array[offset++] = (Byte)(value >> 56);
            array[offset++] = (Byte)(value >> 48);
            array[offset++] = (Byte)(value >> 40);
            array[offset++] = (Byte)(value >> 32);
            array[offset++] = (Byte)(value >> 24);
            array[offset++] = (Byte)(value >> 16);
            array[offset++] = (Byte)(value >>  8);
            array[offset++] = (Byte)(value      );
            return offset;
        }
        public override Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            Int64 value = (Int64)(
                (0xFF00000000000000UL & (UInt64)(array[offset    ] << 56)) |
                (0x00FF000000000000UL & (UInt64)(array[offset + 1] << 48)) |
                (0x0000FF0000000000UL & (UInt64)(array[offset + 2] << 40)) |
                (0x000000FF00000000UL & (UInt64)(array[offset + 3] << 32)) |
                (0x00000000FF000000UL & (UInt64)(array[offset + 4] << 24)) |
                (0x0000000000FF0000UL & (UInt64)(array[offset + 5] << 16)) |
                (0x000000000000FF00UL & (UInt64)(array[offset + 6] <<  8)) |
                (0x00000000000000FFUL & (UInt64)(array[offset + 7]      )) );

            fieldInfo.SetValue(instance, value);
            return offset + 8;
        }
        public override String DataString(Object instance)
        {
            return ((Int64)fieldInfo.GetValue(instance)).ToString();
        }
    }

    //
    // Enum Reflector types
    //
    public class PdlByteEnumReflector<EnumType> : ClassFieldReflector
    {
        public PdlByteEnumReflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(EnumType))
        {
        }
        public override Int32 FixedSerializationLength()           { return 1; }
        public override Int32 SerializationLength(Object instance) { return 1; }
        public override Int32 Serialize(Object instance, Byte[] array, Int32 offset)
        {
            Byte value = Convert.ToByte((Enum)fieldInfo.GetValue(instance));
            array[offset++] = (Byte)(value      );
            return offset;
        }
        public override Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            Byte valueAsByte = array[offset];
            Enum value = (Enum)Enum.ToObject(typeof(EnumType), valueAsByte);
            fieldInfo.SetValue(instance, value);
            return offset + 1;
        }
        public override String DataString(Object instance)
        {
            return ((EnumType)fieldInfo.GetValue(instance)).ToString();
        }
    }


    //
    // Integer Array Reflector Types
    //
    public class PdlByteLengthByteArrayReflector : ClassFieldReflector
    {
        public PdlByteLengthByteArrayReflector(Type classThatHasThisField, String fieldName)
            : base(classThatHasThisField, fieldName, typeof(Byte[]))
        {
        }
        public override Int32 FixedSerializationLength()
        {
            return -1;
        }
        public override Int32 SerializationLength(Object instance)
        {
            Object obj = fieldInfo.GetValue(instance);
            if (obj == null) return 1;
            return ((Byte[])obj).Length + 1;
        }
        public override Int32 Serialize(Object instance, Byte[] array, Int32 offset)
        {
            Object obj = fieldInfo.GetValue(instance);
            if (obj == null)
            {
                array[offset] = 0;
                return offset + 1;
            }

            Byte[] objBytes = (Byte[])obj;
            if(objBytes.Length > 255) throw new InvalidOperationException(String.Format("A byte length array has a max size of 255 but your array is {0}", objBytes.Length));

            array[offset] = (Byte)(objBytes.Length);
            Array.Copy(objBytes, 0, array, offset + 1, objBytes.Length);
            return offset + objBytes.Length + 1;
        }

        Byte GetLength(Object instance)
        {
            Object obj = fieldInfo.GetValue(instance);
            if (obj == null) return 0;
            return (Byte)(((Byte[])obj).Length);
        }

        public override Int32 Deserialize(Object instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            Byte length = array[offset];
            if(length == 0)
            {
                fieldInfo.SetValue(instance, null);
                return offset + 1;
            }

            Byte[] value = new Byte[length];
            Array.Copy(array, offset + 1, value, 0, length);
            fieldInfo.SetValue(instance, value);

            return offset + length + 1;
        }
        public override string DataString(object instance)
        {
            StringBuilder builder = new StringBuilder();
            DataString(instance, builder);
            return builder.ToString();
        }
        public override void DataString(object instance, StringBuilder builder)
        {
            Object obj = fieldInfo.GetValue(instance);
            if (obj == null) builder.Append("[]");
            Byte[] objBytes = (Byte[])obj;
            builder.Append('[');
            for(int i = 0; i < objBytes.Length; i++)
            {
                if(i > 0) builder.Append(',');
                builder.Append(objBytes[i].ToString());
            }
            builder.Append(']');
        }
        public override string  DataSmallString(object instance)
        {
            return String.Format("[{0} bytes]", GetLength(instance));
        }
    }
}
