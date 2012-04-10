using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.OptionsParser
{
    public class OptionException : Exception
    {
        public readonly Option option;

        public OptionException(Option option, String message)
            : base(message)
        {
            this.option = option;
        }
    }

    public class UnknownOptionException : OptionException
    {
        public UnknownOptionException(Char letter)
            : base(null, String.Format("Unknown Option '-{0}'", letter))
        {
        }
    }

    public class OptionParseException : OptionException
    {
        public OptionParseException(Option option, String message)
            : base(option, "Error Parsing Option -" + option.letter + " (" + option.usageName + "): " + message)
        { }
    }

    public class OptionParseDuplicateException : OptionParseException
    {
        public OptionParseDuplicateException(Option option)
            : base(option, "option found twice")
        { }
    }

    public class OptionParseInvalidArgumentException : OptionParseException
    {
        public OptionParseInvalidArgumentException(Option option, String invalidArgument, String whatIsWrong)
            : base(option, "invalid argument \"" + invalidArgument + "\", " + whatIsWrong)
        { }
    }
}
