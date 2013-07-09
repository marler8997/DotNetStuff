using System;
using System.IO;

namespace More.Pdl
{
    /*
    public class CSharpPdlDataStructureFactory : IPdlDataStructureFactory
    {
        public ObjectDefinitionField ConstructObjectDefinitionField(TypeReference typeReference, String name)
        {
            return new CSharpObjectDefinitionField();
        }
    }

    public class CSharpObjectDefinitionField : ObjectDefinitionField
    {
        public override void GenerateReflectorConstructor(TextWriter writer, Int32 tabs, String destination)
        {


        }
    }
    */

    public class CSharpCodeGenerator : ICodeGenerator
    {
        public void GenerateReflectorConstructor(TextWriter writer, UInt32 tabs, String assignTo, String className, String fieldName, IntegerTypeReference type)
        {
            writer.WriteLine(tabs * 4, "{0} = new Pdl{1}{2}{3}Reflector(typeof({4}), \"{5}\");",
                assignTo, (type.arraySizeType == PdlArraySizeType.NotAnArray) ? "" : type.arraySizeType.ToString() + "Length",
            type.type, (type.arraySizeType == PdlArraySizeType.NotAnArray) ? "" : "Array", className, fieldName);
        }
        public void GenerateReflectorConstructor(TextWriter writer, UInt32 tabs, String assignTo, String className, String fieldName, EnumOrFlagsTypeReference type)
        {
            writer.WriteLine(tabs * 4, "{0} = new Pdl{1}Enum{2}Reflector<{3}>(typeof({4}), \"{5}\");",
                assignTo, type.definition.underlyingIntegerType, (type.arraySizeType == PdlArraySizeType.NotAnArray) ? "" : "Array",
                type.definition.typeName, className, fieldName);
        }
        public void GenerateReflectorConstructor(TextWriter writer, UInt32 tabs, String assignTo, String className, String fieldName, ObjectTypeReference type)
        {
            throw new NotImplementedException();
        }
    }
}
