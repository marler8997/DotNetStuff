using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Marler.Net
{
    public enum DnsTypeCode
    {
        A           =  1,       // a host address                            [RFC1035]
        NS          =  2,       // an authoritative name server              [RFC1035]
        MD          =  3,       // a mail destination (OBSOLETE - use MX)    [RFC1035]
        MF          =  4,       // a mail forwarder (OBSOLETE - use MX)      [RFC1035]
        CNAME       =  5,       // the canonical name for an alias           [RFC1035]
        SOA         =  6,       // marks the start of a zone of authority    [RFC1035]
        MB          =  7,       // a mailbox domain name (EXPERIMENTAL)      [RFC1035]
        MG          =  8,       // a mail group member (EXPERIMENTAL)        [RFC1035]
        MR          =  9,       // a mail rename domain name (EXPERIMENTAL)  [RFC1035]
        NULL        = 10,       // a null RR (EXPERIMENTAL)                 [RFC1035]
        WKS         = 11,       // a well known service description         [RFC1035]
        PTR         = 12,       // a domain name pointer                    [RFC1035]
        HINFO       = 13,       // host information                         [RFC1035]
        MINFO       = 14,       // mailbox or mail list information         [RFC1035]
        MX          = 15,       // mail exchange                            [RFC1035]
        TXT         = 16,       // text strings                             [RFC1035]
        RP          = 17,       // for Responsible Person                   [RFC1183]
        AFSDB       = 18,       // for AFS Data Base location               [RFC1183][RFC5864]
        X25         = 19,       // for X.25 PSDN address                    [RFC1183]
        ISDN        = 20,       // for ISDN address                         [RFC1183]
        RT          = 21,       // for Route Through                        [RFC1183]
        NSAP        = 22,       // for NSAP address, NSAP style A record    [RFC1706]
        NSAP_PTR    = 23,       // for domain name pointer, NSAP style      [RFC1348][RFC1637][RFC1706]
        SIG         = 24,       // for security signature                   [RFC4034][RFC3755][RFC2535][RFC2536][RFC2537][RFC2931][RFC3110][RFC3008]
        KEY         = 25,       // for security key                         [RFC4034][RFC3755][RFC2535][RFC2536][RFC2537][RFC2539][RFC3008][RFC3110]
        PX          = 26,       // X.400 mail mapping information           [RFC2163]
        GPOS        = 27,       // Geographical Position                    [RFC1712]
        AAAA        = 28,       // IP6 Address                              [RFC3596]
        LOC         = 29,       // Location Information                     [RFC1876]
        NXT         = 30,       // Next Domain (OBSOLETE)                   [RFC3755][RFC2535]
        EID         = 31,       // Endpoint Identifier                      [Patton][Patton1995]
        NIMLOC      = 32,       // Nimrod Locator                           [Patton][Patton1995]
        SRV         = 33,       // Server Selection                         [RFC2782]
        ATMA        = 34,       // ATM Address                              [ATMDOC]
        NAPTR       = 35,       // Naming Authority Pointer                 [RFC2915][RFC2168][RFC3403]
        KX          = 36,       // Key Exchanger                            [RFC2230]
        CERT        = 37,       // CERT                                     [RFC4398]
        A6          = 38,       // A6 (OBSOLETE - use AAAA)                 [RFC3226][RFC2874][RFC-jiang-a6-to-historic-00.txt]
        DNAME       = 39,       // DNAME                                    [RFC2672]
        SINK        = 40,       // SINK                                     [Eastlake][Eastlake2002]
        OPT         = 41,       // OPT                                      [RFC2671][RFC3225]
        APL         = 42,       // APL                                      [RFC3123]
        DS          = 43,       // Delegation Signer                        [RFC4034][RFC3658]
        SSHFP       = 44,       // SSH Key Fingerprint                      [RFC4255]
        IPSECKEY    = 45,       // IPSECKEY                                 [RFC4025]
        RRSIG       = 46,       // RRSIG                                    [RFC4034][RFC3755]
        NSEC        = 47,       // NSEC                                     [RFC4034][RFC3755]
        DNSKEY      = 48,       // DNSKEY                                   [RFC4034][RFC3755]
        DHCID       = 49,       // DHCID                                    [RFC4701]
        NSEC3       = 50,       // NSEC3                                    [RFC5155]
        NSEC3PARAM  = 51,       // NSEC3PARAM                               [RFC5155]
        //          = 52-54        Unassigned
        HIP         = 55,       // Host Identity Protocol                   [RFC5205]
        NINFO       = 56,       // NINFO                                    [Reid]
        RKEY        = 57,       // RKEY                                     [Reid]
        TALINK      = 58,       // Trust Anchor LINK                        [Wijngaards]
        CDS         = 59,       // Child DS                                 [Barwood]
        //          = 60-98        Unassigned
        SPF         = 99,       //                                          [RFC4408]
        UINFO       = 100,      //                                         [IANA-Reserved]
        UID         = 101,      //                                         [IANA-Reserved]
        GID         = 102,      //                                         [IANA-Reserved]
        UNSPEC      = 103,      //                                         [IANA-Reserved]
        //          = 104-248      Unassigned
        TKEY        = 249,      // Transaction Key                         [RFC2930]
        TSIG        = 250,      // Transaction Signature                   [RFC2845]
        IXFR        = 251,      // incremental transfer                    [RFC1995]
        AXFR        = 252,      // transfer of an entire zone              [RFC1035][RFC5936]
        MAILB       = 253,      // mailbox-related RRs (MB, MG or MR)      [RFC1035]
        MAILA       = 254,      // mail agent RRs (OBSOLETE - see MX)      [RFC1035]
        WILDCARD    = 255,      // A request for all records               [RFC1035]
        URI         = 256,      // URI                                     [Faltstrom]
        CAA         = 257,      // Certification Authority Authorization   [Hallam-Baker]
        //          = 258-32767    Unassigned
        TA          = 32768,    // DNSSEC Trust Authorities                [Weiler]           2005-12-13
        DLV         = 32769,    // DNSSEC Lookaside Validation             [RFC4431]
        //          = 32770-65279  Unassigned
        //          = 65280-65534  Private use
        //          = 65535        Reserved
    }

    public static class Dns2
    {
        public static UInt32 DomainBytesToString(StringBuilder builder, Byte[] bytes, UInt32 offset)
        {
            if (offset >= bytes.Length) throw new FormatException("Dns Packet was too short");

            Byte length = bytes[offset++];

            if (length == 0) return offset;
            if (offset + length >= bytes.Length) throw new FormatException("Dns Packet was too short");

            builder.Append(Encoding.UTF8.GetString(bytes, (int)offset, length));
            offset += length;

            while(true)
            {
                if (offset >= bytes.Length)  throw new FormatException("Dns Packet was too short");

                length = bytes[offset++];

                if (length == 0) return offset;
                if (offset + length >= bytes.Length) throw new FormatException("Dns Packet was too short");

                builder.Append('.');
                builder.Append(Encoding.UTF8.GetString(bytes, (int)offset, length));
                offset += length;
            }
        }

        public static UInt32 InsertResourceRecord(Byte[] packet, UInt32 packetOffset,
            DomainNamePacketHook domainName, UInt16 type, UInt16 @class,
            UInt32 validTimeSeconds, Byte[] rData, UInt32 rDataOffset, UInt16 rDataLength)
        {
            packetOffset = domainName.InsertIntoPacket(packet, packetOffset);

            packet[packetOffset++] = (Byte)(type             >>  8);
            packet[packetOffset++] = (Byte)(type                  );

            packet[packetOffset++] = (Byte)(@class           >>  8);
            packet[packetOffset++] = (Byte)(@class                );

            packet[packetOffset++] = (Byte)(validTimeSeconds >> 24);
            packet[packetOffset++] = (Byte)(validTimeSeconds >> 16);
            packet[packetOffset++] = (Byte)(validTimeSeconds >>  8);
            packet[packetOffset++] = (Byte)(validTimeSeconds      );
            
            packet[packetOffset++] = (Byte)(rDataLength      >>  8);
            packet[packetOffset++] = (Byte)(rDataLength           );

            for (UInt32 limit = rDataOffset + rDataLength; rDataOffset < limit; rDataOffset++)
            {
                packet[packetOffset++] = rData[rDataOffset];
            }
            return packetOffset;
        }




    }


    public class DnsResponseBuilder
    {
        public readonly DnsResponseHeader header;

        public DnsResponseBuilder()
        {
            header = new DnsResponseHeader();
        }

       
    }


    public enum DnsOpCode{ Query = 0, IQuery = 1, Status = 2};
    public enum DnsRCode{
        NoError        = 0,
        FormatError    = 1,
        ServerFailure  = 2,
        NameError      = 3,
        NotImplemented = 4,
        Refused        = 5};

    public class DnsQueryHeader
    {        
        public UInt16 id;
        public DnsOpCode opCode;
        public Boolean recursionDesired;
        public UInt16 qdCount,anCount,nsCount, arCount;

        public UInt32 InsertDnsHeader(Byte[] packet, UInt32 offset)
        {
            packet[offset++] = (Byte)(id >> 8);
            packet[offset++] = (Byte)(id     );

            packet[offset++] = (Byte)(((Byte)opCode << 3) | ( recursionDesired ? 1 : 0 ));
            packet[offset++] = 0;

            packet[offset++] = (Byte)(qdCount >> 8);
            packet[offset++] = (Byte)(qdCount     );

            packet[offset++] = (Byte)(anCount >> 8);
            packet[offset++] = (Byte)(anCount     );

            packet[offset++] = (Byte)(nsCount >> 8);
            packet[offset++] = (Byte)(nsCount     );

            packet[offset++] = (Byte)(arCount >> 8);
            packet[offset++] = (Byte)(arCount     );

            return offset;
        }
    }

    public class DnsResponseHeader
    {
        public UInt16 id;
        public DnsOpCode opCode;
        public Boolean isAuthoritativeResponse;
        public Boolean recursionDesired;
        public Boolean recursionAvailable;
        public DnsRCode rCode;
        public UInt16 qdCount, anCount, nsCount, arCount;

        public UInt32 InsertDnsHeader(Byte[] packet, UInt32 offset)
        {
            packet[offset++] = (Byte)(id >> 8);
            packet[offset++] = (Byte)(id);

            packet[offset++] = 0;// 0x80 & (Byte)(((Byte)opCode << 3) | (recursionDesired ? 1 : 0));
            packet[offset++] = 0;

            packet[offset++] = (Byte)(qdCount >> 8);
            packet[offset++] = (Byte)(qdCount);

            packet[offset++] = (Byte)(anCount >> 8);
            packet[offset++] = (Byte)(anCount);

            packet[offset++] = (Byte)(nsCount >> 8);
            packet[offset++] = (Byte)(nsCount);

            packet[offset++] = (Byte)(arCount >> 8);
            packet[offset++] = (Byte)(arCount);

            return offset;
        }
    }

    public class DnsHeader
    {
        public const Byte ByteLength = 12;

        public readonly UInt16 id;
        public readonly Boolean isResponseFlag;
        public readonly DnsOpCode opCode;

        private readonly Byte thirdByte;
        private readonly Byte fourthByte;

        public readonly DnsRCode rCode;
        public readonly UInt16 qdCount, anCount, nsCount, arCount;

        public DnsHeader(Byte[] bytes, UInt32 offset)
        {
            id = (UInt16)((bytes[offset    ] << 8) |
                          (bytes[offset + 1]       ) );


            thirdByte  = bytes[offset + 2];
            fourthByte = bytes[offset + 3];

            isResponseFlag = (bytes[offset + 2] & 0x80) != 0;

            opCode = (DnsOpCode)((bytes[offset + 2] >> 3) & 0xF);

            rCode = (DnsRCode)(bytes[offset + 3] & 0xF);

            qdCount = (UInt16)((bytes[offset +  4] << 8) |
                               (bytes[offset +  5]     ) );
            anCount = (UInt16)((bytes[offset +  6] << 8) |
                               (bytes[offset +  7]     ) );
            nsCount = (UInt16)((bytes[offset +  8] << 8) |
                               (bytes[offset +  9]     ) );
            arCount = (UInt16)((bytes[offset + 10] << 8) |
                               (bytes[offset + 11]     ) );
        }

        public Boolean IsAuthoritativeResponseFlag()
        {
            return (thirdByte & 0x04) != 0;
        }

        public Boolean TruncationFlag()
        {
            return (thirdByte & 0x02) != 0;
        }

        public Boolean RecursionDesiredFlag()
        {
            return (thirdByte & 0x01) != 0;
        }

        public Boolean RecursionAvailableFlag()
        {
            return (fourthByte & 0x80) != 0;
        }

        public override string ToString()
        {
            if (isResponseFlag)
            {
                return String.Format("DnsHeader Response id={0} opCode={1} AA={2} TC={3} RD={4} RA={5} rCode={6} {7}{8}{9}{10}",
                    id, opCode, IsAuthoritativeResponseFlag(), TruncationFlag(), RecursionDesiredFlag(),
                    RecursionAvailableFlag(), rCode,
                    (qdCount > 0) ? ("QDCount=" + qdCount) : String.Empty,
                    (anCount > 0) ? ("ANCount=" + anCount) : String.Empty,
                    (nsCount > 0) ? ("NSCount=" + nsCount) : String.Empty,
                    (arCount > 0) ? ("ARCount=" + arCount) : String.Empty);
            }
            else
            {
                return String.Format("DnsHeader Query id={0} opCode={1} TC={2} RD={3} rCode={4} {5}{6}{7}{8}",
                    id, opCode, TruncationFlag(), RecursionDesiredFlag(), rCode,
                    (qdCount > 0) ? ("QDCount=" + qdCount) : String.Empty,
                    (anCount > 0) ? ("ANCount=" + anCount) : String.Empty,
                    (nsCount > 0) ? ("NSCount=" + nsCount) : String.Empty,
                    (arCount > 0) ? ("ARCount=" + arCount) : String.Empty);
            }
        }

    }


    public class DnsQuestion
    {
        public readonly DomainName domainName;
        public readonly UInt16 type;
        public readonly UInt16 @class;

        public DnsQuestion(Byte[] bytes, ref UInt32 offset)
        {
            this.domainName = new DomainName(bytes, ref offset);

            if (offset + 4 >= bytes.Length)
                throw new FormatException("Dns Packet was too short, it ended int the middle of the question");

            type   = (UInt16)((bytes[offset + 1] << 8) |
                              (bytes[offset + 2]     ) );
            @class = (UInt16)((bytes[offset + 3] << 8) |
                              (bytes[offset + 4]     ));
        }

        public override String ToString()
        {
            return String.Format("DnsQuestion '{0}' type={1} class={2}", domainName.name, type, @class);
        }
    }


    public class DnsResourceRecord
    {
        public readonly DomainName domainName;
        public readonly UInt16 type;
        public readonly UInt16 @class;
        public readonly UInt32 validTimeSeconds;

        private readonly Byte[] rData;
        private readonly UInt32 rDataOffset;
        public readonly UInt16 rDataLength;

        public DnsResourceRecord(DomainName domainName, UInt16 type,
            UInt16 @class, UInt32 validTimeSeconds, Byte[] rData, UInt32 rDataOffset, UInt16 rDataLength)
        {
            this.domainName = domainName;
            this.type = type;
            this.@class = @class;
            this.validTimeSeconds = validTimeSeconds;
            this.rData = rData;
            this.rDataOffset = rDataOffset;
            this.rDataLength = rDataLength;
        }

        public UInt16 PacketLength()
        {
            return (UInt16)(10 + domainName.packet.Length + rDataLength);
        }

        public UInt32 InsertResourceRecord(Byte[] packet, UInt32 packetOffset)
        {
            packetOffset = domainName.InsertIntoPacket(packet, packetOffset);

            packet[packetOffset++] = (Byte)(type >> 8);
            packet[packetOffset++] = (Byte)(type);

            packet[packetOffset++] = (Byte)(@class >> 8);
            packet[packetOffset++] = (Byte)(@class);

            packet[packetOffset++] = (Byte)(validTimeSeconds >> 24);
            packet[packetOffset++] = (Byte)(validTimeSeconds >> 16);
            packet[packetOffset++] = (Byte)(validTimeSeconds >> 8);
            packet[packetOffset++] = (Byte)(validTimeSeconds);

            packet[packetOffset++] = (Byte)(rDataLength >> 8);
            packet[packetOffset++] = (Byte)(rDataLength);

            UInt32 limit = rDataOffset + rDataLength;
            for(UInt32 offset = rDataOffset; offset < limit; offset++)
            {
                packet[packetOffset++] = rData[offset];
            }
            return packetOffset;
        }
    }


}
