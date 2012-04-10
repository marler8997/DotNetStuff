using System;

namespace Marler.Hmd
{
    public enum HmdType
    {
        String,
        Boolean,
         Int,  Int1,  Int2,  Int3,  Int4,  Int5,  Int6,  Int7,  Int8,  Int9,  Int10,  Int11,  Int12,  Int13,  Int14,  Int15,  Int16,
        UInt, UInt1, UInt2, UInt3, UInt4, UInt5, UInt6, UInt7, UInt8, UInt9, UInt10, UInt11, UInt12, UInt13, UInt14, UInt15, UInt16,
        Decimal,
        Enumeration,
        Empty,
        EnumTypeLength
    };

    public static class HmdTypeClass
    {
        public const String String = "string";
        public const String Boolean = "bool";
        public const String Integer = "int";
        public const String Enumeration = "enum";
        public const String Empty = "empty";

        public static readonly String[] typeStrings = new String[(Int32)HmdType.EnumTypeLength];

        static HmdTypeClass(){
            typeStrings[(Int32)HmdType.String]      = String;
            typeStrings[(Int32)HmdType.Boolean]     = Boolean;
            
            typeStrings[(Int32)HmdType.Int]         = "int";
            typeStrings[(Int32)HmdType.Int1]        = "int1";
            typeStrings[(Int32)HmdType.Int2]        = "int2";
            typeStrings[(Int32)HmdType.Int3]        = "int3";
            typeStrings[(Int32)HmdType.Int4]        = "int4";
            typeStrings[(Int32)HmdType.Int5]        = "int5";
            typeStrings[(Int32)HmdType.Int6]        = "int6";
            typeStrings[(Int32)HmdType.Int7]        = "int7";
            typeStrings[(Int32)HmdType.Int8]        = "int8";
            typeStrings[(Int32)HmdType.Int9]        = "int9";
            typeStrings[(Int32)HmdType.Int10]       = "int10";
            typeStrings[(Int32)HmdType.Int11]       = "int11";
            typeStrings[(Int32)HmdType.Int12]       = "int12";
            typeStrings[(Int32)HmdType.Int13]       = "int13";
            typeStrings[(Int32)HmdType.Int14]       = "int14";
            typeStrings[(Int32)HmdType.Int15]       = "int15";
            typeStrings[(Int32)HmdType.Int16]       = "int16";
            typeStrings[(Int32)HmdType.UInt]        = "uint";
            typeStrings[(Int32)HmdType.UInt1]       = "uint1";
            typeStrings[(Int32)HmdType.UInt2]       = "uint2";
            typeStrings[(Int32)HmdType.UInt3]       = "uint3";
            typeStrings[(Int32)HmdType.UInt4]       = "uint4";
            typeStrings[(Int32)HmdType.UInt5]       = "uint5";
            typeStrings[(Int32)HmdType.UInt6]       = "uint6";
            typeStrings[(Int32)HmdType.UInt7]       = "uint7";
            typeStrings[(Int32)HmdType.UInt8]       = "uint8";
            typeStrings[(Int32)HmdType.UInt9]       = "uint9";
            typeStrings[(Int32)HmdType.UInt10]      = "uint10";
            typeStrings[(Int32)HmdType.UInt11]      = "uint11";
            typeStrings[(Int32)HmdType.UInt12]      = "uint12";
            typeStrings[(Int32)HmdType.UInt13]      = "uint13";
            typeStrings[(Int32)HmdType.UInt14]      = "uint14";
            typeStrings[(Int32)HmdType.UInt15]      = "uint15";
            typeStrings[(Int32)HmdType.UInt16]      = "uint16";

            typeStrings[(Int32)HmdType.Decimal]     = "decimal";
            typeStrings[(Int32)HmdType.Enumeration] = Enumeration;
        }

