using System;
using System.Collections.Generic;
using System.IO;

namespace Marler.Hmd
{

    public static class HmdFileParser
    {
        public static void Parse(HmdBlockID root, String filename, HmdProperties hmdProperties)
        {
            Parse(root, filename, Path.GetDirectoryName(filename), hmdProperties);
        }

        public static void Parse(HmdBlockID root, String filename, String importPath, HmdProperties hmdProperties)
        {
            using (FileStream fileStream = new FileStream(filename, FileMode.Open))
            {
                Parse(root, fileStream, importPath, hmdProperties);
            }
        }

        public static void Parse(HmdBlockID root, Stream stream, String importPath, HmdProperties hmdProperties)
        {
            using (StreamReader fileStream = new StreamReader(stream))
            {
                Parse(root, fileStream, importPath, hmdProperties);
            }
        }

        public static void Parse(HmdBlockID root, TextReader reader, String importPath, HmdProperties hmdProperties)
        {
            Parse(root, new HmdTokenizer(reader, 1), importPath, hmdProperties);
        }

        public static void Parse(HmdBlockID root, HmdTokenizer tokenizer,
            String importPath, HmdProperties hmdProperties)
        {
            if (root == null) throw new ArgumentNullException("root");

            TextWriter debugOutput = HmdDebug.DebugOutput;
            if (debugOutput == null) debugOutput = TextWriter.Null;

            if (importPath == null)
            {
                importPath = String.Empty;
            }

            HmdBlockID parentID = root;
            Stack<HmdBlockID> parentIDStack = new Stack<HmdBlockID>();

            while (true)
            {
                HmdGlobalToken token = tokenizer.NextGlobalToken();

                //
                // Check for EOF
                //
                if (token.type == HmdGlobalTokenType.EOF)
                {
                    debugOutput.WriteLine("EOF");
                    break;
                }

                if (token.type == HmdGlobalTokenType.ID)
                {
                    String idString = token.text;

                    //
                    // Normal Parse
                    //
                    Boolean isBlockID;
                    Boolean isEmptyID = tokenizer.NextIDType(out isBlockID);

                    if (isBlockID)
                    {
                        debugOutput.WriteLine("Block ID  : {0}", idString);

                        parentIDStack.Push(parentID);
                        HmdBlockID temp = parentID;
                        parentID = new HmdBlockID(idString, temp);
                    }
                    else
                    {
                        debugOutput.WriteLine("Value ID  : {0}", idString);

                        if (isEmptyID)
                        {
                            parentID.AddChild(new HmdValueID(idString, null, parentID));
                        }
                        else
                        {
                            String nextValue = tokenizer.NextValue();
                            debugOutput.WriteLine("Value     : {0}", nextValue);

                            parentID.AddChild(new HmdValueID(idString, nextValue, parentID));
                        }
                    }
                }
                else if (token.type == HmdGlobalTokenType.Directive)
                {
                    String directiveID = token.text;

                    //
                    // Handling Directives
                    //
                    Boolean isBlockID;
                    Boolean isEmptyID = tokenizer.NextIDType(out isBlockID);

                    if (isEmptyID)
                    {
                        throw new NotImplementedException();
                    }
                    else if (!isBlockID)
                    {
                        if (directiveID.Equals("import", StringComparison.CurrentCulture) || directiveID.Equals("pimport", StringComparison.CurrentCulture))
                        {
                            Boolean isPimport = directiveID[0] == 'p';

                            String nextValue = tokenizer.NextValue();

                            debugOutput.WriteLine("{0}  : {1}", isPimport ? "%PImport" : "%Import ", nextValue);

                            if (isPimport && hmdProperties == null)
                            {
                                debugOutput.WriteLine("%PImport  : Ignoring props, so skipping this %pimport");
                            }
                            else
                            {
                                String importFileAndPathName = Path.Combine(importPath, nextValue);

                                using (FileStream importStream = new FileStream(importFileAndPathName, FileMode.Open))
                                {
                                    debugOutput.WriteLine("File Start: {0}", importFileAndPathName);
                                    if (isPimport)
                                    {
                                        ParsePropertiesFile(hmdProperties.root, hmdProperties, 
                                            new HmdTokenizer(new StreamReader(importStream), 1), importPath, true);
                                    }
                                    else
                                    {
                                        Parse(parentID, new HmdTokenizer(new StreamReader(importStream), 1), importPath, hmdProperties);
                                    }
                                    debugOutput.WriteLine("File End  : {0}", importFileAndPathName);
                                }
                            }
                        }
                        else if (directiveID.Equals("enum", StringComparison.CurrentCulture))
                        {
                            throw new FormatException("%enum value directive must be in a %props block directive");
                        }
                        else if (directiveID.Equals("props", StringComparison.CurrentCulture))
                        {
                            throw new FormatException("%props value directive must be in a %props block directive");
                        }
                        else
                        {
                            throw new Exception(String.Format("Parser (line {0}): Unrecognized value directive \"{1}\"",
                                token.line, directiveID));
                        }
                    }
                    else
                    {
                        if (directiveID.Equals("props", StringComparison.CurrentCulture))
                        {
                            debugOutput.WriteLine("%Props Blk:");

                            if (hmdProperties == null)
                            {
                                debugOutput.WriteLine("Skipping %props block...");
                                IgnoreCurrentBlock(debugOutput, tokenizer);
                            }
                            else
                            {
                                ParsePropertiesFile(hmdProperties.root, hmdProperties, tokenizer, importPath,false);
                            }
                        }
                        else if (directiveID.Equals("group", StringComparison.CurrentCulture))
                        {
                            throw new FormatException("%group value directive must be in a %props block directive");
                        }
                        else
                        {
                            throw new Exception(String.Format("Parser (line {0}): Unrecognized block directive \"{1}\"",
                                token.line, directiveID));
                        }
                    }
                }
                else if (token.type == HmdGlobalTokenType.CloseBrace)
                {
                    if (parentIDStack.Count <= 0)
                    {
                        throw new FormatException(String.Format("Parser (line {0}): Unmatched close brace {1}", token.line, token));
                    }
                    debugOutput.WriteLine("Block End : {0}", parentID.idOriginalCase);

                    HmdBlockID temp = parentID;
                    parentID = parentIDStack.Pop();
                    parentID.AddChild(temp);
                }
                else
                {
                    throw new FormatException(String.Format("Parser (line {0}): Unexpected token {1}", token.line, token));
                }
            }

            if (parentIDStack.Count > 0)
            {
                throw new FormatException(String.Format("Parser (EOF): Block \"{0}\" was not ended with '}'", parentIDStack.Peek().idOriginalCase));
            }
        }


