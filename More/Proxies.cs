using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using More;

#if WindowsCE
using IPParser = System.Net.MissingInCEIPParser;
using UInt16Parser = System.MissingInCEUInt16Parser;
#else
using IPParser = System.Net.IPAddress;
using UInt16Parser = System.UInt16;
#endif

namespace More.Net
{
    public struct HostWithOptionalProxy
    {
        public static HostWithOptionalProxy FromIPOrHostWithOptionalPort(
            String ipOrHostOptionalPort, UInt16 defaultPort, Proxy proxy)
        {
            Int32 colonIndex = ipOrHostOptionalPort.IndexOf(':');
            if (colonIndex >= 0)
            {
                // NOTE: I could parse this without creating another string
                String portString = ipOrHostOptionalPort.Substring(colonIndex + 1);
                if (!UInt16Parser.TryParse(portString, out defaultPort))
                {
                    throw new FormatException(String.Format("Port '{0}' could not be parsed as a 2 byte unsigned integer", portString));
                }
                ipOrHostOptionalPort = ipOrHostOptionalPort.Remove(colonIndex);
            }

            return new HostWithOptionalProxy(ipOrHostOptionalPort, defaultPort, proxy);
        }
        public static HostWithOptionalProxy FromIPOrHostWithPort(String ipOrHostWithPort, Proxy proxy)
        {
            Int32 colonIndex = ipOrHostWithPort.IndexOf(':');
            if (colonIndex < 0)
                throw new FormatException(String.Format("Missing colon to designate the port on '{0}'", ipOrHostWithPort));

            // NOTE: I could parse this without creating another string
            String portString = ipOrHostWithPort.Substring(colonIndex + 1);
            UInt16 port;
            if (!UInt16Parser.TryParse(portString, out port))
            {
                throw new FormatException(String.Format("Port '{0}' could not be parsed as a 2 byte unsigned integer", portString));
            }
            String ipOrHost = ipOrHostWithPort.Remove(colonIndex);

            return new HostWithOptionalProxy(ipOrHost, port, proxy);
        }

        public readonly StringEndPoint endPoint;
        public readonly Proxy proxy;

        public HostWithOptionalProxy(String ipOrHost, UInt16 port, Proxy proxy)
        {
            this.endPoint = new StringEndPoint(ipOrHost, port);
            this.proxy = proxy;
            if (proxy != null)
            {
                proxy.PrepareEndPoint(ref this.endPoint);
            }
        }
        public HostWithOptionalProxy(HostWithOptionalProxy other, UInt16 overrideTargetPort)
        {
            this.endPoint = new StringEndPoint(other.endPoint.unparsedIPOrHost, overrideTargetPort);
            this.proxy = other.proxy;
            if (proxy != null)
            {
                proxy.PrepareEndPoint(ref this.endPoint);
            }
        }
        public HostWithOptionalProxy(HostWithOptionalProxy other, Proxy overrideProxy)
        {
            this.endPoint = new StringEndPoint(other.endPoint.unparsedIPOrHost, other.endPoint.port);
            this.proxy = other.proxy;
            if (proxy != null)
            {
                proxy.PrepareEndPoint(ref this.endPoint);
            }
        }
        public Boolean Set
        {
            get
            {
                return endPoint.unparsedIPOrHost != null;
            }
        }
        public String TargetString()
        {
            return endPoint.ToString();
        }
    }
    public abstract class Proxy
    {
        /// <summary>
        /// The ipOrHost is the original string used to get the ip address.
        /// It is either a string of the ip address or a hostname that was resolved to an ip.
        /// This member is typically used for logging purposes.
        /// </summary>
        public readonly String ipOrHost;
        public readonly IPEndPoint endPoint;

        /// <summary>
        /// endPoint cannot be null
        /// </summary>
        public Proxy(String ipOrHost, IPEndPoint endPoint)
        {
            this.ipOrHost = ipOrHost;
            this.endPoint = endPoint;
        }

        public abstract ProxyType Type { get; }

        // Only call if impl is not null
        public String ConnectorString(ref StringEndPoint targetEndPoint)
        {
            return String.Format("{0}:{1}:{2}%{3}", Type,
                ipOrHost, endPoint.Port, targetEndPoint);
        }
        // Only call if impl is not null
        public override String ToString()
        {
            return String.Format("{0}:{1}:{2}", Type, ipOrHost, endPoint.Port);
        }

        /// <summary>
        /// If the proxy is ip/host agnostic, it will not parse or try to resolve
        /// the unparsed ip/host, if it does something different for ips/hostnames,
        /// it will only attempt to parse it, if it can only use ip addresses, it will
        /// try to parse or resolve the hostname to an ip.
        ///
        /// When you pass an endpoint to ProxyConnectTcp or ProxyConnectUdp, it is assumed
        /// that you have already called PrepareEndPoint on it.
        /// </summary>
        /// <param name="ipOrHost"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public abstract void PrepareEndPoint(ref StringEndPoint endPoint);

