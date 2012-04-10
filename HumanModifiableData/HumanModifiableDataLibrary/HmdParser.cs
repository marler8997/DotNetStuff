using System;
using System.Collections.Generic;
using System.IO;

namespace Marler.Hmd
{
    public static class HmdParser
    {
        public static HmdParentReference[] ParseParentList(String props, ref int offset)
        {
            // if props is                     "true 0-1 (parent1 parent2 parent3)
            // then the offset sould be here             ^

            if (offset >= props.Length) throw new ArgumentOutOfRangeException("offset");
            if (props[offset] != '(') throw new ArgumentException(String.Format("The string passed here \"{0}\", must have '(' at offset {1}",props,offset), "props");

            List<HmdParentReference> parentOverrideList = new List<HmdParentReference>();
            offset++;

            while(true)
            {
                // Skip Whitespace
                while(true)
                {
                    if (offset >= props.Length) throw new FormatException("Expected ')' but reached end of string");
                    if (!Char.IsWhiteSpace(props[offset])) break;
                    offset++;
                }

                // At the end of the list
                if (props[offset] == ')')
                {
                    offset++;
                    break;
                }

                // At another ID
                if ((props[offset] >= 'a' && props[offset] <= 'z') || (props[offset] >= 'A' && props[offset] <= 'Z'))
                {
                    int saveOffset = offset;
                    do
                    {
                        offset++;
                        if (offset >= props.Length) throw new FormatException("Expected ')' but reached end of string");
                    } while ((props[offset] >= 'a' && props[offset] <= 'z') ||
                             (props[offset] >= 'A' && props[offset] <= 'Z') ||
                             (props[offset] >= '0' && props[offset] <= '9') ||
                             props[offset] == '-' || props[offset] == '_');

                    //
                    // NOTE: SHOULD I TRY TO RESOLVE THE REFERENCE HERE FIRST USING PROPERTY DICTIONARY???
                    //
                    parentOverrideList.Add(new HmdParentReferenceByString(props.Substring(saveOffset, offset - saveOffset)));
                }
                else
                {
                    throw new FormatException(String.Format("Expected 'a-zA-Z' but got '{0}'", props[offset]));
                }
            }

            if (parentOverrideList.Count < 1) throw new FormatException("You can't have an empty parent list!");

            return parentOverrideList.ToArray();
        }


        public static void ParseAndOverrideBlockProperties(HmdBlockIDProperties blockIDProperties, String props)
        {
            if (blockIDProperties == null)
            {
                throw new ArgumentNullException("blockIDProperties");
            }

            ICountProperty countPropertyOverride = null;
            HmdParentReference[] parentOverrideList = null;

            Int32 offset = 0, saveOffset;

            //
            // TODO: To save on memory, maybe I'll add some type of hash lookup so I don't have to instantiate multiple HmdType classes?
            //
            while (true)
            {
                while (true)
                {
                    if (offset >= props.Length)
                    {
                        if (countPropertyOverride != null)
                        {
                            blockIDProperties.OverrideCountProperty(countPropertyOverride);
                        }
                        if (parentOverrideList != null)
                        {
                            blockIDProperties.SetParentOverrideList(parentOverrideList);
                        }
                        return;
                    }
                    if (!Char.IsWhiteSpace(props[offset])) break;
                    offset++;
                }

                if (props[offset] >= '0' && props[offset] <= '9')
                {
                    saveOffset = offset;

                    // keep going while you see 0-9, '-' or '*'
                    do
                    {
                        offset++;
                        if (offset >= props.Length) break;
                    } while ((props[offset] >= '0' && props[offset] <= '9') || props[offset] == '-' || props[offset] == '*');

                    //
                    // Check that the 'count' property has not been specified Twice
                    //
                    if (countPropertyOverride != null) throw new FormatException("You've specified the 'count' property twice!");
                    countPropertyOverride = CountProperty.Parse(props.Substring(saveOffset, offset - saveOffset));
                }
                else if (props[offset] == '(')
                {
                    if (parentOverrideList != null) throw new FormatException("You've specified the 'parents' property twice!");

                    parentOverrideList = ParseParentList(props, ref offset);
                }
                else
                {
                    throw new FormatException(String.Format("Could not recognize first character of property '{0}', of the props string \"{1}\"",
                        props[offset], props));
                }

            }

        }

