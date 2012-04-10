using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.OptionsParser
{
    public abstract class Options
    {
        public const Byte DefaultUsageWidth = 80;

        private readonly Byte descriptionWidth;
        private Dictionary<Char, Option> optionDictionary;

        public Options()
            : this(DefaultUsageWidth)
        {
        }

        public Options(Byte usageWidth)
        {
            if (usageWidth < 15) throw new ArgumentOutOfRangeException("usageWidth");
            this.descriptionWidth = (Byte)(usageWidth - 14);
            this.optionDictionary = null;
        }

        public List<String> Parse(String[] args)
        {
            Option option;
            List<String> nonOptionArguments = new List<String>();

            for (int i = 0; i < args.Length; i++)
            {
                String current = args[i];

                if (String.IsNullOrEmpty(current)) continue;

                if (current[0] != '-')
                {
                    nonOptionArguments.Add(current);
                    continue;
                }

                if (current.Length < 2)
                {
                    throw new OptionException(null, "found option '-' with no letter");
                }

                if (current[1] == '-')
                {
                    if (current.Length < 3)
                    {
                        throw new OptionException(null, "found option '--' with no name");
                    }

                    throw new NotImplementedException("--<name> has not been implemented");
                }
                else
                {
                    for (int letterIndex = 1; letterIndex < current.Length; letterIndex++)
                    {
                        char optionCharacter = current[letterIndex];

                        if (optionDictionary == null) throw new UnknownOptionException(optionCharacter);
                        if (!optionDictionary.TryGetValue(optionCharacter, out option)) throw new UnknownOptionException(optionCharacter);

                        // Check for duplicates
                        if (option.isSet) throw new OptionParseDuplicateException(option);

                        option.isSet = true;

                        if (option.argCount > 0)
                        {
                            if (letterIndex < current.Length - 1)
                            {
                                throw new OptionException(option, String.Format("option '{0}' needs an argument, but it is in a group of options '{1}' and isn't the last one",
                                    option.letter, current));
                            }

                            for (Byte optionArgIndex = 0; optionArgIndex < option.argCount; optionArgIndex++)
                            {
                                i++;
                                if (i >= args.Length) throw new OptionParseException(option, (option.argCount <= 1) ? "needs an argument" :
                                    String.Format("needs {0} more arguments", option.argCount - optionArgIndex));
                                current = args[i];
                                option.ParseArg(current);
                            }

                        }

                    }
                }
            }

            return nonOptionArguments;
        }

        public Int32 ErrorMessage(String message)
        {
            Console.WriteLine(message);
            Console.WriteLine();
            PrintUsage();
            return -1;
        }

        public Int32 ErrorMessage(String message, params Object[] obj)
        {
            Console.WriteLine(message, obj);
            Console.WriteLine();
            PrintUsage();
            return -1;
        }


        protected void AddOption(Option option)
        {
            if (optionDictionary == null)
            {
                optionDictionary = new Dictionary<char, Option>();
            }

            optionDictionary.Add(option.letter, option);
        }

        public abstract void PrintHeader();
        public void PrintUsage()
        {
            Console.WriteLine("Usage:");

            PrintHeader();

            if (optionDictionary != null)
            {
                Console.WriteLine("Options:");
                foreach (Option option in optionDictionary.Values)
                {
                    option.PrintUsage(descriptionWidth);
                }
            }
        }

        public void PrintStatus()
        {
            if (optionDictionary == null)
            {
                Console.WriteLine("No Options.");
            }
            else
            {
                Console.WriteLine("Options Status:");
                Console.WriteLine("Set Opt        Name");
                foreach (Option option in optionDictionary.Values)
                {
                    String argString = String.Empty;
                    if (option.argCount == 1)
                    {
                        argString = Option.ARG_STRING_SINGLE;
                    }
                    else if (option.argCount > 1)
                    {
                        argString = String.Format("{0,-3} args", option.argCount);
                    }


                    Console.WriteLine("[{0}] -{1} {2,-8} {3}", option.isSet ? "x" : " ", option.letter,
                        argString, option.usageName);
                }
            }
        }

    }

}