        private static void IgnoreCurrentBlock(TextWriter debugOutput, HmdTokenizer tokenizer)
        {
            HmdGlobalToken token;
            Int32 ignoreStackDepth = 0;
            while (true)
            {
                token = tokenizer.NextGlobalToken();

                // Check for EOF
                if (token.type == HmdGlobalTokenType.EOF)
                {
                    throw new FormatException(String.Format("Parser (line {0}): Got EOF in the middle of a %props block",
                        token.line));
                }
                if (token.type == HmdGlobalTokenType.ID)
                {
                    String idString = token.text;
                    Boolean isBlockID;
                    Boolean isEmptyID = tokenizer.NextIDType(out isBlockID);

                    if (isEmptyID)
                    {
                        debugOutput.WriteLine("Value ID  : {0} (IGNORED)", idString);
                    }
                    else if (isBlockID)
                    {
                        debugOutput.WriteLine("Block ID  : {0}", idString);
                        ignoreStackDepth++;
                    }
                    else
                    {
                        debugOutput.WriteLine("Value ID  : {0} (IGNORED)", idString);

                        String nextValue = tokenizer.NextValue();
                        debugOutput.WriteLine("Value     : {0} (IGNORED)", nextValue);
                    }
                }
                else if (token.type == HmdGlobalTokenType.Directive)
                {
                    String directiveIDString = token.text;
                    Boolean isBlockID;
                    Boolean isEmptyID = tokenizer.NextIDType(out isBlockID);

                    if (isEmptyID)
                    {
                        debugOutput.WriteLine("Value ID  : {0} (IGNORED)", directiveIDString);
                    }
                    else if (isBlockID)
                    {
                        debugOutput.WriteLine("Block ID  : {0}", directiveIDString);
                        ignoreStackDepth++;
                    }
                    else
                    {
                        debugOutput.WriteLine("Value ID  : {0} (IGNORED)", directiveIDString);

                        String nextValue = tokenizer.NextValue();
                        debugOutput.WriteLine("Value     : {0} (IGNORED)", nextValue);
                    }

                }
                else if (token.type == HmdGlobalTokenType.CloseBrace)
                {
                    if (ignoreStackDepth <= 0)
                    {
                        break;
                    }
                    ignoreStackDepth--;
                }
                else
                {
                    throw new FormatException(String.Format("Parser (line {0}): Unexpected token {1}", token.line, token));
                }

            }
        }


