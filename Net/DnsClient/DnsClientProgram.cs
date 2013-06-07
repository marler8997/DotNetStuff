using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace More.Net
{
    class DnsClientProgram
    {
        static Int32 Main(string[] args)
        {
            DnsClientOptions optionsParser = new DnsClientOptions();
            List<String> nonOptionArgs = optionsParser.Parse(args);

            if (nonOptionArgs.Count > 1)
            {
                Console.WriteLine("Expected up to 1 non-option argument, you gave {0}", nonOptionArgs.Count);
                optionsParser.PrintUsage();
                return -1;
            }

            ISocketConnector connector = null;
            EndPoint endPoint = null;
            if (nonOptionArgs.Count == 1)
            {
                endPoint = ConnectorParser.Parse(nonOptionArgs[0], out connector);
            }


            Byte[] packet = new Byte[1024];
            UInt32 offset = 0;

            DnsQueryHeader query = new DnsQueryHeader();
            query.id = 0x1234;
            query.opCode = DnsOpCode.Query;
            query.recursionDesired = false;
            query.qdCount = 0;
            query.anCount = 0;
            query.nsCount = 0;
            query.arCount = 0;


            offset = query.InsertDnsHeader(packet, offset);


            for (int i = 0; i < offset; i++)
            {
                Console.WriteLine("[{0}] {1:X}", i, packet[i]);
            }

            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            EndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 53);
            sock.SendTo(packet, (int)offset, SocketFlags.None, ep);

            sock.ReceiveTimeout = 500;
            int received = sock.ReceiveFrom(packet, ref ep);

            if (received > 0)
            {
                Console.WriteLine("Received {0} Bytes: {1}", received, BitConverter.ToString(packet, received));
            }
            else
            {
                Console.WriteLine("Received = {0}", received);
            }





            return 0;
        }
    }
}
