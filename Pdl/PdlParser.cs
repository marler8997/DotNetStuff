using System;
using System.Collections.Generic;
using System.IO;

namespace More.Pdl
{
    class ParseException : Exception
    {
        public ParseException(LfdLine line, String reason)
            : base(String.Format("Parse Error (line {0}): {1}\n at line: \"{2}\"",
                line.actualLineNumber, reason, line.ToString()))
        {
        }
        public ParseException(LfdLine line, String reasonFmt, params Object[] reasonObj)
            : this(line, String.Format(reasonFmt, reasonObj))
        {
        }
        public ParseException(LfdReader reader, String reason)
            : base(String.Format("Parse Error (line {0}): {1}", reader.LineNumber, reason))
        {
        }
    }

    public static class PdlFileParser
    {
        static void VerifyFieldCount(LfdLine line, Int32 fieldCount)
        {
            int lineFieldCount = (line.fields == null) ? 0 : line.fields.Length;
            if (lineFieldCount != fieldCount) throw new ParseException(line, "Expected line to have {0} fields but had {1}", fieldCount, lineFieldCount);
        }
        static void VerifyMinFieldCount(LfdLine line, Int32 minFieldCount)
        {
            int lineFieldCount = (line.fields == null) ? 0 : line.fields.Length;
            if (lineFieldCount < minFieldCount) throw new ParseException(line, "Expected line to have at least {0} fields but had {1}", minFieldCount, lineFieldCount);
        }
        static void Debug(String message)
        {
            //Console.Error.WriteLine("[DEBUG] " + message);
            //Console.Out.WriteLine("[DEBUG] " + message);
        }
        static void Debug(String fmt, params Object[] obj)
        {
            Debug(String.Format(fmt, obj));
        }
        public static void ParseObjectFieldLine(PdlFile pdlFile, LfdReader reader, ObjectDefinition currentObjectDefinition, LfdLine fieldLine, out LfdLine nextLine)
        {
            //
            // Check if it is only a definition (enum or flag)
            //
            if (fieldLine.idLowerInvariantCase.Equals("enum"))
            {
                ParseEnumOrFlagsDefinition(pdlFile, reader, currentObjectDefinition, fieldLine, out nextLine, false);
                return;
            }
            if (fieldLine.idLowerInvariantCase.Equals("flags"))
            {
                ParseEnumOrFlagsDefinition(pdlFile, reader, currentObjectDefinition, fieldLine, out nextLine, true);
                return;
            }

            String typeString = fieldLine.idOriginalCase;
            String typeStringLowerInvariant = fieldLine.idLowerInvariantCase;

            //
            // The rest of the fields can have arrayParse the Array Size Type
            //
            PdlArrayType arrayType;

            int indexOfOpenBracket = typeString.IndexOf('[');
            if(indexOfOpenBracket < 0)
            {
                arrayType = null;
            }
            else
            {
                String arraySizeTypeString = typeString.Substring(indexOfOpenBracket + 1);

                typeString = typeString.Remove(indexOfOpenBracket);
                typeStringLowerInvariant = typeStringLowerInvariant.Remove(indexOfOpenBracket);

                int indexOfCloseBracket = arraySizeTypeString.IndexOf(']');
                if (indexOfCloseBracket < 0)
                    throw new ParseException(fieldLine, "Found an opening bracket '[' without a closing bracket");
                if (indexOfCloseBracket != arraySizeTypeString.Length - 1)
                    throw new ParseException(fieldLine, "The array size type '{0}' had a closing bracket, but the closing bracket was not the last character", arraySizeTypeString);

                arraySizeTypeString = arraySizeTypeString.Remove(indexOfCloseBracket);
                arrayType = PdlArrayType.Parse(fieldLine, arraySizeTypeString);
            }

            //
            // Parse object inline definition
            //
            if (typeStringLowerInvariant.Equals("object"))
            {
                VerifyFieldCount(fieldLine, 1);

                String objectDefinitionAndFieldName = fieldLine.fields[0];

                ObjectDefinition fieldObjectDefinition = ParseObjectDefinition(pdlFile, reader, fieldLine, currentObjectDefinition,
                    objectDefinitionAndFieldName, out nextLine);

                currentObjectDefinition.Add(new ObjectDefinitionField(
                    new ObjectTypeReference(objectDefinitionAndFieldName, fieldObjectDefinition, arrayType),
                    objectDefinitionAndFieldName));
                return;
            }

            //
            // Check if it is a serializer
            //
            if (fieldLine.idLowerInvariantCase.Equals("serializer"))
            {
                VerifyFieldCount(fieldLine, 2);
                String serializerLengthTypeString = fieldLine.fields[0];
                String serializerFieldName = fieldLine.fields[1];
                PdlArrayType serializerLengthType = PdlArrayType.Parse(fieldLine, serializerLengthTypeString);

                currentObjectDefinition.Add(new ObjectDefinitionField(new SerializerTypeReference(serializerLengthType, arrayType), serializerFieldName));

                nextLine = reader.ReadLineIgnoreComments();
                return;
            }

            //
            // The field is only one line, so read the next line now for the caller
            //
            nextLine = reader.ReadLineIgnoreComments();



            EnumOrFlagsDefinition enumDefinition = pdlFile.TryGetEnumOrFlagsDefinition(currentObjectDefinition, typeStringLowerInvariant);
            if (enumDefinition != null)
            {
                VerifyFieldCount(fieldLine, 1);
                currentObjectDefinition.Add(new ObjectDefinitionField(new EnumOrFlagsTypeReference(typeString, enumDefinition, arrayType),
                    fieldLine.fields[0]));
                return;
            }

            // Check if it is an object type
            ObjectDefinition objectDefinition = pdlFile.TryGetObjectDefinition(currentObjectDefinition, typeStringLowerInvariant);
            if (objectDefinition != null)
            {
                if (fieldLine.fields == null || fieldLine.fields.Length <= 0)
                {
                    //
                    // Add each field from the object definition to the current object definition
                    //
                    List<ObjectDefinitionField> objectDefinitionFields = objectDefinition.Fields;
                    for (int i = 0; i < objectDefinitionFields.Count; i++)
                    {
                        ObjectDefinitionField fieldDefinition = objectDefinitionFields[i];
                        currentObjectDefinition.Add(fieldDefinition);
                    }
                }
                else if(fieldLine.fields.Length == 1)
                {
                    currentObjectDefinition.Add(new ObjectDefinitionField(
                        new ObjectTypeReference(typeString, objectDefinition, arrayType), fieldLine.fields[0]));
                }
                else
                {
                    throw new ParseException(fieldLine, "Expected line to have 0 or 1 fields but had {0}", fieldLine.fields.Length);
                }                
                return;
            }

            //
            // Check if it a string type
            //
            if (typeStringLowerInvariant.Equals("ascii"))
            {
                VerifyFieldCount(fieldLine, 1);
                currentObjectDefinition.Add(new ObjectDefinitionField(
                    new AsciiTypeReference(typeString, arrayType), fieldLine.fields[0]));
                return;
            }

            //
            // It must be an integer type
            //
            VerifyFieldCount(fieldLine, 1);

            PdlType type = PdlTypeExtensions.ParseIntegerType(typeString);
            currentObjectDefinition.Add(new ObjectDefinitionField(new IntegerTypeReference(type, arrayType), fieldLine.fields[0]));
        }




