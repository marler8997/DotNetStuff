using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;

#if WindowsCE
using EnumReflection = System.MissingInCEEnumReflection;
#else
using EnumReflection = System.Enum;
#endif

namespace More
{
    public class CommandLineException : SystemException
    {
        public CommandLineException(String message) : base(message) {}
    }
    public static class CLUtil
    {
        public static void PrintRightColumn(String str, Int32 leftColumn, Int32 rightColumn, Boolean startAtLeftColumn)
        {
            if (startAtLeftColumn)
            {
                Console.Write("{0," + leftColumn + "}", String.Empty);
            }

            int strOffset = 0;
            while (true)
            {
                Int32 nextLength = str.Length - strOffset;
                if (nextLength > rightColumn)
                {
                    nextLength = rightColumn;
                }

                Console.WriteLine(str.Substring(strOffset, nextLength));
                strOffset += nextLength;
                if (strOffset >= str.Length) break;

                Console.Write("{0," + leftColumn + "}", String.Empty);
            }
        }
    }
    public abstract class CLOption
    {
        public readonly Char letter;
        public readonly String name;
        public readonly Boolean hasArg;
        public readonly String description;

        public readonly String usageLeftColumn;

        public Boolean set;

        protected CLOption(Char letter, Boolean hasArg, String description)
            : this(letter, null, hasArg, description)
        {
        }
        protected CLOption(String name, Boolean hasArg, String description)
            : this('\0', name, hasArg, description)
        {
        }
        protected CLOption(Char letter, String name, Boolean hasArg, String description)
        {
            this.letter = letter;
            this.name = name;
            this.hasArg = hasArg;
            this.description = description;

            if (letter == '\0')
            {
                this.usageLeftColumn = String.Format("  --{0}{1}", name, (hasArg) ? " <arg>" : String.Empty);
            }
            else
            {
                this.usageLeftColumn = String.Format("  -{0}{1}{2}", letter,
                    (name == null) ? "" : ",--" + name,
                    (hasArg) ? " <arg>" : String.Empty);
            }

            this.set = false;
        }
        public void PrintUsage(Int32 leftColumnLength, Int32 rightColumnLength)
        {
            Console.Write(usageLeftColumn + "{0," + (leftColumnLength - usageLeftColumn.Length) + "}", "");

            CLUtil.PrintRightColumn(description, leftColumnLength, rightColumnLength, false);

            // Print Arg Values
            String usageArgValues = UsageArgValues();
            if (usageArgValues != null)
            {
                CLUtil.PrintRightColumn(usageArgValues, leftColumnLength, rightColumnLength, true);
            }
        }
        internal abstract String UsageArgValues();
        public abstract void ParseArg(String arg);

