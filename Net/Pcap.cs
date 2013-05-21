using System;
using System.Text;

using Marler.Common;

namespace Marler.Net
{
    // Types found at http://www.tcpdump.org/linktypes.html
    public enum PcapDataLinkType : uint
    {
        Null     = 0,
        Ethernet = 1,
        AX25     = 3,
        // More can be added from http://www.tcpdump.org/linktypes.html
    }

    public class PcapGlobalHeader : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new SimpleUInt32Reflector(typeof(PcapGlobalHeader), "MagicNumber"),
            new SimpleUInt16Reflector(typeof(PcapGlobalHeader), "CurrentVersionMajor"),
            new SimpleUInt16Reflector(typeof(PcapGlobalHeader), "CurrentVersionMinor"),
            new SimpleInt32Reflector (typeof(PcapGlobalHeader), "gmtToLocalCorrection"),
            new SimpleUInt32Reflector(typeof(PcapGlobalHeader), "timestampSigFigs"),
            new SimpleUInt32Reflector(typeof(PcapGlobalHeader), "maxPacketLength"),
            new SimpleEnumReflector  (typeof(PcapGlobalHeader), "dataLinkType", typeof(PcapDataLinkType)),
        };

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

    public class PcapPacket : ClassSerializer
    {
        public static readonly IReflector[] memberSerializers = new IReflector[] {
            new SimpleUInt32Reflector(typeof(PcapGlobalHeader), "timestampSeconds"),
            new SimpleUInt32Reflector(typeof(PcapGlobalHeader), "timestampMicroseconds"),
            new SimpleUInt32Reflector(typeof(PcapGlobalHeader), "captureLength"),
            new SimpleUInt32Reflector(typeof(PcapGlobalHeader), "actualLength"),
        };

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


}
