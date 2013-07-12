using System;
using System.Collections.Generic;
using System.IO;

namespace More.Pdl
{






    //
    // Type References
    //



    //
    //
    //















    //
    //
    //

    public class ObjectDefinitionField
    {
        public readonly TypeReference typeReference;
        public readonly String name;
        public ObjectDefinitionField(TypeReference typeReference, String name)
        {
            this.typeReference = typeReference;
            this.name = name;
        }
        public void WritePdl(TextWriter writer)
        {
            typeReference.WritePdl(writer, name);
        }
        //public abstract void GenerateReflectorConstructor(TextWriter writer, Int32 tabs, String destination);
    }

    public class ObjectDefinition
    {
        public readonly String name;
        public readonly String nameLowerInvariant;

        public readonly String globalReferenceNameLowerInvariant;

        public readonly ObjectDefinition objectDefinedIn;

        public List<EnumOrFlagsDefinition> enumOrFlagDefinitions;
        public List<ObjectDefinition> objectDefinitions;

        readonly List<ObjectDefinitionField> fields;
        public List<ObjectDefinitionField> Fields
        {
            get
            {
                if (fixedSerializationLength == -2) throw new InvalidOperationException(
                    "Cannot access fields until serialization length has been calculated");
                return fields;
            }
        }
        //public int firstOptionalFieldIndex;

        int fixedSerializationLength;
        public int FixedSerializationLength
        {
            get
            {
                if (fixedSerializationLength == -2) throw new InvalidOperationException(
                    "Cannot access FixedSerializationLength until serialization length has been calculated");
                return fixedSerializationLength;
            }
        }

        public ObjectDefinition(PdlFile pdlFile, String name, String nameLowerInvariant, ObjectDefinition objectDefinedIn)
        {
            this.name = name;
            this.nameLowerInvariant = nameLowerInvariant;

            this.globalReferenceNameLowerInvariant = (objectDefinedIn == null) ? nameLowerInvariant :
                (objectDefinedIn.globalReferenceNameLowerInvariant + "." + nameLowerInvariant);

            this.objectDefinedIn = objectDefinedIn;

            this.fields = new List<ObjectDefinitionField>();
            //this.firstOptionalFieldIndex = -1;

            this.fixedSerializationLength = -2;

            //
            // Add definition to pdl file and parent object
            //
            pdlFile.AddObjectDefinition(this);
            if (objectDefinedIn != null) objectDefinedIn.AddObjectDefinition(this);
        }
        public void AddEnumOrFlagDefinition(EnumOrFlagsDefinition definition)
        {
            if (enumOrFlagDefinitions == null) enumOrFlagDefinitions = new List<EnumOrFlagsDefinition>();
            enumOrFlagDefinitions.Add(definition);
        }
        public void AddObjectDefinition(ObjectDefinition definition)
        {
            if (objectDefinitions == null) objectDefinitions = new List<ObjectDefinition>();
            objectDefinitions.Add(definition);
        }
        public void Add(ObjectDefinitionField field)
        {
            if (fixedSerializationLength != -2) throw new InvalidOperationException(
                "Cannot add fields after FixedSerializationLength has been calculated");
            fields.Add(field);
        }
        public void CalculateFixedSerializationLength()
        {
            if (fixedSerializationLength != -2) throw new InvalidOperationException(
                "Cannot calculate FixedSerializationLength after it has already been calculated");

            Int32 length = 0;
            for (int i = 0; i < fields.Count; i++)
            {
                ObjectDefinitionField field = fields[i];
                TypeReference fieldTypeReference = field.typeReference;
                Int32 fieldFixedSerializationLength = fieldTypeReference.FixedElementSerializationLength;
                if (fieldFixedSerializationLength < 0)
                {
                    this.fixedSerializationLength = -1;
                    return;
                }

                PdlArrayType arrayType = fieldTypeReference.arrayType;
                if (arrayType == null)
                {
                    length += fieldFixedSerializationLength;
                }
                else
                {
                    if (arrayType.type != PdlArraySizeTypeEnum.Fixed)
                    {
                        this.fixedSerializationLength = -1;
                        return;
                    }
                    length += (Int32)arrayType.GetFixedArraySize() * fieldFixedSerializationLength;
                }

            }
            this.fixedSerializationLength = length;
        }
        public void WritePdl(TextWriter writer)
        {
            writer.WriteLine(name + " {");
            for (int i = 0; i < fields.Count; i++)
            {
                ObjectDefinitionField field = fields[i];
                field.WritePdl(writer);
            }
        }
    }



