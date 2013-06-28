using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace More.Net
{
    [NpcInterface]
    public interface ITunnelControlServer
    {
        void Heartbeat();
        void OpenTunnel(String targetIPOrHost, UInt16 targetPort, UInt16 accessorListenPort);
    }
    public class TunnelControlServer : ITunnelControlServer
    {
        readonly SelectTunnelsThread tunnelsThread;
        readonly Accessor accessor;
        public TunnelControlServer(SelectTunnelsThread tunnelsThread, Accessor accessor)
        {
            this.tunnelsThread = tunnelsThread;
            this.accessor = accessor;
        }

        public void Heartbeat()
        {
        }
        public void OpenTunnel(String targetIPOrHost, UInt16 targetPort, UInt16 accessorListenPort)
        {
            EndPoint targetEndPoint = EndPoints.EndPointFromIPOrHost(targetIPOrHost, targetPort);

            //
            // Connect to the target
            //
            Socket targetSocket = new Socket(targetEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            targetSocket.Connect(targetEndPoint);

            //
            // Connect to the accessor
            //
            Socket accessorSocket = accessor.MakeSocketAndConnectOnPort(accessorListenPort);

            //
            // Add the tunnel
            //
            tunnelsThread.Add(targetSocket, accessorSocket);
        }
    }

}
