using System;
using System.IO;

namespace More.Pdl
{
    public enum PdlType
    {
        // Primitive Types

        //   Unsigned
        Byte       = 0,
        UInt16     = 1,
        UInt24     = 2,
        UInt32     = 3,
        UInt64     = 4,

        //   Signed
        SByte      = 5,
        Int16      = 6,
        Int24      = 7,
        Int32      = 8,
        Int64      = 9,

        // Enum/Flags Types
        Enum       = 10,
        Flags      = 11,

        // The Object Type
        Object     = 12,

        // The Serializer Type
        Serializer = 13,
    }
    public enum PdlArraySizeTypeEnum
    {
        Fixed              = 0, // Array Size Length is 0
        Byte               = 1, // Array Size Length is 1
        UInt16             = 2, // Array Size Length is 2
        UInt24             = 3, // Array Size Length is 3
        UInt32             = 4, // Array Size Length is 4
        UInt64             = 5, // Array Size Length is 5
        BasedOnCommand     = 6, // Array Size Length is 0
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
        public static Byte IntegerTypeByteCount(this PdlType integerType)
        {
            switch(integerType)
            {
                case PdlType.Byte  : return 1;
                case PdlType.UInt16: return 2;
                case PdlType.UInt24: return 3;
                case PdlType.UInt32: return 4;
                case PdlType.UInt64: return 8;
                case PdlType.SByte : return 1;
                case PdlType.Int16 : return 2;
                case PdlType.Int24 : return 3;
                case PdlType.Int32 : return 4;
                case PdlType.Int64 : return 8;
                default: throw new InvalidOperationException(String.Format("Type '{0}' is not an integer type", integerType));
            }
        }
        public static Byte LengthByteCount(this PdlArraySizeTypeEnum sizeType)
        {
            if (sizeType == PdlArraySizeTypeEnum.BasedOnCommand) return 0;
            return (Byte)sizeType;
        }
        public static Boolean IntegerTypeIsUnsigned(this PdlType type)
        {
            if (type <= PdlType.UInt64) return true;
            if (type <= PdlType.Int64 ) return true;
            throw new InvalidOperationException(String.Format("Cannot call this method on a non-integer type '{0}'", type));
        }
    }
    public abstract class PdlArrayType
    {
        static readonly PdlVariableLengthArrayType BasedOnCommandLength = new PdlVariableLengthArrayType(PdlArraySizeTypeEnum.BasedOnCommand);

        static readonly PdlVariableLengthArrayType ByteLength = new PdlVariableLengthArrayType(PdlArraySizeTypeEnum.Byte);
        static readonly PdlVariableLengthArrayType UInt16Length = new PdlVariableLengthArrayType(PdlArraySizeTypeEnum.UInt16);
        static readonly PdlVariableLengthArrayType UInt24Length = new PdlVariableLengthArrayType(PdlArraySizeTypeEnum.UInt24);
        static readonly PdlVariableLengthArrayType UInt32Length = new PdlVariableLengthArrayType(PdlArraySizeTypeEnum.UInt32);
        static readonly PdlVariableLengthArrayType UInt64Length = new PdlVariableLengthArrayType(PdlArraySizeTypeEnum.UInt64);

        public static PdlArrayType Parse(LfdLine line, String sizeTypeString)
        {
            if(String.IsNullOrEmpty(sizeTypeString))
            {
                return BasedOnCommandLength;
            }

            Char firstChar = sizeTypeString[0];

            if(firstChar >= '0' && firstChar <= '9')
            {
                UInt32 fixedLength;
                if(!UInt32.TryParse(sizeTypeString, out fixedLength))
                    throw new ParseException(line, "First character of array size type '{0}' was a number but coult not parse it as a number", sizeTypeString);
                
                return new PdlFixedLengthArrayType(fixedLength);
            }
            
            PdlArraySizeTypeEnum typeEnum;
            try
            {
                typeEnum = (PdlArraySizeTypeEnum)Enum.Parse(typeof(PdlArraySizeTypeEnum), sizeTypeString);
            }
            catch (ArgumentException)
            {
                throw new ParseException(line,
                    "The array size type inside the brackets '{0}' is invalid, expected 'Byte','UInt16','UInt24' or 'UInt32'", sizeTypeString);
            }

            if(typeEnum == PdlArraySizeTypeEnum.Fixed)
                throw new InvalidOperationException("Fixed is an invalid array size type, use an unsigned integer to specify a fixed length array");

            switch (typeEnum)
            {
                case PdlArraySizeTypeEnum.BasedOnCommand: return BasedOnCommandLength;
                case PdlArraySizeTypeEnum.Byte          : return ByteLength;
                case PdlArraySizeTypeEnum.UInt16        : return UInt16Length;
                case PdlArraySizeTypeEnum.UInt24        : return UInt24Length;
                case PdlArraySizeTypeEnum.UInt32        : return UInt32Length;
                case PdlArraySizeTypeEnum.UInt64        : return UInt64Length;
            }

            throw new InvalidOperationException(String.Format("CodeBug: Unhandled PdlArraySizeTypeEnum {0}", typeEnum));
        }