    /*
    public class PacketTypeReference
    {
        public readonly PacketPrimitives primitiveType;
        public readonly PdlArraySizeType arraySizeType;
        public readonly String notValidDefaultValue;

        public PacketTypeReference(PacketPrimitives jetlinkPrimitiveType, PdlArraySizeType arraySizeType, String notValidDefaultValue)
        {
            this.primitiveType = jetlinkPrimitiveType;
            this.arraySizeType = arraySizeType;
            this.notValidDefaultValue = notValidDefaultValue;
        }
        public virtual PacketObjectType TryCastToPacketObjectType { get { return null; } }

        public virtual String CSharpElementType()
        {
            return primitiveType.ToString();
        }
        public String CSharpPdlType()
        {
            if (arraySizeType == PdlArraySizeType.NotAnArray)
            {
                return CSharpElementType();
            }
            if (primitiveType == PacketPrimitives.Char)
            {
                return "String";
            }
            return CSharpElementType() + "[]";
        }
        public String CSharpFieldWrapperClass()
        {
            if (arraySizeType == PdlArraySizeType.NotAnArray)
            {
                return String.Format("ClassGenericFieldSerializer<{0}>", CSharpElementType());
            }
            if (primitiveType == PacketPrimitives.Char)
            {
                return String.Format("ClassStringFieldSerializer");
            }
            return String.Format("ClassArrayFieldSerializer<{0}>", CSharpElementType());
        }
        public virtual String CSharpFieldWrapperExtraConstructorArgs()
        {
            if (arraySizeType == PdlArraySizeType.NotAnArray)
            {
                return String.Format(", Jetlink{0}Serializer.instance", CSharpElementType());
            }
            if (primitiveType == PacketPrimitives.Char)
            {
                return String.Format(", Jetlink{0}Serializer.instance", arraySizeType);
            }
            return String.Format(", Jetlink{0}Serializer.instance, Jetlink{1}Serializer.instance", arraySizeType, primitiveType);
        }

        public virtual String JetlinkTypeString()
        {
            if (arraySizeType == PdlArraySizeType.NotAnArray)
            {
                return primitiveType.ToString();
            }
            return String.Format("{0}[{1}]", primitiveType, arraySizeType);
        }
    }

    public class PacketObjectTypeDefinition
    {
        public readonly String name;
        public readonly List<DataBlockDefinition.Field> fields;

        public PacketObjectTypeDefinition(String name)
        {
            this.name = name;
            this.fields = new List<PacketField>();
        }
    }
    public class PacketObjectType : PacketTypeReference
    {
        public readonly PacketObjectTypeDefinition typeObjectDefinition;

        public PacketObjectType(PacketObjectTypeDefinition typeObjectDefinition, PdlArraySizeType arraySizeType)
            : base(PacketPrimitives.Object, arraySizeType, null)
        {
            this.typeObjectDefinition = typeObjectDefinition;
        }
        public override PacketObjectType TryCastToPacketObjectType { get { return this; } }

        public override String CSharpElementType()
        {
            return typeObjectDefinition.name + "Object";
        }
        public override String CSharpFieldWrapperExtraConstructorArgs()
        {
            String arraySizeTypeArgument;
            if (arraySizeType == PdlArraySizeType.NotAnArray)
            {
                arraySizeTypeArgument = "";
            }
            else
            {
                arraySizeTypeArgument = String.Format(", Jetlink{0}Serializer.instance", arraySizeType);
            }

            return String.Format("{0}, new JetlinkObjectSerializer<{1}>({2}Fields)",
                arraySizeTypeArgument, CSharpElementType(), typeObjectDefinition.name);
        }

        public override String ToString()
        {
            if (arraySizeType == PdlArraySizeType.NotAnArray)
            {
                return typeObjectDefinition.name;
            }
            return String.Format("{0}[{1}]", typeObjectDefinition.name, arraySizeType);
        }
    }

    public class JetlinkEnumOrFlagsType : PacketTypeReference
    {
        public readonly EnumOrFlagsTypeDefinition definition;
        public readonly String typeSuffix;
        public JetlinkEnumOrFlagsType(EnumOrFlagsTypeDefinition definition, PdlArraySizeType arraySizeType, String notValidDefaultValue)
            : base(PacketPrimitives.Enum, arraySizeType, notValidDefaultValue)
        {
            this.definition = definition;
            this.typeSuffix = definition.isFlagsDefinition ? "Flags" : "Enum";
        }
        public override String CSharpElementType()
        {
            return definition.typeName + typeSuffix;
        }
        public override String CSharpFieldWrapperExtraConstructorArgs()
        {
            String enumSerializerArgument = String.Format(", Jetlink{0}EnumSerializer<{1}{2}>.instance", definition.underlyingIntegerType, definition.typeName, typeSuffix);
            if (arraySizeType == PdlArraySizeType.NotAnArray)
            {
                return enumSerializerArgument;
            }
            return String.Format(", Jetlink{0}Serializer.instance{1}", arraySizeType, enumSerializerArgument);
        }
        public override String ToString()
        {
            if (arraySizeType == PdlArraySizeType.NotAnArray)
            {
                return definition.typeName;
            }
            return String.Format("{0}[{1}]", definition.typeName, arraySizeType);
        }
    }
*/
}