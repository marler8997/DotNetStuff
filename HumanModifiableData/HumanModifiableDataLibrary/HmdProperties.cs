using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Marler.Hmd
{
    public class HmdIDPropertiesTable<T> where T : HmdIDProperties
    {
        public readonly Dictionary<String, T> definitionDictionary;
        public readonly Dictionary<String, T> extraLinksDictionary;

        public HmdIDPropertiesTable()
        {
            this.definitionDictionary = new Dictionary<String, T>();
            this.extraLinksDictionary = new Dictionary<String, T>();
        }

        public void AddPropertiesFromDefinition(T idProperties)
        {
            if(definitionDictionary.ContainsKey(idProperties.idWithContext))
            {
                throw new InvalidOperationException(String.Format("The idWithContext \"{0}\" has already been added to the HmdIdPropertiesTable::definitionDictionary",idProperties.idWithContext));
            }
            definitionDictionary.Add(idProperties.idWithContext, idProperties);
        }

        public void AddPropertiesFromExtraLinks(HmdBlockIDProperties context, T idProperties)
        {
            String extraLinkContext = HmdIDProperties.CombineIDContext(context.idWithContext, idProperties.idLowerCase);
            if (extraLinksDictionary.ContainsKey(extraLinkContext))
            {
                throw new InvalidOperationException(String.Format("The extraLinksContext \"{0}\" has already been added to the HmdIdPropertiesTable::extraLinksDictionary",  extraLinksDictionary));
            }
            extraLinksDictionary.Add(extraLinkContext, idProperties);
        }

        public void AddPropertiesFromExtraLinks(String context, T idProperties)
        {
            String extraLinkContext = HmdIDProperties.CombineIDContext(context, idProperties.idLowerCase);
            if (extraLinksDictionary.ContainsKey(extraLinkContext))
            {
                throw new InvalidOperationException(String.Format("The extraLinksContext \"{0}\" has already been added to the HmdIdPropertiesTable::extraLinksDictionary", extraLinksDictionary));
            }
            extraLinksDictionary.Add(extraLinkContext, idProperties);
        }

        private T TryGetProperties(String contextString)
        {
            T idProperties;
            if (definitionDictionary.TryGetValue(contextString, out idProperties))
            {
                return idProperties;
            }
            else if (extraLinksDictionary.TryGetValue(contextString, out idProperties))
            {
                return idProperties;
            }
            else
            {
                return null;
            }
        }

        public T GetProperties(HmdID id)
        {
            String contextString = id.CreateContextString();

            T idProperties = TryGetProperties(contextString);
            if (idProperties == null)
            {
                throw new InvalidOperationException(String.Format("The id \"{0}\" was not found in the defintionDictionary or the extraLinksDictionary (context={1})",
                    id.idOriginalCase, contextString));
            }
            return idProperties;
        }

        public HmdBlockIDProperties GetParentPropertiesInScope(HmdIDProperties child, HmdParentReference parentReference)
        {
            String parentContextString = HmdIDProperties.CombineIDContext(child.definitionContext, parentReference.IDLowerCase);

            T idProperties = TryGetProperties(parentContextString);
            if (idProperties == null)
            {
                throw new InvalidOperationException(String.Format("The parent id \"{0}\" was not found in the defintionDictionary or the extraLinksDictionary with this context \"{1}\"", 
                    parentReference.IDOriginalCase, parentContextString));
            }
            return idProperties.CastAsBlockIDProperties;
        }
    }

    public class HmdProperties
    {
        public const String RootName = "%root";

        public readonly ICountProperty defaultCountProperty;
        public readonly HmdType defaultHmdType;

        public readonly HmdBlockIDProperties root;
        public readonly HmdIDPropertiesTable<HmdValueIDProperties> valueIDTable;
        public readonly HmdIDPropertiesTable<HmdBlockIDProperties> blockIDTable;

        private List<HmdEnum> enumList;
        public List<HmdEnum> EnumList { get { return enumList; } }

        public HmdProperties()
            : this(UnrestrictedCount.Instance, HmdType.String)
        {
        }

        public HmdProperties(ICountProperty defaultCountProperty, HmdType defaultHmdType)
        {
            this.defaultCountProperty = defaultCountProperty;
            this.defaultHmdType = defaultHmdType;

            this.root = new HmdBlockIDProperties(RootName, new StaticCount(1), null);
            this.valueIDTable = new HmdIDPropertiesTable<HmdValueIDProperties>();
            this.blockIDTable = new HmdIDPropertiesTable<HmdBlockIDProperties>();

            //AddPropertiesFromDefinition(root); // should I add this?
            this.enumList = null;
            //this.enumInlineList = null;
        }

        public void AddEnum(HmdEnum newEnum)
        {
            if(enumList == null)
            {
                enumList = new List<HmdEnum>();
            }
            enumList.Add(newEnum);
        }

        public void AddPropertiesFromDefinition(HmdValueIDProperties valueIDProperties)
        {
            valueIDTable.AddPropertiesFromDefinition(valueIDProperties);
        }

        public void AddPropertiesFromDefinition(HmdBlockIDProperties blockIDProperties)
        {
            blockIDTable.AddPropertiesFromDefinition(blockIDProperties);
        }

        public void AddPropertiesFromExtraLinks(HmdBlockIDProperties context, HmdIDProperties idProperties)
        {
            if (idProperties.isBlock)
            {
                HmdBlockIDProperties blockIDProperties = idProperties.CastAsBlockIDProperties;
                blockIDTable.AddPropertiesFromExtraLinks(context, blockIDProperties);

                String newContext = HmdIDProperties.CombineIDContext(context.idWithContext, idProperties.idLowerCase);
                foreach (HmdIDProperties childIDProperties in blockIDProperties)
                {
                    AddPropertiesFromExtraLinks(newContext, childIDProperties);
                }               
            }
            else
            {
                valueIDTable.AddPropertiesFromExtraLinks(context, idProperties.CastAsValueIDProperties);
            }
        }

        private void AddPropertiesFromExtraLinks(String context, HmdIDProperties idProperties)
        {
            if (idProperties.isBlock)
            {
                HmdBlockIDProperties blockIDProperties = idProperties.CastAsBlockIDProperties;
                blockIDTable.AddPropertiesFromExtraLinks(context, blockIDProperties);

                String newContext = HmdIDProperties.CombineIDContext(context, idProperties.idLowerCase);
                foreach (HmdIDProperties childIDProperties in blockIDProperties)
                {
                    AddPropertiesFromExtraLinks(newContext, childIDProperties);
                }
            }
            else
            {
                valueIDTable.AddPropertiesFromExtraLinks(context, idProperties.CastAsValueIDProperties);
            }

        }

        public HmdEnum TryGetEnum(String enumName)
        {
            for (int i = 0; i < enumList.Count; i++)
            {
                if (enumList[i].name.Equals(enumName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return enumList[i];
                }
            }
            return null;
        }

        public HmdValueIDProperties GetProperties(HmdValueID valueID)
        {
            return valueIDTable.GetProperties(valueID);
        }
        public HmdBlockIDProperties GetProperties(HmdBlockID blockID)
        {
            return blockIDTable.GetProperties(blockID);
        }

        public HmdIDProperties TryToGetChildInScope(HmdBlockIDProperties parent, HmdIDReference childReference)
        {
            throw new NotImplementedException();
        }

        public HmdBlockIDProperties GetParentPropertiesInScope(HmdIDProperties child, HmdParentReference parentReference)
        {
            return blockIDTable.GetParentPropertiesInScope(child, parentReference);
        }

        public void ResolveChildParentReferences()
        {
            TextWriter debugOutput = HmdDebug.DebugOutput;

            debugOutput.WriteLine("[Resolving Child/Parent References...]");
            foreach (HmdValueIDProperties valueProps in valueIDTable.definitionDictionary.Values)
            {
                debugOutput.WriteLine("Resolving Value ID \"{0}", valueProps);
                valueProps.ResolveParentOverrideLinks(this);
            }

            foreach (HmdBlockIDProperties blockProps in blockIDTable.definitionDictionary.Values)
            {
                debugOutput.WriteLine("Resolving Block ID \"{0}", blockProps);
                blockProps.ResolveParentOverrideLinks(this);
                blockProps.ResolveAdditionalChildrenLinks(this);
            }

            debugOutput.WriteLine("[Done Resolving Child/Parent References]");
        }

        public void PrintEnums(TextWriter output)
        {
            if (enumList == null || enumList.Count <= 0)
            {
                output.WriteLine("Enums Defined: NONE");
            }
            else
            {
                output.WriteLine("Enums Defined:");
                for (int i = 0; i < enumList.Count; i++)
                {
                    HmdEnum hmdEnum = enumList[i];
                    output.Write("   {0}:", hmdEnum.name);
                    for (int j = 0; j < hmdEnum.ValueCount; j++)
                    {
                        output.Write(" {0}", hmdEnum.GetValue(j));
                    }
                    output.WriteLine();
                }
            }
        }

        public void Print(TextWriter output)
        {
            PrintEnums(output);

            output.WriteLine("Value Props:");

            foreach (HmdValueIDProperties valueProps in valueIDTable.definitionDictionary.Values)
            {
                output.WriteLine(valueProps);
            }
            output.WriteLine();
            output.WriteLine("Block Props:");

            foreach (HmdBlockIDProperties blockProps in blockIDTable.definitionDictionary.Values)
            {
                output.WriteLine(blockProps);
            }

            output.WriteLine();
            output.WriteLine("Extra Value Links:");
            foreach (String context in valueIDTable.extraLinksDictionary.Keys)
            {
                output.WriteLine(context);
            }
            output.WriteLine();
            output.WriteLine("Extra Block Links:");
            foreach (String context in blockIDTable.extraLinksDictionary.Keys)
            {
                output.WriteLine(context);
            }
        }
    }
}
