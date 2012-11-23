using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Marler.NetworkTools
{
    public static class Xdr
    {
        public static Int32 UpToNearestMod4(Int32 value)
        {
            Int32 mod4 = value & 0x3;
            if (mod4 == 0) return value;
            return value + 4 - mod4;
        }
    }
    public struct XdrBoolean : ISerializableData
    {
        Boolean value;
        public XdrBoolean(Boolean value)                  { this.value = value; }
        public Int32 GetFixedSerializationLength()        { return 4;           }
        public Int32 SerializationLength()                { return 4;           }
        public Int32 Serialize(Byte[] array, Int32 offset)
        {
            array[offset    ] = 0;
            array[offset + 1] = 0;
            array[offset + 2] = 0;
            array[offset + 3] = (value) ? (Byte)1 : (Byte)0;
            return offset + 4;
        }
        public Int32 Deserialize(Byte[] array, Int32 offset, Int32 maxOffset)
        {
            this.value = (array[offset + 3] == 0) ? false : true;
            return offset + 4;
        }
        public String ToNiceString()                    { return value.ToString();          }
        public void ToNiceString(StringBuilder builder) { builder.Append(value.ToString()); }
        public String ToNiceSmallString()               { return value.ToString();          }
    }
    public struct XdrInt32 : ISerializableData
    {
        Int32 value;
        public XdrInt32(Int32 value)                      { this.value = value; }
        public Int32 GetFixedSerializationLength()        { return 4;           }
        public Int32 SerializationLength()                { return 4;           }
        public int Serialize(byte[] array, int offset)
        {
            array[offset    ] = (Byte)(value >> 24);
            array[offset + 1] = (Byte)(value >> 16);
            array[offset + 2] = (Byte)(value >>  8);
            array[offset + 3] = (Byte)(value      );
            return offset + 4;
        }
        public int Deserialize(byte[] array, int offset, int maxOffset)
        {
            this.value = (Int32)(
                (Int32)(0xFF000000 & (array[offset    ] << 24)) |
                       (0x00FF0000 & (array[offset + 1] << 16)) |
                       (0x0000FF00 & (array[offset + 2] <<  8)) |
                       (0x000000FF & (array[offset + 3]      )) );
            return offset + 4;
        }
        public String ToNiceString()                    { return value.ToString();          }
        public void ToNiceString(StringBuilder builder) { builder.Append(value.ToString()); }
        public String ToNiceSmallString()               { return value.ToString();          }
    }
    public struct XdrUInt32 : ISerializableData
    {
        UInt32 value;
        public XdrUInt32(UInt32 value)                    { this.value = value; }
        public Int32 GetFixedSerializationLength()        { return 4;           }
        public Int32 SerializationLength()                { return 4;           }
        public int Serialize(byte[] array, int offset)
        {
            array[offset]     = (Byte)(value >> 24);
            array[offset + 1] = (Byte)(value >> 16);
            array[offset + 2] = (Byte)(value >>  8);
            array[offset + 3] = (Byte)(value      );
            return offset + 4;
        }
        public int Deserialize(byte[] array, int offset, int maxOffset)
        {
            this.value = (UInt32)(
                (Int32)(0xFF000000 & (array[offset    ] << 24)) |
                       (0x00FF0000 & (array[offset + 1] << 16)) |
                       (0x0000FF00 & (array[offset + 2] <<  8)) |
                       (0x000000FF & (array[offset + 3]      )) );
            return offset + 4;
        }
        public String ToNiceString()                    { return value.ToString();          }
        public void ToNiceString(StringBuilder builder) { builder.Append(value.ToString()); }
        public String ToNiceSmallString()               { return value.ToString();          }
    }
    public struct XdrEnum<EnumType> : ISerializableData
    {
        Enum value;
        public XdrEnum(Enum value)                        { this.value = value; }
        public Int32 GetFixedSerializationLength()        { return 4;           }
        public Int32 SerializationLength()                { return 4;           }
        public int Serialize(byte[] array, int offset)
        {
            Int32 valueAsInt32 = Convert.ToInt32(value);
            array[offset    ] = (Byte)(valueAsInt32 >> 24);
            array[offset + 1] = (Byte)(valueAsInt32 >> 16);
            array[offset + 2] = (Byte)(valueAsInt32 >> 8);
            array[offset + 3] = (Byte)(valueAsInt32);
            return offset + 4;
        }
        public int Deserialize(byte[] array, int offset, int maxOffset)
        {
            Int32 valueAsInt32 = (
                (Int32)(0xFF000000 & (array[offset] << 24)) |
                       (0x00FF0000 & (array[offset + 1] << 16)) |
                       (0x0000FF00 & (array[offset + 2] << 8)) |
                       (0x000000FF & (array[offset + 3])));
            this.value = (Enum)Enum.ToObject(typeof(EnumType), valueAsInt32);

            return offset + 4;
        }
        public String ToNiceString()                    { return value.ToString();          }
        public void ToNiceString(StringBuilder builder) { builder.Append(value.ToString()); }
        public String ToNiceSmallString()               { return value.ToString();          }
    }


    public class XdrBooleanReflector : SingleFieldReflectorSerializer
    {
        public XdrBooleanReflector(Type typeThatContainsThisField, String fieldName)
            : base(typeThatContainsThisField, fieldName, typeof(Boolean))
        {
        }
        public override Int32 GetFixedSerializationLength()
        {
            return 4;
        }
        public override Int32 SerializationLength(ISerializableData instance)
        {
            return 4;
        }
        public override Int32 Serialize(ISerializableData instance, Byte[] array, Int32 offset)
        {
            Boolean value = (Boolean)fieldInfo.GetValue(instance);
            array[offset]     = 0;
            array[offset + 1] = 0;
            array[offset + 2] = 0;
            array[offset + 3] = (value) ? (Byte)1 : (Byte)0;
            return offset + 4;

        }
        public override Int32 Deserialize(ISerializableData instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            fieldInfo.SetValue(instance, (array[offset + 3] == 0) ? false : true);
            return offset + 4;
        }
        public override String ToNiceString(ISerializableData instance)
        {
            return String.Format("{0}:{1}", fieldInfo.Name, (Boolean)fieldInfo.GetValue(instance));
        }
    }
    public class XdrInt32Reflector : SingleFieldReflectorSerializer
    {
        public XdrInt32Reflector(Type typeThatContainsThisField, String fieldName)
            : base(typeThatContainsThisField, fieldName, typeof(Int32))
        {
        }
        public override Int32 GetFixedSerializationLength()
        {
            return 4;
        }
        public override Int32 SerializationLength(ISerializableData instance)
        {
            return 4;
        }
        public override Int32 Serialize(ISerializableData instance, Byte[] array, Int32 offset)
        {
            Int32 value       = (Int32)fieldInfo.GetValue(instance);
            array[offset    ] = (Byte)(value >> 24);
            array[offset + 1] = (Byte)(value >> 16);
            array[offset + 2] = (Byte)(value >>  8);
            array[offset + 3] = (Byte)(value      );
            return offset + 4;

        }
        public override Int32 Deserialize(ISerializableData instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            Int32 value = (Int32)(
                (Int32)(0xFF000000 & (array[offset    ] << 24)) |
                       (0x00FF0000 & (array[offset + 1] << 16)) |
                       (0x0000FF00 & (array[offset + 2] <<  8)) |
                       (0x000000FF & (array[offset + 3]      )) );
            fieldInfo.SetValue(instance, value);
            return offset + 4;
        }
        public override String ToNiceString(ISerializableData instance)
        {
            return String.Format("{0}:{1}", fieldInfo.Name, (Int32)fieldInfo.GetValue(instance));
        }
    }
    public class XdrUInt32Reflector : SingleFieldReflectorSerializer
    {
        public XdrUInt32Reflector(Type typeThatContainsThisField, String fieldName)
            : base(typeThatContainsThisField, fieldName, typeof(UInt32))
        {
        }
        public override Int32 GetFixedSerializationLength()
        {
            return 4;
        }
        public override Int32 SerializationLength(ISerializableData instance)
        {
            return 4;
        }
        public override Int32 Serialize(ISerializableData instance, Byte[] array, Int32 offset)
        {
            UInt32 value = (UInt32)fieldInfo.GetValue(instance);
            array[offset] = (Byte)(value >> 24);
            array[offset + 1] = (Byte)(value >> 16);
            array[offset + 2] = (Byte)(value >> 8);
            array[offset + 3] = (Byte)(value);
            return offset + 4;

        }
        public override Int32 Deserialize(ISerializableData instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            UInt32 value = (UInt32)(
                (Int32)(0xFF000000 & (array[offset] << 24)) |
                       (0x00FF0000 & (array[offset + 1] << 16)) |
                       (0x0000FF00 & (array[offset + 2] << 8)) |
                       (0x000000FF & (array[offset + 3])));
            fieldInfo.SetValue(instance, value);
            return offset + 4;
        }
        public override String ToNiceString(ISerializableData instance)
        {
            return String.Format("{0}:{1}", fieldInfo.Name, (UInt32)fieldInfo.GetValue(instance));
        }
    }
    public class XdrEnumReflector : SingleFieldReflectorSerializer
    {
        //Type enumType;
        public XdrEnumReflector(Type typeThatContainsThisField, String fieldName, Type enumType)
            : base(typeThatContainsThisField, fieldName, enumType)
        {
            //this.enumType = enumType;
        }
        public override Int32 GetFixedSerializationLength()
        {
            return 4;
        }
        public override Int32 SerializationLength(ISerializableData instance)
        {
            return 4;
        }
        public override Int32 Serialize(ISerializableData instance, Byte[] array, Int32 offset)
        {
            Enum valueAsEnum  = (Enum)fieldInfo.GetValue(instance);
            Int32 value = Convert.ToInt32(valueAsEnum);
            array[offset    ] = (Byte)(value >> 24);
            array[offset + 1] = (Byte)(value >> 16);
            array[offset + 2] = (Byte)(value >>  8);
            array[offset + 3] = (Byte)(value      );
            return offset + 4;
        }
        public override Int32 Deserialize(ISerializableData instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            Int32 value = (Int32)(
                (Int32)(0xFF000000 & (array[offset    ] << 24)) |
                       (0x00FF0000 & (array[offset + 1] << 16)) |
                       (0x0000FF00 & (array[offset + 2] <<  8)) |
                       (0x000000FF & (array[offset + 3])));
            fieldInfo.SetValue(instance, value);
            return offset + 4;
        }
        public override String ToNiceString(ISerializableData instance)
        {
            return String.Format("{0}:{1}", fieldInfo.Name, fieldInfo.GetValue(instance));
        }
    }
    public class XdrUInt64Reflector : SingleFieldReflectorSerializer
    {
        public XdrUInt64Reflector(Type typeThatContainsThisField, String fieldName)
            : base(typeThatContainsThisField, fieldName, typeof(UInt64))
        {
        }
        public override Int32 GetFixedSerializationLength()
        {
            return 8;
        }
        public override Int32 SerializationLength(ISerializableData instance)
        {
            return 8;
        }
        public override Int32 Serialize(ISerializableData instance, Byte[] array, Int32 offset)
        {
            UInt64 value = (UInt64)fieldInfo.GetValue(instance);
            array[offset    ] = (Byte)(value >> 56);
            array[offset + 1] = (Byte)(value >> 48);
            array[offset + 2] = (Byte)(value >> 40);
            array[offset + 3] = (Byte)(value >> 32);
            array[offset + 4] = (Byte)(value >> 24);
            array[offset + 5] = (Byte)(value >> 16);
            array[offset + 6] = (Byte)(value >> 8);
            array[offset + 7] = (Byte)(value);
            return offset + 8;

        }
        public override Int32 Deserialize(ISerializableData instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            UInt64 value =
                (0xFF00000000000000UL & ((UInt64)array[offset]     << 56)) |
                (0x00FF000000000000UL & ((UInt64)array[offset + 1] << 48)) |
                (0x0000FF0000000000UL & ((UInt64)array[offset + 2] << 40)) |
                (0x000000FF00000000UL & ((UInt64)array[offset + 3] << 32)) |
                (0x00000000FF000000UL & ((UInt64)array[offset + 4] << 24)) |
                (0x0000000000FF0000UL & ((UInt64)array[offset + 5] << 16)) |
                (0x000000000000FF00UL & ((UInt64)array[offset + 6] <<  8)) |
                (0x00000000000000FFUL & ((UInt64)array[offset + 7]      )) ;
            fieldInfo.SetValue(instance, value);
            return offset + 8;
        }
        public override String ToNiceString(ISerializableData instance)
        {
            return String.Format("{0}:{1}", fieldInfo.Name, (UInt64)fieldInfo.GetValue(instance));
        }
    }

    public class XdrOpaqueFixedLengthReflector : SingleFieldReflectorSerializer
    {
        public readonly Int32 dataLength;
        public readonly Int32 dataLengthNearestContainingMod4;

        public XdrOpaqueFixedLengthReflector(Type typeThatContainsThisField, String fieldName, Int32 dataLength)
            : base(typeThatContainsThisField, fieldName, typeof(Byte[]))
        {
            this.dataLength = dataLength;
            this.dataLengthNearestContainingMod4 = Xdr.UpToNearestMod4(dataLength);
        }
        public override Int32 GetFixedSerializationLength()
        {
            return dataLengthNearestContainingMod4;
        }
        public override Int32 SerializationLength(ISerializableData instance)
        {
            return dataLengthNearestContainingMod4;
        }
        public override Int32 Serialize(ISerializableData instance, Byte[] array, Int32 offset)
        {
            if (dataLength <= 0) return offset;

            Object valueAsObject = fieldInfo.GetValue(instance);
            if (valueAsObject == null)
            {
                Array.Clear(array, offset, dataLength);
            }
            else
            {
                Byte[] valueAsArray = (Byte[])valueAsObject;

                if (valueAsArray.Length != dataLength)
                    throw new InvalidOperationException(String.Format("The XdrOpaqueFixedLength length is {0}, but your the byte array for field '{1}' length is {2}",
                        dataLength, fieldInfo.Name, valueAsArray.Length));

                Array.Copy(valueAsArray, 0, array, offset, dataLength);
            }

            // Add Padding
            for (int i = dataLength; i < dataLengthNearestContainingMod4; i++)
            {
                array[i] = 0;
            }

            return offset + dataLengthNearestContainingMod4;
        }
        public override Int32 Deserialize(ISerializableData instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            Byte[] data = new Byte[dataLength];
            Array.Copy(array, offset, data, 0, dataLength);

            fieldInfo.SetValue(instance, data);

            return offset + dataLengthNearestContainingMod4;
        }
        public override String ToNiceString(ISerializableData instance)
        {
            String dataString;

            Byte[] valueAsArray = (Byte[])fieldInfo.GetValue(instance);
            if (valueAsArray == null) return String.Format("[{0} bytes at value 0]", dataLength);
            dataString = BitConverter.ToString(valueAsArray);

            return String.Format("{0}:{1}", fieldInfo.Name, dataString);
        }
    }
    //
    // TODO: Check For Max Length
    //
    public class XdrOpaqueVarLengthReflector : SingleFieldReflectorSerializer
    {
        public readonly Int32 maxLength;

        public XdrOpaqueVarLengthReflector(Type typeThatContainsThisField, String fieldName, Int32 maxLength)
            : base(typeThatContainsThisField, fieldName, typeof(Byte[]))
        {
            this.maxLength = maxLength;
        }
        public override Int32 GetFixedSerializationLength()
        {
            return -1;
        }
        public override Int32 SerializationLength(ISerializableData instance)
        {
            Object valueAsObject = fieldInfo.GetValue(instance);
            if (valueAsObject == null) return 4;
            return 4 + Xdr.UpToNearestMod4(((Byte[])valueAsObject).Length);
        }
        public override Int32 Serialize(ISerializableData instance, Byte[] array, Int32 offset)
        {
            Object valueAsObject = fieldInfo.GetValue(instance);
            if (valueAsObject == null)
            {
                array[offset    ] = 0;
                array[offset + 1] = 0;
                array[offset + 2] = 0;
                array[offset + 3] = 0;
                return offset + 4;
            }

            Byte[] valueAsArray = (Byte[])valueAsObject;

            array[offset    ] = (Byte)(valueAsArray.Length >> 24);
            array[offset + 1] = (Byte)(valueAsArray.Length >> 16);
            array[offset + 2] = (Byte)(valueAsArray.Length >>  8);
            array[offset + 3] = (Byte)(valueAsArray.Length      );
            offset += 4;

            Array.Copy(valueAsArray, 0, array, offset, valueAsArray.Length);

            Int32 valueAsArrayMod4Length = Xdr.UpToNearestMod4(valueAsArray.Length);
            for (int i = valueAsArray.Length; i < valueAsArrayMod4Length; i++)
            {
                array[i] = 0;
            }

            return offset + valueAsArrayMod4Length;
        }
        public override Int32 Deserialize(ISerializableData instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            Int32 length = (Int32)(
                (Int32)(0xFF000000 & (array[offset] << 24)) |
                       (0x00FF0000 & (array[offset + 1] << 16)) |
                       (0x0000FF00 & (array[offset + 2] << 8)) |
                       (0x000000FF & (array[offset + 3])));
            offset += 4;

            if (length == 0)
            {
                fieldInfo.SetValue(instance, null);
                return offset;
            }

            Byte[] data = new Byte[length];
            Array.Copy(array, offset, data, 0, length);

            fieldInfo.SetValue(instance, data);

            Int32 lengthMod4 = Xdr.UpToNearestMod4(length);

            return offset + lengthMod4;
        }
        public override String ToNiceString(ISerializableData instance)
        {
            String dataString;

            Object valueAsObject = fieldInfo.GetValue(instance);
            if (valueAsObject == null)
            {
                dataString = "null";
            }
            else
            {
                Byte[] valueAsArray = (Byte[])valueAsObject;
                dataString = BitConverter.ToString(valueAsArray);
            }
            return String.Format("{0}:{1}", fieldInfo.Name, dataString);
        }
    }

    public class XdrOpaqueVarLengthReflector<SerializationType> : SingleFieldReflectorSerializer where SerializationType : ISerializableData, new()
    {
        public readonly Int32 maxLength;

        public XdrOpaqueVarLengthReflector(Type typeThatContainsThisField, String fieldName, Int32 maxLength)
            : base(typeThatContainsThisField, fieldName, typeof(SerializationType))
        {
            this.maxLength = maxLength;
        }
        public override Int32 GetFixedSerializationLength()
        {
            return -1;
        }
        public override Int32 SerializationLength(ISerializableData instance)
        {
            Object valueAsObject = fieldInfo.GetValue(instance);
            if (valueAsObject == null) return 4;

            SerializationType value = (SerializationType)valueAsObject;
            return 4 + Xdr.UpToNearestMod4(value.SerializationLength());
        }
        public override Int32 Serialize(ISerializableData instance, Byte[] array, Int32 offset)
        {
            Object valueAsObject = fieldInfo.GetValue(instance);
            if (valueAsObject == null)
            {
                array[offset    ] = 0;
                array[offset + 1] = 0;
                array[offset + 2] = 0;
                array[offset + 3] = 0;
                return offset + 4;
            }

            SerializationType value = (SerializationType)valueAsObject;

            Int32 offsetForSize = offset;
            offset = value.Serialize(array, offset + 4);

            Int32 valueSize = offset - offsetForSize - 4;
            array[offsetForSize    ] = (Byte)(valueSize >> 24);
            array[offsetForSize + 1] = (Byte)(valueSize >> 16);
            array[offsetForSize + 2] = (Byte)(valueSize >>  8);
            array[offsetForSize + 3] = (Byte)(valueSize      );

            Int32 valueAsArrayMod4Length = Xdr.UpToNearestMod4(valueSize);
            for (int i = valueSize; i < valueAsArrayMod4Length; i++)
            {
                array[offset++] = 0;
            }

            return offset;
        }
        public override Int32 Deserialize(ISerializableData instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            Int32 length = (Int32)(
                (Int32)(0xFF000000 & (array[offset] << 24)) |
                       (0x00FF0000 & (array[offset + 1] << 16)) |
                       (0x0000FF00 & (array[offset + 2] << 8)) |
                       (0x000000FF & (array[offset + 3])));
            offset += 4;

            if (length == 0)
            {
                fieldInfo.SetValue(instance, null);
                return offset;
            }

            SerializationType value = new SerializationType();
            offset = value.Deserialize(array, offset, offset + length);

            fieldInfo.SetValue(instance, value);

            Int32 lengthMod4 = Xdr.UpToNearestMod4(length);

            return offset + lengthMod4 - length;
        }
        public override String ToNiceString(ISerializableData instance)
        {
            String dataString;

            Object valueAsObject = fieldInfo.GetValue(instance);
            if (valueAsObject == null)
            {
                dataString = "<null>";
            }
            else
            {
                SerializationType value = (SerializationType)valueAsObject;
                dataString = value.ToNiceString();
            }
            return String.Format("{0}:{1}", fieldInfo.Name, dataString);
        }
        public override String ToNiceSmallString(ISerializableData instance)
        {
            String dataString;

            Object valueAsObject = fieldInfo.GetValue(instance);
            if (valueAsObject == null)
            {
                dataString = "<null>";
            }
            else
            {
                SerializationType value = (SerializationType)valueAsObject;
                dataString = value.ToNiceSmallString();
            }
            return String.Format("{0}:{1}", fieldInfo.Name, dataString);
        }
    }

    //
    // TODO: Check For Max Length
    //
    public class XdrStringReflector : SingleFieldReflectorSerializer
    {
        public readonly Int32 maxLength;

        public XdrStringReflector(Type typeThatContainsThisField, String fieldName, Int32 maxLength)
            : base(typeThatContainsThisField, fieldName, typeof(String))
        {
            this.maxLength = maxLength;
        }
        public override Int32 GetFixedSerializationLength()
        {
            return -1;
        }
        public override Int32 SerializationLength(ISerializableData instance)
        {
            Object valueAsObject = fieldInfo.GetValue(instance);
            if (valueAsObject == null) return 0;
            return 4 + Xdr.UpToNearestMod4(((String)valueAsObject).Length);
        }
        public override Int32 Serialize(ISerializableData instance, Byte[] array, Int32 offset)
        {
            Object valueAsObject = fieldInfo.GetValue(instance);
            if (valueAsObject == null)
            {
                array[offset    ] = 0;
                array[offset + 1] = 0;
                array[offset + 2] = 0;
                array[offset + 3] = 0;
                return offset + 4;
            }

            String valueAsString = (String)valueAsObject;

            array[offset    ] = (Byte)(valueAsString.Length >> 24);
            array[offset + 1] = (Byte)(valueAsString.Length >> 16);
            array[offset + 2] = (Byte)(valueAsString.Length >>  8);
            array[offset + 3] = (Byte)(valueAsString.Length      );
            offset += 4;

            for (int i = 0; i < valueAsString.Length; i++)
            {
                array[offset++] = (Byte)valueAsString[i];
            }

            Int32 valueAsArrayMod4Length = Xdr.UpToNearestMod4(valueAsString.Length);
            for (int i = valueAsString.Length; i < valueAsArrayMod4Length; i++)
            {
                array[offset++] = 0;
            }

            return offset;
        }
        public override Int32 Deserialize(ISerializableData instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            Int32 length = (Int32)(
                (Int32)(0xFF000000 & (array[offset] << 24)) |
                       (0x00FF0000 & (array[offset + 1] << 16)) |
                       (0x0000FF00 & (array[offset + 2] << 8)) |
                       (0x000000FF & (array[offset + 3])));
            offset += 4;

            if (length == 0)
            {
                fieldInfo.SetValue(instance, null);
                return offset;
            }

            String data = Encoding.UTF8.GetString(array, offset, length);
            fieldInfo.SetValue(instance, data);

            Int32 lengthMod4 = Xdr.UpToNearestMod4(length);

            return offset + lengthMod4;
        }
        public override String ToNiceString(ISerializableData instance)
        {
            String dataString;

            Object valueAsObject = fieldInfo.GetValue(instance);
            if (valueAsObject == null)
            {
                dataString = "null";
            }
            else
            {
                dataString = (String)valueAsObject;
            }
            return String.Format("{0}:\"{1}\"", fieldInfo.Name, dataString);
        }
    }

    public class XdrVarLengthArray<ElementType> : SingleFieldReflectorSerializer where ElementType : ISerializableData, new()
    {
        public readonly Int32 maxLength;

        public XdrVarLengthArray(Type typeThatContainsThisField, String fieldName, Int32 maxLength)
            : base(typeThatContainsThisField, fieldName, typeof(ElementType[]))
        {
            this.maxLength = maxLength;
        }
        public override Int32 GetFixedSerializationLength()
        {
            return -1;
        }
        public override Int32 SerializationLength(ISerializableData instance)
        {
            Object valueAsObject = fieldInfo.GetValue(instance);
            if (valueAsObject == null) return 4;

            ElementType[] valueAsArray = (ElementType[])valueAsObject;

            if (valueAsArray.Length <= 0) return 4;
            Int32 elementFixedSerializationLength = valueAsArray[0].GetFixedSerializationLength();
            if (elementFixedSerializationLength >= 0)
            {
                return 4 + (elementFixedSerializationLength * valueAsArray.Length);
            }

            int dataLength = 0;
            for (int i = 0; i < valueAsArray.Length; i++)
            {
                dataLength += valueAsArray[i].SerializationLength();
            }

            return 4 + dataLength;
        }
        public override Int32 Serialize(ISerializableData instance, Byte[] array, Int32 offset)
        {
            Object valueAsObject = fieldInfo.GetValue(instance);
            if (valueAsObject == null)
            {
                array[offset    ] = 0;
                array[offset + 1] = 0;
                array[offset + 2] = 0;
                array[offset + 3] = 0;
                return offset + 4;
            }

            ElementType[] valueAsArray = (ElementType[])valueAsObject;

            array[offset    ] = (Byte)(valueAsArray.Length >> 24);
            array[offset + 1] = (Byte)(valueAsArray.Length >> 16);
            array[offset + 2] = (Byte)(valueAsArray.Length >>  8);
            array[offset + 3] = (Byte)(valueAsArray.Length      );
            offset += 4;

            for (int i = 0; i < valueAsArray.Length; i++)
            {
                offset = valueAsArray[i].Serialize(array, offset);
            }

            return offset;
        }
        public override Int32 Deserialize(ISerializableData instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            Int32 length = (Int32)(
                (Int32)(0xFF000000 & (array[offset] << 24)) |
                       (0x00FF0000 & (array[offset + 1] << 16)) |
                       (0x0000FF00 & (array[offset + 2] << 8)) |
                       (0x000000FF & (array[offset + 3])));
            offset += 4;

            if (length == 0)
            {
                fieldInfo.SetValue(instance, null);
                return offset;
            }

            ElementType[] data = new ElementType[length];
            
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new ElementType();
                offset = data[i].Deserialize(array, offset, maxOffset);
            }

            fieldInfo.SetValue(instance, data);

            return offset;
        }
        public override string ToNiceString(ISerializableData instance)
        {
            StringBuilder builder = new StringBuilder();
            ToNiceString(instance, builder);
            return builder.ToString();
        }
        public override void ToNiceString(ISerializableData instance, StringBuilder builder)
        {
            builder.Append(fieldInfo.Name);
            builder.Append(':');

            Object valueAsObject = fieldInfo.GetValue(instance);
            if (valueAsObject == null)
            {
                builder.Append("null");
                return;
            }

            ElementType[] valueAsArray = (ElementType[])valueAsObject;

            builder.Append('[');
            for (int i = 0; i < valueAsArray.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                valueAsArray[i].ToNiceString(builder);
            }
            builder.Append(']');
        }

    }
    public class XdrStructFieldReflector<CSharpType> : SingleFieldReflectorSerializer where CSharpType : ISerializableData, new()
    {
        private Int32 fixedSerializationLength;
        private ISerializableReflector[] fieldSerializers;

        public XdrStructFieldReflector(Type typeThatContainsThisField, String fieldName, params ISerializableReflector[] fieldSerializers)
            : base(typeThatContainsThisField, fieldName)
        {
            this.fieldSerializers = fieldSerializers;

            this.fixedSerializationLength = 0;
            for (int i = 0; i < fieldSerializers.Length; i++)
            {
                Int32 fieldFixedSerializationLength = fieldSerializers[i].GetFixedSerializationLength();
                if (fieldFixedSerializationLength < 0)
                {
                    this.fixedSerializationLength = -1; // length is not fixed
                    return;
                }
                this.fixedSerializationLength += fieldFixedSerializationLength;
            }
        }
        public override Int32 GetFixedSerializationLength()
        {
            return fixedSerializationLength;
        }
        public override Int32 SerializationLength(ISerializableData instance)
        {
            if (fieldSerializers == null) return 0;
            if (fixedSerializationLength >= 0) return fixedSerializationLength;

            ISerializableData structInstance = (ISerializableData)fieldInfo.GetValue(instance);

            Int32 length = 0;
            for (int i = 0; i < fieldSerializers.Length; i++)
            {
                length += fieldSerializers[i].SerializationLength(structInstance);
            }
            return length;
        }
        public override Int32 Serialize(ISerializableData instance, Byte[] array, Int32 offset)
        {
            if (fieldSerializers == null) return offset;

            ISerializableData structInstance = (instance == null) ? null : (ISerializableData)fieldInfo.GetValue(instance);

            for (int i = 0; i < fieldSerializers.Length; i++)
            {
                ISerializableReflector serializer = fieldSerializers[i];
                offset = serializer.Serialize(structInstance, array, offset);
            }
            return offset;
        }
        public override Int32 Deserialize(ISerializableData instance, Byte[] array, Int32 offset, Int32 maxOffset)
        {
            if (fieldSerializers == null) return offset;

            CSharpType structObject = new CSharpType();

            for (int i = 0; i < fieldSerializers.Length; i++)
            {
                ISerializableReflector serializer = fieldSerializers[i];
                offset = serializer.Deserialize(structObject, array, offset, maxOffset);
            }

            fieldInfo.SetValue(instance, structObject);

            return offset;
        }
        public override String ToNiceString(ISerializableData instance)
        {
            StringBuilder builder = new StringBuilder();
            ToNiceString(instance, builder);
            return builder.ToString();
        }
        public override void ToNiceString(ISerializableData instance, StringBuilder builder)
        {
            builder.Append(fieldInfo.Name);
            builder.Append(':');

            ISerializableData structInstance = (ISerializableData)fieldInfo.GetValue(instance);

            if (structInstance == null)
            {
                builder.Append("<null>");
                return;
            }
            builder.Append("{");
        
            if (fieldSerializers != null)
            {
                for (int i = 0; i < fieldSerializers.Length; i++)
                {
                    if (i > 0) builder.Append(", ");

                    ISerializableReflector serializer = fieldSerializers[i];
                    builder.Append(serializer.ToNiceString(structInstance));
                }
            }
            builder.Append("}");
        }
        public override string ToNiceSmallString(ISerializableData instance)
        {
            ISerializableData structInstance = (ISerializableData)fieldInfo.GetValue(instance);

            if (structInstance == null) return String.Format("{0}:<null>", fieldInfo.Name);

            StringBuilder builder = new StringBuilder();
            builder.Append(fieldInfo.Name);
            builder.Append(':');

            builder.Append("{");

            if (fieldSerializers != null)
            {
                for (int i = 0; i < fieldSerializers.Length; i++)
                {
                    if (i > 0) builder.Append(", ");

                    ISerializableReflector serializer = fieldSerializers[i];
                    builder.Append(serializer.ToNiceSmallString(structInstance));
                }
            }
            builder.Append("}");
            return builder.ToString();
        }
    }
    
    public class XdrBooleanDescriminateReflector : ISerializableReflector
    {
        public readonly Int32 fixedSerializationLength;

        readonly XdrBooleanReflector descriminate;
        readonly ISerializableReflector trueSerializer;
        readonly ISerializableReflector falseSerializer;

        public XdrBooleanDescriminateReflector(XdrBooleanReflector descriminate,
            ISerializableReflector trueSerializer, ISerializableReflector falseSerializer)
        {
            this.descriminate = descriminate;
            this.trueSerializer = trueSerializer;
            this.falseSerializer = falseSerializer;            

            Int32 trueFixedSerializationLength = trueSerializer.GetFixedSerializationLength();
            if(trueFixedSerializationLength < 0)
            {
                this.fixedSerializationLength = -1;
                return;
            }
            Int32 falseFixedSerializationLength = falseSerializer.GetFixedSerializationLength();
            if(falseFixedSerializationLength == trueFixedSerializationLength)
            {
                this.fixedSerializationLength = trueFixedSerializationLength + 4;
            }
            else
            {
                this.fixedSerializationLength = -1;
                return;
            }            
        }
        private ISerializableReflector GetFieldSerializer(ISerializableData instance)
        {
            if (instance == null)
                throw new InvalidOperationException("Cannot retreive the descriminate value because the instance of this object is null");
            Boolean descriminateValue = (Boolean)descriminate.fieldInfo.GetValue(instance);
            return descriminateValue ? trueSerializer : falseSerializer;
        }
        public Int32 GetFixedSerializationLength()
        {
            return fixedSerializationLength;
        }
        public int SerializationLength(ISerializableData instance)
        {
            if (fixedSerializationLength >= 0) return fixedSerializationLength;

            ISerializableReflector fieldSerializer = GetFieldSerializer(instance);
            return 4 + fieldSerializer.SerializationLength(instance);
        }
        public int Serialize(ISerializableData instance, byte[] array, int offset)
        {
            offset = descriminate.Serialize(instance, array, offset);
            return GetFieldSerializer(instance).Serialize(instance, array, offset);
        }
        public int Deserialize(ISerializableData instance, byte[] array, int offset, Int32 maxOffset)
        {
            offset = descriminate.Deserialize(instance, array, offset, maxOffset);

            ISerializableReflector fieldSerializer = GetFieldSerializer(instance);
            offset = fieldSerializer.Deserialize(instance, array, offset, maxOffset);

            return offset;
        }
        public String ToNiceString(ISerializableData instance)
        {
            return String.Format("{0},{1}", descriminate.ToNiceString(instance),
                GetFieldSerializer(instance).ToNiceString(instance));
        }
        public void ToNiceString(ISerializableData instance, StringBuilder builder)
        {
            builder.Append(ToNiceString(instance));
        }
        public String ToNiceSmallString(ISerializableData instance)
        {
            return String.Format("{0},{1}", descriminate.ToNiceSmallString(instance),
                GetFieldSerializer(instance).ToNiceSmallString(instance));
        }
    }
    public class XdrDescriminatedUnionReflector<DescriminateCSharpType> : ISerializableReflector
    {
        public class KeyAndSerializer
        {
            public readonly DescriminateCSharpType descriminateKey;
            public readonly ISerializableReflector fieldSerializer;

            public KeyAndSerializer(DescriminateCSharpType descriminateKey,
                ISerializableReflector fieldSerializer)
            {
                this.descriminateKey = descriminateKey;
                this.fieldSerializer = fieldSerializer;
            }
        }

        //Int32 fixedSerializationLength;
        SingleFieldReflectorSerializer descriminate;
        Dictionary<DescriminateCSharpType, ISerializableReflector> unionDictionary;
        ISerializableReflector defaultSerializer;

        /*
        public XdrDescriminatedUnionReflector(FieldReflectorSerializer descriminate, params KeyAndSerializer[] fieldKeyAndSerializers)
            : this(descriminate, null, fieldKeyAndSerializers)
        {
        }
        */
        public XdrDescriminatedUnionReflector(SingleFieldReflectorSerializer descriminate, ISerializableReflector defaultSerializer,
            params KeyAndSerializer[] fieldKeyAndSerializers)
        {
            this.descriminate = descriminate;

            this.descriminate = descriminate;
            this.defaultSerializer = defaultSerializer;

            this.unionDictionary = new Dictionary<DescriminateCSharpType, ISerializableReflector>();
            for (int i = 0; i < fieldKeyAndSerializers.Length; i++)
            {
                KeyAndSerializer keyAndSerializer = fieldKeyAndSerializers[i];
                unionDictionary.Add(keyAndSerializer.descriminateKey,
                    keyAndSerializer.fieldSerializer);
            }
        }
        private ISerializableReflector GetFieldSerializer(ISerializableData instance)
        {
            if (instance == null)
            {
                throw new InvalidOperationException(String.Format("The descriminate value (fieldInfo.Name='{0}') is 'null'", descriminate.fieldInfo.Name));
            }

            DescriminateCSharpType descriminateValue = (DescriminateCSharpType)descriminate.fieldInfo.GetValue(instance);
            ISerializableReflector fieldSerializer;

            if (!unionDictionary.TryGetValue(descriminateValue, out fieldSerializer))
            {
                if (defaultSerializer == null)
                {
                    throw new InvalidOperationException(String.Format("The descriminate value '{0}' was not found in the union dictionary and there's no default descriminate", descriminateValue));
                }
                return defaultSerializer;
            }

            return fieldSerializer;
        }
        public Int32 GetFixedSerializationLength()
        {
            return -1; // TODO: Maybe I should check for fixed length later?
        }
        public int SerializationLength(ISerializableData instance)
        {
            int length = descriminate.SerializationLength(instance);

            ISerializableReflector fieldSerializer = GetFieldSerializer(instance);
            length += fieldSerializer.SerializationLength(instance);

            return length;
        }
        public int Serialize(ISerializableData instance, byte[] array, int offset)
        {
            offset = descriminate.Serialize(instance, array, offset);
            return GetFieldSerializer(instance).Serialize(instance, array, offset);
        }
        public int Deserialize(ISerializableData instance, byte[] array, int offset, Int32 maxOffset)
        {
            offset = descriminate.Deserialize(instance, array, offset, maxOffset);

            ISerializableReflector fieldSerializer = GetFieldSerializer(instance);
            offset = fieldSerializer.Deserialize(instance, array, offset, maxOffset);

            return offset;
        }
        public String ToNiceString(ISerializableData instance)
        {
            return String.Format("{0},{1}", descriminate.ToNiceString(instance),
                GetFieldSerializer(instance).ToNiceString(instance));
        }
        public void ToNiceString(ISerializableData instance, StringBuilder builder)
        {
            builder.Append(ToNiceString(instance));
        }
        public String ToNiceSmallString(ISerializableData instance)
        {
            return String.Format("{0},{1}", descriminate.ToNiceSmallString(instance),
                GetFieldSerializer(instance).ToNiceSmallString(instance));
        }
    }
}