        // Design Note: The ProxyConnect method could also connect the socket, however,
        //              in some cases the socket will already be connected, so, this api
        //              doesn't wrap connecting the socket.  Note that this means typically
        //              this call will always follow immediately after calling socket.Connect()
        /// <summary>
        /// Setup the proxy connection. The given socket must already be connected.
        /// The endpoint should have been retrieved from the proxy's CreateEndpoint method.
        /// 
        /// Once the method is done, any leftover data from the socket will be in the given buffer
        /// </summary>
        /// <param name="socket">A connected tcp socket</param>
        /// <param name="ipEndPoint">The final destination, retreived from calling proxy.CreateEndpoint(...).</param>
        public abstract void ProxyConnectTcp(Socket socket, StringEndPoint endpoint,
            ProxyConnectOptions options, ref BufStruct buf);
        /// <summary>
        /// Setup a UDP proxy.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="endpoint"></param>
        public abstract void ProxyConnectUdp(Socket socket, StringEndPoint endpoint);
    }
    public static class ConnectorParser
    {
        /// <summary>
        /// A connector is a hostname/port combo with an option proxy specifier prefix of the format:
        ///   [proxy-type:proxy-ip-or-hostname:proxy-port%]ip-or-hostname:port
        /// </summary>
        public static HostWithOptionalProxy ParseConnectorWithPortAndOptionalProxy(AddressFamily limit, String connectorString)
        {
            Int32 percentIndex = connectorString.IndexOf('%');
            Proxy proxy;
            if (percentIndex < 0)
            {
                proxy = default(Proxy);
            }
            else
            {
                proxy = ParseProxy(limit, connectorString.Remove(percentIndex));
                connectorString = connectorString.Substring(percentIndex + 1);
            }
            return HostWithOptionalProxy.FromIPOrHostWithPort(connectorString, proxy);
        }
        /// <summary>
        /// A connector is a hostname/port combo with an option proxy specifier prefix of the format:
        ///   [proxy-type:proxy-ip-or-hostname:proxy-port%]ip-or-hostname:port
        /// </summary>
        public static HostWithOptionalProxy ParseConnectorWithOptionalPortAndProxy(AddressFamily limit, String connectorString, UInt16 defaultPort)
        {
            Int32 percentIndex = connectorString.IndexOf('%');
            Proxy proxy;
            if (percentIndex < 0)
            {
                proxy = default(Proxy);
            }
            else
            {
                proxy = ParseProxy(limit, connectorString.Remove(percentIndex));
                connectorString = connectorString.Substring(percentIndex + 1);
            }
            return HostWithOptionalProxy.FromIPOrHostWithOptionalPort(connectorString, defaultPort, proxy);
        }
        /// <summary>
        /// A connector is a hostname/port combo with an option proxy specifier prefix of the format:
        ///   [proxy-type:proxy-ip-or-hostname:proxy-port%]ip-or-hostname:port
        /// </summary>
        public static HostWithOptionalProxy ParseConnectorWithNoPortAndOptionalProxy(AddressFamily limit, String connectorString, UInt16 port)
        {
            Int32 percentIndex = connectorString.IndexOf('%');
            Proxy proxy;
            if (percentIndex < 0)
            {
                proxy = default(Proxy);
            }
            else
            {
                proxy = ParseProxy(limit, connectorString.Remove(percentIndex));
                connectorString = connectorString.Substring(percentIndex + 1);
            }
            return new HostWithOptionalProxy(connectorString, port, proxy);
        }

        /*
        //
        // EndPoint: [proxy-type:proxy-ip-or-hostname:proxy-port%]ip-or-hostname:port
        //
        public static String ParseAndStripProxy(String connectorString, out Proxy proxy)
        {
            //
            // Check for proxy
            //
            Int32 percentIndex = connectorString.IndexOf('%');

            if (percentIndex < 0)
            {
                proxy = null;
                return connectorString;
            }
            else
            {
                String proxyString = connectorString.Remove(percentIndex);
                proxy = ParseProxy(proxyString);
                return connectorString.Substring(percentIndex + 1);
            }
        }
        */

