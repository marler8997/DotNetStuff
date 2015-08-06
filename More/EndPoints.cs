using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

#if WindowsCE
using IPParser = System.Net.MissingInCEIPParser;
using UInt16Parser = System.MissingInCEUInt16Parser;
#else
using IPParser = System.Net.IPAddress;
using UInt16Parser = System.UInt16;
#endif

namespace More
{
    /// <summary>
    /// A StringEndPoint is used when an endpoint starts out as a string, usually
    /// provided by the user or configuration at runtime.  This class will store the original
    /// string, and provides the ability to to parse/resolve the string as an ip address at any time
    /// and this structure will cache the resulting ip.
    ///
    /// If the ip is cached in this endpoint, then the library will exclusively use the ip for any
    /// network operations, otherwise, it will try to use the original raw string until it's necessary
    /// to parse or resolve the string as an ip.
    ///
    /// This structure is especially useful for proxies since some proxies require
    /// an ip address and some do not.  The proxy can then decide whether or not to resolve
    /// the ip or leave it as a raw string.
    /// </summary>
    public struct StringEndPoint
    {
        public readonly String unparsedIPOrHost;
        public readonly UInt16 port;

        /// <summary>
        /// If parsedOrResolvedIP is null, that means there has been no need
        /// to resolve this endpoint's ip address.
        /// </summary>
        public IPEndPoint parsedOrResolvedIP;