        public readonly PdlArraySizeTypeEnum type;
        protected PdlArrayType(PdlArraySizeTypeEnum type)
        {
            this.type = type;
        }
        public abstract String GetPdlArraySizeString();
        public Byte GetArraySizeByteCount()
        {
            if (type == PdlArraySizeTypeEnum.BasedOnCommand) return 0;
            return (Byte)type;
        }
        public abstract UInt32 GetFixedArraySize();
        public abstract String LengthSerializerReference();

        class PdlVariableLengthArrayType : PdlArrayType
        {
            readonly Byte lengthByteCount;
            public PdlVariableLengthArrayType(PdlArraySizeTypeEnum type)
                : base(type)
            {
                if (type == PdlArraySizeTypeEnum.Fixed) throw new InvalidOperationException("This class does not accept Fixed array types");
                this.lengthByteCount = type.LengthByteCount();
            }
            public override String GetPdlArraySizeString()
            {
                if (type == PdlArraySizeTypeEnum.BasedOnCommand) return "[]";
                return "[" + type.ToString() + "]";
            }
            public override UInt32 GetFixedArraySize()
            {
                throw new InvalidOperationException("CodeBug: This method cannot be called on an array of variable length");
            }
            public override String LengthSerializerReference()
            {
                if(type == PdlArraySizeTypeEnum.BasedOnCommand)
                    throw new InvalidOperationException(String.Format("Cannot call this method on PdlArrayType '{0}'", type));
                return String.Format("{0}{1}Serializer", (lengthByteCount == 1) ? "" : "BigEndian", type);
            }
        }
        public class PdlFixedLengthArrayType : PdlArrayType
        {
            public readonly UInt32 fixedLength;
            public PdlFixedLengthArrayType(UInt32 fixedLength)
                : base(PdlArraySizeTypeEnum.Fixed)
            {
                this.fixedLength = fixedLength;
            }
            public override String GetPdlArraySizeString()
            {
                return fixedLength.ToString();
            }
            public override UInt32 GetFixedArraySize()
            {
                return fixedLength;
            }
            public override String LengthSerializerReference()
            {
                throw new InvalidOperationException(String.Format("Cannot call this method on PdlArrayType '{0}'", type));
            }
        }
    }

    public abstract class TypeReference
    {
        public readonly String relativePdlTypeReferenceString;
        public readonly PdlType type;
        public readonly PdlArrayType arrayType;

        public TypeReference(String relativePdlTypeReferenceString, PdlType type, PdlArrayType arrayType)
        {
            this.relativePdlTypeReferenceString = relativePdlTypeReferenceString;
            this.type = type;
            this.arrayType = arrayType;
        }

        // return UInt32.Max if element serialization length is not fixed, otherwise, return fixed serialization length
        public abstract UInt32 FixedElementSerializationLength { get; }

        public abstract String ElementDynamicSerializationLengthExpression(String instanceString);
        public abstract String ElementSerializeExpression(String arrayString, String offsetString, String instanceString);
        public abstract String ElementDeserializeExpression(String arrayString, String offsetString);
        public abstract String ElementDeserializeArrayExpression(String arrayString, String offsetString, String countString);
        public abstract String ElementDataStringExpression(String builderString, String instanceString, Boolean small);

        //public abstract String SerializeString(String valueString);

        public abstract IntegerTypeReference AsIntegerTypeReference { get; }
        public abstract EnumOrFlagsTypeReference AsEnumOrFlagsTypeReference { get; }
        public abstract ObjectTypeReference AsObjectTypeReference { get; }
        public abstract SerializerTypeReference AsSerializerTypeReference { get; }

        public virtual void WritePdl(TextWriter writer, String fieldName)
        {
            writer.Write(relativePdlTypeReferenceString);

            if (arrayType != null) writer.WriteLine(arrayType.GetPdlArraySizeString());

            writer.Write(' ');
            writer.WriteLine(fieldName);
        }

        public abstract String CodeBaseTypeString { get; }
        public String CodeTypeString()
        {
            if (type == PdlType.Serializer) return "ISerializer";
            if (arrayType == null) return CodeBaseTypeString;
            return CodeBaseTypeString + "[]";
        }
    }
    public class IntegerTypeReference : TypeReference
    {
        public readonly Byte byteCount;

