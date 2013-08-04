using System;

using More;

namespace CampaignApp
{
	public static class CampaignAppServer
	{
		public const String HostName = "capp.marler.info";

		public static readonly DnsEndPoint httpCampaignAppServerEndPoint
			= new DnsEndPoint(HostName, 80);
		public static readonly DnsEndPoint httpsCampaignAppServerEndPoint
			= new DnsEndPoint(HostName, 443);
	}
}