        public StringEndPoint(String unparsedIPOrHost, UInt16 port)
        {
            this.unparsedIPOrHost = unparsedIPOrHost;
            this.port = port;
            this.parsedOrResolvedIP = null;
        }
        public StringEndPoint(StringEndPoint other, UInt16 overridePort)
        {
            this.unparsedIPOrHost = other.unparsedIPOrHost;
            this.port = overridePort;
            if (other.parsedOrResolvedIP == null)
            {
                this.parsedOrResolvedIP = null;
            }
            else
            {
                this.parsedOrResolvedIP = new IPEndPoint(other.parsedOrResolvedIP.Address, overridePort);
            }
        }
        /// <summary>
        /// Only call this if you know it has not been called before.
        /// You can tell if the hostname was an ip by checking whether or not
        /// parsedOrResolvedIP is no longer null.
        /// </summary>
        public void Parse()
        {
            if (parsedOrResolvedIP == null)
            {
                IPAddress address;
                if (IPAddress.TryParse(unparsedIPOrHost, out address))
                {
                    parsedOrResolvedIP = new IPEndPoint(address, port);
                }
            }
        }
        /// <summary>
        /// After calling this, parsedOrResolvedIP will not be null
        /// </summary>
        public void ForceIPResolution(AddressFamily specificFamily)
        {
            if (parsedOrResolvedIP != null)
            {
                if (specificFamily != AddressFamily.Unspecified &&
                    specificFamily != parsedOrResolvedIP.AddressFamily)
                    throw new InvalidOperationException(String.Format("This endpoint has already resolved it's ip to a {0} type address, but you requested to resolve it to a {1} type address",
                        parsedOrResolvedIP.AddressFamily, specificFamily));
            }

            IPAddress address;
            if (!IPAddress.TryParse(unparsedIPOrHost, out address))
            {
                Console.WriteLine("[DEBUG] Resolving '{0}'...", unparsedIPOrHost);
                address = EndPoints.DnsResolve(specificFamily, unparsedIPOrHost);
                Console.WriteLine("[DEBUG] Resolved '{0}' to {1}", unparsedIPOrHost, address);
            }
            this.parsedOrResolvedIP = new IPEndPoint(address, port);
        }
        public override String ToString()
        {
            if (parsedOrResolvedIP == null)
            {
                return unparsedIPOrHost + ":" + port.ToString();
            }
            else
            {
                return parsedOrResolvedIP.ToString();
            }
        }
    }
    /*
    public struct IPOrHostEndPoint
    {
        /// <summary>
        /// Note: host should not be an IP Address
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static IPOrHostEndPoint FromHostThasIsNotAnIP(String host, UInt16 port)
        {
            return new IPOrHostEndPoint(host, port);
        }
        public static IPOrHostEndPoint FromIPOrHost(String ipOrHost, UInt16 port)
        {
            IPAddress address;
            return IPParser.TryParse(ipOrHost, out address) ?
                new IPOrHostEndPoint(new IPEndPoint(address, port)) :
                new IPOrHostEndPoint(ipOrHost, port);
        }
        public static IPOrHostEndPoint FromIPOrHostAndPort(String ipOrHostAndPort)
        {
            Int32 colonIndex = ipOrHostAndPort.IndexOf(':');
            if (colonIndex < 0)
                throw new FormatException(String.Format("Missing colon to designate the port on '{0}'", ipOrHostAndPort));

            String ipOrHost = ipOrHostAndPort.Remove(colonIndex);
            // NOTE: I could parse this without creating another string
            String portString = ipOrHostAndPort.Substring(colonIndex + 1);
            UInt16 port;
            if (!UInt16Parser.TryParse(portString, out port))
            {
                throw new FormatException(String.Format("Port '{0}' could not be parsed as a 2 byte unsigned integer", portString));
            }

            return FromIPOrHost(ipOrHost, port);

        }
        public static IPOrHostEndPoint FromIPOrHostAndOptionalPort(String ipOrHostAndOptionalPort, UInt16 defaultPort)
        {
            Int32 colonIndex = ipOrHostAndOptionalPort.IndexOf(':');

            String ipOrHost;
            UInt16 port;

            if (colonIndex < 0)
            {
                ipOrHost = ipOrHostAndOptionalPort;
                port = (UInt16)defaultPort;
            }
            else
            {
                ipOrHost = ipOrHostAndOptionalPort.Remove(colonIndex);

                String portString = ipOrHostAndOptionalPort.Substring(colonIndex + 1);
                if (!UInt16Parser.TryParse(portString, out port))
                {
                    throw new FormatException(String.Format("Port '{0}' could not be parsed as a 2 byte unsigned integer", portString));
                }
            }

            return FromIPOrHost(ipOrHost, port);
        }

        public readonly IPEndPoint ipEndPoint;
        public readonly String hostName;
        public readonly UInt16 port;
        public IPOrHostEndPoint(IPEndPoint ipEndPoint)
        {
            this.ipEndPoint = ipEndPoint;
            this.hostName = null;
            this.port = (UInt16)ipEndPoint.Port;
        }
        // Note: hostName should never be an ip address
        private IPOrHostEndPoint(String hostName, UInt16 port)
        {
            this.ipEndPoint = null;
            this.hostName = hostName;
            this.port = port;
        }
        // The end point is going to be whatever the ProxyEndPoint
        // was originally.
        public IPOrHostEndPoint(StringEndPoint stringEndPoint)
        {
            IPAddress address;
            if(IPParser.TryParse(stringEndPoint.unparsedIPOrHost, out address))
            {
                this.ipEndPoint = (stringEndPoint.parsedOrResolvedIP == null) ?
                    new IPEndPoint(address, stringEndPoint.port) :
                    stringEndPoint.parsedOrResolvedIP;
                this.hostName = null;
                this.port = (UInt16)ipEndPoint.Port;
            }
            else
            {
                this.ipEndPoint = null;
                this.hostName = stringEndPoint.unparsedIPOrHost;
                this.port = stringEndPoint.port;
            }
        }
        public IPOrHostEndPoint(IPOrHostEndPoint other, UInt16 overridePort)
        {
            if (other.ipEndPoint == null)
            {
                this.ipEndPoint = null;
                this.hostName = other.hostName;
                this.port = overridePort;
            }
            else
            {
                this.ipEndPoint = new IPEndPoint(other.ipEndPoint.Address, overridePort);
                this.hostName = null;
                this.port = overridePort;
            }
        }
        public AddressFamily AddressFamily { get { return AddressFamily.InterNetwork; } }
        public IPEndPoint GetOrResolveToIPEndPoint()
        {
            return (ipEndPoint != null) ? ipEndPoint : new IPEndPoint(EndPoints.DnsResolve(hostName), port);
        }
        public override string ToString()
        {
            return (ipEndPoint == null) ? hostName + ":" + port.ToString() : ipEndPoint.ToString();
        }
    }
    */
    public static class EndPoints
    {
        public static String SplitIPOrHostAndPort(String ipOrHostAndPort, out UInt16 port)
        {
            Int32 colonIndex = ipOrHostAndPort.IndexOf(':');
            if (colonIndex < 0)
            {
                throw new FormatException(String.Format("Missing colon to designate the port on '{0}'", ipOrHostAndPort));
            }

            String portString = ipOrHostAndPort.Substring(colonIndex + 1);
            if (!UInt16Parser.TryParse(portString, out port))
            {
                throw new FormatException(String.Format("Port '{0}', could not be parsed as a 2 byte unsigned integer", portString));
            }
            return ipOrHostAndPort.Remove(colonIndex);
        }
        public static String RemoveOptionalPort(String ipOrHostAndOptionalPort)
        {
            Int32 colonIndex = ipOrHostAndOptionalPort.IndexOf(':');

            if (colonIndex < 0) return ipOrHostAndOptionalPort;
            return ipOrHostAndOptionalPort.Remove(colonIndex);
        }
        /*
        // Deprecated: Use IPOrHostEndPoint instead
        public static EndPoint EndPointFromIPOrHostAndPort(String ipOrHostAndPort)
        {
            Int32 colonIndex = ipOrHostAndPort.IndexOf(':');
            
            if (colonIndex < 0)
            {
                throw new FormatException(String.Format("Missing colon to designate the port on '{0}'", ipOrHostAndPort));
            }

            String ipOrHost = ipOrHostAndPort.Remove(colonIndex);

            String portString = ipOrHostAndPort.Substring(colonIndex + 1);
            UInt16 port;
            if (!UInt16Parser.TryParse(portString, out port))
            {
                throw new FormatException(String.Format("Port '{0}', could not be parsed as a 2 byte unsigned integer", portString));
            }

            return EndPoints.EndPointFromIPOrHost(ipOrHost, port);

        }
        // Deprecated: Use IPOrHostEndPoint instead
        public static EndPoint EndPointFromIPOrHostAndOptionalPort(String ipOrHostAndOptionalPort, UInt16 defaultPort)
        {
            Int32 colonIndex = ipOrHostAndOptionalPort.IndexOf(':');

            String ipOrHost;
            UInt16 port;

            if (colonIndex < 0)
            {
                ipOrHost = ipOrHostAndOptionalPort;
                port = (UInt16)defaultPort;
            }
            else
            {
                ipOrHost = ipOrHostAndOptionalPort.Remove(colonIndex);

                String portString = ipOrHostAndOptionalPort.Substring(colonIndex + 1);
                if (!UInt16Parser.TryParse(portString, out port))
                {
                    throw new FormatException(String.Format("Port '{0}', could not be parsed as a 2 byte unsigned integer", portString));
                }
            }

            return EndPoints.EndPointFromIPOrHost(ipOrHost, port);
        }
        */
        /*
        public static IPEndPoint ParseOrResolve(String ipOrHost, UInt16 port)
        {
            IPAddress address;
            if (IPParser.TryParse(ipOrHost, out address)) return new IPEndPoint(address, port);
            return new IPEndPoint(DnsResolve(ipOrHost), port);
        }
        public static EndPoint EndPointFromIPOrHost(String ipOrHost, UInt16 port)
        {
            IPAddress address;
            if (IPParser.TryParse(ipOrHost, out address)) return new IPEndPoint(address, port);
            return new DnsEndPoint(ipOrHost, port, false);
        }
        */
        public static IPAddress ParseIPOrResolveHost(AddressFamily specificFamily, String ipOrHost)
        {
            IPAddress ip;
            if (IPParser.TryParse(ipOrHost, out ip)) return ip;
            return DnsResolve(specificFamily, ipOrHost);
        }
        public static IPEndPoint ParseIPOrResolveHost(AddressFamily specificFamily, String ipOrHostOptionalPort, UInt16 port)
        {
            return new IPEndPoint(ParseIPOrResolveHost(specificFamily, ipOrHostOptionalPort), port);
        }
        public static IPEndPoint ParseIPOrResolveHostWithOptionalPort(AddressFamily specificFamily, String ipOrHostOptionalPort, UInt16 defaultPort)
        {
            Int32 colonIndex = ipOrHostOptionalPort.IndexOf(':');
            if (colonIndex >= 0)
            {
                // NOTE: I could parse this without creating another string
                String portString = ipOrHostOptionalPort.Substring(colonIndex + 1);
                if (!UInt16Parser.TryParse(portString, out defaultPort))
                {
                    throw new FormatException(String.Format("Port '{0}' could not be parsed as a 2 byte unsigned integer", portString));
                }
                ipOrHostOptionalPort = ipOrHostOptionalPort.Remove(colonIndex);
            }

            return new IPEndPoint(ParseIPOrResolveHost(specificFamily, ipOrHostOptionalPort), defaultPort);
        }
        /*
        public static IPAddress DnsResolve(this String domainName)
        {
            return DnsResolve(domainName, AddressFamily.Unspecified);
        }
        */
        public static IPAddress DnsResolve(AddressFamily specificFamily, String host)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(host);
            if (hostEntry == null || hostEntry.AddressList == null || hostEntry.AddressList.Length <= 0)
                throw new NoAddresForDomainNameException(host);

