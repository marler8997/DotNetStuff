using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.CodeDom.Compiler;
using System.CodeDom;

namespace More.Net
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
            String[] portStrings = portListString.SplitCorrectly(',');
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
    }
#endif
}
