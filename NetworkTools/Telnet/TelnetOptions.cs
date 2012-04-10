using System;
using Marler.OptionsParser;

namespace Marler.NetworkTools
{
    public class TelnetOptions : Options
    {
        public readonly OptionGenericArg<UInt16> port;
        public readonly OptionGenericArg<TelnetWindowSize> windowSize;
        public readonly OptionNoArg wantServerEcho;

        public TelnetOptions()
        {
            port = new OptionGenericArg<UInt16>(UInt16.Parse, 'p', "port");
            port.SetDefault(23);
            AddOption(port);

            windowSize = new OptionGenericArg<TelnetWindowSize>(TelnetWindowSize.Parse, 'w', "Telnet Window Size");
            windowSize.SetDefault(null);
            AddOption(windowSize);

            wantServerEcho = new OptionNoArg('e', "Want Server To Echo", "Tries to negotiate with the server to make the server echo");
            AddOption(wantServerEcho);
        }

        public override void PrintHeader()
        {
            Console.WriteLine("Telnet [options] [host]");
        }
    }

    public class TelnetWindowSize
    {
        public readonly UInt16 width, height;
        public TelnetWindowSize(UInt16 width, UInt16 height)
        {
            this.width = width;
            this.height = height;
        }

        public static TelnetWindowSize Parse(String str)
        {
            Int32 xIndex = str.IndexOf('x');

            if (xIndex <= 0)
            {
                throw new FormatException(String.Format("Could not parse '{0}' as a telnet window size, {1}",
                    str, (xIndex == 0) ? "expected a number before 'x'" : "could not find the 'x' character"));
            }
            if (xIndex >= str.Length - 1)
            {
                throw new FormatException(String.Format("Could not parse '{0}' as a telnet window size, expected a number after 'x'", str));
            }

            UInt16 width = UInt16.Parse(str.Remove(xIndex));
            UInt16 height = UInt16.Parse(str.Substring(xIndex + 1));

            return new TelnetWindowSize(width, height);
        }
    }
}
