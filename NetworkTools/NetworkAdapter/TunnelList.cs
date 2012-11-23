using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.NetworkTools
{
    public class TunnelList
    {
        private readonly ITunnel[] tunnels;
        public readonly PortSet fullPortSet;
        private readonly Dictionary<UInt16, Boolean> portsToSendConnectRequests;

        public TunnelList(ITunnel[] tunnels, UInt16 [] portsToSendConnectRequestsArray)
        {
            if (tunnels == null || tunnels.Length <= 0) throw new ArgumentException("tunnels must have at least one element", "tunnels");

            this.tunnels = tunnels;

            this.fullPortSet = tunnels[0].FullPortSet;
            for (int i = 1; i < tunnels.Length; i++)
            {
                this.fullPortSet = this.fullPortSet.Combine(tunnels[i].FullPortSet);
            }

            //
            // Create the dictionary
            //
            if (portsToSendConnectRequests != null)
            {
                this.portsToSendConnectRequests = new Dictionary<UInt16, Boolean>();
                for (int i = 0; i < portsToSendConnectRequestsArray.Length; i++)
                {
                    this.portsToSendConnectRequests[portsToSendConnectRequestsArray[i]] = true;
                }
                for (int i = 0; i < fullPortSet.Length; i++)
                {
                    UInt16 nextKey = fullPortSet[i];
                    if (!portsToSendConnectRequests.ContainsKey(nextKey))
                    {
                        portsToSendConnectRequests[nextKey] = false;
                    }
                }
            }
        }

        public ITunnel IsATunnel(UInt16 port1, UInt16 port2)
        {
            for (int i = 0; i < tunnels.Length; i++)
            {
                if (tunnels[i].IsValidTunnel(port1, port2))
                {
                    return tunnels[i];
                }
            }
            return null;
        }

        public Boolean PortMustSendConnectRequest(UInt16 port)
        {
            if (portsToSendConnectRequests == null) return false;

            Boolean output;
            if(portsToSendConnectRequests.TryGetValue(port, out output))
            {
                return output;
            }
            throw new ArgumentOutOfRangeException("port");
        }

    }
}
