using System;
using System.IO;

namespace More.Pdl
{
    public abstract class TypeReference
    {
        public readonly String relativePdlTypeReferenceString;
        public readonly PdlType type;
        public readonly PdlArraySizeType arraySizeType;
        public TypeReference(String relativePdlTypeReferenceString, PdlType type, PdlArraySizeType arraySizeType)
        {
            this.relativePdlTypeReferenceString = relativePdlTypeReferenceString;
            this.type = type;
            this.arraySizeType = arraySizeType;
        }

        public abstract IntegerTypeReference AsIntegerTypeReference { get; }
        public abstract EnumOrFlagsTypeReference AsEnumOrFlagsTypeReference { get; }
        public abstract ObjectTypeReference AsObjectTypeReference { get; }

        public virtual void WritePdl(TextWriter writer, String fieldName)
        {
            writer.Write(relativePdlTypeReferenceString);

            String arraySizeString = arraySizeType.GetPdlArraySizeString();
            if (arraySizeString != null) writer.Write(arraySizeString);

            writer.Write(' ');
            writer.WriteLine(fieldName);
        }
    }
    public class IntegerTypeReference : TypeReference
    {
        public IntegerTypeReference(PdlType integerType, PdlArraySizeType arraySizeType)
            : base(integerType.ToString(), integerType, arraySizeType)
        {
            //if (!type.IsIntegerType()) throw new InvalidOperationException(String.Format(
            //     "Cannot construct an IntegerTypeReference with a non-integer PdlType enum '{0}'", type));
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
    }
    public class EnumOrFlagsTypeReference : TypeReference
    {
        public readonly String relativeEnumReferenceTypeString;
        public readonly EnumOrFlagsDefinition definition;
        public EnumOrFlagsTypeReference(String relativeEnumReferenceTypeString, EnumOrFlagsDefinition definition, PdlArraySizeType arraySizeType)
            : base(relativeEnumReferenceTypeString, definition.isFlagsDefinition ? PdlType.Flags : PdlType.Enum, arraySizeType)
        {
            this.relativeEnumReferenceTypeString = relativeEnumReferenceTypeString;
            this.definition = definition;
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
    }
    public class ObjectTypeReference : TypeReference
    {
        public readonly String relativeObjectReferenceTypeString;
        public readonly ObjectDefinition definition;
        public ObjectTypeReference(String relativeObjectReferenceTypeString, ObjectDefinition definition, PdlArraySizeType arraySizeType)
            : base(relativeObjectReferenceTypeString, PdlType.Object, arraySizeType)
        {
            this.relativeObjectReferenceTypeString = relativeObjectReferenceTypeString;
            this.definition = definition;
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
        public override void WritePdl(TextWriter writer, String fieldName)
        {
            writer.Write(relativeObjectReferenceTypeString);

            String arraySizeString = arraySizeType.GetPdlArraySizeString();
            if (arraySizeString != null) writer.Write(arraySizeString);

            writer.Write(' ');
            writer.WriteLine(fieldName);
        }
    }
}