        public override string ToString()
        {
            return String.Format("Letter '{0}'", letter);
        }
    }
    public class CLSwitch : CLOption
    {
        public CLSwitch(Char letter, String description)
            : base(letter, null, false, description)
        {
        }
        public CLSwitch(String name, String description)
            : base(name, false, description)
        {
        }
        public CLSwitch(Char letter, String name, String description)
            : base(letter, name, false, description)
        {
        }
        internal override String UsageArgValues()
        {
            return null;
        }
        public override void ParseArg(String arg)
        {
            throw new InvalidOperationException("Cannot parse an argument from an option with no argument");
        }
    }
    public class CLStringArgument : CLOption
    {
        String defaultValue;
        String value;
        public CLStringArgument(Char letter, String description)
            : base(letter, true, description)
        {
        }
        public CLStringArgument(String name, String description)
            : base(name, true, description)
        {
        }
        public CLStringArgument(Char letter, String name, String description)
            : base(letter, name, true, description)
        {
        }
        public void SetDefault(String defaultValue)
        {
            this.defaultValue = defaultValue;
            this.value = defaultValue;
        }
        public String ArgValue
        {
            get { return value; }
        }
        internal override String UsageArgValues()
        {
            if (defaultValue != null) return String.Format("(default={0})", defaultValue);
            return null;
        }
        public override void ParseArg(String arg)
        {
            this.value = arg;
        }
    }
    public class CLInt32Argument : CLOption
    {
        Boolean hasDefault;
        Int32 defaultValue;
        Int32 value;
        public CLInt32Argument(Char letter, String description)
            : base(letter, true, description)
        {
        }
        public CLInt32Argument(String name, String description)
            : base(name, true, description)
        {
        }
        public CLInt32Argument(Char letter, String name, String description)
            : base(letter, name, true, description)
        {
        }
        public void SetDefault(Int32 defaultValue)
        {
            this.hasDefault = true;
            this.defaultValue = defaultValue;
            this.value = defaultValue;
        }
        public Int32 ArgValue
        {
            get { return value; }
        }
        internal override String UsageArgValues()
        {
            if (hasDefault) return String.Format("(Default={0})", defaultValue);
            return null;
        }
        public override void ParseArg(String arg)
        {
            this.value = Int32.Parse(arg);
        }
    }
    public class CLEnumArgument<EnumType> : CLOption
    {
        Boolean hasDefault;
        EnumType defaultValue;
        EnumType value;
        public CLEnumArgument(Char letter, String description)
            : base(letter, true, description)
        {
        }
        public CLEnumArgument(String name, String description)
            : base(name, true, description)
        {
        }
        public CLEnumArgument(Char letter, String name, String description)
            : base(letter, name, true, description)
        {
        }
        public void SetDefault(EnumType defaultValue)
        {
            this.hasDefault = true;
            this.defaultValue = defaultValue;
            this.value = defaultValue;
        }
        public EnumType ArgValue
        {
            get { return value; }
        }
        internal override String UsageArgValues()
        {
            String[] enumNames = EnumReflection.GetNames(typeof(EnumType));

            StringBuilder builder = new StringBuilder();
            builder.Append("Values={");
            for (int i = 0; i < enumNames.Length; i++)
            {
                if (i > 0) builder.Append(',');
                builder.Append(enumNames[i]);
            }
            builder.Append("}");
            if(hasDefault)
            {
                builder.Append(" Default=");
                builder.Append(defaultValue);
            }
            return builder.ToString();
        }
        public override void ParseArg(String arg)
        {
            this.value = (EnumType)Enum.Parse(typeof(EnumType), arg, true);
            if (this.value == null) throw new CommandLineException(String.Format("Could not parse '{0}' as an enum of type '{1}'",
                 arg, typeof(EnumType).Name));
        }
    }
    public class CLGenericArgument<T> : CLOption
    {
        readonly Parser<T> parser;

        Boolean hasDefault;
        T defaultValue;

        T value;
        
        public CLGenericArgument(Parser<T> parser, Char letter, String description)
            : base(letter, true, description)
        {
            this.parser = parser;
        }
        public CLGenericArgument(Parser<T> parser, String name, String description)
            : base(name, true, description)
        {
            this.parser = parser;
        }
        public CLGenericArgument(Parser<T> parser, Char letter, String name, String description)
            : base(letter, name, true, description)
        {
            this.parser = parser;
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
        internal override String UsageArgValues()
        {
            if (hasDefault) return String.Format("(default={0})", defaultValue);
            return null;
        }
        public override void ParseArg(String arg)
        {
            this.value = parser(arg);
        }
    }
    public class CLParser
    {
        static Int32 DefaultBufferWidth
        {
            get
            {
                try {
#if WindowsCE
                    return 80;
#else
                    return Console.BufferWidth;
#endif
                    }
                catch (IOException) { return 80; }
                catch (SecurityException) { return 80; }
            }
        }

        Int32 bufferWidth;

        readonly Dictionary<Char, CLOption> optionLetters;
        readonly Dictionary<String, CLOption> optionNames;
        readonly List<CLOption> options;

        Int32 maxUsageLeftColumnWidth;

