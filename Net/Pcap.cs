using System;
using System.IO;
using System.Net;
using System.Text;

using More;

namespace More.Net
{
    // Types found at http://www.tcpdump.org/linktypes.html
    public enum PcapDataLinkType : uint
    {
        Null     = 0,
        Ethernet = 1,
        AX25     = 3,
        // More can be added from http://www.tcpdump.org/linktypes.html
    }

    public class PcapGlobalHeader : SubclassSerializer
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new BigEndianUInt32Reflector(typeof(PcapGlobalHeader), "MagicNumber"),
            new BigEndianUInt16Reflector(typeof(PcapGlobalHeader), "CurrentVersionMajor"),
            new BigEndianUInt16Reflector(typeof(PcapGlobalHeader), "CurrentVersionMinor"),
            new BigEndianInt32Reflector (typeof(PcapGlobalHeader), "gmtToLocalCorrection"),
            new BigEndianUInt32Reflector(typeof(PcapGlobalHeader), "timestampSigFigs"),
            new BigEndianUInt32Reflector(typeof(PcapGlobalHeader), "maxPacketLength"),
            new BigEndianUnsignedEnumReflector<PcapDataLinkType>(typeof(PcapGlobalHeader), "dataLinkType", 4),
        });

        public const UInt32 MagicNumber         = 0xa1b2c3d4;
        public const UInt16 CurrentVersionMajor = 2;
        public const UInt16 CurrentVersionMinor = 4;

        public Int32 gmtToLocalCorrection;
        public UInt32 timestampSigFigs;
        public UInt32 maxPacketLength;
        public PcapDataLinkType dataLinkType;

        public PcapGlobalHeader(Int32 gmtToLocalCorrection, UInt32 timestampSigFigs,
            UInt32 maxPacketLength, PcapDataLinkType dataLinkType)
            : base(memberSerializers)
        {
            this.gmtToLocalCorrection = gmtToLocalCorrection;
            this.timestampSigFigs = timestampSigFigs;
            this.maxPacketLength = maxPacketLength;
            this.timestampSigFigs = timestampSigFigs;
        }
    }

    public class PcapPacket : SubclassSerializer
    {
        public static readonly Reflectors memberSerializers = new Reflectors(new IReflector[] {
            new BigEndianUInt32Reflector(typeof(PcapGlobalHeader), "timestampSeconds"),
            new BigEndianUInt32Reflector(typeof(PcapGlobalHeader), "timestampMicroseconds"),
            new BigEndianUInt32Reflector(typeof(PcapGlobalHeader), "captureLength"),
            new BigEndianUInt32Reflector(typeof(PcapGlobalHeader), "actualLength"),
        });

        public UInt32 timestampSeconds;
        public UInt32 timestampMicroseconds;
        public UInt32 captureLength;
        public UInt32 actualLength;

        public PcapPacket(UInt32 timestampSeconds, UInt32 timestampMicroseconds,
            UInt32 captureLength, UInt32 actualLength)
            : base(memberSerializers)
        {
            this.timestampSeconds = timestampSeconds;
            this.timestampMicroseconds = timestampMicroseconds;
            this.captureLength = captureLength;
            this.actualLength = actualLength;
        }
    }

    public class PcapLogger
    {
        readonly Stream stream;
        public readonly UInt32 snaplen;
        public PcapLogger(Stream stream, UInt32 snaplen)
        {
            this.stream = stream;
            this.snaplen = snaplen;
        }
        public void WriteHeader()
        {
            PcapGlobalHeader header = new PcapGlobalHeader(0, 0, snaplen, PcapDataLinkType.Ethernet);
        }
        public void LogTcpData(IPEndPoint src, IPEndPoint dst, Byte[] data, UInt32 offset, UInt32 length)
        {

        }
    }

}
