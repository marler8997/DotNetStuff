using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Marler.Net
{
    enum IPProtocol
    {
        HopOpt    = 0,
        Icmp      = 1,
        Igmp      = 2,
        Ggp       = 3,
        Ip        = 4,
        St        = 5,
        Tcp       = 6,
        Cbt       = 7,
        Egp       = 8,
        Igp       = 9,
        BbnRrcMon = 10,
        NvpII     = 11,
        Pup       = 12,
        Argus     = 13,
        Emcom     = 14,
        Xnet      = 15,
        Chaos     = 16,
        Udp       = 17,
    }

    class ProxyServerProgram
    {
        static Int32 Main(string[] args)
        {
            ProxyServerOptions optionsParser = new ProxyServerOptions();
            List<String> nonOptionArgs = optionsParser.Parse(args);

            if (nonOptionArgs.Count > 0)
            {
                Console.WriteLine("Expected 0 non-option arguments, you gave {0}", nonOptionArgs.Count);
                optionsParser.PrintUsage();
                return -1;
            }

            //ProxyServer proxyServer = new ProxyServer(optionsParser.port.ArgValue, optionsParser.socketBackLog.ArgValue);
            //proxyServer.Start();

            //Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Tcp);
            //Console.WriteLine("Port = {0}", optionsParser.port.ArgValue);
            //s.Bind(new IPEndPoint(IPAddress.Any, 0));
            //s.Listen(1);
            //Socket client = s.Accept();
            /*
            Byte [] buffer = new Byte[1024];

            while (true)
            {
                s.IOControl(IOControlCode.ReceiveAll, null
                int read = client.Receive(buffer, SocketFlags.None);

                if(read <= 0) break;

                Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, read));
            }
             * */
            Test();

            return 0;
        }


        private static Byte[] buffer = new Byte[65535];
        public static void Test()
        {
            


            Byte[] buffer = new Byte[4096];

            //Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            //sock.Bind(new IPEndPoint(IPAddress.Any, 0));
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            sock.Bind(new IPEndPoint(IPAddress.Any, 0));
            //sock.Bind(new IPEndPoint(IPAddress.Parse("15.8.29.69"), 0));
            sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, 1);
            //byte[] trueBytes = new byte[] { 1, 0, 0, 0 };
            //byte[] outBytes = new byte[] { 0, 0, 0, 0 };
            //sock.IOControl(IOControlCode.ReceiveAll, trueBytes, outBytes);




            int i = 10;
            while (true)
            {
                if (i >= 10)
                {
                    i = 0;
                    Console.WriteLine("---------------------------------------------------------------------------");
                    Console.WriteLine("DateTime             | Bytes     | IP          Source Destination     | Data");
                    Console.WriteLine("---------------------------------------------------------------------------");
                }

                //Console.WriteLine("Waiting for packet...");
                int received = sock.Receive(buffer);

                if (received < 20)
                {
                    if(received > 0)
                    {
                        Console.WriteLine("Error: packets must be at least 20 byte but found one that was {0}", received);
                    }
                    else
                    {
                        Console.WriteLine("Socket.Receive returned {0}", received);
                        break;
                    }
                }

                if (received > 0)
                {
                    Byte ipHeaderLength = (Byte)((buffer[0] & 0xF) << 2);

                    Byte protocol = buffer[9];

                    Byte[] srcAddress = new Byte[] { buffer[12], buffer[13], buffer[14], buffer[15] };
                    Byte[] dstAddress = new Byte[] { buffer[16], buffer[17], buffer[18], buffer[19] };
                    

                    //Console.Write("{0,9} | {1,2} {2,15} {3,15} | ", received,
                    //    ipHeaderLength, new IPAddress(srcAddress), new IPAddress(dstAddress));

                    UInt16 srcPort, dstPort;
                    switch (protocol)
                    {
                        case (Byte)IPProtocol.Tcp:
                            srcPort = (UInt16)(buffer[ipHeaderLength] << 8 | buffer[ipHeaderLength + 1]);
                            dstPort = (UInt16)(buffer[ipHeaderLength + 2] << 8 | buffer[ipHeaderLength + 3]);
                            UInt32 sequenceNumber = (UInt32)(
                                (            buffer[ipHeaderLength +  4] << 24 ) |
                                (0xFF0000 & (buffer[ipHeaderLength +  5] << 16)) |
                                (0xFF00   & (buffer[ipHeaderLength +  6] <<  8)) |
                                (0xFF     & (buffer[ipHeaderLength +  7]      )));
                            UInt32 acknowledgementNumber = (UInt32)(
                                (            buffer[ipHeaderLength +  8] << 24 ) |
                                (0xFF0000 & (buffer[ipHeaderLength +  9] << 16)) |
                                (0xFF00   & (buffer[ipHeaderLength + 10] <<  8)) |
                                (0xFF     & (buffer[ipHeaderLength + 11]      )));
                            int tcpHeaderLength = (buffer[ipHeaderLength + 12] >> 4) << 2;
                            Byte flags = buffer[13];
                            Boolean synFlag = (flags & 0x02) != 0;
                            Boolean ackFlag = (flags & 0x10) != 0;
                            Boolean pshFlag = (flags & 0x08) != 0;

                            String flagsString =
                                (synFlag ?
                                    (ackFlag ?
                                        (pshFlag ? "SYN-ACK-PSH" : "SYN-ACK") :
                                        (pshFlag ? "SYN-PSH" : "SYN")) :
                                    (ackFlag ?
                                        (pshFlag ? "ACK-PSH" : "ACK") :
                                        (pshFlag ? "PSH" : String.Empty)));
                                        


                            i++;
                            Console.Write("{0,-20} | {1,9} | {2,2} {3,15} {4,15} | ", DateTime.Now, received,
                                ipHeaderLength, new IPAddress(srcAddress), new IPAddress(dstAddress));

                            int dataLength = received - ipHeaderLength - tcpHeaderLength;
                            if (dataLength > 0)
                            {
                                Console.WriteLine("Tcp {0,5} > {1,-5}             Seq=0x{2,-8:X} {3} DataLength={4}",
                                    srcPort, dstPort, sequenceNumber,
                                    ackFlag ? String.Format("Ack=0x{0,-8:X}", acknowledgementNumber) : String.Empty,
                                    dataLength);
                            }
                            else
                            {
                                Console.WriteLine("Tcp {0,5} > {1,-5} {2,11} Seq=0x{3,-8:X} {4}",
                                    srcPort, dstPort, flagsString, sequenceNumber,
                                    ackFlag ? String.Format("Ack=0x{0,-8:X}",acknowledgementNumber) : String.Empty);
                            }
                            break;
                        case (Byte)IPProtocol.Udp:
                            srcPort = (UInt16)(buffer[ipHeaderLength    ] << 8 | buffer[ipHeaderLength + 1]);
                            dstPort = (UInt16)(buffer[ipHeaderLength + 2] << 8 | buffer[ipHeaderLength + 3]);

                            /*
                            Console.Write("{0,9} | {1,2} {2,15} {3,15} | ", received,
                                ipHeaderLength, new IPAddress(srcAddress), new IPAddress(dstAddress));
                            Console.WriteLine("Udp {0,5} > {1,-5}", srcPort, dstPort);
                            */
                            break;
                        default:
                            IPProtocol protocolEnum = (IPProtocol)protocol;

                            Console.Write("{0,-20} | {1,9} | {2,2} {3,15} {4,15} | ", DateTime.Now, received,
                                ipHeaderLength, new IPAddress(srcAddress), new IPAddress(dstAddress));
                            Console.WriteLine(protocolEnum.ToString());
                            //Console.WriteLine("\r\n\r\n");
                            break;
                    }
                }

            }
        }

    }
}
