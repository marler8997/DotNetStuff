using System;
using System.Collections.Generic;
using System.Text;

namespace More.Net
{
    public static class PortSet
    {
        public static SortedNumberSet ParsePortSet(String ports)
        {
            return ParsePortSet(ports, 0, (uint)ports.Length);
        }
        public static SortedNumberSet ParsePortSet(String ports, UInt32 offset, UInt32 limit)
        {
            if (limit < offset)
            {
                throw new FormatException(String.Format("limit ({0}) cannot be less then offset ({1})", limit, offset));
            }

            SortedNumberSet portSet = new SortedNumberSet();

            UInt32 lastStartOffset = offset;
            while (true)
            {
                if (offset >= limit)
                {
                    if (offset > lastStartOffset)
                    {
                        String portString = ports.Substring((int)lastStartOffset, (int)(offset - lastStartOffset));
                        UInt16 port;
                        if (!UInt16.TryParse(portString, out port))
                        {
                            throw new FormatException(String.Format("Failed to parse '{0}' as a port", portString));
                        }
                        portSet.Add(port);
                    }

                    return portSet;
                }

                Char c = ports[(int)offset];
                if (c == ',')
                {
                    String portString = ports.Substring((int)lastStartOffset, (int)(offset - lastStartOffset));
                    UInt16 port;
                    if (!UInt16.TryParse(portString, out port))
                    {
                        throw new FormatException(String.Format("Failed to parse '{0}' as a port", portString));
                    }
                    portSet.Add(port);
                    offset++;
                    lastStartOffset = offset;
                }
                else if (c == '-')
                {
                    throw new NotImplementedException();
                    /*
                    String portString = ports.Substring((int)lastStartOffset, (int)(offset - lastStartOffset));
                    UInt16 port;
                    if (!UInt16.TryParse(portString, out port))
                    {
                        throw new FormatException(String.Format("Failed to parse '{0}' as a port", portString));
                    }
                    portSet.Add(port);
                    */
                }
                else
                {
                    offset++;
                }
            }
        }
    }
}
