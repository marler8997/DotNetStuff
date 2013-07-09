using System;

namespace More
{
    public class LfdFormatException : FormatException
    {
        public readonly UInt32 lineNumber;
        public readonly String line;

        public LfdFormatException(UInt32 lineNumber, String line, String msg)
            : base(String.Format("Line {0} \"{1}\" : {2}", lineNumber, line, msg))
        {
        }
    }
}
