using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Marler.NetworkTools
{
    public enum ProxyType
    {
        Http  ,
        Https ,
        Ftp   ,
        Socks4,
        Socks5,
    }

    public abstract class Proxy
    {
        public readonly ProxyType type;
        protected Proxy(ProxyType type)
        {
            this.type = type;
        }

        public abstract void Connect(Socket socket, EndPoint endPoint);
    }

    public class HttpProxy : Proxy
    {
        EndPoint httpProxyEndPoint;

        public HttpProxy(EndPoint httpProxyEndPoint)
            : base(ProxyType.Http)
        {
            this.httpProxyEndPoint = httpProxyEndPoint;
        }
        public override void Connect(Socket socket, EndPoint endPoint)
        {
            socket.Connect(httpProxyEndPoint);
        }
    }

    public class HttpsProxy : Proxy
    {
        EndPoint httpsProxyEndPoint;

        public HttpsProxy(EndPoint httpsProxyEndPoint)
            : base(ProxyType.Https)
        {
            this.httpsProxyEndPoint = httpsProxyEndPoint;

        }
        public override void Connect(Socket socket, EndPoint targetEndPoint)
        {
            String targetEndPointString = targetEndPoint.ToString();

            //
            // Check if the proxy end point string is valid
            //
            Int32 colonIndex = targetEndPointString.IndexOf(':');
            if (colonIndex <= 0) throw new InvalidOperationException(
                 String.Format("Remote End Point String '{0}' is invalid because no colon was found", targetEndPointString));


            socket.Connect(httpsProxyEndPoint);
            socket.Send(Encoding.UTF8.GetBytes(String.Format("CONNECT {0} HTTP/1.1\r\nHost: {0}\r\n\r\n",
                targetEndPointString)));
            
            NetworkStream stream = new NetworkStream(socket);            

            //
            // Process first line
            //
            for(int i = 0; i < 9; i++)
            {
                Int32 nextByte = stream.ReadByte();
                if((nextByte & 0xFF) != nextByte) throw new SocketException();
            }

            //
            // Get response code
            //
            Char[] responseCodeChars = new Char[3];
            responseCodeChars[0] = (Char)stream.ReadByte();
            responseCodeChars[1] = (Char)stream.ReadByte();
            responseCodeChars[2] = (Char)stream.ReadByte();
            String responseCodeString = new String(responseCodeChars);

            Int32 responseCode;
            if(!Int32.TryParse(responseCodeString, out responseCode))
                throw new InvalidOperationException(String.Format("First line of HTTP Connect response was not formatted correctly (Expected response code but got '{0}')", responseCodeString));
           
            if(responseCode != 200) throw new InvalidOperationException(String.Format("Expected response code 200 but got {0}", responseCode));


            //
            // Read until end of response
            //
            Int32 lineLength = 12;


            while (true)
            {
                Int32 nextByte = stream.ReadByte();
                Console.WriteLine("[HttpsProxyDebug] Got Char '{0}'", (Char)nextByte);
                if((nextByte & 0xFF) != nextByte) throw new SocketException();                
                                
                if(nextByte != '\r')
                {
                    lineLength++;
                }
                else
                {
                    nextByte = stream.ReadByte();
                    if((nextByte & 0xFF) != nextByte) throw new SocketException();
                    if(nextByte != '\n') throw new InvalidOperationException(String.Format(
                        "Received '\\r' and expected '\\n' next but got (Char)'{0}' (Int32)'{1}'",
                        (Char)nextByte, nextByte));


                    if (lineLength <= 0) break;

                    lineLength = 0;
                }
            }
        }
    }

    public interface IEndPointSet
    {
        Boolean InSet(EndPoint endPoint);
        Boolean InSet(IPEndPoint endPoint);
        Boolean InSet(DnsEndPoint endPoint);
    }

    public class EndPointSetByPort : IEndPointSet
    {
        public bool InSet(EndPoint endPoint)
        {
            throw new NotImplementedException();
        }
        public bool InSet(IPEndPoint endPoint)
        {
            throw new NotImplementedException();
        }
        public bool InSet(DnsEndPoint endPoint)
        {
            throw new NotImplementedException();
        }
    }

    public class IPV4EndPointSet : IEndPointSet
    {
        public UInt32 mask;

        public bool InSet(EndPoint endPoint)
        {
            throw new NotImplementedException();
        }
        public bool InSet(IPEndPoint endPoint)
        {
            throw new NotImplementedException();
        }
        public bool InSet(DnsEndPoint endPoint)
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
        private CatchAllEndPointSet() { }
        public bool InSet(EndPoint endPoint)
        {
            return true;
        }
        public Boolean InSet(IPEndPoint endPoint)
        {
            return true;
        }
        public Boolean InSet(DnsEndPoint endPoint)
        {
            return true;
        }
    }
    public class EndPointSetAndProxy
    {
        public readonly IEndPointSet endPointSet;
        public readonly Proxy proxy;
        public EndPointSetAndProxy(IEndPointSet endPointSet, Proxy proxy)
        {
            this.endPointSet = endPointSet;
            this.proxy = proxy;
        }
    }


    public interface IProxySelector
    {
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
        public void Connect(Socket socket, EndPoint endPoint)
        {
            socket.Connect(endPoint);
        }
    }
    public class SingleEndPointProxySelector : IProxySelector
    {
        public readonly IEndPointSet endPointSet;
        public readonly Proxy proxy;
        public SingleEndPointProxySelector(IEndPointSet endPointSet, Proxy proxy)
        {
            this.endPointSet = endPointSet;
            this.proxy = proxy;
        }
        public void  Connect(Socket socket, EndPoint endPoint)
        {
            if (endPointSet.InSet(endPoint))
            {
                proxy.Connect(socket, endPoint);
                return;
            }
            socket.Connect(endPoint);
        }
    }
    public class SingleProxySelector : IProxySelector
    {
        public readonly Proxy proxy;
        public SingleProxySelector(Proxy proxy)
        {
            this.proxy = proxy;
        }
        public void  Connect(Socket socket, EndPoint endPoint)
        {
            proxy.Connect(socket, endPoint);
        }
    }
    public class ProxySelectorByPriority : IProxySelector
    {
        EndPointSetAndProxy[] proxies;

        // The proxies are in order of priority
        public ProxySelectorByPriority(EndPointSetAndProxy[] proxies)
        {
            this.proxies = proxies;
        }
        public void Connect(Socket socket, EndPoint endPoint)
        {
            if (proxies != null && proxies.Length > 0)
            {
                for (int i = 0; i < proxies.Length; i++)
                {
                    EndPointSetAndProxy setAndProxy = proxies[i];
                    if (setAndProxy.endPointSet.InSet(endPoint))
                    {
                        setAndProxy.proxy.Connect(socket, endPoint);
                        return;
                    }
                }
            }
            socket.Connect(endPoint);
            return;
        }
    }
}