        /// <summary>
        /// Use limitAddressFamily = AddressFamily.Unspecified to not limit address family
        /// </summary>
        /// <param name="limitAddressFamily"></param>
        /// <param name="proxyString"></param>
        /// <returns></returns>
        public static Proxy ParseProxy(AddressFamily specificFamily, String proxySpecifier)
        {
            // format
            // http:<ip-or-host>:<port>
            // socks4:<ip-or-host>:<port>
            if (proxySpecifier == null || proxySpecifier.Length <= 0) return default(Proxy);

            String[] splitStrings = proxySpecifier.Split(':');
            if (splitStrings.Length != 3)
                throw new FormatException(String.Format("Invalid proxy '{0}', expected 'http:<host>:<port>', 'socks4:<host>:<port>' or 'socks5:<host>:<port>'", proxySpecifier));

            String proxyTypeString = splitStrings[0];
            String ipOrHost        = splitStrings[1];
            String portString      = splitStrings[2];

            UInt16 port;
            if (!UInt16Parser.TryParse(portString, out port))
                throw new FormatException(String.Format("Invalid port '{0}'", portString));

            IPEndPoint ipEndPoint = new IPEndPoint(EndPoints.ParseIPOrResolveHost(specificFamily, ipOrHost), port);

            if (proxyTypeString.Equals("socks4", StringComparison.CurrentCultureIgnoreCase))
            {
                return new Socks4Proxy(ipOrHost,ipEndPoint, null);
            }
            else if (proxyTypeString.Equals("socks5", StringComparison.CurrentCultureIgnoreCase))
            {
                return new Socks5NoAuthenticationConnectSocket(ipOrHost,ipEndPoint);
            }
            else if (proxyTypeString.Equals("http", StringComparison.CurrentCultureIgnoreCase))
            {
                return new HttpProxy(ipOrHost,ipEndPoint);
            }
            else if (proxyTypeString.Equals("gateway", StringComparison.CurrentCultureIgnoreCase))
            {
                return new GatewayProxy(ipOrHost,ipEndPoint);
            }
            else if (proxyTypeString.Equals("httpconnect", StringComparison.CurrentCultureIgnoreCase))
            {
                return new HttpConnectProxyProxy(ipOrHost,ipEndPoint);
            }

            throw new FormatException(String.Format("Unexpected proxy type '{0}', expected 'gateway', 'http', 'httpconnect', 'socks4' or 'socks5'", proxyTypeString));
        }
    }
    public enum ProxyType
    {
        Gateway,     // No special data is sent to the proxy, the client just talks to the proxy like it is end intended target.
                     // Protocols can use this type of proxy if the protocol itself contains the intended host name.
                     // Applicable Protocols: HTTP
                     // Note that a typical gateway proxy will also support HttpConnect, in which case you have a full Http proxy.
        HttpConnect, // Initiates connection to the target by using an HTTP CONNECT method
                     // Typically used for HTTPS proxy connections, but could be used for anything.
        Http,        // Gateway for unencrypted HTTP traffic, and HttpConnect for everything else
        Socks4,
        Socks5,

    }
    [Flags]
    public enum ProxyConnectOptions
    {
        None = 0x00,
        ContentIsRawHttp = 0x01,
    }
    /// <summary>
    /// A Gateway proxy is just a proxy that works with no handshake.  Typically it only
    /// works with protocols that include the destination endpoint, for example, HTTP's Host header.
    /// </summary>
    public class GatewayProxy : Proxy
    {
        public GatewayProxy(String ipOrHost, IPEndPoint endPoint)
            : base(ipOrHost, endPoint)
        {
        }
        public override ProxyType Type
        {
            get { return ProxyType.Gateway; }
        }
        public override void PrepareEndPoint(ref StringEndPoint endPoint)
        {
        }
        public override void ProxyConnectTcp(Socket socket, StringEndPoint endpoint,
            ProxyConnectOptions options, ref BufStruct buf)
        {
        }
        public override void ProxyConnectUdp(Socket socket, StringEndPoint endpoint)
        {
        }
    }
    public class HttpConnectProxyProxy : Proxy
    {
        public HttpConnectProxyProxy(String ipOrHost, IPEndPoint endPoint)
            : base(ipOrHost, endPoint)
        {
        }
        public override ProxyType Type
        {
            get { return ProxyType.HttpConnect; }
        }
        public override void PrepareEndPoint(ref StringEndPoint endPoint)
        {
        }
        public override void ProxyConnectUdp(Socket socket, StringEndPoint endpoint)
        {
            throw new NotSupportedException("The Http Connect protocol does not support Udp (as far as I know)");
        }
        public override void ProxyConnectTcp(Socket socket, StringEndPoint endpoint,
            ProxyConnectOptions options, ref BufStruct buf)
        {
            throw new NotImplementedException();
            /*
            //
            // Check if the proxy end point string is valid
            //
            socket.Send(Encoding.UTF8.GetBytes(String.Format(
                "CONNECT {0} HTTP/1.1\r\nHost: {0}:{1}\r\n\r\n",
                endpoint.unparsedIPOrHost, endpoint.port)));

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
            */
        }
    }
    /// <summary>
    /// A Gateway proxy is just a proxy that works with no handshake.  Typically it only
    /// works with protocols that include the destination endpoint, for example, HTTP's Host header.
    /// </summary>
    public class HttpProxy : Proxy
    {
        Buf buf;
        public HttpProxy(String ipOrHost, IPEndPoint endPoint)
            : base(ipOrHost, endPoint)
        {
        }
        public override ProxyType Type
        {
            get { return ProxyType.Http; }
        }
        public override void PrepareEndPoint(ref StringEndPoint endPoint)
        {
        }
        public override void ProxyConnectUdp(Socket socket, StringEndPoint endpoint)
        {
            throw new NotSupportedException("The Http protocol does not support Udp (as far as I know)");
        }
        public override void ProxyConnectTcp(Socket socket, StringEndPoint endpoint,
            ProxyConnectOptions options, ref BufStruct buf)
        {
            if ((options & ProxyConnectOptions.ContentIsRawHttp) == 0)
            {
                throw new NotImplementedException();
            }
        }
    }
}
