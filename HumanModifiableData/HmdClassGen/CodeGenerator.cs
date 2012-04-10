using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace Marler.Hmd
{
    public class CodeTypeNameTable
    {
        private readonly Dictionary<HmdBlockIDProperties, String> typeDictionary;
        private readonly Dictionary<HmdEnum, String> enumDictionary;

        public CodeTypeNameTable()
        {
            this.typeDictionary = new Dictionary<HmdBlockIDProperties, String>();
            this.enumDictionary = new Dictionary<HmdEnum, String>();
        }

        public String GetTypeName(HmdBlockIDProperties blockIDProperties)
        {
            String typeName;
            if (!typeDictionary.TryGetValue(blockIDProperties, out typeName))
            {
                typeName = GenerateBlockIDTypeName(blockIDProperties);
                typeDictionary.Add(blockIDProperties, typeName);
            }
            return typeName;
        }

        public String GetTypeName(HmdEnum hmdEnum)
        {
            String typeName;
            if (!enumDictionary.TryGetValue(hmdEnum, out typeName))
            {
                typeName = GenerateEnumIDTypeName(hmdEnum);
                enumDictionary.Add(hmdEnum, typeName);
            }
            return typeName;
        }

        private String GenerateEnumIDTypeName(HmdEnum hmdEnum)
        {
            String nameWithoutRoot = (hmdEnum.name[0] == '%') ? hmdEnum.name.Substring(6) : hmdEnum.name;
            return nameWithoutRoot.Replace(".", String.Empty);
        }

        private String GenerateBlockIDTypeName(HmdBlockIDProperties blockIDProperties)
        {
            return blockIDProperties.idWithContext.Replace(".", String.Empty);
        }
    }

    public class CodeGenerator
    {
        private readonly CodeTypeNameTable typeNameTable;
        private readonly ILanguageGenerator languageGenerator;
        private readonly TextWriter output;

        private readonly String @namespace;
        private readonly String rootClassName;
        private readonly String hmdTypePrefix;

        public CodeGenerator(ILanguageGenerator languageGenerator, TextWriter output,
            String @namespace, String rootClassName, String hmdTypePrefix)
        {
            this.typeNameTable = new CodeTypeNameTable();
            this.languageGenerator = (languageGenerator == null) ? CSharpLanguageGenerator.Instance : languageGenerator;
            this.output = (output == null) ? Console.Out : output;

            this.@namespace = @namespace;
            this.rootClassName = rootClassName;
            this.hmdTypePrefix = hmdTypePrefix;
        }

        public void Generate(HmdProperties hmdProperties)
        {

            int tabs = 0;

            languageGenerator.PrintFileHeader(output, hmdProperties);
            output.WriteLine();
            output.WriteLine(tabs, "namespace {0}", @namespace);
            output.WriteLine(tabs++, "{");
            //
            // Print Enum Values
            //
            List<HmdEnum> hmdEnumList = hmdProperties.EnumList;
            if (hmdEnumList != null)
            {
                foreach (HmdEnum hmdEnum in hmdEnumList)
                {
                    output.Write(tabs, "public enum {0} {{", typeNameTable.GetTypeName(hmdEnum));
                    int i;
                    for (i = 0; i < hmdEnum.ValueCount - 1; i++)
                    {
                        output.Write(hmdEnum.GetValue(i));
                        output.Write(", ");
                    }
                    output.Write(hmdEnum.GetValue(i));
                    output.WriteLine("};");
                }
            }
            //
            // Define Classes
            //
            foreach (HmdBlockIDProperties blockProperties in hmdProperties.blockIDTable.definitionDictionary.Values)
            {
                GenerateParserClasses(output, blockProperties, hmdProperties, hmdTypePrefix, false);
            }
            GenerateParserClasses(output, hmdProperties.root, hmdProperties, hmdTypePrefix, true);

            output.WriteLine(--tabs, "}");
        }


        public void GenerateParserClasses(TextWriter output, HmdBlockIDProperties block,
            HmdProperties hmdProperties, String hmdTypePrefix, Boolean isRoot)
        {
            String className = isRoot ? rootClassName : (hmdTypePrefix + typeNameTable.GetTypeName(block));

            //
            // Generate the class for the current block
            //
            int tabs = 1;
            output.WriteLine(tabs, "public class {0}", className);
            output.WriteLine(tabs++, "{");

            //
            // Print Fields
            //
            foreach (HmdIDProperties childIDProperties in block)
            {                
                if (childIDProperties.isBlock)
                {
                    HmdBlockIDProperties childBlockProperties = childIDProperties.CastAsBlockIDProperties;

                    String type = hmdTypePrefix + typeNameTable.GetTypeName(childBlockProperties);

                    if (childBlockProperties.CountProperty.Multiple)
                    {
                        type = languageGenerator.ListType(type);
                    }

                    output.WriteLine(tabs, "public {0} {1};", type, childBlockProperties.idOriginalCase);
                }
                else
                {
                    HmdValueIDProperties childValueProperties = childIDProperties.CastAsValueIDProperties;
                    if (childValueProperties.hmdType != HmdType.Empty)
                    {
                        String type;
                        if (childValueProperties.hmdType == HmdType.Enumeration)
                        {
                            HmdEnum hmdEnum = childValueProperties.EnumReference.TryGetReference;
                            if (hmdEnum == null)
                            {
                                hmdEnum = hmdProperties.TryGetEnum(childValueProperties.EnumReference.Name);
                                if (hmdEnum == null)
                                {
                                    throw new FormatException(String.Format("Can't resolve enum reference '{0}'", childValueProperties.EnumReference.Name));
                                }
                                childValueProperties.ResolveEnumReference(hmdEnum);
                            }
                            type = typeNameTable.GetTypeName(hmdEnum);
                        }
                        else
                        {
                            type = languageGenerator.HmdTypeToLanguageType(childValueProperties.hmdType);
                        }

                        if(childValueProperties.CountProperty.Multiple)
                        {
                            type = languageGenerator.ListType(type);
                        }
                        output.WriteLine(tabs, "public {0} {1};", type, childValueProperties.idOriginalCase);
                    }
                }
            }

            //
            // Print Constructor
            //
            output.WriteLine(tabs, "public {0}(HmdBlockID blockID, HmdProperties hmdProperties)", className);
            output.WriteLine(tabs++, "{");
            output.WriteLine(tabs  , "for(int i = 0; i < blockID.ChildCount; i++)");
            output.WriteLine(tabs++, "{");
            output.WriteLine(tabs  , "HmdID childID = blockID.GetChild(i);");
            output.WriteLine(tabs  , "if(childID.isBlock)");
            output.WriteLine(tabs++, "{");
            output.WriteLine(tabs  , "HmdBlockID childBlockID = (HmdBlockID)childID;");
            Int32 blockChildCount = 0;
            foreach (HmdIDProperties childIDProperties in block)
            {
                if (childIDProperties.isBlock)
                {
                    HmdBlockIDProperties childBlockProperties = childIDProperties.CastAsBlockIDProperties;
                    blockChildCount++;
                    
                    output.WriteLine(tabs, "// parse field {0}", childIDProperties.idOriginalCase);
                    output.WriteLine(tabs, "{0}if(childBlockID.idLowerCase.Equals(\"{1}\",StringComparison.CurrentCultureIgnoreCase))", blockChildCount > 1 ? "else " : String.Empty, childIDProperties.idLowerCase);
                    output.WriteLine(tabs++, "{");
                    if (childIDProperties.CountProperty.Multiple)
                    {
                        output.WriteLine(tabs, "// set List to not null");
                        output.WriteLine(tabs, "this.{0}.Add(new {1}{2}(childBlockID, hmdProperties));",
                            childIDProperties.idOriginalCase, hmdTypePrefix, typeNameTable.GetTypeName(childBlockProperties));
                    }
                    else
                    {
                        output.WriteLine(tabs, "// check that field is not set already");
                        output.WriteLine(tabs, "if(this.{0} != null)", childIDProperties.idOriginalCase);
                        output.WriteLine(tabs++, "{");
                        output.WriteLine(tabs, "throw new FormatException(\"Found multiple block id's \\\"{0}\\\"\");", childIDProperties.idOriginalCase);
                        output.WriteLine(--tabs, "}");
                        output.WriteLine(tabs, "this.{0} = new {1}{2}(childBlockID, hmdProperties);",
                            childIDProperties.idOriginalCase, hmdTypePrefix, typeNameTable.GetTypeName(childBlockProperties));
                    }
                    output.WriteLine(--tabs, "}");  
                }
            }
            if (blockChildCount > 0)
            {
                output.WriteLine(tabs, "else");
                output.WriteLine(tabs++, "{");
            }
            output.WriteLine(tabs, "throw new FormatException(String.Format(\"Unrecognized child block id \\\"{0}\\\"\",childID.idOriginalCase));");
            if (blockChildCount > 0)
            {
                output.WriteLine(--tabs, "}");
            }
            output.WriteLine(--tabs, "}");
            output.WriteLine(tabs, "else");
            output.WriteLine(tabs++, "{");
            output.WriteLine(tabs  , "HmdValueID childValueID = (HmdValueID)childID;");
            Int32 valueChildCount = 0;
            foreach (HmdIDProperties childIDProperties in block)
            {
                if (!childIDProperties.isBlock)
                {
                    HmdValueIDProperties childValueIDProperties = childIDProperties.CastAsValueIDProperties;
                    valueChildCount++;
                    output.WriteLine(tabs  , "// parse field {0}", childIDProperties.idOriginalCase);
                    output.WriteLine(tabs  , "{0}if(childValueID.idLowerCase.Equals(\"{1}\",StringComparison.CurrentCultureIgnoreCase))", valueChildCount > 1 ? "else " : String.Empty, childIDProperties.idLowerCase);
                    output.WriteLine(tabs++, "{");

                    String variableName = "childValueID.value";
                    String parseCode = null;
                    if (childValueIDProperties.hmdType == HmdType.Enumeration)
                    {
                        parseCode = languageGenerator.GenerateStringToEnumParseCode(
                            typeNameTable.GetTypeName(childValueIDProperties.EnumReference.TryGetReference),
                            variableName);
                    }
                    else
                    {
                        parseCode = languageGenerator.GenerateStringToTypeParseCode(
                            childValueIDProperties.hmdType,
                            variableName);
                    }

                    if (childIDProperties.CountProperty.Multiple)
                    {
                        output.WriteLine(tabs, "this.{0}.Add({1});", childIDProperties.idOriginalCase,parseCode);
                    }
                    else
                    {
                        output.WriteLine(tabs  , "// check that field is not set already");
                        output.WriteLine(tabs  , "if(this.{0} != null)", childIDProperties.idOriginalCase);
                        output.WriteLine(tabs++, "{");
                        output.WriteLine(tabs  , "throw new FormatException(\"Found multiple value id's \\\"{0}\\\"\");", childIDProperties.idOriginalCase);
                        output.WriteLine(--tabs, "}");
                        output.WriteLine(tabs  , "this.{0} = {1};", childIDProperties.idOriginalCase,parseCode);
                    }
                    output.WriteLine(--tabs, "}");                   
                }
            }
            if (valueChildCount > 0)
            {
                output.WriteLine(tabs  , "else");
                output.WriteLine(tabs++, "{");
            }
            output.WriteLine(tabs  , "throw new FormatException(String.Format(\"Unrecognized child value id \\\"{0}\\\"\",childID.idOriginalCase));");
            if(valueChildCount > 0)
            {
                output.WriteLine(--tabs, "}");
            }
            output.WriteLine(--tabs, "}");

            output.WriteLine(--tabs, "}");
            output.WriteLine(--tabs, "}");
            output.WriteLine(--tabs, "}");
        }

    }
}
