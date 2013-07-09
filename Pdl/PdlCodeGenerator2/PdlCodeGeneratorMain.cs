using System;
using System.Collections.Generic;
using System.IO;

namespace More.Pdl
{
    class PdlCodeGeneratorOptions
    {

    }




    class PdlCodeGeneratorMain
    {
        static void Main(string[] args)
        {
            PdlFile pdlFile = PdlFileParser.ParsePdlFile(Console.In);

            /*
            Console.WriteLine("{0} Object Definitinos", objectDefinitions.Count);
            for (int i = 0; i < objectDefinitions.Count; i++)
            {
                ObjectDefinition objectDefinition = objectDefinitions[i];
                Console.WriteLine(objectDefinition);
            }
            */

            CSharpCodeGenerator csharpCodeGenerator = new CSharpCodeGenerator();

            GenerateCode(Console.Out, pdlFile, csharpCodeGenerator);
        }


        static void GenerateCode(TextWriter writer, PdlFile pdlFile, ICodeGenerator generator)
        {
            writer.WriteLine("using System;");
            writer.WriteLine();
            writer.WriteLine("using More;");
            writer.WriteLine();

            //
            // Print global enum definitions
            //
            foreach (EnumOrFlagsDefinition enumOrFlagsDefinition in pdlFile.EnumOrFlagsDefinitions)
            {
                if (enumOrFlagsDefinition.isFlagsDefinition)
                {
                    writer.WriteLine("    [Flags]");
                }
                writer.WriteLine("    public enum {0} {{", enumOrFlagsDefinition.typeName);
                if (enumOrFlagsDefinition.isFlagsDefinition)
                {
                    foreach (FlagsValueDefinition flagValues in enumOrFlagsDefinition.flagValues)
                    {
                        writer.WriteLine("        {0} = {1},", flagValues.name, 0x01 << flagValues.bit);
                    }
                }
                else
                {
                    foreach (EnumValueDefinition enumValues in enumOrFlagsDefinition.enumValues)
                    {
                        writer.WriteLine("        {0} = {1},", enumValues.name, enumValues.value);
                    }
                }
                writer.WriteLine("    }");
            }


            //
            // Print class definitions
            //
            for (int i = 0; i < pdlFile.objectDefinitions.Count; i++)
            {
                UInt32 tabs;
                ObjectDefinition objectDefinition = pdlFile.objectDefinitions[i];

                Console.WriteLine("    public class {0}", objectDefinition.name);
                Console.WriteLine("    {");
                //
                // Print static serializer
                //
                writer.WriteLine("        static IReflector reflector = null;");
                writer.WriteLine("        public static IReflector Reflector");
                writer.WriteLine("        {");
                writer.WriteLine("            get");
                writer.WriteLine("            {");
                writer.WriteLine("                if(reflector == null)");
                writer.WriteLine("                {");
                writer.WriteLine("                    IReflector[] reflectors = new IReflector[{0}];", objectDefinition.fields.Count);
                tabs = 5;
                for(int fieldIndex = 0; fieldIndex < objectDefinition.fields.Count; fieldIndex++)
                {
                ObjectDefinitionField field = objectDefinition.fields[fieldIndex];
                String assignTo = String.Format("reflectors[{0}]", fieldIndex);
                PdlType type = field.typeReference.type;
                if(type == PdlType.Object)
                {
                    generator.GenerateReflectorConstructor(writer, tabs, assignTo, objectDefinition.name, field.name, field.typeReference.AsObjectTypeReference);
                }
                else if(type == PdlType.Enum || type == PdlType.Flags)
                {
                    generator.GenerateReflectorConstructor(writer, tabs, assignTo, objectDefinition.name, field.name, field.typeReference.AsEnumOrFlagsTypeReference);
                }
                else
                {
                    generator.GenerateReflectorConstructor(writer, tabs, assignTo, objectDefinition.name, field.name, field.typeReference.AsIntegerTypeReference);
                }
                }
                writer.WriteLine("                    reflector = new IReflectors(reflectors);");
                writer.WriteLine("                }");
                writer.WriteLine("                return reflector;");
                writer.WriteLine("            }");
                writer.WriteLine("        }");

                //
                // Print fields
                //
                tabs = 2;
                for(int fieldIndex = 0; fieldIndex < objectDefinition.fields.Count; fieldIndex++)
                {
                    ObjectDefinitionField field = objectDefinition.fields[fieldIndex];
                    TypeReference typeReference = field.typeReference;
                    writer.WriteLine(tabs * 4, "public {0}{1} {2};", typeReference.relativePdlTypeReferenceString,
                        (typeReference.arraySizeType == PdlArraySizeType.NotAnArray) ? "" : "[]", field.name);

                    /*
                if(type == PdlType.Object)
                {
                    generator.GenerateField(writer, tabs, field.name, field.typeReference.AsObjectTypeReference);
                }
                else if(type == PdlType.Enum || type == PdlType.Flags)
                {
                    generator.GenerateField(writer, tabs, field.name, field.typeReference.AsEnumOrFlagsTypeReference);
                }
                else
                {
                    generator.GenerateField(writer, tabs, field.name, field.typeReference.AsIntegerTypeReference);
                }
                    */
                }
                //
                // Print Deserialization Constructor
                //
                writer.WriteLine();
                writer.WriteLine("        // Deserialization constructor");
                writer.WriteLine("        public {0}(Byte[] array, Int32 offset, Int32 maxOffset)", objectDefinition.name);
                writer.WriteLine("        {");
                writer.WriteLine("            int finalOffset = Reflector.Deserialize(this, array, offset, maxOffset);");
                writer.WriteLine("            if(finalOffset != maxOffset) throw new FormatException(String.Format(");
                writer.WriteLine("                \"Expected packet '{0}' to be {{1}} bytes but was {{2}} bytes\", maxOffset - offset, finalOffset - offset));", objectDefinition.name);
                writer.WriteLine("        }");
                //
                // Print Serialization Constructor
                //
                writer.Write("        public {0}(", objectDefinition.name);
                for (int fieldIndex = 0; fieldIndex < objectDefinition.fields.Count; fieldIndex++)
                {
                    ObjectDefinitionField field = objectDefinition.fields[fieldIndex];
                    if(fieldIndex > 0) Console.Write(", ");
                    Console.Write("{0}{1} {2}", field.typeReference.relativePdlTypeReferenceString,
                        (field.typeReference.arraySizeType == PdlArraySizeType.NotAnArray) ? "" : "[]", field.name);
                }
                writer.WriteLine(")");
                writer.WriteLine("        {");
                for (int fieldIndex = 0; fieldIndex < objectDefinition.fields.Count; fieldIndex++)
                {
                    ObjectDefinitionField field = objectDefinition.fields[fieldIndex];
                    Console.WriteLine("            this.{0} = {0};", field.name);
                }
                writer.WriteLine("        }");




                Console.WriteLine("    }");
            }
        }
    }

    
}
