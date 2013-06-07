using System;
using System.Net;
using System.Net.Sockets;

namespace More.Net
{
    class DnsServer
    {
        public readonly DomainNameResolver resolver;
        public readonly UInt16 port;

        public DnsServer(DomainNameResolver resolver, UInt16 port)
        {
            this.resolver = resolver;
            this.port = port;
        }

        public void Run()
        {
            Byte [] buffer = new Byte[1024];

            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                udpSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            }
            catch (SocketException e)
            {
                throw new SocketExceptionWithExtraMessage(e, "Maybe another DnsServer is running or Windows ICS could be using the dns port?");
            }

            while (true)
            {
                int received = udpSocket.Receive(buffer);

                if (received <= 0) break;
                if (received < DnsHeader.ByteLength)
                {
                    Console.Error.WriteLine("Received {0} byte packet but minimum size is {1}", received, DnsHeader.ByteLength);
                    continue;
                }

                DnsHeader dnsHeader = new DnsHeader(buffer, 0);
                if (dnsHeader.isResponseFlag)
                {
                    Console.WriteLine(dnsHeader.ToString());
                }
                else
                {
                    DnsResponseBuilder responseBuilder = new DnsResponseBuilder();

                    Console.WriteLine(dnsHeader.ToString());

                    UInt32 offset = DnsHeader.ByteLength;

                    //
                    // Process Questions
                    //
                    for (int i = 0; i < dnsHeader.qdCount; i++)
                    {
                        DnsQuestion dnsQuestion = new DnsQuestion(buffer, ref offset);

                        if(dnsQuestion.type == (UInt16)DnsTypeCode.URI)
                        {
                            IPAddress ip = resolver.Resolve(dnsQuestion.domainName);
                            if (ip != null)
                            {
                                Console.WriteLine("      - URI Query '{0}' Resolved to Address {1}", dnsQuestion.domainName.name,ip);
                                DnsResourceRecord answerRecord = new DnsResourceRecord(dnsQuestion.domainName,
                                    dnsQuestion.type, dnsQuestion.@class, 86400, ip.GetAddressBytes(), 0, 4);
                            }
                            else
                            {
                                Console.WriteLine("      - URI Query '{0}' No Address Resolution", dnsQuestion.domainName.name);



                            }
                        }
                        else
                        {
                            Console.WriteLine("   - {0}", dnsQuestion);
                        }


                    }
              


                }
            }
        }
    }
}
