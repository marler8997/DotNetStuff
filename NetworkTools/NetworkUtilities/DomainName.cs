using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marler.NetworkTools
{

    public class DomainName
    {
        public readonly String name;
        public readonly Byte[] packet;

        public DomainName(Byte[] bytes, ref UInt32 offset)
        {
            UInt32 offsetOriginal = offset;

            StringBuilder builder = new StringBuilder();
            offset = builder.DomainBytesToString(bytes, offset);
            this.name = builder.ToString();

            this.packet = new Byte[offset - offsetOriginal];
            for(UInt32 i = 0; i < this.packet.Length; i++)
            {
                this.packet[i] = bytes[offsetOriginal + i];
            }
        }

        public UInt32 InsertIntoPacket(Byte[] packet, UInt32 offset)
        {
            for (UInt32 i = 0; i < this.packet.Length; i++)
            {
                packet[offset++] = this.packet[i];
            }
            return offset;
        }
    }

    public class DomainNamePacketHook
    {
        private readonly Byte[] bytes;
        private readonly UInt32 offset;
        public readonly UInt16 packetLength;
        public readonly String name;

        public DomainNamePacketHook(Byte[] bytes, ref UInt32 offset)
        {
            this.bytes = bytes;
            this.offset = offset;

            StringBuilder builder = new StringBuilder();
            offset = builder.DomainBytesToString(bytes, offset);

            this.packetLength = (UInt16)(offset - this.offset);

            this.name = builder.ToString();
        }

        public UInt32 InsertIntoPacket(Byte[] packet, UInt32 offset)
        {
            for (UInt32 bytesOffset = this.offset; this.bytes[bytesOffset] != 0; bytesOffset++)
            {
                packet[offset++] = bytes[bytesOffset];
            }
            packet[offset++] = 0;
            return offset;
        }

    }
}
