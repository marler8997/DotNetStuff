using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using More;

namespace More.Net
{
    public interface ISocketConnector
    {
        String ConnectorString(EndPoint endPoint);

        void Connect(Socket socket, IPEndPoint ipEndPoint);
        void Connect(Socket socket, DnsEndPoint dnsEndPoint);
        void Connect(Socket socket, EndPoint endPoint);
    }
    public abstract class SocketGenericEndPointConnector : ISocketConnector
    {
        public abstract String ConnectorString(EndPoint endPoint);
        public void Connect(Socket socket, IPEndPoint ipEndPoint)
        {
            Connect(socket, (EndPoint)ipEndPoint);
        }
        public void Connect(Socket socket, DnsEndPoint dnsEndPoint)
        {
            Connect(socket, (EndPoint)dnsEndPoint);
        }
        public abstract void Connect(Socket socket, EndPoint endPoint);
    }
    public abstract class SocketIPOrDnsConnector : ISocketConnector
    {
        public abstract String ConnectorString(EndPoint endPoint);
        public abstract void Connect(Socket socket, IPEndPoint ipEndPoint);
        public abstract void Connect(Socket socket, DnsEndPoint dnsEndPoint);
        public void Connect(Socket socket, EndPoint endPoint)
        {
            IPEndPoint ipEndPoint = endPoint as IPEndPoint;
            if (ipEndPoint != null)
            {
                Connect(socket, ipEndPoint);
                return;
            }

            DnsEndPoint dnsEndPoint = endPoint as DnsEndPoint;
            if (dnsEndPoint != null)
            {
                Connect(socket, dnsEndPoint);
                return;
            }

            throw new InvalidOperationException(String.Format("Unknown EndPoint class '{0}'", endPoint.GetType().Name));
        }
    }
    public static class ConnectorParser
    {
        public static String Format(ProxyType proxyType, EndPoint proxyEndPoint, EndPoint targetEndPoint)
        {
            return String.Format("{0}:{1}%{3}", proxyType.ToString().ToLower(), proxyEndPoint.ToString(), targetEndPoint.ToString());
        }

