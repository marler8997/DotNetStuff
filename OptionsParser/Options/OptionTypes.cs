using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.OptionsParser
{
    public class OptionGenericArg<T> : Option
    {
        public delegate T ParseMethod(String str);

        public readonly ParseMethod parseMethod;
        private bool hasDefault;
        private T defaultValue;

        private T value;

        public OptionGenericArg(ParseMethod parseMethod, char letter, String usageName)
            : base(letter, 1, usageName)
        {
            this.parseMethod = parseMethod;
            this.hasDefault = false;
        }

        public OptionGenericArg(ParseMethod parseMethod, char letter, String usageName, String description)
            : base(letter, 1, usageName, description)
        {
            this.parseMethod = parseMethod;
            this.hasDefault = false;
        }

        public void SetDefault(T defaultValue)
        {
            this.hasDefault = true;
            this.defaultValue = defaultValue;
            this.value = defaultValue;
        }

        public T ArgValue
        {
            get { return value; }
        }

        public override String ArgValues()
        {
            if (hasDefault)
            {
                return String.Format("(default={0})", defaultValue);
            }
            return String.Empty;
        }

        public override void ParseArg(String argString)
        {
            this.argString = argString;
            this.value = parseMethod(argString);
        }
    }

    public class OptionNoArg : Option
    {
        public OptionNoArg(char letter, String usageName)
            : base(letter, 0, usageName)
        { }

        public OptionNoArg(char letter, String usageName, String description)
            : base(letter, 0, usageName, description)
        { }

        public override String ArgValues()
        {
            return String.Empty;
        }

        public override void ParseArg(String argString)
        {
            throw new InvalidOperationException("Cannot parse an argument from an option with no argument");
        }
    }

    public class OptionIntArg : Option
    {
        private bool hasDefault;
        private int defaultValue;

        private int value;

        public OptionIntArg(char letter, String usageName)
            : base(letter, 1, usageName)
        {
            this.hasDefault = false;
        }

        public OptionIntArg(char letter, String usageName, String description)
            : base(letter, 1, usageName, description)
        {
            this.hasDefault = false;
        }

        public void SetDefault(int defaultValue)
        {
            this.hasDefault = true;
            this.defaultValue = defaultValue;
            this.value = defaultValue;
        }


        public int ArgValue
        {
            get { return value; }
        }

        public override String ArgValues()
        {
            if (hasDefault)
            {
                return String.Format("(default={0})", defaultValue);
            }
            return String.Empty;
        }

        public override void ParseArg(String argString)
        {
            this.argString = argString;
            try
            {
                value = Int32.Parse(argString);
            }
            catch (FormatException fe)
            {
                throw new OptionParseInvalidArgumentException(this, argString, String.Format("need an integer: {0}", fe.Message));
            }
            catch (Exception e)
            {
                throw new OptionParseInvalidArgumentException(this, argString, e.Message);
            }
        }
    }

    public class OptionStringArg : Option
    {
        private bool hasDefault;
        public String defaultValue;

        public OptionStringArg(char letter, String usageName)
            : base(letter, 1, usageName)
        {
            this.hasDefault = false;
        }

        public OptionStringArg(char letter, String usageName, String description)
            : base(letter, 1, usageName, description)
        {
            this.hasDefault = false;
        }

        public void SetDefault(String defaultValue)
        {
            this.hasDefault = true;
            this.defaultValue = defaultValue;
            this.argString = defaultValue;
        }

        public String ArgValue
        {
            get { return argString; }
        }

        public override String ArgValues()
        {
            if (hasDefault)
            {
                return String.Format("(default={0})", defaultValue);
            }
            return String.Empty;
        }

        public override void ParseArg(String argString)
        {
            this.argString = argString;
        }
    }

    public class OptionEnumStringArg : Option
    {
        private bool hasDefault;
        public int defaultEnumValue;

        private List<String> enumStrings;
        int enumValue;

        public OptionEnumStringArg(char letter, String usageName)
            : base(letter, 1, usageName)
        {
            this.hasDefault = false;
            this.enumStrings = new List<string>();
        }

        public OptionEnumStringArg(char letter, String usageName, String description)
            : base(letter, 1, usageName, description)
        {
            this.hasDefault = false;
            this.enumStrings = new List<string>();
        }

        public int ArgValue
        {
            get { return enumValue; }
        }

        public String ArgValueString
        {
            get { return enumStrings[enumValue]; }
        }

        public void SetDefault(int defaultEnumValue)
        {
            if (defaultEnumValue < 0 || defaultEnumValue >= enumStrings.Count)
            {
                throw new Exception("Cannot set a default index value of " + defaultEnumValue +
                    " because there are only " + enumStrings + " possible values");
            }
            this.hasDefault = true;
            this.defaultEnumValue = defaultEnumValue;
            this.enumValue = defaultEnumValue;
        }

        public int AddEnumString(String enumString)
        {
            this.enumStrings.Add(enumString);
            return enumStrings.Count - 1;
        }

        public override String ArgValues()
        {
            if (enumStrings.Count > 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("Values: ");
                int i;
                for (i = 0; i < enumStrings.Count - 1; i++)
                {
                    stringBuilder.Append(enumStrings[i]);
                    stringBuilder.Append(" | ");
                }
                stringBuilder.Append(enumStrings[i]);

                if (hasDefault)
                {
                    stringBuilder.Append(String.Format(" (default={0})", enumStrings[defaultEnumValue]));
                }
                return stringBuilder.ToString();
            }
            else
            {
                if (hasDefault)
                {
                    return String.Format("(default={0})", enumStrings[defaultEnumValue]);
                }
            }
            return String.Empty;
        }

        public override void ParseArg(String argString)
        {
            this.argString = argString;
            if (enumStrings.Count > 0)
            {
                for (int i = 0; i < enumStrings.Count; i++)
                {
                    if (argString.Equals(enumStrings[i], StringComparison.CurrentCultureIgnoreCase))
                    {
                        this.enumValue = i;
                        return;
                    }
                }

                //
                // Throw an exception
                //
                StringBuilder strBuilder = new StringBuilder(enumStrings[0]);
                for (int i = 1; i < enumStrings.Count; i++)
                {
                    strBuilder.Append("," + enumStrings[i]);
                }
                throw new OptionParseInvalidArgumentException(this, argString, "accepted values are (" + strBuilder.ToString() + ")");

            }
            else
            {
                throw new InvalidOperationException(String.Format("The option -{0} is set up to accept a set of arguments but no arguments have been specified",
                    letter));
            }
        }
    }

}
