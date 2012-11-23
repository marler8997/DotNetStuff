using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Marler.NetworkTools
{
#if !WindowsCE
    public class ProxyException : Exception
    {
        public ProxyException(String message)
            : base(message)
        {
        }
    }

    public class Proxy4Exception : ProxyException
    {
        public static String Proxy4ResultCodeToMessage(byte resultCode)
        {
            switch(resultCode)
            {
                case 90:
                    return "request granted";
                case 91:
                    return "request rejected or failed";
                case 92:
                    return "request rejected because SOCKS server cannot connect to identd on the client";
                case 93:
                    return "request rejected because the client program and identd report different user-ids";
                default:
                    return String.Format("Unknown Result Code {0}",resultCode);
            }
        }

        public readonly Int32 resultCode;

        public Proxy4Exception(byte resultCode)
            : base(Proxy4ResultCodeToMessage(resultCode))

        {
            this.resultCode = resultCode;
        }
    }

    public class Proxy5Exception : ProxyException
    {
        public static String Proxy5ResultCodeToMessage(byte resultCode)
        {
            switch (resultCode)
            {
                case 0:
                    return "request granted";
                case 1:
                    return "general failure";
                case 2:
                    return "connection not allowed by ruleset";
                case 3:
                    return "network unreachable";
                case 4:
                    return "host unreachable";
                case 5:
                    return "connection refused by destination host";
                case 6:
                    return "TTL expired";
                case 7:
                    return "command not supported / protocol error";
                case 8:
                    return "address type not supported";
                default:
                    return String.Format("Unknown Result Code {0}", resultCode);
            }
        }

        public Proxy5Exception(byte resultCode)
            : base(Proxy5ResultCodeToMessage(resultCode))
        {
        }

        public Proxy5Exception(String message)
            : base(message)
        {

        }
    }

    public static class SocksProxy
    {
        public const Byte ProxyVersion4                = 0x04;
        public const Byte ProxyVersion5                = 0x05;

        public const Byte ProxyVersion4ConnectFunction = 0x01;
        public const Byte ProxyVersion4BindFunction    = 0x02;


        public const Byte Proxy5AddressTypeIPv4        = 0x01;
        public const Byte Proxy5AddressTypeDomainName  = 0x03;
        public const Byte Proxy5AddressTypeIPv6        = 0x04;

#if !WindowsCE
        public static void RequestBind(String proxyHost, UInt16 proxyPort, IPAddress bindIP, UInt16 bindPort, byte[] userID)
        {
            if (userID == null) throw new ArgumentNullException("userID");

            byte[] buffer = new byte[9 + userID.Length];

            //
            // Initialize BIND Request Packet
            //
            buffer[0] = 4; // Version 4 of SOCKS protocol
            buffer[1] = 2; // Command 2 "BIND"

            // Insert ipAddress and port into connectRequest packet
            buffer[2] = (byte)(bindPort >> 8);
            buffer[3] = (byte)(bindPort);

            byte[] bindIPArray = bindIP.GetAddressBytes();
            buffer[4] = bindIPArray[0];
            buffer[5] = bindIPArray[1];
            buffer[6] = bindIPArray[2];
            buffer[7] = bindIPArray[3];

            Int32 offset = 8;
            for (int i = 0; i < userID.Length; i++)
            {
                buffer[offset++] = userID[i];
            }
            buffer[offset] = 0;

            Socket proxySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            proxySocket.Connect(proxyHost, proxyPort);
            proxySocket.Send(buffer);
            GenericUtilities.ReadFullSize(proxySocket, buffer, 0, 8);


            if (buffer[0] != 0)
            {
                Console.WriteLine("WARNING: The first byte of the proxy response was expected to be 0 but it was {0}", buffer[0]);
            }

            if (buffer[1] != 90) throw new Proxy4Exception(buffer[1]);

            byte[] responseIPArray = new byte[4];
            responseIPArray[0] = buffer[4];
            responseIPArray[2] = buffer[6];
            responseIPArray[3] = buffer[7];
            responseIPArray[4] = buffer[8];

            IPAddress responseIP = new IPAddress(responseIPArray);
            UInt16 responsePort = (UInt16)((responseIPArray[2] << 8) | responseIPArray[3]);
            Console.WriteLine("Response IP={0}, Port={1}", responseIP, responsePort);
        }
#endif
    }

    public abstract class ProxySocket
    {
        protected readonly IDirectSocketConnector proxyConnector;

        public ProxySocket(IDirectSocketConnector proxyConnector)
        {
            if (proxyConnector == null) throw new ArgumentNullException("proxyConnector");
            this.proxyConnector = proxyConnector;
        }
    }

    public class Proxy4ConnectSocket : ProxySocket
    {
        private readonly Byte[] userID;

        public Proxy4ConnectSocket(byte[] userID, IDirectSocketConnector proxyConnector)
            : base(proxyConnector)
        {
            this.userID = userID;
        }

        public Socket ListenAndAccept(UInt16 port)
        {
            Byte[] bindRequest = new Byte[9];
            Byte[] replyBuffer = new Byte[8];

            bindRequest[0] = 4; // Version 4 of SOCKS protocol
            bindRequest[1] = 2; // Command 2 "BIND"

            bindRequest[2] = (byte)(port >> 8);
            bindRequest[3] = (byte)(port);

            bindRequest[4] = 174;
            bindRequest[5] = 34;
            bindRequest[6] = 174;
            bindRequest[7] = 4;

            bindRequest[8] = 0; // User ID

            //
            // Connect to the proxy server
            //
            Socket socket = proxyConnector.Connect();
            socket.Send(bindRequest);
            GenericUtilities.ReadFullSize(socket, replyBuffer, 0, 8);

            if (replyBuffer[0] != 0)
            {
                Console.WriteLine("WARNING: The first byte of the proxy response was expected to be 0 but it was {0}", replyBuffer[0]);
            }

            if (replyBuffer[1] != 90) throw new Proxy4Exception(replyBuffer[1]);
            
            UInt16 listenPort = (UInt16)(
                (0xFF00 & (replyBuffer[2] << 8)) | 
                (0xFF   & (replyBuffer[3]     ))
                );
            Byte[] ip = new Byte[4];
            ip[0] = replyBuffer[4];
            ip[1] = replyBuffer[5];
            ip[2] = replyBuffer[6];
            ip[3] = replyBuffer[7];
            Console.WriteLine("EndPoint {0}:{1}", new IPAddress(ip).ToString(), listenPort);

            return socket;
        }

        public Socket Connect(IPAddress host, UInt16 port)
        {
            Byte[] connectRequest = new Byte[9 + ((userID == null) ? 0 : userID.Length)];
            Byte[] replyBuffer = new Byte[8];

            //
            // Initialize Connect Request Packet
            //
            connectRequest[0] = 4;                 // Version 4 of SOCKS protocol
            connectRequest[1] = 1;                 // Command 1 "CONNECT"            
            connectRequest[2] = (byte)(port >> 8);
            connectRequest[3] = (byte)(port);
            byte[] hostIPArray = host.GetAddressBytes();
            connectRequest[4] = hostIPArray[0];
            connectRequest[5] = hostIPArray[1];
            connectRequest[6] = hostIPArray[2];
            connectRequest[7] = hostIPArray[3];
            Int32 offset = 8;
            if (userID != null)
            {
                for (int i = 0; i < userID.Length; i++)
                {
                    connectRequest[offset++] = userID[i];
                }
            }
            connectRequest[offset] = 0;

            //
            // Connect to the proxy server
            //
            Socket socket = proxyConnector.Connect();
            socket.Send(connectRequest);
            GenericUtilities.ReadFullSize(socket, replyBuffer, 0, 8);

            if (replyBuffer[0] != 0)
            {
                Console.WriteLine("WARNING: The first byte of the proxy response was expected to be 0 but it was {0}", replyBuffer[0]);
            }

            if (replyBuffer[1] != 90) throw new Proxy4Exception(replyBuffer[1]);

            return socket;
        }
    }

    public class Proxy4aConnectSocket : ProxySocket
    {
        private readonly byte[] userID;
        public readonly Int32 userIDLength;
        public readonly byte[] replyBuffer;

        public Proxy4aConnectSocket(byte[] userID, IDirectSocketConnector proxyConnector)
            : base(proxyConnector)
        {
            this.userID = userID;
            this.userIDLength = (userID == null) ? 0 : userID.Length;

            //
            // Initialize the replyBuffer
            //
            this.replyBuffer = new byte[8];
        }

        public Socket Connect(String host, UInt16 port)
        {
            //
            // Initialize Connect Request Packet
            //
            byte[] hostAsBytes = Encoding.UTF8.GetBytes(host);

            byte[] connectRequest = new byte[10 + userIDLength + hostAsBytes.Length];
            connectRequest[0] = 4; // Version 4 of SOCKS protocol
            connectRequest[1] = 1; // Command 1 "CONNECT"

            connectRequest[2] = (byte)(port >> 8);
            connectRequest[3] = (byte)(port);

            connectRequest[4] = 0;
            connectRequest[5] = 0;
            connectRequest[6] = 0;
            connectRequest[7] = 1;

            Int32 offset = 8;
            if (userID != null)
            {
                for (int i = 0; i < userID.Length; i++)
                {
                    connectRequest[offset++] = userID[i];
                }
            }
            connectRequest[offset++] = 0;


            for (int i = 0; i < hostAsBytes.Length; i++)
            {
                connectRequest[offset++] = hostAsBytes[i];
            }
            connectRequest[offset] = 0;

            //
            // Connect to the proxy server
            //
            Socket socket = proxyConnector.Connect();

            socket.Send(connectRequest);
            GenericUtilities.ReadFullSize(socket, replyBuffer, 0, 8);

            if (replyBuffer[0] != 0)
            {
                Console.WriteLine("WARNING: The first byte of the proxy response was expected to be 0 but it was {0}", replyBuffer[0]);
            }

            if (replyBuffer[1] != 90) throw new Proxy4Exception(replyBuffer[1]);

            return socket;
        }
    }


    public class Proxy5NoAuthenticationConnectSocket : ProxySocket
    {
        private readonly byte[] buffer;

        public Proxy5NoAuthenticationConnectSocket(ProtocolType protocolType, IDirectSocketConnector proxyConnector)
            : base(proxyConnector)
        {
            //int maxAuthenticationBuffer = 3 + usernameBytes.Length + passwordBytes.Length;

            this.buffer = new byte[21];
        }

        public Socket Connect(IPAddress hostIP, UInt16 port)
        {
            Int32 offset;

            Socket socket = proxyConnector.Connect();

            //
            // 1. Send Initial Greeting
            //
            buffer[0] = 5; // Version 5 of the SOCKS protocol
            buffer[1] = 1; // This class only supports 1 protocol
            buffer[2] = 0; // The 'No Authentication' protocol
            socket.Send(buffer, 0, 3, SocketFlags.None);

            //
            // 2. Receive Initial Response
            //
            Console.WriteLine("Waiting for initial response...");
            GenericUtilities.ReadFullSize(socket, buffer, 0, 2);
            if (buffer[0] != 5)
            {
                Console.WriteLine("WARNING: The first byte of the proxy response was expected to be 5 (for SOCKS5 version) but it was {0}", buffer[0]);
            }
            if (buffer[1] != 0) throw new Proxy5Exception(String.Format("Expected server's response to be 0 (Means no authenticaion), but it was {0}", buffer[1]));

            //
            // 3. Issue a CONNECT command
            //
            buffer[0] = 5; // Version 5 of the SOCKS protocol
            buffer[1] = 1; // The CONNECT Command
            buffer[2] = 0; // Reserved
            buffer[3] = SocksProxy.Proxy5AddressTypeIPv4; // 1 = IPv4 Address (3 = DomainName, 4 = IPv6 Address)
            byte[] hostIPArray = hostIP.GetAddressBytes();
            buffer[4] = hostIPArray[0];
            buffer[5] = hostIPArray[1];
            buffer[6] = hostIPArray[2];
            buffer[7] = hostIPArray[3];
            buffer[8] = (byte)(port >> 8);
            buffer[9] = (byte)(port);
            socket.Send(buffer, 0, 10, SocketFlags.None);

            //
            // 4. Get Response
            //
            Console.WriteLine("Waiting for CONNECT response...");
            GenericUtilities.ReadFullSize(socket, buffer, 0, 7);
            if (buffer[0] != 5) Console.WriteLine("WARNING: The first byte of the proxy response was expected to be 5 (for SOCKS5 version) but it was {0}", buffer[0]);
            if (buffer[1] != 0) throw new Proxy5Exception(buffer[1]);
            if (buffer[2] != 0) Console.WriteLine("WARNING: The third byte of the proxy response was expected to be 0 (It is RESERVED) but it was {0}", buffer[2]);

            switch (buffer[3])
            {
                case SocksProxy.Proxy5AddressTypeIPv4:
                    GenericUtilities.ReadFullSize(socket, buffer, 7, 3);

                    //port = (UInt16) ( (buffer[8] << 8) | buffer[9] );
                    break;
                case SocksProxy.Proxy5AddressTypeDomainName:
                    byte[] domainNameArray = new byte[buffer[4]];
                    Int32 bytesLeft = domainNameArray.Length - 2;
                    if (bytesLeft > 0)
                    {
                        GenericUtilities.ReadFullSize(socket, buffer, 7, bytesLeft);
                    }
                    offset = 5;
                    for (int i = 0; i < domainNameArray.Length; i++)
                    {
                        domainNameArray[i] = buffer[offset++];
                    }
                    String domainName = Encoding.UTF8.GetString(domainNameArray);

                    //port = (UInt16) ( (buffer[offset] << 8) | buffer[offset + 1] );
                    break;
                case SocksProxy.Proxy5AddressTypeIPv6:
                    GenericUtilities.ReadFullSize(socket, buffer, 7, 15);

                    //port = (UInt16) ( (buffer[20] << 8) | buffer[21] );
                    break;
                default:
                    throw new Proxy5Exception(String.Format("Expected Address type to be 1, 3, or 4, but got {0}", buffer[3]));
            }

            return socket;
        }
    }

    public class Proxy5UsernamePasswordConnectSocket : ProxySocket
    {
        public readonly byte[] usernameBytes, passwordBytes;

        public readonly byte[] buffer;

        public Proxy5UsernamePasswordConnectSocket(ProtocolType protocolType, IDirectSocketConnector proxyConnector,
            String username, String password)
            : base(proxyConnector)
        {
            if (username == null || username.Length > 255) throw new ArgumentException("username must be a string, with Length <= 255", "username");
            if (password == null || password.Length > 255) throw new ArgumentException("password must be a string, with Length <= 255", "password");
            
            this.usernameBytes = Encoding.UTF8.GetBytes(username);
            this.passwordBytes = Encoding.UTF8.GetBytes(password);

            int maxAuthenticationBuffer = 3 + usernameBytes.Length + passwordBytes.Length;

            this.buffer = new byte[(maxAuthenticationBuffer > 21) ? maxAuthenticationBuffer : 21];
        }


        public Socket Connect(IPAddress hostIP, UInt16 port)
        {
            Int32 offset;
            //Int32 port;

            Socket socket = proxyConnector.Connect();

            //
            // 1. Send Initial Greeting
            //
            buffer[0] = 5; // Version 5 of the SOCKS protocol
            buffer[1] = 1; // This class only supports 1 authentication protocol
            buffer[2] = 2; // Username/Password authenticaion protocol

            socket.Send(buffer, 0, 3, SocketFlags.None);
            
            //
            // 2. Receive Initial Response
            //
            GenericUtilities.ReadFullSize(socket, buffer, 0, 2);
            if (buffer[0] != 5)
            {
                Console.WriteLine("WARNING: The first byte of the proxy response was expected to be 5 (for SOCKS5 version) but it was {0}", buffer[0]);
            }
            if (buffer[1] != 2) throw new Proxy5Exception(String.Format("Expected server's response to be 2 (Means to use Username/Password authenticaion), but it was {0}", buffer[1]));

            //
            // 3. Send Username/Password
            //
            buffer[0] = 1;
            buffer[1] = (byte)usernameBytes.Length;
            offset = 2;
            for (int i = 0; i < usernameBytes.Length; i++)
            {
                buffer[offset++] = usernameBytes[i]; 
            }
            buffer[offset++] = (byte)passwordBytes.Length;
            for (int i = 0; i < passwordBytes.Length; i++)
            {
                buffer[offset++] = passwordBytes[i];
            }
            socket.Send(buffer, 0, offset, SocketFlags.None);

            //
            // 4. Get Response
            //
            GenericUtilities.ReadFullSize(socket, buffer, 0, 2);
            if (buffer[0] != 5)
            {
                Console.WriteLine("WARNING: The first byte of the proxy response was expected to be 5 (for SOCKS5 version) but it was {0}", buffer[0]);
            }
            if (buffer[1] != 0) throw new Proxy5Exception(String.Format("Expected server's response to be 0 (Means authentication succeeded), but it was {0}", buffer[1]));

            //
            // 5. Issue a CONNECT command
            //
            buffer[0] = 5; // Version 5 of the SOCKS protocol
            buffer[1] = 1; // The CONNECT Command
            buffer[2] = 0; // Reserved
            buffer[3] = SocksProxy.Proxy5AddressTypeIPv4; // 1 = IPv4 Address (3 = DomainName, 4 = IPv6 Address)
            byte[] hostIPArray = hostIP.GetAddressBytes();
            buffer[4] = hostIPArray[0];
            buffer[5] = hostIPArray[1];
            buffer[6] = hostIPArray[2];
            buffer[7] = hostIPArray[3];
            buffer[8] = (byte)(port >> 8);
            buffer[9] = (byte)(port);
            socket.Send(buffer, 0, 10, SocketFlags.None);

            //
            // 5. Get Response
            //
            GenericUtilities.ReadFullSize(socket, buffer, 0, 7);
            if (buffer[0] != 5) Console.WriteLine("WARNING: The first byte of the proxy response was expected to be 5 (for SOCKS5 version) but it was {0}", buffer[0]);
            if (buffer[1] != 0) throw new Proxy5Exception(buffer[1]);
            if (buffer[2] != 0) Console.WriteLine("WARNING: The third byte of the proxy response was expected to be 0 (It is RESERVED) but it was {0}", buffer[2]);

            switch(buffer[3])
            {
                case SocksProxy.Proxy5AddressTypeIPv4:
                    GenericUtilities.ReadFullSize(socket, buffer, 7, 3);
                    
                    //port = (UInt16) ( (buffer[8] << 8) | buffer[9] );
                    break;
                case SocksProxy.Proxy5AddressTypeDomainName:
                    byte[] domainNameArray = new byte[buffer[4]]; 
                    Int32 bytesLeft = domainNameArray.Length - 2;
                    if(bytesLeft > 0)
                    {
                        GenericUtilities.ReadFullSize(socket, buffer, 7, bytesLeft);
                    }
                    offset = 5;
                    for(int i = 0; i < domainNameArray.Length; i++)
                    {
                        domainNameArray[i] = buffer[offset++];
                    }
                    String domainName = Encoding.UTF8.GetString(domainNameArray);

                    //port = (UInt16) ( (buffer[offset] << 8) | buffer[offset + 1] );
                    break;
                case SocksProxy.Proxy5AddressTypeIPv6:
                    GenericUtilities.ReadFullSize(socket, buffer, 7, 15);
                    
                    //port = (UInt16) ( (buffer[20] << 8) | buffer[21] );
                    break;
                default:
                    throw new Proxy5Exception(String.Format("Expected Address type to be 1, 3, or 4, but got {0}", buffer[3]));
            }

            return socket;
        }
    }

#endif
}
