﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace More.Pdl
{
    public static class PdlCodeGenerator
    {
        static void GenerateEnumDefinition(TextWriter writer, UInt32 tabs, EnumOrFlagsDefinition definition)
        {
            if (definition.isFlagsDefinition)
            {
                writer.WriteLine(tabs * 4, "[Flags]");
            }
            writer.WriteLine(tabs * 4, "public enum {0} {{", definition.typeName);
            if (definition.isFlagsDefinition)
            {
                foreach (FlagsValueDefinition flagValues in definition.flagValues)
                {
                    writer.WriteLine(tabs * 4, "    {0} = {1},", flagValues.name, 0x01 << flagValues.bit);
                }
            }
            else
            {
                foreach (EnumValueDefinition enumValues in definition.enumValues)
                {
                    writer.WriteLine(tabs * 4, "    {0} = {1},", enumValues.name, enumValues.value);
                }
            }
            writer.WriteLine(tabs * 4, "}");

        }
        public static void GenerateCode(TextWriter writer, PdlFile pdlFile, String @namespace)
        {
            //
            // Write Code Generation Information
            //
            writer.WriteLine("//");
            writer.WriteLine("// This file was autogenerated using the PdlCodeGenerator");
            writer.WriteLine("//     GenerationDateTime : {0}", DateTime.Now);
            writer.WriteLine("//");


            writer.WriteLine("using System;");
            writer.WriteLine("using System.Text;");
            writer.WriteLine();
            writer.WriteLine("using More;");
            writer.WriteLine();
            writer.WriteLine("namespace {0}", @namespace);
            writer.WriteLine("{");

            //
            // Print global enum definitions
            //
            foreach (EnumOrFlagsDefinition enumOrFlagsDefinition in pdlFile.EnumOrFlagsDefinitions)
            {
                if (enumOrFlagsDefinition.isGlobalType)
                {
                    GenerateEnumDefinition(writer, 1, enumOrFlagsDefinition);
                }
            }

            //
            // Print class definitions
            //
            foreach (ObjectDefinition objectDefinition in pdlFile.ObjectDefinitions)
            {
                List<ObjectDefinitionField> objectDefinitionFields = objectDefinition.Fields;

                Int32 fixedSerializationLength = objectDefinition.FixedSerializationLength;

                UInt32 tabs;

                writer.WriteLine("    public class {0}", objectDefinition.name);
                writer.WriteLine("    {");
                if (fixedSerializationLength >= 0)
                {
                    writer.WriteLine("        public const Int32 FixedSerializationLength = {0};", fixedSerializationLength);
                    writer.WriteLine();
                }

                /*
                //
                // Print static reflector
                //
                writer.WriteLine("        static IReflector reflector = null;");
                writer.WriteLine("        public static IReflector Reflector");
                writer.WriteLine("        {");
                writer.WriteLine("            get");
                writer.WriteLine("            {");
                writer.WriteLine("                if(reflector == null)");
                writer.WriteLine("                {");
                writer.WriteLine("                    reflector = new Reflectors(new IReflector[] {");
                tabs = 6;
                for (int fieldIndex = 0; fieldIndex < objectDefinitionFields.Count; fieldIndex++)
                {
                ObjectDefinitionField field = objectDefinitionFields[fieldIndex];
                PdlType type = field.typeReference.type;
                if(type == PdlType.Object)
                {
                    generator.GenerateReflectorConstructor(writer, tabs, objectDefinition.name, field.name, field.typeReference.AsObjectTypeReference);
                }
                else if(type == PdlType.Enum || type == PdlType.Flags)
                {
                    generator.GenerateReflectorConstructor(writer, tabs, objectDefinition.name, field.name, field.typeReference.AsEnumOrFlagsTypeReference);
                }
                else if (type == PdlType.Serializer)
                {
                    generator.GenerateReflectorConstructor(writer, tabs, objectDefinition.name, field.name, field.typeReference.AsSerializerTypeReference);
                }
                else
                {
                    generator.GenerateReflectorConstructor(writer, tabs, objectDefinition.name, field.name, field.typeReference.AsIntegerTypeReference);
                }
                }
                writer.WriteLine("                    });");
                writer.WriteLine("                }");
                writer.WriteLine("                return reflector;");
                writer.WriteLine("            }");
                writer.WriteLine("        }");
                writer.WriteLine();
                */




                //
                // Print static instance serializer
                //
                writer.WriteLine("        static InstanceSerializer serializer = null;");
                writer.WriteLine("        public static {0}InstanceSerializer<{1}> Serializer", (fixedSerializationLength >= 0) ? "FixedLength" : "I", objectDefinition.name);
                writer.WriteLine("        {");
                writer.WriteLine("            get");
                writer.WriteLine("            {");
                writer.WriteLine("                if(serializer == null) serializer = new InstanceSerializer();");
                writer.WriteLine("                return serializer;");
                writer.WriteLine("            }");
                writer.WriteLine("        }");
                writer.WriteLine();
                writer.WriteLine("        class InstanceSerializer : {0}InstanceSerializer<{1}>",
                    (fixedSerializationLength >= 0) ? "FixedLength" : "I", objectDefinition.name);
                writer.WriteLine("        {");
                writer.WriteLine("            public InstanceSerializer() {}");

                //
                // FixedLength Object Serializer
                //
                if (fixedSerializationLength >= 0)
                {
                    writer.WriteLine("            public override Int32 FixedSerializationLength() {{ return {0}.FixedSerializationLength; }}", objectDefinition.name);
                    writer.WriteLine("            public override void FixedLengthSerialize(Byte[] bytes, Int32 offset, {0} instance)", objectDefinition.name);
                    writer.WriteLine("            {");
                    for (int fieldIndex = 0; fieldIndex < objectDefinitionFields.Count; fieldIndex++)
                    {
                        ObjectDefinitionField field = objectDefinitionFields[fieldIndex];
                        TypeReference typeReference = field.typeReference;
                        if (typeReference.arrayType == null)
                        {
                            writer.WriteLine("                {0};", typeReference.ElementSerializeExpression("bytes", "offset", "instance." + field.name));
                            writer.WriteLine("                offset += {0};", typeReference.FixedElementSerializationLength);
                        }
                        else
                        {
                            throw new NotImplementedException("Arrays inside fixed length objects not implemented");
                        }
                    }
                    writer.WriteLine("            }");
                    writer.WriteLine("            public override {0} FixedLengthDeserialize(Byte[] bytes, Int32 offset)", objectDefinition.name);
                    writer.WriteLine("            {");
                    writer.WriteLine("                return new {0} (", objectDefinition.name);
                    Int32 offset = 0;
                    for (int fieldIndex = 0; fieldIndex < objectDefinitionFields.Count; fieldIndex++)
                    {
                        ObjectDefinitionField field = objectDefinitionFields[fieldIndex];
                        TypeReference typeReference = field.typeReference;
                        if (typeReference.arrayType == null)
                        {
                            writer.Write("                    {0}", typeReference.ElementDeserializeExpression("bytes", "offset + " + offset.ToString()));
                            offset += typeReference.FixedElementSerializationLength;
                        }
                        else
                        {
                            throw new NotImplementedException("Arrays inside fixed length objects not implemented");
                        }
                        if (fieldIndex < objectDefinitionFields.Count - 1) writer.Write(',');
                        writer.WriteLine(" // {0}", field.name);
                    }
                    writer.WriteLine("                );");
                    writer.WriteLine("            }");
                }
                //
                // DynamicLength Object Serializer
                //
                else
                {
                    writer.WriteLine("            public Int32 SerializationLength({0} instance)", objectDefinition.name);
                    writer.WriteLine("            {");
                    writer.WriteLine("                int dynamicLengthPart = 0;");
                    Int32 fixedSerializationLengthPart = 0;
                    for (int fieldIndex = 0; fieldIndex < objectDefinitionFields.Count; fieldIndex++)
                    {
                        ObjectDefinitionField field = objectDefinitionFields[fieldIndex];
                        TypeReference typeReference = field.typeReference;

                        Int32 fieldFixedElementSerializationLength = typeReference.FixedElementSerializationLength;
                        if (fieldFixedElementSerializationLength >= 0)
                        {
                            if (typeReference.arrayType == null)
                            {
                                fixedSerializationLengthPart += fieldFixedElementSerializationLength;
                            }
                            else
                            {
                                if (typeReference.arrayType.type == PdlArraySizeTypeEnum.Fixed)
                                {
                                    fixedSerializationLengthPart += fieldFixedElementSerializationLength * (Int32)typeReference.arrayType.GetFixedArraySize();
                                }
                                else
                                {
                                    writer.WriteLine("                if(instance.{0} != null) dynamicLengthPart += instance.{0}.Length * {1};", field.name, fieldFixedElementSerializationLength);
                                }
                            }
                        }
                        else
                        {
                            if (typeReference.arrayType == null)
                            {
                                writer.WriteLine("                dynamicLengthPart += {0};", typeReference.ElementDynamicSerializationLengthExpression("instance." + field.name));
                            }
                            else
                            {
                                String serializationLengthExpression = typeReference.ElementDynamicSerializationLengthExpression("instance." + field.name + "[i]");
                                if (typeReference.arrayType.type == PdlArraySizeTypeEnum.Fixed)
                                {
                                    writer.WriteLine("                for(int i = 0; i < {0}; i++)", typeReference.arrayType.GetFixedArraySize());
                                    writer.WriteLine("                {");
                                    writer.WriteLine("                    dynamicLengthPart += {0};", serializationLengthExpression);
                                    writer.WriteLine("                }");
                                }
                                else
                                {
                                    writer.WriteLine("                if(instance.{0} != null)", field.name);
                                    writer.WriteLine("                {");
                                    writer.WriteLine("                    for(int i = 0; i < instance.{0}.Length; i++)", field.name);
                                    writer.WriteLine("                    {");
                                    writer.WriteLine("                        dynamicLengthPart += {0};", serializationLengthExpression);
                                    writer.WriteLine("                    }");
                                    writer.WriteLine("                }");
                                }
                            }
                        }
                    }
                    writer.WriteLine("                return {0} + dynamicLengthPart;", fixedSerializationLengthPart);
                    writer.WriteLine("            }");
                    writer.WriteLine("            public Int32 Serialize(Byte[] bytes, Int32 offset, {0} instance)", objectDefinition.name);
                    writer.WriteLine("            {");
                    for (int fieldIndex = 0; fieldIndex < objectDefinitionFields.Count; fieldIndex++)
                    {
                        ObjectDefinitionField field = objectDefinitionFields[fieldIndex];
                        TypeReference typeReference = field.typeReference;

                        Int32 fieldFixedElementSerializationLength = typeReference.FixedElementSerializationLength;
                        if (fieldFixedElementSerializationLength >= 0)
                        {
                            if (typeReference.arrayType == null)
                            {
                                writer.WriteLine("                {0};", typeReference.ElementSerializeExpression("bytes", "offset", "instance." + field.name));
                                writer.WriteLine("                offset += {0};", fieldFixedElementSerializationLength);
                            }
                            else
                            {
                                writer.WriteLine("                // Arrays inside dynamic length objects not implemented");
                            }
                        }
                        else
                        {
                            if (typeReference.arrayType == null)
                            {
                                writer.WriteLine("                offset = {0};", typeReference.ElementSerializeExpression("bytes", "offset", "instance." + field.name));
                            }
                            else
                            {
                                writer.WriteLine("                // Arrays inside dynamic length objects not implemented");
                            }
                        }
                    }
                    writer.WriteLine("                return offset;");
                    writer.WriteLine("            }");
                    writer.WriteLine("            public Int32 Deserialize(Byte[] bytes, Int32 offset, Int32 offsetLimit, out {0} outInstance)", objectDefinition.name);
                    writer.WriteLine("            {");
                    writer.WriteLine("                {0} instance = new {0}();", objectDefinition.name);
                    for (int fieldIndex = 0; fieldIndex < objectDefinitionFields.Count; fieldIndex++)
                    {
                        ObjectDefinitionField field = objectDefinitionFields[fieldIndex];
                        TypeReference typeReference = field.typeReference;
                        if (typeReference.arrayType == null)
                        {
                            writer.WriteLine("                instance.{0} = {1};", field.name, typeReference.ElementDeserializeExpression("bytes", "offset"));
                            writer.WriteLine("                offset += {0};", typeReference.FixedElementSerializationLength);
                        }
                        else
                        {
                            writer.WriteLine("                // Arrays inside dynamic length objects not implemented");
                        }
                    }
                    writer.WriteLine("                outInstance = instance;");
                    writer.WriteLine("                return offset;");
                    writer.WriteLine("            }");
                }

                writer.WriteLine("            public{0} void DataString({1} instance, StringBuilder builder)", (fixedSerializationLength >= 0) ? " override" : "", objectDefinition.name);
                writer.WriteLine("            {");
                writer.WriteLine("                builder.Append(\"{0}:{{\");", objectDefinition.name);
                for (int fieldIndex = 0; fieldIndex < objectDefinitionFields.Count; fieldIndex++)
                {
                    if (fieldIndex > 0) writer.WriteLine("                builder.Append(',');");

                    ObjectDefinitionField field = objectDefinitionFields[fieldIndex];
                    TypeReference typeReference = field.typeReference;
                    if (typeReference.arrayType == null)
                    {
                        writer.WriteLine("                {0};", typeReference.ElementDataStringExpression("builder", "instance." + field.name, false));
                    }
                    else
                    {
                        writer.WriteLine("                // Arrays inside dynamic length objects not implemented");
                    }
                }
                writer.WriteLine("                builder.Append(\"}\");");
                writer.WriteLine("            }");
                writer.WriteLine("            public{0} void DataSmallString({1} instance, StringBuilder builder)", (fixedSerializationLength >= 0) ? " override" : "", objectDefinition.name);
                writer.WriteLine("            {");
                writer.WriteLine("                builder.Append(\"{0}:{{\");", objectDefinition.name);
                for (int fieldIndex = 0; fieldIndex < objectDefinitionFields.Count; fieldIndex++)
                {
                    if (fieldIndex > 0) writer.WriteLine("                builder.Append(',');");

                    ObjectDefinitionField field = objectDefinitionFields[fieldIndex];
                    TypeReference typeReference = field.typeReference;
                    if (typeReference.arrayType == null)
                    {
                        writer.WriteLine("                {0};", typeReference.ElementDataStringExpression("builder", "instance." + field.name, true));
                    }
                    else
                    {
                        writer.WriteLine("            // Arrays inside dynamic length objects not implemented");
                    }
                }
                writer.WriteLine("                builder.Append(\"}\");");
                writer.WriteLine("            }");


                writer.WriteLine("        }"); // End Of Serializer Class








                //
                // Print Enum definitinos
                //
                if (objectDefinition.enumOrFlagDefinitions != null)
                {
                    writer.WriteLine();
                    foreach (EnumOrFlagsDefinition enumOrFlagsDefinition in objectDefinition.enumOrFlagDefinitions)
                    {
                        GenerateEnumDefinition(writer, 2, enumOrFlagsDefinition);
                    }
                }

                //
                // Print fields
                //
                writer.WriteLine();
                tabs = 2;
                for (int fieldIndex = 0; fieldIndex < objectDefinitionFields.Count; fieldIndex++)
                {
                    ObjectDefinitionField field = objectDefinitionFields[fieldIndex];
                    TypeReference typeReference = field.typeReference;
                    writer.WriteLine(tabs * 4, "public {0} {1};", typeReference.CodeTypeString(), field.name);
                }
                //
                // Print No-Parameter Constructor
                //
                writer.WriteLine("        private {0}() {{ }}", objectDefinition.name);

                //
                // Print Parameter Constructor
                //
                writer.Write("        public {0}(", objectDefinition.name);
                for (int fieldIndex = 0; fieldIndex < objectDefinitionFields.Count; fieldIndex++)
                {
                    ObjectDefinitionField field = objectDefinitionFields[fieldIndex];
                    if (fieldIndex > 0) writer.Write(", ");
                    writer.Write("{0} {1}", field.typeReference.CodeTypeString(), field.name);
                }
                writer.WriteLine(")");
                writer.WriteLine("        {");
                for (int fieldIndex = 0; fieldIndex < objectDefinitionFields.Count; fieldIndex++)
                {
                    ObjectDefinitionField field = objectDefinitionFields[fieldIndex];
                    writer.WriteLine("            this.{0} = {0};", field.name);
                }
                writer.WriteLine("        }");
                //
                // Print Deserialization Constructor
                //
                /*
                writer.WriteLine();
                writer.WriteLine("        // Deserialization constructor");
                writer.WriteLine("        public {0}(Byte[] array, Int32 offset, Int32 offsetLimit)", objectDefinition.name);
                writer.WriteLine("        {");
                writer.WriteLine("            Int32 newOffset = Reflector.Deserialize(this, array, offset, offsetLimit);");
                writer.WriteLine("            if(newOffset != offsetLimit) throw new FormatException(String.Format(");
                writer.WriteLine("                \"Expected packet '{0}' to be {{1}} bytes but was {{2}} bytes\", offsetLimit - offset, newOffset - offset));", objectDefinition.name);
                writer.WriteLine("        }");
                writer.WriteLine("        public {0}(Byte[] array, Int32 offset, Int32 offsetLimit, out Int32 newOffset)", objectDefinition.name);
                writer.WriteLine("        {");
                writer.WriteLine("            newOffset = Reflector.Deserialize(this, array, offset, offsetLimit);");
                writer.WriteLine("        }");
                */
                /*
                //
                // Print Deserializer Method
                //
                if (fixedSerializationLength >= 0)
                {
                    writer.WriteLine("        public override {0} Deserialize(Byte[] array, Int32 offset)", objectDefinition.name);
                    writer.WriteLine("        {");
                    writer.WriteLine("            return new {0}(array, offset, offset + FixedSerializationLength);", objectDefinition.name);
                    writer.WriteLine("        }");
                }
                else
                {
                    writer.WriteLine("        public override Int32 Deserialize(Byte[] array, Int32 offset, Int32 offsetLimit, out {0} outInstance)", objectDefinition.name);
                    writer.WriteLine("        {");
                    writer.WriteLine("            Int32 newOffset;");
                    writer.WriteLine("            outInstance = new {0}(array, offset, offsetLimit, out newOffset);", objectDefinition.name);
                    writer.WriteLine("            return newOffset;");
                    writer.WriteLine("        }");
                }
                */



                //
                // Print Serialization Methods
                //
                /*
                if (fixedSerializationLength >= 0)
                {
                    writer.WriteLine("        public const Int32 FixedSerializationLength = {0};", fixedSerializationLength);
                    writer.WriteLine("        public static void FixedLengthSerialize(Byte[] array, Int32 offset, {0} instance)", objectDefinition.name);
                    writer.WriteLine("        {");
                    writer.WriteLine("            Int32 serializationLength = Reflector.Serialize(instance, array, offset) - FixedSerializationLength;");
                    writer.WriteLine("            if(offset != serializationLength) throw new InvalidOperationException(String.Format(");
                    writer.WriteLine("                \"Expected serialization length to be {0} but was {1}\",");
                    writer.WriteLine("                FixedSerializationLength, serializationLength));");
                    writer.WriteLine("        }");
                }
                else
                {
                    writer.WriteLine("        public static Int32 SerializationLength({0} obj)", objectDefinition.name);
                    writer.WriteLine("        {");
                    writer.WriteLine("            return Reflector.SerializationLength(obj);");
                    writer.WriteLine("        }");
                    writer.WriteLine("        public static Int32 DynamicLengthSerialize(Byte[] array, Int32 offset, {0} instance)", objectDefinition.name);
                    writer.WriteLine("        {");
                    writer.WriteLine("            return Reflector.Serialize(instance, array, offset);");
                    writer.WriteLine("        }");
                }
                writer.WriteLine("        public static void DataString({0} instance, StringBuilder builder)", objectDefinition.name);
                writer.WriteLine("        {");
                writer.WriteLine("            Reflector.DataString(instance, builder);");
                writer.WriteLine("        }");
                */

                //
                // Print serializer adapater factory method
                //
                if (fixedSerializationLength >= 0)
                {
                    writer.WriteLine("        public FixedLengthInstanceSerializerAdapter<{0}> CreateSerializerAdapater()", objectDefinition.name);
                    writer.WriteLine("        {");
                    writer.WriteLine("            return new FixedLengthInstanceSerializerAdapter<{0}>(Serializer, this);", objectDefinition.name);
                    writer.WriteLine("        }");
                }
                else
                {
                    writer.WriteLine("        public InstanceSerializerAdapter<{0}> CreateSerializerAdapater()", objectDefinition.name);
                    writer.WriteLine("        {");
                    writer.WriteLine("            return new InstanceSerializerAdapter<{0}>(Serializer, this);", objectDefinition.name);
                    writer.WriteLine("        }");
                }




                writer.WriteLine("    }");
            }

            writer.WriteLine("}");
        }
    }
}
