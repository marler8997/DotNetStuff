using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.OptionsParser
{
    public abstract class Option
    {
        public const String ARG_STRING_SINGLE = "<arg>";
        public const String ARG_STRING_MULTIPLE = "<arg{0}>";

        public readonly Char letter;
        public readonly Byte argCount;

        public readonly String usageName;
        public readonly String description;

        public Boolean isSet;
        public String argString;

        public Option(char letter, Byte argCount, String usageName)
            : this(letter, argCount, usageName, null)
        { }

        public Option(char letter, Byte argCount, String usageName, String description)
        {
            this.letter = letter;
            this.argCount = argCount;

            this.usageName = usageName;
            this.description = description;

            this.isSet = false;
        }

        public void PrintUsage(Byte descriptionWidth)
        {
            String displayArgString = (argCount <= 0) ? String.Empty :
                               (argCount == 1) ? ARG_STRING_SINGLE :
                                String.Format(ARG_STRING_MULTIPLE, argCount);

            Console.WriteLine(" -{0} {1,-9} {2}", letter, displayArgString, usageName);

            // Print the Description
            if (description != null)
            {
                int charsPrinted = 0;
                while (charsPrinted < description.Length)
                {
                    int nextLength = description.Length - charsPrinted;
                    if (nextLength > descriptionWidth)
                    {
                        nextLength = descriptionWidth;
                    }
                    Console.WriteLine("{0,14}{1}", String.Empty, description.Substring(charsPrinted, nextLength));
                    charsPrinted += nextLength;
                }
            }

            // Print Arg Values
            Console.WriteLine("{0,14}{1}", String.Empty, ArgValues());
        }

        public abstract String ArgValues();
        public abstract void ParseArg(String argString);
    }
}