        public IntegerTypeReference(PdlType integerType, PdlArrayType arrayType)
            : base(integerType.ToString(), integerType, arrayType)
        {
            this.byteCount = integerType.IntegerTypeByteCount();
        }
        public override UInt32 FixedElementSerializationLength
        {
            get { return byteCount; }
        }
        public override String ElementDynamicSerializationLengthExpression(String instanceString)
        {
            throw new InvalidOperationException("CodeBug: this method should not be called on an integer type reference");
        }
        public override String ElementSerializeExpression(String arrayString, String offsetString, String instanceString)
        {
            if (byteCount == 1)
            {
                return String.Format("{0}[{1}] = {2}{3}", arrayString, offsetString, (type == PdlType.SByte) ? "(Byte)" : "", instanceString);
            }
            else
            {
                return String.Format("{0}.BigEndianSet{1}({2}, {3})",
                    arrayString, type, offsetString, instanceString);
            }
        }
        public override String ElementDeserializeExpression(String arrayString, String offsetString)
        {
            if (byteCount == 1)
            {
                return String.Format("{0}{1}[{2}]", (type == PdlType.SByte) ? "(SByte)" : "", arrayString, offsetString);
            }
            else
            {
                return String.Format("{0}.BigEndianRead{1}({2})",
                    arrayString, type, offsetString);
            }
        }
        public override String ElementDeserializeArrayExpression(String arrayString, String offsetString, String countString)
        {
            if (byteCount == 1)
            {
                return String.Format("{0}.CreateSub{1}Array({2}, {3})",
                    arrayString, (type == PdlType.Byte) ? "" : "SByte", offsetString, countString);
            }
            else
            {
                return String.Format("BigEndian{0}Serializer.Instance.FixedLengthDeserializeArray({1}, {2}, {3})",
                    type, arrayString, offsetString, countString);
            }
        }
        public override string ElementDataStringExpression(string builderString, string instanceString, bool small)
        {
            return String.Format("{0}.Append({1})", builderString, instanceString);
        }
        public override IntegerTypeReference AsIntegerTypeReference
        {
            get { return this; }
        }
        public override EnumOrFlagsTypeReference AsEnumOrFlagsTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override ObjectTypeReference AsObjectTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override SerializerTypeReference AsSerializerTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override string CodeBaseTypeString
        {
            get {
                switch (type)
                {
                    case PdlType.UInt24: return "UInt32";
                    case PdlType.Int24: return "Int32";
                    default: return type.ToString();
                }
            }
        }
    }
    public class EnumOrFlagsTypeReference : TypeReference
    {
        public readonly String relativeEnumReferenceTypeString;
        public readonly EnumOrFlagsDefinition definition;
        public EnumOrFlagsTypeReference(String relativeEnumReferenceTypeString, EnumOrFlagsDefinition definition, PdlArrayType arrayType)
            : base(relativeEnumReferenceTypeString, definition.isFlagsDefinition ? PdlType.Flags : PdlType.Enum, arrayType)
        {
            this.relativeEnumReferenceTypeString = relativeEnumReferenceTypeString;
            this.definition = definition;
        }
        public override UInt32 FixedElementSerializationLength
        {
            get { return definition.byteCount; }
        }
        public override String ElementDynamicSerializationLengthExpression(String instanceString)
        {
            throw new InvalidOperationException("CodeBug: this method should not be called on an enum type reference");
        }
        public override String ElementSerializeExpression(String arrayString, String offsetString, String instanceString)
        {
            return String.Format("{0}{1}EnumSerializer<{2}>.Instance.FixedLengthSerialize({3}, {4}, {5})",
                (definition.byteCount == 1) ? "" : "BigEndian", definition.underlyingIntegerType,
                definition.typeName, arrayString, offsetString, instanceString);
        }
        public override String ElementDeserializeExpression(String arrayString, String offsetString)
        {
            return String.Format("{0}{1}EnumSerializer<{2}>.Instance.FixedLengthDeserialize({3}, {4})",
                (definition.byteCount == 1) ? "" : "BigEndian", definition.underlyingIntegerType,
                definition.typeName, arrayString, offsetString);
        }
        public override String ElementDeserializeArrayExpression(String arrayString, String offsetString, String countString)
        {
            return String.Format("{0}{1}EnumSerializer<{2}>.Instance.FixedLengthDeserializeArray({3}, {4}, {5})",
                (definition.byteCount == 1) ? "" : "BigEndian", definition.underlyingIntegerType,
                definition.typeName, arrayString, offsetString, countString);
        }
        public override string ElementDataStringExpression(string builderString, string instanceString, bool small)
        {
            return String.Format("{0}.Append({1})", builderString, instanceString);
        }
        public override IntegerTypeReference AsIntegerTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override EnumOrFlagsTypeReference AsEnumOrFlagsTypeReference
        {
            get { return this; }
        }
        public override ObjectTypeReference AsObjectTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override SerializerTypeReference AsSerializerTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override string CodeBaseTypeString
        {
            get { return relativeEnumReferenceTypeString; }
        }
    }
    public class ObjectTypeReference : TypeReference
    {
        public readonly String relativeObjectReferenceTypeString;
        public readonly ObjectDefinition definition;