        public CLParser()
            : this(DefaultBufferWidth)
        {
        }
        public CLParser(Int32 bufferWidth)
        {
            this.bufferWidth = bufferWidth;
            this.optionLetters = new Dictionary<Char, CLOption>();
            this.optionNames = new Dictionary<String, CLOption>();
            this.options = new List<CLOption>();

            this.maxUsageLeftColumnWidth = 0;
        }
        public void Add(CLOption option)
        {
            if (option.letter == '\0' && option.name == null)
                throw new ArgumentException(String.Format("Option '{0}' has a null letter and name", option), "option");


            CLOption otherOption;
            if (option.letter != '\0')
            {
                if (optionLetters.TryGetValue(option.letter, out otherOption))
                {
                    throw new InvalidOperationException(String.Format("You've added two options with same letter '{0}' ({1}) and ({2})",
                        option.letter, otherOption, option));
                }
                optionLetters.Add(option.letter, option);
            }

            if (option.name != null)
            {
                if (optionNames.TryGetValue(option.name, out otherOption))
                {
                    throw new InvalidOperationException(String.Format("You've added two options with same name '{0}' ({1}) and ({2})",
                        option.name, otherOption, option));
                }
                optionNames.Add(option.name, option);
            }

            this.options.Add(option);

            //
            // Update maximum usage left column length
            //
            if (option.usageLeftColumn.Length > maxUsageLeftColumnWidth)
            {
                maxUsageLeftColumnWidth = option.usageLeftColumn.Length;
                if (maxUsageLeftColumnWidth >= bufferWidth - 40)
                {
                    bufferWidth = maxUsageLeftColumnWidth + 40;
                }
            }
        }
        public List<String> Parse(String[] args)
        {
            List<String> nonOptionArguments = new List<String>();

            for (int originalArgIndex = 0; originalArgIndex < args.Length; originalArgIndex++)
            {
                String current = args[originalArgIndex];

                if (String.IsNullOrEmpty(current)) continue;

                if (current[0] != '-' || current.Length <= 1)
                {
                    nonOptionArguments.Add(current);
                    continue;
                }

                if (current[1] == '-')
                {
                    if (current.Length <= 2)
                    {
                        nonOptionArguments.Add(current);
                        continue;
                    }

                    String optionName = current.Substring(2);

                    CLOption option;
                    if (!optionNames.TryGetValue(optionName, out option))
                        throw new CommandLineException(String.Format("Unknown Option Name '{0}'", optionName));

                    ProcessOption(option, args, true, ref originalArgIndex);
                }
                else
                {
                    for (int letterIndex = 1; letterIndex < current.Length; letterIndex++)
                    {
                        Char optionCharacter = current[letterIndex];

                        CLOption option;
                        if (!optionLetters.TryGetValue(optionCharacter, out option))
                            throw new CommandLineException(String.Format("Unknown Option Letter '{0}' in argument '{1}'", optionCharacter, current));

                        ProcessOption(option, args, letterIndex >= current.Length - 1, ref originalArgIndex);
                    }
                }
            }
            return nonOptionArguments;
        }
        void ProcessOption(CLOption option, String[] args, Boolean optionIsLastOrAlone, ref Int32 originalArgIndex)
        {
            // Check for duplicates
            if (option.set) throw new CommandLineException(String.Format("Found option '{0}' twice", option));
            option.set = true;

            if (option.hasArg)
            {
                if (!optionIsLastOrAlone)
                    throw new CommandLineException(String.Format("Option '{0}' requires an argument but it is not the last letter in a list of options", option));

                originalArgIndex++;
                if (originalArgIndex >= args.Length)
                    throw new CommandLineException(String.Format("Option '{0}' requires an argument but there was none", option));

                option.ParseArg(args[originalArgIndex]);
            }
        }

        public Int32 ErrorAndUsage(String errorMessage, params Object[] obj)
        {
            return ErrorAndUsage(String.Format(errorMessage, obj));
        }
        public Int32 ErrorAndUsage(String errorMessage)
        {
            Console.WriteLine(errorMessage);
            PrintUsage();
            return -1;
        }

        public virtual void PrintUsageHeader()
        {
            Console.WriteLine("  not specified");
        }
        public virtual void PrintUsageFooter()
        {
        }
        public void PrintUsage()
        {
            PrintUsageHeader();

            if (options.Count > 0)
            {
                Console.WriteLine("Options:");
                foreach (CLOption option in options)
                {
                    option.PrintUsage(maxUsageLeftColumnWidth + 2, bufferWidth - maxUsageLeftColumnWidth - 2);
                }
            }

            PrintUsageFooter();
        }
        public void PrintStatus()
        {
            Console.WriteLine("Options Status:");
            Console.WriteLine("Set Opt        Name");
            foreach (CLOption option in options)
            {
                Console.WriteLine("[{0}] -{1} <arg> {2}", option.set ? "x" : " ", option.letter, option.name);
            }
        }
    }
}