        public static HmdProperties ParsePropertiesFile(String filename, String importPath)
        {
            using (FileStream fileStream = new FileStream(filename, FileMode.Open))
            {
                return ParsePropertiesFile(fileStream, importPath);
            }
        }

        public static HmdProperties ParsePropertiesFile(Stream stream, String importPath)
        {
            using (StreamReader fileStream = new StreamReader(stream))
            {
                return ParsePropertiesFile(fileStream, importPath);
            }
        }

        public static HmdProperties ParsePropertiesFile(TextReader reader, String importPath)
        {
            HmdProperties hmdProperties = new HmdProperties();
            ParsePropertiesFile(hmdProperties.root, hmdProperties, new HmdTokenizer(reader, 0), importPath, true);
            return hmdProperties;
        }


        private static void ParsePropertiesFile(HmdBlockIDProperties propertyBlockRoot, HmdProperties hmdProperties,
            HmdTokenizer tokenizer, String importPath, Boolean isPImportFile)
        {
            TextWriter debugOutput = HmdDebug.DebugOutput;
            if (debugOutput == null) debugOutput = TextWriter.Null;

            if (propertyBlockRoot == null) throw new ArgumentNullException("propertyBlockRoot");

            //
            // TODO: What about pimport files? Should they require a %props block? If not, then can they put it anyways? I'll need to add another argument to this argument list
            //       that will say whether or not it is a PImport so it can gracefully end on EOF instead of '}' (close brace)
            //

            //
            // TODO: MAKE SURE YOU DISALLOW SPECIFYING %PROPS VALUE ID in a BLOCK TWICE!!!
            //
            //
            HmdBlockIDProperties currentParent = propertyBlockRoot;
            Stack<HmdBlockIDProperties> parentStack = new Stack<HmdBlockIDProperties>();

            while (true)
            {
                HmdGlobalToken token = tokenizer.NextGlobalToken();

                //
                // Check for EOF
                //
                if (token.type == HmdGlobalTokenType.EOF)
                {
                    if (isPImportFile && parentStack.Count <= 0)
                    {
                        return;
                    }
                    else
                    {
                        throw new FormatException("Reached EOF inside a %props block directive");
                    }
                }

                if (token.type == HmdGlobalTokenType.ID)
                {
                    String idString = token.text;

                    Boolean isBlockID;
                    Boolean isEmptyID = tokenizer.NextIDType(out isBlockID);

                    if (isEmptyID)
                    {
                        // this just means the value ID has all the defaults
                        debugOutput.WriteLine("EmptyProp ID: {0}", idString);

                        HmdValueIDProperties valueIDProperties = new HmdValueIDProperties(idString, hmdProperties.defaultCountProperty,
                            hmdProperties.defaultHmdType, null, currentParent, null);
                        if (!valueIDProperties.DirectParentIsOverriden)
                        {
                            currentParent.AddDirectChildWithNoParentOverrideList(valueIDProperties);
                        }
                        hmdProperties.AddPropertiesFromDefinition(valueIDProperties);
                    }
                    else if (!isBlockID)
                    {
                        debugOutput.WriteLine("ValProp ID: {0}", idString);

                        String nextValue = tokenizer.NextValue();
                        debugOutput.WriteLine("ValProp   : {0}", nextValue);

                        HmdValueIDProperties valueIDProperties = HmdParser.ParseValueProperties(idString, nextValue, currentParent, hmdProperties);
                        if (!valueIDProperties.DirectParentIsOverriden)
                        {
                            currentParent.AddDirectChildWithNoParentOverrideList(valueIDProperties);
                        }
                        hmdProperties.AddPropertiesFromDefinition(valueIDProperties);
                    }
                    else
                    {
                        debugOutput.WriteLine("BlkProp ID: {0}", idString);

                        HmdBlockIDProperties blockIDProperties = new HmdBlockIDProperties(idString, hmdProperties.defaultCountProperty, currentParent);
                        // wait to add this child to the current parent so we know whether or not it's default parent is overriden
                        hmdProperties.AddPropertiesFromDefinition(blockIDProperties);

                        parentStack.Push(currentParent);
                        currentParent = blockIDProperties;
                    }
                }
                else if (token.type == HmdGlobalTokenType.Directive)
                {
                    String directiveID = token.text;

                    Boolean isBlockID;
                    Boolean isEmptyID = tokenizer.NextIDType(out isBlockID);

                    if (isEmptyID)
                    {
                        throw new NotImplementedException();
                    }
                    else if (!isBlockID)
                    {
                        //
                        // TODO: Props Blocks should not allow %import directives, they should only allow %pimport directives
                        //
                        if (token.text.Equals("import", StringComparison.CurrentCulture))
                        {
                            throw new FormatException("%import directive not allowed inside a %prop block");

                        }
                        else if (token.text.Equals("pimport", StringComparison.CurrentCulture))
                        {
                            String nextValue = tokenizer.NextValue();
                            debugOutput.WriteLine("%PImport  : {0}", nextValue);

                            String importFileAndPathName = Path.Combine(importPath, nextValue);

                            using (FileStream importStream = new FileStream(importFileAndPathName, FileMode.Open))
                            {
                                debugOutput.WriteLine("File Start: {0}", importFileAndPathName);
                                ParsePropertiesFile(currentParent,hmdProperties, new HmdTokenizer(new StreamReader(importStream), 1), importPath, false);
                                debugOutput.WriteLine("File End  : {0}", importFileAndPathName);
                            }
                        }
                        else if (token.text.Equals("enum", StringComparison.CurrentCulture))
                        {
                            String nextValue = tokenizer.NextValue();
                            debugOutput.WriteLine("%Enum     : {0}", nextValue);

                            hmdProperties.AddEnum(new HmdEnum(nextValue));
                        }
                        else if (token.text.Equals("props", StringComparison.CurrentCulture))
                        {
                            if (parentStack.Count <= 0)
                            {
                                throw new FormatException("You can't specify a %props value on the root");
                            }
                            String nextValue = tokenizer.NextValue();
                            debugOutput.WriteLine("%Props    : {0}", nextValue);

                            HmdParser.ParseAndOverrideBlockProperties(currentParent, nextValue);
                        }
                        else
                        {
                            throw new Exception(String.Format("Parser (line {0}): Unrecognized value directive \"{1}\"",
                                token.line, token.text));
                        }
                    }
                    else
                    {
                        if (token.text.Equals("props", StringComparison.CurrentCulture))
                        {
                            throw new FormatException("Right now this is just weird, why do you have a %props block inside a props block (Maybe I'll let this slide later)?");
                            debugOutput.WriteLine("Block ID  : %props Directive");

                            if (hmdProperties == null)
                            {
                                debugOutput.WriteLine("Not Parsing %props Directive Block ID...");
                                throw new NotImplementedException("Haven't implemented the feature to parse without doing the props block");
                            }
                            else
                            {
                                ParsePropertiesFile(hmdProperties.root, hmdProperties, tokenizer, importPath, false);
                            }
                        }
                        else if (token.text.Equals("group", StringComparison.CurrentCulture))
                        {
                            throw new NotImplementedException();
                        }
                        else
                        {
                            throw new Exception(String.Format("Parser (line {0}): Unrecognized block directive \"{1}\"",
                                token.line, token.text));
                        }
                    }
                }
                else if (token.type == HmdGlobalTokenType.CloseBrace)
                {
                    if (parentStack.Count <= 0)
                    {
                        debugOutput.WriteLine("%Props End:");
                        return;
                    }

                    debugOutput.WriteLine("Block End : {0}", currentParent.idOriginalCase);

                    HmdBlockIDProperties temp = currentParent;
                    currentParent = parentStack.Pop();
                    if (!temp.DirectParentIsOverriden)
                    {
                        currentParent.AddDirectChildWithNoParentOverrideList(temp);
                    }

                }
                else
                {
                    throw new FormatException(String.Format("Parser (line {0}): Unexpected token {1}", token.line, token));
                }
            }
        }
    }
}
