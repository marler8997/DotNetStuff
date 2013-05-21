using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

using Marler.Common;

namespace Marler.Net
{
    public static class EndPoints
    {
        public static EndPoint EndPointFromIPOrHost(String ipOrHost, Int32 port)
        {
            IPAddress address;
            if (IPAddress.TryParse(ipOrHost, out address)) return new IPEndPoint(address, port);
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
            if (IPAddress.TryParse(ipOrHostName, out ip)) return ip;
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
