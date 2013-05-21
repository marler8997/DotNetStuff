using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.CodeDom.Compiler;
using System.CodeDom;

namespace Marler.Net
{
    public class ParseException : FormatException
    {
        public ParseException(String message)
            : base(message)
        {
        }
    }

#if !WindowsCE
    public static class ParseUtilities
    {
        public static Byte[] ParseLiteralString(String literal, Int32 offset, out Int32 outLength)
        {
            /*
            Escape  Character Name              Unicode encoding
            ======  ==============              ================
            \\      Backslash                   0x005C
            \0      Null                        0x0000
            \a      Alert                       0x0007
            \b      Backspace                   0x0008
            \f      Form feed                   0x000C
            \n      New line                    0x000A
            \r      Carriage return             0x000D
            \t      Horizontal tab              0x0009
            \v      Vertical tab                0x000B
            \x      Hexadecimal Byte            \x41 = "A" = 0x41
            */

            Int32 length = 0;
            byte[] buffer = new byte[literal.Length];

            Int32 save;

            while (true)
            {
                if (offset >= literal.Length)
                {
                    outLength = length;
                    return buffer;
                    //return builder.ToString();
                }

                save = offset;
                while (true)
                {
                    if (literal[offset] == '\\') break;
                    offset++;
                    if (offset >= literal.Length)
                    {
                        do
                        {
                            buffer[length++] = (byte)literal[save++]; // do I need to do an Encoding?
                        } while (save < literal.Length);
                        outLength = length;
                        return buffer;
                    }
                }

                // the character at i is '\'
                while(save < offset)
                {
                    buffer[length++] = (byte)literal[save++]; // do I need to do an Encoding?
                }
                offset++;
                if (offset >= literal.Length) throw new FormatException("Your literal string ended with '\'");

                char escapeChar = literal[offset];
                if (escapeChar == 'n') buffer[length++] = (byte)'\n';
                else if (escapeChar == '\\') buffer[length++] = (byte)'\\';
                else if (escapeChar == '0') buffer[length++] = (byte)'\0';
                else if (escapeChar == 'a') buffer[length++] = (byte)'\a';
                else if (escapeChar == 'r') buffer[length++] = (byte)'\r';
                else if (escapeChar == 't') buffer[length++] = (byte)'\t';
                else if (escapeChar == 'v') buffer[length++] = (byte)'\v';
                else if (escapeChar == 'x')
                {
                    offset++;
                    if (offset + 1 >= literal.Length) throw new FormatException("The escape character 'x' needs at least 2 digits");

                    Byte output;
                    String sequence = literal.Substring(offset, 2);
                    if (!Byte.TryParse(sequence, System.Globalization.NumberStyles.HexNumber, null, out output))
                    {
                       throw new FormatException(String.Format("Could not parse the hexadecimal escape sequence '\\x{0}' as a hexadecimal byte", sequence));
                    }
                    Console.WriteLine("Parsed '\\x{0}' as '{1}' (0x{2:X2}) ((char)0x{3:X2})", sequence, (char)output, output, (byte)(char)output);
                    buffer[length++] = output;
                    offset++;
                }
                else throw new FormatException(String.Format("Unrecognized escape sequence '\\{0}'", escapeChar));

                offset++;
            }
        }

        public static IPortTunnel[] ParseTunnels(List<String> tunnelStrings)
        {
            IPortTunnel[] tunnelArray = new IPortTunnel[tunnelStrings.Count];
            Int32 index = 0;
            foreach (String tunnelString in tunnelStrings)
            {
                tunnelArray[index++] = ParseTunnel(tunnelString);
            }
            return tunnelArray;
        }
        
        public static IPortTunnel ParseTunnel(String tunnel)
        {
            Int32 dashIndex = tunnel.IndexOf('-');
            if(dashIndex < 0)
            {
                UInt16[] ports = ParsePorts(tunnel);

                if (ports == null || ports.Length == 0) throw new FormatException(String.Format("Could not parse port list of tunnel '{0}'", tunnel));

                if (ports.Length == 1)
                {
                    return new TunnelOnSinglePort(ports[0]);
                }
                if (ports.Length == 2)
                {
                    PortSetDouble portSetDouble = new PortSetDouble(ports[0], ports[1]);
                    return new TunnelMultipleInAndOut(portSetDouble, portSetDouble);
                }
                Array.Sort(ports);
                PortSetArray portSetArray = new PortSetArray(ports);
                return new TunnelMultipleInAndOut(portSetArray, portSetArray);
            }
            else
            {
                UInt16 [] firstPorts = ParsePorts(tunnel.Substring(0, dashIndex));

                if(firstPorts == null || firstPorts.Length == 0) throw new FormatException(String.Format("Could not parse first port list (before the '-') of tunnel '{0}'", tunnel));

                UInt16 [] secondPorts = ParsePorts(tunnel.Substring(dashIndex+1));
                
                if(secondPorts == null || secondPorts.Length == 0) throw new FormatException(String.Format("Could not parse second port list (after the '-') of tunnel '{0}'", tunnel));


                //
                // Negotiate the Tunnel Type
                //
                UInt16[] smaller, bigger;
                if (firstPorts.Length <= secondPorts.Length)
                {
                    smaller = firstPorts;
                    bigger = secondPorts;
                }
                else
                {
                    smaller = secondPorts;
                    bigger = firstPorts;
                }

                if (smaller.Length == 1)
                {
                    UInt16 port = smaller[0];

                    if (bigger.Length == 1)
                    {
                        if (port == bigger[0])
                        {
                            return new TunnelOnSinglePort(port);
                        }
                        return new TunnelOneInOneOut(port, bigger[0]);
                    }
                    if (bigger.Length == 2)
                    {
                        return new TunnelOneInMultipleOut(port, new PortSetDouble(bigger[0], bigger[1]));
                    }
                    Array.Sort(bigger);
                    return new TunnelOneInMultipleOut(port, new PortSetArray(bigger));
                }

                if (smaller.Length == 2)
                {
                    PortSetDouble smallerPortSet = new PortSetDouble(smaller[0], smaller[1]);
                    if (bigger.Length == 2)
                    {
                        return new TunnelMultipleInAndOut(smallerPortSet,
                            new PortSetDouble(bigger[0], bigger[1]));
                    }
                    Array.Sort(bigger);
                    return new TunnelMultipleInAndOut(smallerPortSet, new PortSetArray(bigger));
                }

                Array.Sort(smaller);
                Array.Sort(bigger);
                return new TunnelMultipleInAndOut(new PortSetArray(smaller), new PortSetArray(bigger));
            }
        }

