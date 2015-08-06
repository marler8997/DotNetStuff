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
        public readonly Proxy proxy;
        public EndPointSetAndConnector(IEndPointSet endPointSet, Proxy proxy)
        {
            this.endPointSet = endPointSet;
            this.proxy = proxy;
        }
    }
}
