using System;
using System.Net;

using More;

namespace CampaignApp
{
	public static class CampaignAppServer
	{
		public const String HostName = "capp.marler.info";

        static IPAddress ipAddress;
        public static IPAddress IPAddress
        {
            get
            {
                if (ipAddress == null)
                {
                    ipAddress = EndPoints.DnsResolve(HostName);
                }
                return ipAddress;
            }
        }

		public static readonly More.DnsEndPoint HttpDnsEndPoint
            = new More.DnsEndPoint(HostName, 80);
        public static readonly More.DnsEndPoint HttpsDnsEndPoint
            = new More.DnsEndPoint(HostName, 443);
	}
}