        public ObjectTypeReference(String relativeObjectReferenceTypeString, ObjectDefinition definition, PdlArrayType arrayType)
            : base(relativeObjectReferenceTypeString, PdlType.Object, arrayType)
        {
            this.relativeObjectReferenceTypeString = relativeObjectReferenceTypeString;
            this.definition = definition;
        }
        public override UInt32 FixedElementSerializationLength
        {
            get { return definition.FixedSerializationLength; }
        }
        public override String ElementDynamicSerializationLengthExpression(String instanceString)
        {
            if(FixedElementSerializationLength != UInt32.MaxValue)
                throw new InvalidOperationException(
                    "CodeBug: this method should not be called on an object type reference with a fixed element serialization length");

            return String.Format("{0}.Serializer.SerializationLength({1})",
                definition.name, instanceString);
        }
        public override String ElementSerializeExpression(String arrayString, String offsetString, String instanceString)
        {
            return String.Format("{0}.Serializer.Serialize({1}, {2}, {3})",
                definition.name, arrayString, offsetString, instanceString);
        }
        public override String ElementDeserializeExpression(String arrayString, String offsetString)
        {
            return String.Format("{0}.Serializer.Deserialize({1}, {2})",
                definition.name, arrayString, offsetString);
        }
        public override String ElementDeserializeArrayExpression(String arrayString, String offsetString, String countString)
        {
            if(FixedElementSerializationLength != UInt32.MaxValue)
            {
                return String.Format("{0}.Serializer.FixedLengthDeserializeArray({1}, {2}, {3})",
                    definition.name, arrayString, offsetString, countString);
            }
            throw new InvalidOperationException("This method only applies to fixed object types");
        }
        public override string ElementDataStringExpression(string builderString, string instanceString, bool small)
        {
            return String.Format("{0}.Serializer.Data{1}String({2}, {3})",
                definition.name, small ? "Small" : "", instanceString, builderString);
        }
        public override IntegerTypeReference AsIntegerTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override EnumOrFlagsTypeReference AsEnumOrFlagsTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override ObjectTypeReference AsObjectTypeReference
        {
            get { return this; }
        }
        public override SerializerTypeReference AsSerializerTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override void WritePdl(TextWriter writer, String fieldName)
        {
            writer.Write(relativeObjectReferenceTypeString);

            if (arrayType != null) writer.Write(arrayType.GetPdlArraySizeString());

            writer.Write(' ');
            writer.WriteLine(fieldName);
        }
        public override string CodeBaseTypeString
        {
            get { return relativeObjectReferenceTypeString; }
        }
    }
    public class SerializerTypeReference : TypeReference
    {
        PdlArrayType lengthType;
        public SerializerTypeReference(PdlArrayType lengthType, PdlArrayType arrayType)
            : base("Serializer", PdlType.Serializer, arrayType)
        {
            if (lengthType == null)
                throw new InvalidOperationException("A Serializer must have an array type");
            this.lengthType = lengthType;
        }
        public override UInt32 FixedElementSerializationLength
        {
            get { return UInt32.MaxValue; }
        }
        public override String ElementDynamicSerializationLengthExpression(String instanceString)
        {
            return String.Format("{0}.SerializationLength()", instanceString);
        }
        public override String ElementSerializeExpression(String arrayString, String offsetString, String instanceString)
        {
            return String.Format("{0}.Serialize({1}, {2})",
                instanceString, arrayString, offsetString);
        }
        public override String ElementDeserializeExpression(String arrayString, String offsetString)
        {
            //return String.Format("{0}.Deserialize({1}, {2})", "theInstance", arrayString, offsetString);
            return "null /*This functionality is not yet implemented*/";
        }
        public override String ElementDeserializeArrayExpression(String arrayString, String offsetString, String countString)
        {
            throw new InvalidOperationException("This method only applies to fixed object types");
        }
        public override string ElementDataStringExpression(string builderString, string instanceString, bool small)
        {
            return String.Format("{0}.Data{1}String({2})", instanceString, small ? "Small" : "", builderString);
        }
        public override IntegerTypeReference AsIntegerTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override EnumOrFlagsTypeReference AsEnumOrFlagsTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override ObjectTypeReference AsObjectTypeReference
        {
            get { throw new InvalidOperationException(); }
        }
        public override SerializerTypeReference AsSerializerTypeReference
        {
            get { return this; }
        }
        public override string CodeBaseTypeString
        {
            get
            {
                return "ISerializer";
            }
        }
    }
}