using System;
using System.Collections.Generic;
using System.IO;

namespace Marler.Pdl
{
    /*
    public static class SpaceWriter
    {
        public static void Spaces(this TextWriter output, Int32 length)
        {
            for (int i = 0; i < length; i++) output.Write(' ');
        }
    }

    public enum UnsignedIntegerType
    {
        Byte = PacketDescriptionLanguageLexer.BYTE_TYPE,
        UShort = PacketDescriptionLanguageLexer.USHORT_TYPE,
        UInt = PacketDescriptionLanguageLexer.UINT_TYPE,
        ULong = PacketDescriptionLanguageLexer.ULONG_TYPE,
    }
    public abstract class UserDefinedType
    {
        public readonly String name;
        public readonly String[] parentDataBlockDefinitions;
        public readonly String fullName;

        public UserDefinedType(String name, Queue<String> parentDataBlockDefinitions)
        {
            this.name = name;
            this.parentDataBlockDefinitions = parentDataBlockDefinitions;

            if(parentDataBlockDefinitions == null || parentDataBlockDefinitions.Length <= 0)
            {
                this.fullName = name;
            }
            else
            {
                fullName = parentDataBlockDefinitions[0];
                for(int i = 1; i < parentDataBlockDefinitions.Length; i++)
                {
                    fullName += '.' + parentDataBlockDefinitions[i];
                }
                fullName += '.' + name;
            }
        }
    }
    public class EnumDefinition : UserDefinedType
    {
        public readonly UnsignedIntegerType? type;
        public readonly EnumValue[] values;
        public EnumDefinition(String name, Queue<String> parentDataBlockDefinitions, UnsignedIntegerType? type, EnumValue[] values)
            : base(name, parentDataBlockDefinitions)
        {
            this.name = name;
            this.type = type;
            this.values = values;
        }
    }
    public class EnumValue
    {
        public readonly String name;
        public readonly String valueString;

        public Boolean actualValueIsNegative;
        public UInt64 actualAbsoluteValue;

        public EnumValue(String name, String valueString)
        {
            this.name = name;
            this.valueString = valueString;
        }
    }
    public class ArrayLength
    {
        public readonly Int32 fixedLength;
        public readonly PdlType unsignedIntegerType;
        public ArrayLength(Int32 fixedLength)
        {
            this.fixedLength = fixedLength;
        }
        public ArrayLength(PdlType unsignedIntegerType)
        {
            this.fixedLength = -1;
            this.unsignedIntegerType = unsignedIntegerType;
        }
        public override String ToString()
        {
            if (fixedLength >= 0) return fixedLength.ToString();
            return Enum.GetName(typeof(PdlType), unsignedIntegerType);
        }
    }
    public enum PdlType
    {
        Bit = PacketDescriptionLanguageLexer.BIT_TYPE,
        
        //
        // Unsigned Integer Types
        //
        Byte = PacketDescriptionLanguageLexer.BYTE_TYPE,
        UShort = PacketDescriptionLanguageLexer.USHORT_TYPE,
        UInt = PacketDescriptionLanguageLexer.UINT_TYPE,
        ULong = PacketDescriptionLanguageLexer.ULONG_TYPE,

        //
        // Signed Integer Types
        //
        SByte = PacketDescriptionLanguageLexer.SBYTE_TYPE,
        Short = PacketDescriptionLanguageLexer.SHORT_TYPE,
        Int = PacketDescriptionLanguageLexer.INT_TYPE,
        Long = PacketDescriptionLanguageLexer.LONG_TYPE,

        //
        // Floating Point Types
        //
        Float = PacketDescriptionLanguageLexer.FLOAT_TYPE,
        Double = PacketDescriptionLanguageLexer.DOUBLE_TYPE,

        //
        // User Defined Type
        //
        UserDefinedType = PacketDescriptionLanguageLexer.ID,
    }
    public abstract class DataBlockField
    {
        public readonly String name;
        public DataBlockField(String name)
        {
            this.name = name;
        }
        public abstract void PrintPdl(TextWriter output, Int32 level);
    }
    public class SimpleTypeField : DataBlockField
    {
        public readonly PdlType pdlType;
        public readonly String typeString;

        public readonly Boolean isOptional;

        public SimpleTypeField(String name, PdlType pdlType, String typeString, Boolean isOptional)
            : base(name)
        {
            this.pdlType = pdlType;
            this.typeString = typeString;
            this.isOptional = isOptional;
        }
        public override void  PrintPdl(TextWriter output, int level)
        {
            output.Spaces(level);
            Console.WriteLine(ToString());
        }
        public override String ToString()
        {
            return String.Format("{0}{1} {2}",
                isOptional ? "optional " : "",
                typeString,
                name);
        }            
    }
    public class ArrayField : DataBlockField
    {
        public readonly PdlType pdlType;
        public readonly String typeString;

        public readonly ArrayLength arrayLength;

        public ArrayField(String name, PdlType pdlType, String typeString, ArrayLength arrayLength)
            : base(name)
        {
            this.pdlType = pdlType;
            this.typeString = typeString;

            this.arrayLength = arrayLength;
        }
        public override void PrintPdl(TextWriter output, int level)
        {
            output.Spaces(level);
            Console.WriteLine(ToString());
        }
        public override String ToString()
        {
            return String.Format("{0}[{1}] {2}", typeString, arrayLength, name);
        }
    }


    public interface IPdlBuilder
    {
        void AddDataBlockDefinitionField(DataBlockDefinitionField dataBlockDefinitionField);
        void AddGlobalDataBlockDefinition(GlobalDataBlockDefinition globalDataBlockDefinition);

        void AddEnumDefinition(EnumDefinition enumDefinition);
    }
    public class DataBlockFieldsBuilder : IPdlBuilder
    {
        DataBlockField[] fields;
        Int32 currentFieldCount;

        public List<EnumDefinition> enumDefinition;
        public DataBlockFieldsBuilder(Int32 maxFieldCount)
        {
            this.fields = new DataBlockField[maxFieldCount];
            this.currentFieldCount = 0;
            this.enumDefinition = new List<EnumDefinition>();
        }
        public DataBlockField[] BuildFields()
        {
            if (currentFieldCount < fields.Length)
            {
                DataBlockField[] packedFields = new DataBlockField[currentFieldCount];
                Array.Copy(fields, packedFields, currentFieldCount);
                fields = packedFields;
            }
            return fields;
        }
        public void AddField(DataBlockField field)
        {
            fields[currentFieldCount++] = field;
        }
        public void AddDataBlockDefinitionField(DataBlockDefinitionField dataBlockDefinitionField)
        {
            fields[currentFieldCount++] = dataBlockDefinitionField;
        }
        public void AddGlobalDataBlockDefinition(GlobalDataBlockDefinition globalDataBlockDefinition)
        {
            throw new InvalidOperationException("Cannot add a global data block definition to another data block definition");
        }
        public void AddEnumDefinition(EnumDefinition enumDefinition)
        {
            throw new NotImplementedException();
        }
    }
    public abstract class DataBlockDefinition : DataBlockField
    {
        public readonly DataBlockField[] fields;
        public readonly List<EnumDefinition> enumDefinitions;
        public DataBlockDefinition(String name, DataBlockFieldsBuilder fieldsBuilder)
            : base(name)
        {
            this.fields = fieldsBuilder.BuildFields();
        }
    }
    public class DataBlockDefinitionField : DataBlockDefinition
    {
        ArrayLength arrayLength;
        public DataBlockDefinitionField(String name, DataBlockFieldsBuilder fieldsBuilder, ArrayLength arrayLength)
            : base(name, fieldsBuilder)
        {
            this.arrayLength = arrayLength;
        }
        public override void PrintPdl(TextWriter output, int level)
        {
            output.Spaces(level);
            output.WriteLine("[{0}] {1}", arrayLength, name);

            output.Spaces(level);
            output.WriteLine("{");
            for (int i = 0; i < fields.Length; i++)
            {
                DataBlockField field = fields[i];
                output.Spaces(level);
                if (field == null)
                {
                    output.WriteLine("<field translation from AST to PdlTree not yet implemented for this field>");
                }
                else
                {
                    field.PrintPdl(output, level + 1);
                }
            }

            output.Spaces(level);
            output.WriteLine("}");
        }
    }
    public class GlobalDataBlockDefinition : DataBlockDefinition
    {
        public GlobalDataBlockDefinition(String name, DataBlockFieldsBuilder fieldsBuilder)
            : base(name, fieldsBuilder)
        {
        }
        public override void PrintPdl(TextWriter output, int level)
        {
            output.Spaces(level);
            output.WriteLine("{0} ({1} fields)", name, fields.Length);

            output.Spaces(level);
            output.WriteLine("{");
            for (int i = 0; i < fields.Length; i++)
            {
                DataBlockField field = fields[i];
                output.Spaces(level);
                if (field == null)
                {
                    output.WriteLine("<field translation from AST to PdlTree not yet implemented for this field>");
                }
                else
                {
                    field.PrintPdl(output, level + 1);
                }
            }

            output.Spaces(level);
            output.WriteLine("}");
        }
    }
    public class PdlFile : IPdlBuilder
    {
        readonly List<EnumDefinition> enumDefinitions;
        readonly List<DataBlockDefinition> dataBlockDefinitions;

        Dictionary<String,UserDefinedType>
        public PdlFile()
        {
            this.enumDefinitions = new List<EnumDefinition>();
            this.dataBlockDefinitions = new List<DataBlockDefinition>();
        }
        public void AddDataBlockDefinitionField(DataBlockDefinitionField dataBlockDefinitionField)
        {
            throw new InvalidOperationException("Cannot add a data block definition field directly to a pdl file");
        }
        public void AddGlobalDataBlockDefinition(GlobalDataBlockDefinition globalDataBlockDefinition)
        {
            dataBlockDefinitions.Add(globalDataBlockDefinition);
        }
        public void AddEnumDefinition(EnumDefinition enumDefinition)
        {
            enumDefinitions.Add(enumDefinition);
        }


        public void ValidateSemantics()
        {

        }


        public void PrintPdl(TextWriter output)
        {
            //
            // Print enum definitions
            //
            foreach (EnumDefinition enumDefinition in enumDefinitions)
            {
                Int32 enumValueCount = (enumDefinition.values == null) ? 0 : enumDefinition.values.Length;

                output.WriteLine("enum {0}{1} ({2} values)", enumDefinition.name,
                    (enumDefinition.type == null) ? "" : " " + enumDefinition.type.ToString(),
                    enumValueCount);
                output.WriteLine("{");

                for (int i = 0; i < enumValueCount; i++)
                {
                    EnumValue enumValue = enumDefinition.values[i];
                    output.WriteLine("    {0}{1}", enumValue.name,
                        (enumValue.valueString == null) ? "" : " = " + enumValue.valueString);

                }
                output.WriteLine("}");
            }
            //
            // Print Data Blocks
            //
            foreach (DataBlockDefinition dataBlockDefinition in dataBlockDefinitions)
            {
                dataBlockDefinition.PrintPdl(output, 0);
            }
        }
    }
    */
}