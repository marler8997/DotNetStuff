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
        }
        static void Debug(String fmt, params Object[] obj)
        {
            Debug(String.Format(fmt, obj));
        }
        public static TypeReference ParsePacketFieldType(PdlFile pdlFile, LfdReader reader, ObjectDefinition currentObjectDefinition, LfdLine fieldLine, out LfdLine nextLine)
        {
            String typeString = fieldLine.idOriginalCase;

            PdlArraySizeType arraySizeType;

            int indexOfOpenBracket = typeString.IndexOf('[');
            if(indexOfOpenBracket < 0)
            {
                arraySizeType = PdlArraySizeType.NotAnArray;
            }
            else
            {
                String arraySizeTypeString = typeString.Substring(indexOfOpenBracket + 1);
                typeString = typeString.Remove(indexOfOpenBracket);

                int indexOfCloseBracket = arraySizeTypeString.IndexOf(']');
                if (indexOfCloseBracket < 0)
                    throw new ParseException(fieldLine, "Found an opening bracket '[' without a closing bracket");
                if (indexOfCloseBracket != arraySizeTypeString.Length - 1)
                    throw new ParseException(fieldLine, "The array size type '{0}' had a closing bracket, but the closing bracket was not the last character", arraySizeTypeString);

                arraySizeTypeString = arraySizeTypeString.Remove(indexOfCloseBracket);
                if (String.IsNullOrEmpty(arraySizeTypeString))
                {
                    arraySizeType = PdlArraySizeType.BasedOnCommandSize;
                }
                else
                {
                    try
                    {
                        arraySizeType = (PdlArraySizeType)Enum.Parse(typeof(PdlArraySizeType), arraySizeTypeString);
                    }
                    catch (ArgumentException)
                    {
                        throw new ParseException(fieldLine,
                            "The array size type inside the brackets '{0}' is invalid, expected 'Byte','UInt16' or 'UInt32'", arraySizeTypeString);
                    }
                }
            }

            //
            // Parse Object Type
            //
            if (typeString.Equals("Object", StringComparison.InvariantCultureIgnoreCase))
            {
                VerifyFieldCount(fieldLine, 1);

                String objectFieldName = fieldLine.fields[0];
                ObjectDefinition objectTypeDefinition = new ObjectDefinition(objectFieldName, objectFieldName.ToLowerInvariant(), currentObjectDefinition);
                Debug("Entering Object Type Definition '{0}'", objectTypeDefinition.name);

                nextLine = reader.ReadLineIgnoreComments();
                while (nextLine != null && nextLine.parent == fieldLine)
                {
                    VerifyMinFieldCount(nextLine, 1);
                    String fieldName = nextLine.fields[0];
                    TypeReference fieldType = ParsePacketFieldType(pdlFile, reader, currentObjectDefinition, nextLine, out nextLine);
                    objectTypeDefinition.fields.Add(new ObjectDefinitionField(fieldType, fieldName));
                }
                return new ObjectTypeReference(String.Format("{0}.{1}", currentObjectDefinition.name, objectFieldName),
                    objectTypeDefinition, arraySizeType);
            }

            nextLine = reader.ReadLineIgnoreComments(); // Read the next line for the caller

            // Check if it is an enum
            String typeStringLowerInvariant = typeString.ToLowerInvariant();

            EnumOrFlagsDefinition enumDefinition = pdlFile.TryGetDefinition(currentObjectDefinition, typeStringLowerInvariant);
            if (enumDefinition != null)
            {
                return new EnumOrFlagsTypeReference(typeString, enumDefinition, arraySizeType);
            }

            //
            // It must be an integer type
            //
            PdlType type = PdlTypeExtensions.ParseIntegerType(typeString);
            return new IntegerTypeReference(type, arraySizeType);
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
        public static PdlFile ParsePdlFile(LfdReader reader)
        {
            PdlFile pdlFile = new PdlFile();

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
                    ObjectDefinition currentObject = new ObjectDefinition(currentCommandLine.idOriginalCase,
                        currentCommandLine.idLowerInvariantCase, null);
                    Debug("Entering Object '{0}'", currentObject.name);

                    //
                    // Read the command
                    //
                    nextLine = reader.ReadLineIgnoreComments();
                    while (nextLine != null && nextLine.parent == currentCommandLine)
                    {
                        /*
                        if (nextLine.idLowerInvariantCase.Equals("extensions"))
                        {
                            VerifyFieldCount(nextLine, 0);
                            if (currentObject.extensionOffset != -1)
                                throw new ParseException(nextLine, "You've specified Extensions twice in the same command '{0}'", currentObject.name);
                            currentObject.extensionOffset = currentObject.fields.Count;
                            nextLine = reader.ReadLineIgnoreComments();
                        }
                        */
                        if (nextLine.idLowerInvariantCase.Equals("enum"))
                        {
                            ParseEnumOrFlagsDefinition(pdlFile, reader, currentObject, nextLine, out nextLine, false);
                        }
                        else if (nextLine.idLowerInvariantCase.Equals("flags"))
                        {
                            ParseEnumOrFlagsDefinition(pdlFile, reader, currentObject, nextLine, out nextLine, true);
                        }
                        else
                        {
                            VerifyMinFieldCount(nextLine, 1);
                            String fieldName = nextLine.fields[0];
                            TypeReference fieldType = ParsePacketFieldType(pdlFile, reader, currentObject, nextLine, out nextLine);
                            currentObject.fields.Add(new ObjectDefinitionField(fieldType, fieldName));
                        }
                    }

                    Debug("Exiting Object '{0}'", currentObject.name);
                    pdlFile.objectDefinitions.Add(currentObject);
                }
            }

            return pdlFile;
        }
    }
}