        static void ParseEnumOrFlagsDefinition(PdlFile pdlFile, LfdReader reader, ObjectDefinition currentObjectDefinition, LfdLine enumDefinitionLine, out LfdLine nextLine, Boolean isFlagsDefinition)
        {
            String enumOrFlagsString = isFlagsDefinition ? "Flags" : "Enum";

            VerifyFieldCount(enumDefinitionLine, 2);

            String underlyingIntegerTypeString = enumDefinitionLine.fields[0];
            PdlType underlyingIntegerType = PdlTypeExtensions.ParseIntegerType(underlyingIntegerTypeString);

            EnumOrFlagsDefinition definition;
            try { definition = new EnumOrFlagsDefinition(pdlFile, isFlagsDefinition, currentObjectDefinition,
                underlyingIntegerType, enumDefinitionLine.fields[1]); }
            catch (FormatException e) { throw new ParseException(enumDefinitionLine, e.Message); }

            Debug("  Entering {0} '{1}' (IntegerType={2})", enumOrFlagsString, enumDefinitionLine.idOriginalCase, definition.underlyingIntegerType);

            //
            // Read enum values
            //
            nextLine = reader.ReadLineIgnoreComments();
            while (nextLine != null && nextLine.parent == enumDefinitionLine)
            {
                LfdLine enumValueLine = nextLine;
                VerifyFieldCount(enumValueLine, 1);
                Debug("    {0} {1} {2}", enumOrFlagsString, enumValueLine.idOriginalCase, enumValueLine.fields[0]);

                if (isFlagsDefinition)
                {
                    definition.Add(new FlagsValueDefinition(enumValueLine.fields[0], Byte.Parse(enumValueLine.idOriginalCase)));
                }
                else
                {
                    definition.Add(new EnumValueDefinition(enumValueLine.idOriginalCase, enumValueLine.fields[0]));
                }

                nextLine = reader.ReadLineIgnoreComments();
            }
            Debug("  Exiting {0} '{1}'", enumOrFlagsString, definition.typeName);
        }


