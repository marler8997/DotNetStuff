using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Marler.Pdl;

using Antlr.Runtime;
using Antlr.Runtime.Tree;

using More;

namespace PdlCodeGenerator
{
    /*
    class PdlCodeGeneratorOptions : CLParser
    {
        public readonly CLSwitch help;

        public PdlCodeGeneratorOptions()
        {
            help = new CLSwitch('h', "help", "show the usage");
            Add(help);

        }

        public override void PrintUsageHeader()
        {
            Console.WriteLine("PdlCodeGenerator.exe [options] <pdl-file>");
        }
    }

    public static class PdlExtensions
    {
        public static PdlType GetPdlType(this ITree typeNode)
        {
            try
            {
                return (PdlType)typeNode.Type;
            }
            catch (InvalidCastException)
            {
                throw new InvalidOperationException(String.Format(
                    "Internal bug: found a type token of type '{0}' but cannot convert it to the TokenType enum", typeNode.Type));
            }
        }
        public static ArrayLength GetPdlArrayLength(this ITree arrayLengthNode)
        {
            if (arrayLengthNode.Type == PacketDescriptionLanguageLexer.INTEGER)
            {
                String lengthText = arrayLengthNode.Text;
                if (lengthText[0] == '-') throw new InvalidOperationException("An array length cannot be negative");
                return new ArrayLength(Int32.Parse(lengthText));
            }

            switch (arrayLengthNode.Type)
            {
                case PacketDescriptionLanguageLexer.BYTE_TYPE: return new ArrayLength(PdlType.Byte);
                case PacketDescriptionLanguageLexer.USHORT_TYPE: return new ArrayLength(PdlType.UShort);
                case PacketDescriptionLanguageLexer.UINT_TYPE: return new ArrayLength(PdlType.UInt);
                case PacketDescriptionLanguageLexer.ULONG_TYPE: return new ArrayLength(PdlType.ULong);
            }
            throw new InvalidOperationException(String.Format(
                "Expected unsigned integer type (byte,ushort,uint,ulong) or an unsigned integer but got '{0}'", arrayLengthNode.Text));
        }
        public static PdlType GetPdlUnsignedIntegerType(this ITree typeNode)
        {
            switch (typeNode.Type)
            {
                case PacketDescriptionLanguageLexer.BYTE_TYPE: return PdlType.Byte;
                case PacketDescriptionLanguageLexer.USHORT_TYPE: return PdlType.UShort;
                case PacketDescriptionLanguageLexer.UINT_TYPE: return PdlType.UInt;
                case PacketDescriptionLanguageLexer.ULONG_TYPE: return PdlType.ULong;
            }
            throw new InvalidOperationException(String.Format(
                "Expected unsigned integer type (byte,ushort,uint,ulong) but got '{0}'", typeNode.Text));
        }
    }


    public class PdlFormatException : FormatException
    {
        public readonly ITree tree;
        public PdlFormatException(ITree tree, String context, String message)
            : base(String.Format("Context '{0}' on line {1}: {2}: {3}", context, tree.Line, message, tree.ToStringTree()))
        {
            this.tree = tree;
        }
    }

    class PdlFile
    {
    }

    class Program
    {
        static Int32 Main(string[] args)
        {
            PdlCodeGeneratorOptions options = new PdlCodeGeneratorOptions();

            List<String> nonOptionArgs = options.Parse(args);

            if(options.help.set)
            {
                options.PrintUsage();
                return 1;
            }

            if(nonOptionArgs.Count != 1)
            {
                Console.WriteLine("Error: expected exactly 1 non option argument but got {0}", nonOptionArgs.Count);
                options.PrintUsage();
                return 1;
            }

            String fileName = nonOptionArgs[0];

            ANTLRFileStream antlrStream = null;
            try
            {
                antlrStream = new ANTLRFileStream(fileName);

                PacketDescriptionLanguageLexer lexer = new PacketDescriptionLanguageLexer(antlrStream);
                CommonTokenStream tokenStream = new CommonTokenStream(lexer);
                PacketDescriptionLanguageParser parser = new PacketDescriptionLanguageParser(tokenStream);






                AstParserRuleReturnScope<Object,IToken> parserResult = parser.packetDescriptionLanguage();
                CommonTree tree = (CommonTree)parserResult.Tree;

                PdlFile pdlFile = ProcessPdlFileTree(tree);

                pdlFile.PrintPdl(Console.Out);

                return 0;
            }
            finally
            {
                if(antlrStream != null)
                {
                    // dispose of the antlrstream
                }
            }
        }
        static PdlFile ProcessPdlFileTree(CommonTree tree)
        {
            PdlFile pdlFile = new PdlFile();

            IList<ITree> children = tree.Children;

            Console.WriteLine(tree.ToStringTree());

            for (int i = 0; i < children.Count; i++)
            {
                ITree definition = children[i];

                switch (definition.Type)
                {
                    case PacketDescriptionLanguageLexer.ENUM_DEFINITION:
                        SimpleTypeField declaredEnumField = ProcessEnumDefinition(pdlFile, definition);
                        if (declaredEnumField != null)
                            throw new PdlFormatException(definition, "Global Enum Definition", "Semantic Error: a global enum definition cannot declare an enum field");
                        break;
                    case PacketDescriptionLanguageLexer.DATA_BLOCK_DEFINITION:
                        ProcessDataBlockDefinition(pdlFile, definition, false , null);
                        break;
                    case PacketDescriptionLanguageLexer.EOF:
                        break;
                    default:
                        throw new InvalidOperationException(String.Format("Unknown token type {0} text '{1}'", definition.Type, definition.Text));
                }
            }

            return pdlFile;
        }
        // returns a non null name if the enum definition is also declaring an enum field
        static SimpleTypeField ProcessEnumDefinition(IPdlBuilder pdlBuilder, ITree enumDefinition)
        {
            SimpleTypeField enumField = null;
            UnsignedIntegerType? unsignedIntegerType = null;
            EnumValue[] enumValues = null;

            String enumName = enumDefinition.GetChild(0).Text;
            
            Int32 nextChildIndex = 1;
            if (nextChildIndex >= enumDefinition.ChildCount) goto FINISH_DEFINITION;
            ITree nextChild = enumDefinition.GetChild(nextChildIndex);


            //
            // Check if next child is an unsigned integer type
            //
            switch (nextChild.Type)
            {
                case PacketDescriptionLanguageLexer.BYTE_TYPE:
                    unsignedIntegerType = UnsignedIntegerType.Byte;
                    break;
                case PacketDescriptionLanguageLexer.USHORT_TYPE:
                    unsignedIntegerType = UnsignedIntegerType.UShort;
                    break;
                case PacketDescriptionLanguageLexer.UINT_TYPE:
                    unsignedIntegerType = UnsignedIntegerType.UInt;
                    break;
                case PacketDescriptionLanguageLexer.ULONG_TYPE:
                    unsignedIntegerType = UnsignedIntegerType.ULong;
                    break;
                default:
                    unsignedIntegerType = null;
                    break;
            }

            if (unsignedIntegerType != null)
            {
                nextChildIndex++;
                if (nextChildIndex >= enumDefinition.ChildCount) goto FINISH_DEFINITION;
                nextChild = enumDefinition.GetChild(nextChildIndex);
            }
            
            //
            // Check if next child is a declared enum (only works inside data block definitions
            //
            if (nextChild.Type == PacketDescriptionLanguageLexer.ENUM_DECLARATION)
            {
                enumField = new SimpleTypeField(nextChild.GetChild(0).Text, PdlType.UserDefinedType, enumName, false);

                nextChildIndex++;
                if (nextChildIndex >= enumDefinition.ChildCount) goto FINISH_DEFINITION;
                nextChild = enumDefinition.GetChild(nextChildIndex);
            }



            Int32 valueCount = enumDefinition.ChildCount - nextChildIndex;
            enumValues = new EnumValue[valueCount];

            Int32 enumValueArrayIndex = 0;
            while (nextChildIndex < enumDefinition.ChildCount)
            {
                ITree enumValueTree = enumDefinition.GetChild(nextChildIndex);
                ITree enumIntegerValueTree = enumValueTree.GetChild(0);
                String enumIntegerValue = (enumIntegerValueTree == null) ? null : enumIntegerValueTree.Text;

                enumValues[enumValueArrayIndex] = new EnumValue(enumValueTree.Text, enumIntegerValue);

                enumValueArrayIndex++;
                nextChildIndex++;                
            }


            FINISH_DEFINITION:

            pdlBuilder.AddEnumDefinition(new EnumDefinition(enumName, unsignedIntegerType, enumValues));
            return enumField;
        }
        static void ProcessDataBlockDefinition(IPdlBuilder pdlBuilder,
            ITree dataBlockDefinition, Boolean isDataBlockField, ArrayLength fieldArrayLength)
        {                        
            String dataBlockName = dataBlockDefinition.GetChild(0).Text;
            //Console.WriteLine("DEBUG [AST_TO_PDL_TREE] At '{0}'", dataBlockName);

            Int32 fieldTreeIndex = 1;
            Int32 fieldCount = dataBlockDefinition.ChildCount - fieldTreeIndex;
            DataBlockFieldsBuilder dataBlockFieldsBuilder = new DataBlockFieldsBuilder(fieldCount);

            Int32 fieldArrayIndex = 0;
            while (fieldTreeIndex < dataBlockDefinition.ChildCount)
            {
                ITree fieldTree = dataBlockDefinition.GetChild(fieldTreeIndex);
                
                String fieldName;
                ITree typeNode;

                switch (fieldTree.Type)
                {
                    case PacketDescriptionLanguageLexer.ENUM_DEFINITION:

                        SimpleTypeField declaredEnumField = ProcessEnumDefinition(pdlBuilder, fieldTree);
                        if (declaredEnumField != null)
                        {
                            dataBlockFieldsBuilder.AddField(declaredEnumField);
                        }

                        break;
                    case PacketDescriptionLanguageLexer.SIMPLE_TYPE_FIELD:
                        typeNode = fieldTree.GetChild(0);
                        fieldName = fieldTree.GetChild(1).Text;

                        Boolean isOptional = false;

                        // Process type modifiers
                        for(int i = 2; i < fieldTree.ChildCount; i++)
                        {
                            ITree node = fieldTree.GetChild(i);
                            if(node.Type == PacketDescriptionLanguageLexer.OPTIONAL_KEYWORD)
                            {
                                isOptional = true;
                            }
                            else
                            {
                                throw new InvalidOperationException(String.Format("Unknown primitive type modifier '{0}' (TokenType={1})", node.Text, node.Type));
                            }
                        }

                        dataBlockFieldsBuilder.AddField(new SimpleTypeField(fieldName, typeNode.GetPdlType(), typeNode.Text, isOptional));
                        break;
                    case PacketDescriptionLanguageLexer.ARRAY_TYPE_FIELD:
                        typeNode = fieldTree.GetChild(0);
                        fieldName = fieldTree.GetChild(1).Text;
                        ArrayLength arrayLength = fieldTree.GetChild(2).GetPdlArrayLength();

                        dataBlockFieldsBuilder.AddField(new ArrayField(fieldName, typeNode.GetPdlType(), typeNode.Text, arrayLength));

                        break;
                    case PacketDescriptionLanguageLexer.DATA_BLOCK_DEFINITION:
                        ArrayLength dataBlockFieldArrayLength = fieldTree.GetChild(0).GetPdlArrayLength();
                        ITree dataBlockDefinitionTree = fieldTree.GetChild(1);
                        //Console.WriteLine("DataBlockDefinitionField {0}", dataBlockDefinitionTree.ToStringTree());

                        ProcessDataBlockDefinition(dataBlockFieldsBuilder, dataBlockDefinitionTree,
                            true, dataBlockFieldArrayLength);

                        break;
                    default:
                        throw new InvalidOperationException(String.Format("Unknown data block field type '{0}'", fieldTree.Type));
                }

                fieldTreeIndex++;
                fieldArrayIndex++;
            }

            if (isDataBlockField) // means you are adding it to the pdl file, not another data block definition
            {
                pdlBuilder.AddDataBlockDefinitionField(new DataBlockDefinitionField(dataBlockName, dataBlockFieldsBuilder, fieldArrayLength));
            }
            else
            {
                pdlBuilder.AddGlobalDataBlockDefinition(new GlobalDataBlockDefinition(dataBlockName, dataBlockFieldsBuilder));
            }
        }
    }
*/
    public static class TempMain
    {
        public static void Main(String[] args)
        {
        }
    }
}