        public static HmdType GetIntegerType(Boolean isUnsigned, Byte byteSize)
        {
            switch (byteSize)
            {
                case 0:
                    return isUnsigned ? HmdType.UInt : HmdType.Int;
                case 1:
                    return isUnsigned ? HmdType.UInt1 : HmdType.Int1;
                case 2:
                    return isUnsigned ? HmdType.UInt2 : HmdType.Int2;
                case 3:
                    return isUnsigned ? HmdType.UInt3 : HmdType.Int3;
                case 4:
                    return isUnsigned ? HmdType.UInt4 : HmdType.Int4;
                case 5:
                    return isUnsigned ? HmdType.UInt5 : HmdType.Int5;
                case 6:
                    return isUnsigned ? HmdType.UInt6 : HmdType.Int6;
                case 7:
                    return isUnsigned ? HmdType.UInt7 : HmdType.Int7;
                case 8:
                    return isUnsigned ? HmdType.UInt8 : HmdType.Int8;
                case 9:
                    return isUnsigned ? HmdType.UInt9 : HmdType.Int9;
                case 10:
                    return isUnsigned ? HmdType.UInt10 : HmdType.Int10;
                case 11:
                    return isUnsigned ? HmdType.UInt11 : HmdType.Int11;
                case 12:
                    return isUnsigned ? HmdType.UInt12 : HmdType.Int12;
                case 13:
                    return isUnsigned ? HmdType.UInt13 : HmdType.Int13;
                case 14:
                    return isUnsigned ? HmdType.UInt14 : HmdType.Int14;
                case 15:
                    return isUnsigned ? HmdType.UInt15 : HmdType.Int15;
                case 16:
                    return isUnsigned ? HmdType.UInt16 : HmdType.Int16;
                default:
                    throw new ArgumentException(String.Format("expected [1,16], actual {0}", byteSize), "byteSize");
            }
        }

        public static String ToHmdTypeString(this HmdType hmdType)
        {
            Int32 index = (Int32)hmdType;
            if (index >= 0 && index < typeStrings.Length)
            {
                if (typeStrings[index] == null)
                {
                    throw new InvalidOperationException(
                        String.Format("Error, the typeStrings entry for hmdType \"{0}\" ({1}) was null?", hmdType.ToString(), index));
                }
                return typeStrings[index];
            }

            throw new InvalidOperationException(
                String.Format("Error, unrecognized hmdType \"{0}\" ({1})", hmdType.ToString(), index));
        }

        // =========================================
        // Types
        // =========================================
        // 1. string (default)
        // 2. bool
        // 3. int, uint, intX, uintX (where X is the number of bytes) (int and uint are the default size)
        //    regex: (u?int)([1-9]+[0-9]*)?
        // 4. float, double, decimal?
        // 5. enum <enum-name>
        //    enum (name value1 value2 ...)
        public static HmdType ParseHmdType(String hmdType, out String enumReferenceTypeName)
        {
            if (hmdType.Equals("string", StringComparison.InvariantCulture))
            {
                enumReferenceTypeName = null;
                return HmdType.String;
            }
            else if (hmdType.Equals("bool", StringComparison.InvariantCulture))
            {
                enumReferenceTypeName = null;
                return HmdType.Boolean;
            }
            else if (hmdType[0] == 'u' || hmdType[0] == 'i')
            {
                Int32 stringOffset = (hmdType[0] == 'u') ? 1 : 0;

                if (hmdType[stringOffset++] != 'i' || hmdType[stringOffset++] != 'n' || hmdType[stringOffset++] != 't')
                {
                    throw new FormatException(String.Format("Unrecognized type \"{0}\"", hmdType));
                }
                if (hmdType.Length > stringOffset++)
                {
                    Byte byteSize = Byte.Parse(hmdType.Substring(4));

                    enumReferenceTypeName = null;
                    return HmdTypeClass.GetIntegerType(hmdType[0] == 'u', byteSize);
                }
                else
                {
                    enumReferenceTypeName = null;
                    return HmdTypeClass.GetIntegerType(hmdType[0] == 'u', 0);
                }
            }
            else if (hmdType[0] == 'e')
            {
                if (hmdType.Length < 6)
                {
                    throw new FormatException(String.Format("Unrecognized type \"{0}\"", hmdType));
                }

                if (hmdType[1] != 'n' || hmdType[2] != 'u' || hmdType[3] != 'm' || hmdType[4] != ' ')
                {
                    throw new FormatException(String.Format("Unrecognized type \"{0}\"", hmdType));
                }

                enumReferenceTypeName = hmdType.Substring(5);
                return HmdType.Enumeration;
            }
            else
            {
                throw new FormatException(String.Format("Unrecognized type \"{0}\"", hmdType));
            }
        }
    }
}