        public static PdlFile ParsePdlFile(String filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ParsePdlFile(stream);
            }
        }
        public static PdlFile ParsePdlFile(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                return ParsePdlFile(reader);
            }
        }
        public static PdlFile ParsePdlFile(TextReader reader)
        {
            using (LfdReader lfdReader = new LfdReader(reader))
            {
                return ParsePdlFile(lfdReader);
            }
        }



        static ObjectDefinition ParseObjectDefinition(PdlFile pdlFile, LfdReader reader, LfdLine objectDefinitionLine, ObjectDefinition currentObjectDefinition, String objectDefinitionName, out LfdLine nextLine)
        {
            ObjectDefinition objectDefinition = new ObjectDefinition(pdlFile, objectDefinitionName,
                objectDefinitionName.ToLowerInvariant(), currentObjectDefinition);

            Debug("Entering Object Definition '{0}'", objectDefinition.name);

            nextLine = reader.ReadLineIgnoreComments();
            while (nextLine != null && nextLine.parent == objectDefinitionLine)
            {
                ParseObjectFieldLine(pdlFile, reader, objectDefinition, nextLine, out nextLine);
            }

            objectDefinition.CalculateFixedSerializationLength();

            return objectDefinition;
        }
        public static PdlFile ParsePdlFile(LfdReader reader)
        {
            PdlFile pdlFile = new PdlFile();
            ParsePdlFile(pdlFile, reader);
            return pdlFile;
        }
        public static void ParsePdlFile(PdlFile pdlFile, LfdReader reader)
        {
            LfdLine nextLine = reader.ReadLineIgnoreComments();
            while (nextLine != null)
            {
                if (nextLine.idLowerInvariantCase.Equals("enum", StringComparison.InvariantCulture))
                {
                    ParseEnumOrFlagsDefinition(pdlFile, reader, null, nextLine, out nextLine, false);
                }
                else if (nextLine.idLowerInvariantCase.Equals("flags"))
                {
                    ParseEnumOrFlagsDefinition(pdlFile, reader, null, nextLine, out nextLine, true);
                }
                else
                {
                    LfdLine currentCommandLine = nextLine;

                    VerifyFieldCount(currentCommandLine, 0);
                    ObjectDefinition objectDefinition = ParseObjectDefinition(pdlFile, reader, currentCommandLine, null, currentCommandLine.idOriginalCase, out nextLine);
                }
            }
        }
    }
}