        //
        // EndPoint: [proxy-type:proxy-ip-or-hostname:proxy-port%]ip-or-hostname:port
        //
        public static EndPoint Parse(String endPointString, Int32 defaultPort, out ISocketConnector proxyConnector)
        {
            //
            // Check for proxy
            //
            Int32 percentIndex = endPointString.IndexOf('%');

            if (percentIndex < 0)
            {
                //connector = DirectSocketConnector.Instance;
                proxyConnector = null;
            }
            else
            {
                String proxyString = endPointString.Remove(percentIndex);
                proxyConnector = ParseProxy(proxyString);
                endPointString = endPointString.Substring(percentIndex + 1);
            }

            //
            // Parse host and port
            //
            Int32 colonIndex = endPointString.IndexOf(':');

            String ipOrHostName;
            UInt16 port;

            //if (colonIndex == 0) throw new ParseException(String.Format("'{0}' needs a host before the colon at the beginning", endPointString));
            //if (colonIndex >= endPointString.Length - 1) throw new ParseException(String.Format("'{0}' needs a port after the colon", endPointString));
            if (colonIndex < 0)
            {
                if (defaultPort <= 0) throw new ParseException(String.Format("'{0}' needs a port, i.e. '{0}:80' would work", endPointString));

                ipOrHostName = endPointString;
                port = (UInt16)defaultPort;
            }
            else
            {
                ipOrHostName = endPointString.Remove(colonIndex);

                String portString = endPointString.Substring(colonIndex + 1);
                if (!UInt16.TryParse(portString, out port))
                {
                    throw new ParseException(String.Format("Port '{0}', could not be parsed as a 2 byte unsigned integer", portString));
                }
                if (port == 0) throw new ParseException("You can't have a port of 0");
            }

            return EndPoints.EndPointFromIPOrHost(ipOrHostName, port);
        }
        public static ISocketConnector ParseProxy(String proxyString)
        {
            // format
            // http:<ip-or-host>:<port>
            // socks4:<ip-or-host>:<port>
            if (proxyString == null || proxyString.Length <= 0) return null;

            String[] splitStrings = proxyString.Split(':');
            if (splitStrings.Length != 3)
                throw new ParseException(String.Format("Invalid proxy '{0}', expected 'http:<host>:<port>', 'socks4:<host>:<port>' or 'socks5:<host>:<port>'", proxyString));

            String proxyTypeString = splitStrings[0];
            String ipOrHostString = splitStrings[1];
            String portString = splitStrings[2];

            Int32 port;
            if (!Int32.TryParse(portString, out port))
                throw new ParseException(String.Format("Invalid port '{0}'", portString));

            if (proxyTypeString.Equals("socks4", StringComparison.CurrentCultureIgnoreCase))
            {
                return new Socks4Proxy(EndPoints.EndPointFromIPOrHost(ipOrHostString, port), null);
            }
            else if (proxyTypeString.Equals("socks5", StringComparison.CurrentCultureIgnoreCase))
            {
                return new Socks5NoAuthenticationConnectSocket(EndPoints.EndPointFromIPOrHost(ipOrHostString, port));
            }
            else if (proxyTypeString.Equals("gateway", StringComparison.CurrentCultureIgnoreCase))
            {
                return new GatewayProxy(EndPoints.EndPointFromIPOrHost(ipOrHostString, port));
            }
            else if (proxyTypeString.Equals("httpconnect", StringComparison.CurrentCultureIgnoreCase))
            {
                return new HttpConnectProxyProxy(EndPoints.EndPointFromIPOrHost(ipOrHostString, port));
            }

            throw new ParseException(String.Format("Unexpected proxy type '{0}', expected 'http', 'socks4' or 'socks5'", proxyTypeString));
        }
    }
    public enum ProxyType
    {
        Gateway,     // No special data is sent to the proxy, the client just talks to the proxy like it is end intended target.
                     // Protocols can use this type of proxy if the protocol itself contains the intended host name.
                     // Applicable Protocols: HTTP
        HttpConnect, // Initiates connection to the target by using an HTTP CONNECT method
                     // Typically used for HTTPS proxy connections, but could be used for anything.
        Socks4,
        Socks5,
    }
    public abstract class GenericEndPointProxy : SocketGenericEndPointConnector
    {
        public readonly ProxyType type;
        protected GenericEndPointProxy(ProxyType type)
        {
            this.type = type;
        }
    }
    public abstract class IPOrDnsProxy : SocketIPOrDnsConnector
    {
        public readonly ProxyType type;
        protected IPOrDnsProxy(ProxyType type)
        {
            this.type = type;
        }
    }
    public class GatewayProxy : GenericEndPointProxy
    {
        EndPoint gatewayProxyEndPoint;
        public GatewayProxy(EndPoint gatewayProxyEndPoint)
            : base(ProxyType.Gateway)
        {
            this.gatewayProxyEndPoint = gatewayProxyEndPoint;
        }
        public override String ConnectorString(EndPoint endPoint)
        {
            return ConnectorParser.Format(type, gatewayProxyEndPoint, endPoint);
        }
        public override void Connect(Socket socket, EndPoint endPoint)
        {
            socket.Connect(gatewayProxyEndPoint);
        }
    }
    public class HttpConnectProxyProxy : IPOrDnsProxy
    {
        EndPoint httpConnectProxyEndPoint;
        public HttpConnectProxyProxy(EndPoint httpConnectProxyEndPoint)
            : base(ProxyType.HttpConnect)
        {
            this.httpConnectProxyEndPoint = httpConnectProxyEndPoint;
        }
        public override String ConnectorString(EndPoint endPoint)
        {
            return ConnectorParser.Format(type, httpConnectProxyEndPoint, endPoint);
        }
        public override void Connect(Socket socket, DnsEndPoint dnsEndPoint)
        {
            Connect(socket, dnsEndPoint.ToString());
        }
        public override void Connect(Socket socket, IPEndPoint ipEndPoint)
        {
            Connect(socket, ipEndPoint.ToString());
        }
        public void Connect(Socket socket, String targetEndPointString)
        {
            //
            // Check if the proxy end point string is valid
            //
            socket.Connect(httpConnectProxyEndPoint);
            socket.Send(Encoding.UTF8.GetBytes(String.Format("CONNECT {0} HTTP/1.1\r\nHost: {0}\r\n\r\n",
                targetEndPointString)));

            NetworkStream stream = new NetworkStream(socket);

            //
            // Process first line
            //
            for (int i = 0; i < 9; i++)
            {
                Int32 nextByte = stream.ReadByte();
                if ((nextByte & 0xFF) != nextByte) throw new SocketException();
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
            if (!Int32.TryParse(responseCodeString, out responseCode))
                throw new InvalidOperationException(String.Format("First line of HTTP Connect response was not formatted correctly (Expected response code but got '{0}')", responseCodeString));

            if (responseCode != 200) throw new InvalidOperationException(String.Format("Expected response code 200 but got {0}", responseCode));

            //
            // Read until end of response
            //
            Int32 lineLength = 12;

            while (true)
            {
                Int32 nextByte = stream.ReadByte();
                //Console.WriteLine("[HttpsProxyDebug] Got Char '{0}'", (Char)nextByte);
                if ((nextByte & 0xFF) != nextByte) throw new SocketException();

                if (nextByte != '\r')
                {
                    lineLength++;
                }
                else
                {
                    nextByte = stream.ReadByte();
                    if ((nextByte & 0xFF) != nextByte) throw new SocketException();
                    if (nextByte != '\n') throw new InvalidOperationException(String.Format(
                         "Received '\\r' and expected '\\n' next but got (Char)'{0}' (Int32)'{1}'",
                         (Char)nextByte, nextByte));


                    if (lineLength <= 0) break;

                    lineLength = 0;
                }
            }
        }
    }
}