            if (specificFamily == AddressFamily.Unspecified)
                return hostEntry.AddressList[0];

            for(int i = 0; i < hostEntry.AddressList.Length; i++)
            {
                var address = hostEntry.AddressList[i];
                if (address.AddressFamily == specificFamily)
                    return address;
                Console.WriteLine("[DEBUG] skipping {0} address '{1}'", address.AddressFamily, address);
            }
            throw new NoAddresForDomainNameException(String.Format("No ip address of family '{0}' found for '{1}'",
                specificFamily, host), host);
        }
        /*
        public static IPEndPoint ToIPEndPoint(this EndPoint endPoint)
        {
            IPEndPoint ipEndPoint = endPoint as IPEndPoint;
            if (ipEndPoint != null) return ipEndPoint;

            DnsEndPoint dnsEndPoint = endPoint as DnsEndPoint;
            if (dnsEndPoint != null) return dnsEndPoint.IPEndPoint;

            // TODO: maybe try converting the end point to a string and perform a dns resolve

            throw new InvalidOperationException("Could not convert end point to an IP address");
        }
        */
    }
    public class NoAddresForDomainNameException : Exception
    {
        public readonly String domainName;
        public NoAddresForDomainNameException(String domainName)
            : base(String.Format("No address found for domain name '{0}'", domainName))
        {
            this.domainName = domainName;
        }
        public NoAddresForDomainNameException(String message, String domainName)
            : base(message)
        {
            this.domainName = domainName;
        }
    }
    /// <summary>
    /// Note: this class isn't really useful in the general case.  It helps by allowing you
    /// to not specify where the Dns resolution is performed by creating a DnsEndPoint
    /// instead of converting it to an IPEndPoint first, however, the application should
    /// really make the decision of when to do that resolution anyway.  This class has attempted
    /// to abstract that but it has resulted in keeping tracking of the last time it was refreshed
    /// and checking if that time has expired...a little weird.
    /// I think a better solution would be to delete this class and supplement any apis that support
    /// passing an IPEndPoint with the ability to pass a string/port.  Note that those functions that allow
    /// a string should assume the string is not an ip address, and should fail DNS resolution if it is.
    /// </summary>
    public class DnsEndPoint : EndPoint
    {
        public readonly AddressFamily specificFamily;
        public readonly String domainName;
        public readonly Int32 port;
        public readonly Int64 millisecondRefreshTime;

        Int64 lastRefreshStopwatchTicks;

        IPAddress lastRefreshIPAddress;
        IPEndPoint lastRefreshEndPoint;

        public DnsEndPoint(AddressFamily specificFamily, String domainName, Int32 port)
            : this(domainName, port, 0)
        {
            this.specificFamily = specificFamily;
        }

        // neverRefresh: true to never refresh, false to always refresh
        public DnsEndPoint(String domainName, Int32 port, Boolean neverRefresh)
            : this(domainName, port, neverRefresh ? 0 : -1)
        {
        }

        //
        // millisecondsRefreshTime: negative Always refresh
        // millisecondsRefreshTime:        0 Never refresh
        // millisecondsRefreshTime: positive Refresh after this many milliseconds
        // 
        public DnsEndPoint(String domainName, Int32 port, Int64 millisecondRefreshTime)
        {
            this.domainName = domainName;
            this.port = port;
            this.millisecondRefreshTime = millisecondRefreshTime;

            this.lastRefreshStopwatchTicks = 0;
            this.lastRefreshEndPoint = null;
        }
        public void DnsRefreshAddress()
        {
            IPAddress newAddress = EndPoints.DnsResolve(specificFamily, domainName);
            if (lastRefreshEndPoint == null || !lastRefreshIPAddress.Equals(newAddress))
            {
                //if (lastRefreshEndPoint != null)
                //Console.WriteLine("[DnsEndPointDebug] Address is different old='{0}' new='{1}'", lastRefreshIPAddress, newAddress);
                this.lastRefreshIPAddress = newAddress;
                this.lastRefreshEndPoint = new IPEndPoint(newAddress, port);
            }
        }
        void RefreshAddressIfOldOneTimedOut()
        {
            if (lastRefreshEndPoint == null)
            {
                DnsRefreshAddress();
                return;
            }

            if (millisecondRefreshTime < 0)
            {
                DnsRefreshAddress();
                return;
            }

            if (millisecondRefreshTime > 0)
            {
                Int64 millisecondsSinceRefrech = (Stopwatch.GetTimestamp() - lastRefreshStopwatchTicks).StopwatchTicksAsInt64Milliseconds();
                if (millisecondsSinceRefrech > millisecondRefreshTime)
                {
                    DnsRefreshAddress();
                }
                return;
            }
        }
        public override AddressFamily AddressFamily
        {
            get
            {
                //Console.WriteLine("[DnsEndPointDebug] AddressFamily");
                if (lastRefreshEndPoint == null)
                {
                    DnsRefreshAddress();
                }
                return lastRefreshEndPoint.AddressFamily;
            }
        }
        public override EndPoint Create(SocketAddress socketAddress)
        {
            //Console.WriteLine("[DnsEndPointDebug] Create");
            RefreshAddressIfOldOneTimedOut();
            return lastRefreshEndPoint;
        }
        public IPEndPoint IPEndPoint
        {
            get
            {
                RefreshAddressIfOldOneTimedOut();
                return lastRefreshEndPoint;
            }
        }
        public override SocketAddress Serialize()
        {
            RefreshAddressIfOldOneTimedOut();
            return lastRefreshEndPoint.Serialize();
        }
        public override String ToString()
        {
            //Console.WriteLine("[DnsEndPointDebug] ToString");
            return String.Format("{0}:{1}", domainName, port);
        }
    }
}
