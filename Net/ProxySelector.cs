using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

using More;

namespace More.Net
{
    public interface IEndPointSet
    {
        Boolean InSet(IPEndPoint endPoint);
        Boolean InSet(DnsEndPoint endPoint);
        Boolean InSet(EndPoint endPoint);
    }
    public class EndPointSetByPort : IEndPointSet
    {
        public bool InSet(IPEndPoint endPoint)
        {
            throw new NotImplementedException();
        }
        public bool InSet(DnsEndPoint endPoint)
        {
            throw new NotImplementedException();
        }
        public bool InSet(EndPoint endPoint)
        {
            throw new NotImplementedException();
        }
    }
    public class IPV4EndPointSet : IEndPointSet
    {
        public UInt32 mask;
        public bool InSet(IPEndPoint endPoint)
        {
            throw new NotImplementedException();
        }
        public bool InSet(DnsEndPoint endPoint)
        {
            throw new NotImplementedException();
        }
        public bool InSet(EndPoint endPoint)
        {
            throw new NotImplementedException();
        }
    }
    public class CatchAllEndPointSet : IEndPointSet
    {
        private static CatchAllEndPointSet instance = null;
        public static CatchAllEndPointSet Instance
        {
            get
            {
                if (instance == null) instance = new CatchAllEndPointSet();
                return instance;
            }
        }
        private CatchAllEndPointSet() {}
        public Boolean InSet(IPEndPoint endPoint)
        {
            return true;
        }
        public Boolean InSet(DnsEndPoint endPoint)
        {
            return true;
        }
        public bool InSet(EndPoint endPoint)
        {
            return true;
        }
    }
    public class EndPointSetAndConnector
    {
        public readonly IEndPointSet endPointSet;
        public readonly ISocketConnector connector;
        public EndPointSetAndConnector(IEndPointSet endPointSet, ISocketConnector connector)
        {
            this.endPointSet = endPointSet;
            this.connector = connector;
        }
    }
    public interface IProxySelector
    {
        void Connect(Socket socket, IPEndPoint ipEndPoint);
        void Connect(Socket socket, DnsEndPoint dnsEndPoint);
        void Connect(Socket socket, EndPoint endPoint);
    }
    public class NoProxy : IProxySelector
    {
        private static NoProxy instance = null;
        public static NoProxy Instance
        {
            get
            {
                if (instance == null) instance = new NoProxy();
                return instance;
            }
        }
        private NoProxy() { }
        public void Connect(Socket socket, IPEndPoint ipEndPoint)
        {
            socket.Connect(ipEndPoint);
        }
        public void Connect(Socket socket, DnsEndPoint dnsEndPoint)
        {
            socket.Connect(dnsEndPoint);
        }
        public void Connect(Socket socket, EndPoint endPoint)
        {
            socket.Connect(endPoint);
        }
    }
    public class SingleEndPointConnectorSelector : IProxySelector
    {
        public readonly IEndPointSet endPointSet;
        public readonly ISocketConnector connector;
        public SingleEndPointConnectorSelector(IEndPointSet endPointSet, ISocketConnector connector)
        {
            this.endPointSet = endPointSet;
            this.connector = connector;
        }
        public void Connect(Socket socket, IPEndPoint ipEndPoint)
        {
            if (endPointSet.InSet(ipEndPoint))
            {
                if (connector == null) socket.Connect(ipEndPoint); else connector.Connect(socket, ipEndPoint);
            }
            else
            {
                socket.Connect(ipEndPoint);
            }
        }
        public void Connect(Socket socket, DnsEndPoint dnsEndPoint)
        {
            if (endPointSet.InSet(dnsEndPoint))
            {
                if (connector == null) socket.Connect(dnsEndPoint); else connector.Connect(socket, dnsEndPoint);
            }
            else
            {
                socket.Connect(dnsEndPoint);
            }
        }
        public void Connect(Socket socket, EndPoint endPoint)
        {
            if (endPointSet.InSet(endPoint))
            {
                if (connector == null) socket.Connect(endPoint); else connector.Connect(socket, endPoint);
            }
            else
            {
                socket.Connect(endPoint);
            }
        }
    }
    public class SingleConnectorSelector : IProxySelector
    {
        public readonly ISocketConnector connector;
        public SingleConnectorSelector(ISocketConnector connector)
        {
            this.connector = connector;
        }
        public void Connect(Socket socket, IPEndPoint ipEndPoint)
        {
            if (connector == null) socket.Connect(ipEndPoint); else connector.Connect(socket, ipEndPoint);
        }
        public void Connect(Socket socket, DnsEndPoint dnsEndPoint)
        {
            if (connector == null) socket.Connect(dnsEndPoint); else connector.Connect(socket, dnsEndPoint);
        }
        public void Connect(Socket socket, EndPoint endPoint)
        {
            if (connector == null) socket.Connect(endPoint); else connector.Connect(socket, endPoint);
        }
    }
    public class ConnectorSelectorByPriority : IProxySelector
    {
        EndPointSetAndConnector[] connectors;

        // The proxies are in order of priority
        public ConnectorSelectorByPriority(EndPointSetAndConnector[] connectors)
        {
            this.connectors = connectors;
        }
        public void Connect(Socket socket, IPEndPoint ipEndPoint)
        {
            if (connectors != null && connectors.Length > 0)
            {
                for (int i = 0; i < connectors.Length; i++)
                {
                    EndPointSetAndConnector setAndProxy = connectors[i];
                    if (setAndProxy.endPointSet.InSet(ipEndPoint))
                    {
                        if (setAndProxy.connector == null) socket.Connect(ipEndPoint);
                        else setAndProxy.connector.Connect(socket, ipEndPoint);
                        return;
                    }
                }
            }
            socket.Connect(ipEndPoint);
        }
        public void Connect(Socket socket, DnsEndPoint dnsEndPoint)
        {
            if (connectors != null && connectors.Length > 0)
            {
                for (int i = 0; i < connectors.Length; i++)
                {
                    EndPointSetAndConnector setAndProxy = connectors[i];
                    if (setAndProxy.endPointSet.InSet(dnsEndPoint))
                    {
                        if (setAndProxy.connector == null) socket.Connect(dnsEndPoint);
                        else setAndProxy.connector.Connect(socket, dnsEndPoint);
                        return;
                    }
                }
            }
            socket.Connect(dnsEndPoint);
        }
        public void Connect(Socket socket, EndPoint endPoint)
        {
            if (connectors != null && connectors.Length > 0)
            {
                for (int i = 0; i < connectors.Length; i++)
                {
                    EndPointSetAndConnector setAndProxy = connectors[i];
                    if (setAndProxy.endPointSet.InSet(endPoint))
                    {
                        if (setAndProxy.connector == null) socket.Connect(endPoint);
                        else setAndProxy.connector.Connect(socket, endPoint);
                        return;
                    }
                }
            }
            socket.Connect(endPoint);
        }
    }
}
