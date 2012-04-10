using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.Hmd
{
    public interface HmdEnumReference
    {
        String Name { get; }
        HmdEnum TryGetReference { get; }
    }

    public class HmdEnumReferenceByString : HmdEnumReference
    {
        public readonly String name;
        public HmdEnumReferenceByString(String name) { this.name = name; }
        String HmdEnumReference.Name { get { return name; } }
        HmdEnum HmdEnumReference.TryGetReference { get { return null; } }
    }

    public class HmdEnum : HmdEnumReference
    {
        public static HmdEnum ResolveEnumReference(String enumName, HmdProperties hmdProperties)
        {
            List<HmdEnum> enumList = hmdProperties.EnumList;

            if (enumList != null)
            {
                for (int i = 0; i < enumList.Count; i++)
                {
                    if (enumList[i].name.Equals(enumName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return enumList[i];
                    }
                }
            }
            throw new InvalidOperationException(String.Format("The enum \"{0}\", has not been defined", enumName));
        }

        public readonly String name;
        private readonly String[] values;

        public HmdEnum(String name, String[] values)
        {
            this.name = name;
            this.values = values;
        }

        public HmdEnum(String definition)
        {
            int saveOffset,offset = 0;

            // skip whitespace
            while (true)
            {
                if (offset >= definition.Length) throw new FormatException("Reached end of enum definition too soon");
                if (!Char.IsWhiteSpace(definition[offset])) break;
                offset++;
            }

            saveOffset = offset;

            while (true)
            {
                offset++;
                if (offset >= definition.Length) throw new FormatException("Reached end of enum definition too soon");
                if (Char.IsWhiteSpace(definition[offset])) break;
            }

            this.name = definition.Substring(saveOffset, offset - saveOffset);

            offset++;
            this.values = HmdStringExtensions.SplitByWhitespace(definition, offset);
            if (this.values == null || this.values.Length <= 0)
            {
                throw new FormatException("Reached end of enum definition too soon");
            }
        }

        public HmdEnum(String context, String inlineDefinition)
        {
            inlineDefinition = inlineDefinition.ToLower();

            this.name = context;

            this.values = HmdStringExtensions.SplitByWhitespace(inlineDefinition, 0);
            if (this.values == null || this.values.Length <= 0)
            {
                throw new FormatException("Invalid inline enum definition");
            }
        }

        String HmdEnumReference.Name { get { return name; } }
        HmdEnum HmdEnumReference.TryGetReference { get { return this; } }

        public Boolean IsValidEnumValue(String value)
        {
            value = value.Trim();

            for (int i = 0; i < values.Length; i++)
            {
                if (value.Equals(values[i], StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public Int32 ValueCount { get { return values.Length; } }

        public String GetValue(Int32 index)
        {
            return values[index];
        }

        public override string ToString()
        {
            StringBuilder valuesAsString = new StringBuilder();

            for (int i = 0; i < values.Length; i++)
            {
                valuesAsString.Append(' ');
                valuesAsString.Append(values[i]);
            }

            return String.Format("%enum:{0}{1}", name, valuesAsString.ToString());
        }

    }
}