        //
        // Maybe change to pass in the string offset and length?
        //
        public static HmdValueIDProperties ParseValueProperties(String idString, String props, HmdBlockIDProperties definitionParent, HmdProperties hmdProperties)
        {
            if (idString == null) throw new ArgumentNullException("idString");

            Boolean defaultCountPropertyOverriden = false;
            Boolean defaultHmdTypeOverriden = false;

            ICountProperty countProperty = hmdProperties.defaultCountProperty;
            HmdType hmdType = hmdProperties.defaultHmdType;

            HmdEnumReference enumReference = null;

            HmdParentReference[] parentOverrideList = null;

            Int32 offset = 0, saveOffset;

            //
            // TODO: To save on memory, maybe I'll add some type of hash lookup so I don't have to instantiate multiple HmdType classes?
            //
            while (true)
            {
                while (true)
                {
                    if (offset >= props.Length)
                    {
                        return new HmdValueIDProperties(idString, countProperty, hmdType, enumReference, 
                            definitionParent, parentOverrideList);
                    }
                    if (!Char.IsWhiteSpace(props[offset])) break;
                    offset++;
                }

                if (props[offset] >= '0' && props[offset] <= '9')
                {
                    saveOffset = offset;

                    // keep going while you see 0-9, '-' or '*'
                    do
                    {
                        offset++;
                        if (offset >= props.Length) { break; }
                    } while ((props[offset] >= '0' && props[offset] <= '9') || props[offset] == '-' || props[offset] == '*');

                    //
                    // Check that the 'count' property has not been specified Twice
                    //
                    if (defaultCountPropertyOverriden) throw new FormatException("You've specified the 'count' property twice!");
                    defaultCountPropertyOverriden = true;

                    countProperty = CountProperty.Parse(props.Substring(saveOffset, offset - saveOffset));
                }
                else if (props[offset] == 's')
                {
                    if (offset + 6 > props.Length) // 6 = "string".Length
                        throw new FormatException(String.Format("Found character '{0}', expected to become \"{1}\", but there are some characters missing",
                            HmdTypeClass.String[0], HmdTypeClass.String));

                    if (props[offset + 1] != 't' ||
                        props[offset + 2] != 'r' ||
                        props[offset + 3] != 'i' ||
                        props[offset + 4] != 'n' ||
                        props[offset + 5] != 'g')
                    {
                        throw new FormatException(String.Format("Expected 'string', but got '{0}'", props.Substring(offset, 6)));
                    }

                    offset += 6;

                    //
                    // Check that the 'type' property has not been specified Twice
                    //
                    if (defaultHmdTypeOverriden) throw new FormatException("You've specified the 'type' property twice!");
                    defaultHmdTypeOverriden = true;

                    hmdType = HmdType.String;
                }
                else if (props[offset] == HmdTypeClass.Boolean[0])
                {
                    if (offset + HmdTypeClass.Boolean.Length > props.Length)
                    {
                        throw new FormatException(
                            String.Format("Found character '{0}', expected to become \"{1}\", but there are some characters missing",
                            HmdTypeClass.Boolean[0], HmdTypeClass.Boolean));
                    }
                    offset++;
                    for (int i = 1; i < HmdTypeClass.Boolean.Length; i++)
                    {
                        if (props[offset] != HmdTypeClass.Boolean[i])
                        {
                            throw new FormatException(String.Format("Expected \"{0}\", but got \"{1}\"", HmdTypeClass.Boolean, props.Substring(offset - i, HmdTypeClass.Boolean.Length)));
                        }
                        offset++;
                    }

                    //
                    // Check that the 'type' property has not been specified Twice
                    //
                    if (defaultHmdTypeOverriden) throw new FormatException("You've specified the 'type' property twice!");
                    defaultHmdTypeOverriden = true;

                    hmdType = HmdType.Boolean;
                }
                else if (props[offset] == 'i' || props[offset] == 'u')
                {
                    Byte byteSize;
                    Boolean isUnsigned = false;
                    if (props[offset] == 'u')
                    {
                        isUnsigned = true;
                        offset++;
                        if (props[offset] != 'i') throw new FormatException(
                                String.Format("Found character 'u', expected to become \"uint\", but the next character was '{0}'", props[offset]));
                    }

                    if (offset + 3 > props.Length) throw new FormatException(
                            String.Format("Found character '{0}', expected to become \"{1}\", but there are some characters missing",
                            isUnsigned ? 'u' : 'i', isUnsigned ? "uint" : "int"));

                    if (props[offset + 1] != 'n' || props[offset + 2] != 't') throw new FormatException(String.Format("Expected \"{0}\", but got \"{1}\"",
                            isUnsigned ? "uint" : "int", isUnsigned ? props.Substring(offset - 1, 4) : props.Substring(offset, 3)));

                    offset += 3;

                    if (offset < props.Length && props[offset] >= '0' && props[offset] <= '9')
                    {
                        saveOffset = offset;
                        do
                        {
                            offset++;
                        } while (offset < props.Length && props[offset] >= '0' && props[offset] <= '9');
                        byteSize = Byte.Parse(props.Substring(saveOffset, offset - saveOffset));
                    }
                    else
                    {
                        byteSize = 0;
                    }

                    //
                    // Check that the 'type' property has not been specified Twice
                    //
                    if (defaultHmdTypeOverriden) throw new FormatException("You've specified the 'type' property twice!");
                    defaultHmdTypeOverriden = true;

                    hmdType = HmdTypeClass.GetIntegerType(isUnsigned, byteSize);
                }
                else if (props[offset] == 'e')
                {
                    offset++;
                    if (offset >= props.Length) throw new FormatException("Found character 'e', expected to become \"enum\" or \"empty\" but the string abrubtly ended");

                    if (props[offset] == HmdTypeClass.Empty[1])
                    {
                        if (offset + HmdTypeClass.Empty.Length - 1 > props.Length) throw new FormatException("Found \"em\", expected to become \"empty\", but there are some characters missing");
                        offset++;
                        for (int i = 2; i < HmdTypeClass.Empty.Length; i++)
                        {
                            if (props[offset] != HmdTypeClass.Empty[i]) throw new FormatException(String.Format("Expected \"{0}\", but got \"{1}\"", HmdTypeClass.Empty, props.Substring(offset - i, HmdTypeClass.Empty.Length)));
                            offset++;
                        }

                        //
                        // Check that the 'type' property has not been specified Twice
                        //
                        if (defaultHmdTypeOverriden) throw new FormatException("You've specified the 'type' property twice!");
                        defaultHmdTypeOverriden = true;

                        hmdType = HmdType.Empty;
                    }
                    else if(props[offset] == HmdTypeClass.Enumeration[1])
                    {                        
                        if (offset + HmdTypeClass.Enumeration.Length + 1 > props.Length)
                        {
                            throw new FormatException(
                                String.Format("Found \"en\", expected to become \"{0}\", but there are some characters missing",
                                HmdTypeClass.Enumeration));
                        }
                        offset++;
                        for (int i = 2; i < HmdTypeClass.Enumeration.Length; i++)
                        {
                            if (props[offset] != HmdTypeClass.Enumeration[i]) throw new FormatException(String.Format("Expected \"{0}\", but got \"{1}\"", HmdTypeClass.Enumeration, props.Substring(offset - i, HmdTypeClass.Enumeration.Length)));
                            offset++;
                        }

                        // skip whitespace
                        while (true)
                        {
                            if (offset >= props.Length) throw new FormatException("Expected '(' or 'a-zA-Z', but got EOF");
                            if (!Char.IsWhiteSpace(props[offset])) break;
                            offset++;
                        }

                        if (props[offset] == '(')
                        {
                            saveOffset = offset + 1;
                            // skip to the next whitespace or ';'
                            while (true)
                            {
                                offset++;
                                if (offset >= props.Length) throw new FormatException("Expected ')' but reached end of string");
                                if (props[offset] == ')') { break; }
                            }

                            String enumReferenceName = (definitionParent == null || definitionParent.definitionContext == null) ?
                                idString.ToLower() : HmdIDProperties.CombineIDContext(definitionParent.idWithContext, idString.ToLower());
                            HmdEnum newInlineEnum = new HmdEnum(enumReferenceName, props.Substring(saveOffset, offset - saveOffset));

                            enumReference = newInlineEnum;
                            if (hmdProperties != null)
                            {
                                hmdProperties.AddEnum(newInlineEnum);
                            }

                            offset++;

                            if (defaultHmdTypeOverriden) throw new FormatException("You've specified the 'type' property twice!");
                            defaultHmdTypeOverriden = true;
                            hmdType = HmdType.Enumeration;
                        }
                        else if ((props[offset] >= 'a' && props[offset] <= 'z') || (props[offset] >= 'A' && props[offset] <= 'Z'))
                        {
                            saveOffset = offset;
                            // skip to the next whitespace or ';'
                            while (true)
                            {
                                offset++;
                                if (offset >= props.Length) { break; }
                                if (Char.IsWhiteSpace(props[offset])) { break; }
                            }
                            if (offset - saveOffset <= 0) throw new FormatException("Unable to parse enum type, the \"enum\" keyword must be either \"enum <type>\" (with only one space before <type>) or \"enum(<value> <value> ...)\"");

                            enumReference = new HmdEnumReferenceByString(props.Substring(saveOffset, offset - saveOffset));

                            if (defaultHmdTypeOverriden)
                            {
                                throw new FormatException("You've specified the 'type' property twice!");
                            }
                            defaultHmdTypeOverriden = true;
                            hmdType = HmdType.Enumeration;
                        }
                        else
                        {
                            throw new FormatException(String.Format("Expected '(' or 'a-zA-Z' after \"enum\", but got '{0}'", props[offset]));
                        }
                    }
                    else
                    {
                        throw new FormatException(String.Format(
                            "Found character 'e', expected to become \"enum\" or \"empty\" but the second character is '{0}'",props[offset]));
                    }

                }
                else if (props[offset] == '(')
                {
                    if (parentOverrideList != null) throw new FormatException("You've specified the 'parents' property twice!");

                    parentOverrideList = ParseParentList(props, ref offset);
                }
                else
                {
                    throw new FormatException(
                        String.Format("Could not recognize first character of property '{0}', of the props string \"{1}\"", props[offset], props));
                }

            }

        }
    }
}
