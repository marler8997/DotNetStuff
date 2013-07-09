using System;
using System.Collections.Generic;

namespace More.Pdl
{
    public class PdlFile
    {
        readonly Dictionary<String, EnumOrFlagsDefinition> enumOrFlagsDefinitionDictionary =
            new Dictionary<String, EnumOrFlagsDefinition>();
        readonly List<EnumOrFlagsDefinition> enumDefinitions = new List<EnumOrFlagsDefinition>();
        readonly List<EnumOrFlagsDefinition> flagsDefinitions = new List<EnumOrFlagsDefinition>();

        public readonly List<ObjectDefinition> objectDefinitions = new List<ObjectDefinition>();

        public IEnumerable<EnumOrFlagsDefinition> EnumOrFlagsDefinitions
        { get { return enumOrFlagsDefinitionDictionary.Values; } }
        public IEnumerable<EnumOrFlagsDefinition> EnumDefinitions
        { get { return enumDefinitions; } }
        public IEnumerable<EnumOrFlagsDefinition> FlagsDefinitions
        { get { return flagsDefinitions; } }

        public String MakeDefinitionKey(ObjectDefinition objectDefinedIn, String typeNameLowerInvariant)
        {
            return (objectDefinedIn == null) ? typeNameLowerInvariant :
                String.Format("{0}.{1}", objectDefinedIn.nameLowerInvariantCase, typeNameLowerInvariant);
        }

        // returns definition key
        public String Add(EnumOrFlagsDefinition enumOrFlagsDefinition, ObjectDefinition objectDefinedIn)
        {
            String definitionKey = MakeDefinitionKey(objectDefinedIn, enumOrFlagsDefinition.typeNameLowerInvariantCase);
            enumOrFlagsDefinitionDictionary.Add(definitionKey, enumOrFlagsDefinition);
            if (enumOrFlagsDefinition.isFlagsDefinition)
            {
                flagsDefinitions.Add(enumOrFlagsDefinition);
            }
            else
            {
                enumDefinitions.Add(enumOrFlagsDefinition);
            }
            return definitionKey;
        }

        public EnumOrFlagsDefinition TryGetDefinition(ObjectDefinition currentObject, String typeNameLowerInvariant)
        {
            EnumOrFlagsDefinition definition;
            //
            // Try to get the defintion from any enum defined in the command itself
            //
            if (enumOrFlagsDefinitionDictionary.TryGetValue(MakeDefinitionKey(currentObject, typeNameLowerInvariant), out definition))
            {
                return definition;
            }
            //
            // Try to get the definition from a global enum definition
            //
            if (enumOrFlagsDefinitionDictionary.TryGetValue(typeNameLowerInvariant, out definition))
            {
                return definition;
            }
            return null;
        }

        //public readonly IPdlDataStructureFactory dataStructureFactory;
        public PdlFile(/*IPdlDataStructureFactory dataStructureFactory*/)
        {
            //this.dataStructureFactory = dataStructureFactory;
        }
    }

    //
    // Definition Classes
    //
    public class EnumValueDefinition
    {
        public readonly String name;
        public readonly String value;
        public EnumValueDefinition(String name, String value)
        {
            this.name = name;
            this.value = value;
        }
    }
    public class FlagsValueDefinition
    {
        public readonly String name;
        public readonly Byte bit;
        public FlagsValueDefinition(String name, Byte bit)
        {
            this.name = name;
            this.bit = bit;
        }
    }
    public class EnumOrFlagsDefinition
    {
        public readonly Boolean isFlagsDefinition;
        public readonly Boolean isGlobalType;

        public readonly PdlType underlyingIntegerType;

        public readonly String typeName;
        public readonly String typeNameLowerInvariantCase;
        public readonly String definitionKey;

        public readonly List<EnumValueDefinition> enumValues;
        public readonly List<FlagsValueDefinition> flagValues;

        public EnumOrFlagsDefinition(PdlFile pdlFile, Boolean isFlagsDefinition, ObjectDefinition objectDefinedIn,
            PdlType underlyingIntegerType, String typeName)
        {
            if (!underlyingIntegerType.IsValidUnderlyingEnumIntegerType()) throw new InvalidOperationException(String.Format(
                 "'{0}' is not a valid underlying integer type for an enum or flags definition", underlyingIntegerType));

            this.isFlagsDefinition = isFlagsDefinition;
            this.isGlobalType = (objectDefinedIn == null);

            this.underlyingIntegerType = underlyingIntegerType;

            this.typeName = typeName;
            this.typeNameLowerInvariantCase = typeName.ToLowerInvariant();
            this.definitionKey = pdlFile.Add(this, objectDefinedIn);

            //
            // Add the definition to the static flags or enum definitions list
            //
            if (isFlagsDefinition)
            {
                enumValues = null;
                flagValues = new List<FlagsValueDefinition>();
            }
            else
            {
                enumValues = new List<EnumValueDefinition>();
                flagValues = null;
            }

            //
            // Add the defintion to the command
            //
            //if (objectDefinedIn != null) objectDefinedIn.ADefinition(this);
        }
        public void Add(EnumValueDefinition definition)
        {
            enumValues.Add(definition);
        }
        public void Add(FlagsValueDefinition definition)
        {
            flagValues.Add(definition);
        }
    }
}