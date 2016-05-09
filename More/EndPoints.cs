﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
    /// 
    /// NOTE: since this is a struct with non-readonly members, alway use 'ref' when passing it to functions
    /// and never make it a readonly member of a struct/class.
    /// </summary>
    public struct StringEndPoint
    {
        public readonly String ipOrHost;
        public readonly UInt16 port;
        public readonly Boolean stringIsAnIP;
        public IPEndPoint ipEndPoint;

        public StringEndPoint(String ipOrHost, UInt16 port)
        {
            this.ipOrHost = ipOrHost;
            this.port = port;

            IPAddress ip;
            if(IPParser.TryParse(ipOrHost, out ip))
            {
                this.stringIsAnIP = true;
                this.ipEndPoint = new IPEndPoint(ip, port);
            }
            else
            {
                this.stringIsAnIP = false;
                this.ipEndPoint = null;
            }
        }
        public StringEndPoint(StringEndPoint other, UInt16 port)
        {
            this.ipOrHost = other.ipOrHost;
            this.port = port;
            this.stringIsAnIP = other.stringIsAnIP;
            if (other.ipEndPoint == null)
            {
                this.ipEndPoint = null;
            }
            else
            {
                this.ipEndPoint = new IPEndPoint(other.ipEndPoint.Address, port);
            }
        }

        /// <summary>
        /// Note: Since this is a struct and this function can modify the fields of the struct,
        /// make sure you are calling this on a "ref" version of the struct, and if it's a member
        /// of another object, make sure it IS NOT readonly.
        /// </summary>
        /// <param name="dnsPriorityQuery">Callback that determines priority to select an address from Dns record.
        ///   Use DnsPriority.(QueryName) for standard queries.</param>
        public void ForceIPResolution(PriorityQuery<IPAddress> dnsPriorityQuery)
        {
            if (ipEndPoint == null)
            {
                ipEndPoint = new IPEndPoint(EndPoints.DnsResolve(ipOrHost, dnsPriorityQuery), port);
            }
        }
        /*
        public void ParseOrResolve(DnsPriority dnsPriority)
        {
            ParseOrResolve(dnsPriority.GetQuery());
        }
        public void ParseOrResolve(PriorityQuery<IPAddress> dnsPriorityQuery)
        {
            if (ipEndPoint == null)
            {
                ipEndPoint = new IPEndPoint(EndPoints.DnsResolve(ipOrHost, dnsPriorityQuery), port);
            }
        }
        */

        /*
        /// <summary>
        /// If the CurrentIPEndPoint is null, will attempt to parse the OriginalIPOrHost as
        /// an ip address. Success is determined by checking if CurrentEndPoint is null afterwards.
        /// </summary>
        public void Parse()
        {
            if (parsedOrResolvedIP == null)
            {
                IPAddress address;
                if (IPParser.TryParse(unparsedIPOrHost, out address))
                {
                    parsedOrResolvedIP = new IPEndPoint(address, port);
                }
            }
        }
        /// <summary>
        /// Forces host to be parsed or resolved to an ip address.
        /// </summary>
        public void ForceIPResolution(DnsPriority dnsPriority)
        {
            ForceIPResolution(dnsPriority.GetQuery());
        }
        /// <summary>
        /// Forces host to be parsed or resolved to an ip address.
        /// </summary>
        public void ForceIPResolution(PriorityQuery<IPAddress> dnsPriorityQuery)
        {
            if (parsedOrResolvedIP != null)
            {
                if (dnsPriorityQuery(parsedOrResolvedIP.Address).IsIgnore)
                {
                    throw new InvalidOperationException(String.Format("This endpoint has already resolved it's ip to a {0} (AddressFamily {1}), the given priority query says this address should have been ignored!",
                        parsedOrResolvedIP, parsedOrResolvedIP.AddressFamily));
                }
                return;
            }

            IPAddress address;
            if (!IPParser.TryParse(unparsedIPOrHost, out address))
            {
                address = EndPoints.DnsResolve(unparsedIPOrHost, dnsPriorityQuery);
            }
            this.parsedOrResolvedIP = new IPEndPoint(address, port);
        }
        public override String ToString()
        {
            if (ipEndPoint == null)
            {
                return String.Format("UNRESOLVED({0}:{1})", unparsedIPOrHost, port);
            }
            else
            {
                return String.Format("RESOLVED({0})", parsedOrResolvedIP.ToString());
            }
        }
        */
    }

    public static class DnsPriority
    {
        public static Priority IPv4ThenIPv6(IPAddress address)
        {
            switch (address.AddressFamily)
            {
                case AddressFamily.InterNetwork  : return Priority.Highest;
                case AddressFamily.InterNetworkV6: return Priority.SecondHighest;
                default: return Priority.Ignore;
            }
        }
        public static Priority IPv4Only(IPAddress address)
        {
            switch (address.AddressFamily)
            {
                case AddressFamily.InterNetwork  : return Priority.Highest;
                default: return Priority.Ignore;
            }
        }
        public static Priority IPv6ThenIPv4(IPAddress address)
        {
            switch (address.AddressFamily)
            {
                case AddressFamily.InterNetwork  : return Priority.SecondHighest;
                case AddressFamily.InterNetworkV6: return Priority.Highest;
                default: return Priority.Ignore;
            }
        }
        public static Priority IPv6Only(IPAddress address)
        {
            switch (address.AddressFamily)
            {
                case AddressFamily.InterNetworkV6: return Priority.Highest;
                default: return Priority.Ignore;
            }
        }
        public static Priority IPv4ThenIPv6ThenOther(IPAddress address)
        {
            switch (address.AddressFamily)
            {
                case AddressFamily.InterNetwork  : return Priority.Highest;
                case AddressFamily.InterNetworkV6: return Priority.SecondHighest;
                default: return new Priority(Priority.HighestValue - 2);
            }
        }
        public static Priority IPv6ThenIPv4ThenOther(IPAddress address)
        {
            switch (address.AddressFamily)
            {
                case AddressFamily.InterNetwork  : return Priority.SecondHighest;
                case AddressFamily.InterNetworkV6: return Priority.Highest;
                default: return new Priority(Priority.HighestValue - 2);
            }
        }
    }
    public static class EndPoints
    {
        /// <summary>
        /// Resolves the DNS host name, and uses the given <paramref name="dnsPriorityQuery"/> to determine which address to use.
        /// </summary>
        /// <param name="host">host to resolve</param>
        /// <param name="dnsPriorityQuery">Callback that determines priority to select an address from Dns record.
        ///   Use DnsPriority.(QueryName) for standard queries.</param>
        /// <returns>The resolved ip address and it's priority</returns>
        public static IPAddress DnsResolve(String host, PriorityQuery<IPAddress> dnsPriorityQuery)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(host);
            IPAddress[] addresses = hostEntry.AddressList;
            if (hostEntry == null || addresses == null || addresses.Length <= 0)
            {
                throw new DnsException("host");
            }
            var priorityValue = addresses.PrioritySelect(dnsPriorityQuery);
            if (priorityValue.value == null)
            {
                throw new NoSuitableAddressesException(host, addresses);
            }
            return priorityValue.value;
        }

        /// <summary>
        /// Resolves the DNS host name, and uses the given <paramref name="dnsPriorityQuery"/> to determine which address to use.
        /// Note: in order to distinguish between dns resolution errors and having no suitable addresses,
        /// it is recommended to not ignore any addresses in your priority query, but instead,
        /// give them low priority and check if the address family is suitable in the return value.
        /// </summary>
        /// <param name="host">host to resolve</param>
        /// <param name="dnsPriorityQuery">Callback that determines priority to select an address from Dns record.
        ///   Use DnsPriority.(QueryName) for standard queries.</param>
        /// <returns>The resolved ip address with the highest priority</returns>
        public static PriorityValue<IPAddress> TryDnsResolve(String host, PriorityQuery<IPAddress> dnsPriorityQuery)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(host);
            IPAddress[] addresses = hostEntry.AddressList;
            if (hostEntry == null || addresses == null || addresses.Length <= 0)
            {
                return new PriorityValue<IPAddress>(Priority.Ignore, null);
            }
            return addresses.PrioritySelect(dnsPriorityQuery);
        }

        /// <summary>
        /// Removes and parses the ':port' prefix from the given string.
        /// </summary>
        /// <param name="ipOrHostAndPort">IP or Hostname with a ':port' postfix</param>
        /// <param name="port">The port number parsed from the host ':port' postfix.</param>
        /// <returns><paramref name="ipOrHostAndPort"/> with the ':port' postix removed.
        /// Also returns the ':port' postix in the form of a parsed port number.</returns>
        /// <exception cref="FormatException">If no ':port' postfix or if port is invalid.</exception>
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
        /// <summary>
        /// Removes and parses the optional ':port' prefix from the given string.
        /// </summary>
        /// <param name="ipOrHostAndPort">IP or Hostname with a ':port' postfix</param>
        /// <param name="port">The port number parsed from the host ':port' postfix.</param>
        /// <returns><paramref name="ipOrHostAndPort"/> with the ':port' postix removed.
        /// Also returns the ':port' postix in the form of a parsed port number.</returns>
        /// <exception cref="FormatException">If no ':port' postfix or if port is invalid.</exception>
        public static String SplitIPOrHostAndOptionalPort(String ipOrHostOptionalPort, ref UInt16 port)
        {
            Int32 colonIndex = ipOrHostOptionalPort.IndexOf(':');
            if (colonIndex < 0)
            {
                return ipOrHostOptionalPort;
            }

            String portString = ipOrHostOptionalPort.Substring(colonIndex + 1);
            if (!UInt16Parser.TryParse(portString, out port))
            {
                throw new FormatException(String.Format("Port '{0}', could not be parsed as a 2 byte unsigned integer", portString));
            }
            return ipOrHostOptionalPort.Remove(colonIndex);
        }

        /// <summary>
        /// Tries to parse the string as an IPAddress.  If that fails, it will resolve
        /// it as a host name and select the best address using the given priority query.
        /// </summary>
        /// <param name="ipOrHost">An ip address or host name</param>
        /// <param name="dnsPriorityQuery">Callback that determines priority to select an address from Dns record.
        ///   Use DnsPriority.(QueryName) for standard queries.</param>
        /// <returns>The parsed or resolved ip address.</returns>
        public static IPAddress ParseIPOrResolveHost(String ipOrHost, PriorityQuery<IPAddress> dnsPriorityQuery)
        {
            IPAddress ip;
            if (IPParser.TryParse(ipOrHost, out ip)) return ip;
            return DnsResolve(ipOrHost, dnsPriorityQuery);
        }

        /// <summary>
        /// It will check if <paramref name="ipOrHostOptionalPort"/>
        /// has a ':port' postfix.  If it does, it will parse that port number and use that instead of <paramref name="defaultPort"/>.
        /// Then tries to parse the string as an IPAddress.  If that fails, it will resolve
        /// it as a host name and select the best address using the given priority query.
        /// </summary>
        /// <param name="ipOrHostOptionalPort">IP or Hostname with an optional ':port' postfix</param>
        /// <param name="defaultPort">The default port to use if there is no ':port' postfix</param>
        /// <param name="dnsPriorityQuery">Callback that determines priority to select an address from Dns record.
        ///   Use DnsPriority.(QueryName) for standard queries.</param>
        /// <returns></returns>
        public static IPEndPoint ParseIPOrResolveHostWithOptionalPort(String ipOrHostOptionalPort, UInt16 defaultPort, PriorityQuery<IPAddress> dnsPriorityQuery)
        {
            Int32 colonIndex = ipOrHostOptionalPort.IndexOf(':');
            if (colonIndex >= 0)
            {
                // NOTE: It would be nice to parse this without creating another string
                String portString = ipOrHostOptionalPort.Substring(colonIndex + 1);
                if (!UInt16Parser.TryParse(portString, out defaultPort))
                {
                    throw new FormatException(String.Format("Port '{0}' could not be parsed as a 2 byte unsigned integer", portString));
                }
                ipOrHostOptionalPort = ipOrHostOptionalPort.Remove(colonIndex);
            }

            return new IPEndPoint(ParseIPOrResolveHost(ipOrHostOptionalPort, dnsPriorityQuery), defaultPort);
        }
    }
    public class DnsException : Exception
    {
        public readonly String host;
        public DnsException(String host)
            : base(String.Format("DNS found no addresses for '{0}'", host))
        {
            this.host = host;
        }
    }
    public class NoSuitableAddressesException : Exception
    {
        public static String GetAddressFamilyStrings(IEnumerable<IPAddress> addresses)
        {
            if (addresses == null)
            {
                return "";
            }
            StringBuilder builder = new StringBuilder();
            HashSet<AddressFamily> found = new HashSet<AddressFamily>();
            foreach(var address in addresses)
            {
                if (!found.Contains(address.AddressFamily))
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(address.AddressFamily);
                    found.Add(address.AddressFamily);
                }
            }
            return builder.ToString();
        }

        public readonly String host;
        public readonly IPAddress[] unsuitableAddresses;
        public NoSuitableAddressesException(String host, IPAddress[] unsuitableAddresses)
            : base(String.Format("DNS for '{0}' succeeded, but no suitable address were found in the set ({1})", host, GetAddressFamilyStrings(unsuitableAddresses)))
        {
            this.host = host;
            this.unsuitableAddresses = unsuitableAddresses;
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
        public readonly String domainName;
        public readonly Int32 port;
        public readonly PriorityQuery<IPAddress> dnsPriorityQuery;
        public readonly Int64 millisecondRefreshTime;

        Int64 lastRefreshStopwatchTicks;

        IPAddress lastRefreshIPAddress;
        IPEndPoint lastRefreshEndPoint;

        public DnsEndPoint(String domainName, Int32 port, PriorityQuery<IPAddress> dnsPriorityQuery)
            : this(domainName, port, dnsPriorityQuery, 0)
        {
        }

        // neverRefresh: true to never refresh, false to always refresh
        public DnsEndPoint(String domainName, Int32 port, PriorityQuery<IPAddress> dnsPriorityQuery, Boolean neverRefresh)
            : this(domainName, port, dnsPriorityQuery, neverRefresh ? 0 : -1)
        {
        }

        //
        // millisecondsRefreshTime: negative Always refresh
        // millisecondsRefreshTime:        0 Never refresh
        // millisecondsRefreshTime: positive Refresh after this many milliseconds
        // 
        public DnsEndPoint(String domainName, Int32 port, PriorityQuery<IPAddress> dnsPriorityQuery, Int64 millisecondRefreshTime)
        {
            this.domainName = domainName;
            this.port = port;
            this.dnsPriorityQuery = dnsPriorityQuery;
            this.millisecondRefreshTime = millisecondRefreshTime;

            this.lastRefreshStopwatchTicks = 0;
            this.lastRefreshEndPoint = null;
        }
        public void DnsRefreshAddress()
        {
            IPAddress newAddress = EndPoints.DnsResolve(domainName, dnsPriorityQuery);
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