        public static PortSet ParsePortSet(List<String> portList, Int32 offset)
        {
            if (offset >= portList.Count) throw new ArgumentOutOfRangeException("offset",
                 String.Format("offset must be less than portList.Count (which is {0})", portList.Count));

            UInt16[] ports = new UInt16[portList.Count - offset];
            for (Int32 i = 0; i < offset; i++)
            {
                if(!UInt16.TryParse(portList[offset + i], out ports[i]))
                {
                    throw new FormatException(String.Format("Could not parse port '{0}'", portList[offset + i]));
                }
            }

            if (ports.Length == 1) return new PortSetSingle(ports[0]);
            if (ports.Length == 2) return new PortSetDouble(ports[0], ports[1]);
            Array.Sort(ports);
            return new PortSetArray(ports);
        }

        public static PortSet ParsePortSet(String portListString)
        {
            UInt16[] ports = ParsePorts(portListString);

            if (ports == null || ports.Length == 0) throw new FormatException(String.Format("Could not parse port list '{0}'", portListString));

            if (ports.Length == 1) return new PortSetSingle(ports[0]);
            if (ports.Length == 2) return new PortSetDouble(ports[0], ports[1]);
            Array.Sort(ports);
            return new PortSetArray(ports);
        }

        public static UInt16[] ParsePorts(String portListString)
        {
            String[] portStrings = ParseUtilities.SplitCorrectly(portListString, ',');
            if(portStrings == null || portStrings.Length == 0)
            {
                return null;
            }
            UInt16 [] ports = new UInt16[portStrings.Length];
            for(int i = 0; i < portStrings.Length; i++)
            {
                if(!UInt16.TryParse(portStrings[i],out ports[i]))
                {
                    return null;
                }
            }

            return ports;
        }

        public static String ParseHostAndPort(String hostAndPort, out UInt16 port)
        {
            Int32 colonIndex = hostAndPort.IndexOf(':');
            if (colonIndex < 0) throw new ParseException(String.Format("'{0}' needs a port, i.e. '{0}:80' would work", hostAndPort));
            if (colonIndex == 0) throw new ParseException(String.Format("'{0}' needs a host before the colon at the beginning", hostAndPort));
            if (colonIndex >= hostAndPort.Length - 1) throw new ParseException(String.Format("'{0}' needs a port after the colon", hostAndPort));

            String hostString = hostAndPort.Substring(0, colonIndex);
            String portString = hostAndPort.Substring(colonIndex + 1);

            if (!UInt16.TryParse(portString, out port))
            {
                throw new ParseException(String.Format("Port '{0}', could not be parsed as a 2 byte unsigned integer", portString));
            }
            if (port == 0) throw new ParseException("You can't have a port of 0");

            return hostString;
        }
        public static String[] SplitCorrectly(String str, Char seperator)
        {
            if (str == null || str.Length == 0) return null;

            if (str[0] == seperator) throw new FormatException(String.Format("In the string '{0}', the first character can't be a seperator '{1}'", 
                str, seperator));
            if (str[str.Length - 1] == seperator) throw new FormatException(String.Format("In the string '{0}', the last character can't be a seperator '{1}'",
                str, seperator));

            Int32 seperatorCount = 0;
            for (int i = 1; i < str.Length - 1; i++)
            {
                if (str[i] == seperator)
                {
                    if (str[i - 1] == seperator)
                    {
                        throw new FormatException(String.Format("In the string '{0}', expected something in between the seperator '{1}'", 
                            str, seperator));
                    }
                    seperatorCount++;
                }
            }

            String[] splitStrings = new String[seperatorCount + 1];
            Int32 splitOffset = 0;

            Int32 lastOffset = 0;
            Int32 currentOffset = 1;
            while(currentOffset < str.Length)
            {
                if (str[currentOffset] == seperator)
                {
                    splitStrings[splitOffset++] = str.Substring(lastOffset, currentOffset - lastOffset);
                    lastOffset = currentOffset + 1;
                    currentOffset += 2;
                }
                else
                {
                    currentOffset++;
                }

            }
            
            splitStrings[splitOffset++] = str.Substring(lastOffset, currentOffset - lastOffset);

            return splitStrings;
        }
    }
#endif
}
