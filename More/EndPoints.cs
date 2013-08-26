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
    public static class EndPoints
    {
        public static String RemoveOptionalPort(String ipOrHostAndOptionalPort)
        {
            Int32 colonIndex = ipOrHostAndOptionalPort.IndexOf(':');

            if (colonIndex < 0) return ipOrHostAndOptionalPort;
            return ipOrHostAndOptionalPort.Remove(colonIndex);
        }
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
        public static EndPoint EndPointFromIPOrHost(String ipOrHost, UInt16 port)
        {
            IPAddress address;
            if (IPParser.TryParse(ipOrHost, out address)) return new IPEndPoint(address, port);
            return new DnsEndPoint(ipOrHost, port, false);
        }
        public static IPAddress DnsResolve(this String domainName)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(domainName);
            if (hostEntry == null || hostEntry.AddressList == null || hostEntry.AddressList.Length <= 0)
                throw new NoAddresForDomainNameException(domainName);
            return hostEntry.AddressList[0];
        }
        public static IPAddress ParseIPOrResolveHost(this String ipOrHostName)
        {
            IPAddress ip;
            if (IPParser.TryParse(ipOrHostName, out ip)) return ip;
            IPHostEntry hostEntry = Dns.GetHostEntry(ipOrHostName);
            if (hostEntry == null || hostEntry.AddressList == null || hostEntry.AddressList.Length <= 0)
                throw new NoAddresForDomainNameException(ipOrHostName);
            return hostEntry.AddressList[0];
        }
    }
    public class NoAddresForDomainNameException : Exception
    {
        public readonly String domainName;
        public NoAddresForDomainNameException(String domainName)
            : base(String.Format("No address found for domain name '{0}'", domainName))
        {
            this.domainName = domainName;
        }
    }
    public class DnsEndPoint : EndPoint
    {
        public readonly String domainName;
        public readonly Int32 port;
        public readonly Int64 millisecondRefreshTime;

        Int64 lastRefreshStopwatchTicks;

        IPAddress lastRefreshIPAddress;
        IPEndPoint lastRefreshEndPoint;

        public DnsEndPoint(String domainName, Int32 port)
            : this(domainName, port, 0)
        {
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
            IPAddress newAddress = domainName.DnsResolve();
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
