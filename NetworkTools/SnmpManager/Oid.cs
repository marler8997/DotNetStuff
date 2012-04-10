using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marler.NetworkTools
{
    public class Oid
    {
        public readonly String dottedString;
        private readonly UInt32[] oid;
        private readonly Byte[] oidPacket;

        public readonly Int32 idCount;
        public readonly Int32 packetLength;

        public UInt32 Id(Int32 index)
        {
            return oid[index];
        }
        public Byte Packet(Int32 index)
        {
            return oidPacket[index];
        }

        public Oid(String dottedString)
        {
            this.dottedString = dottedString;

            //
            // Parse all oid identifiers
            //
            List<UInt32> oidList = new List<UInt32>(dottedString.Length);
            int lastDotIndex = 0;
            for(int i = 1; i < dottedString.Length; i++)
            {
                if(dottedString[i] == '.')
                {
                    oidList.Add(UInt32.Parse(dottedString.Substring(lastDotIndex, i - lastDotIndex)));
                    i++;
                    lastDotIndex = i;
                }
            }
            oidList.Add(UInt32.Parse(dottedString.Substring(lastDotIndex)));
            this.oid = oidList.ToArray();
            this.idCount = oid.Length;
            
            //
            // Create mibForPacket
            //
            List<Byte> oidPacketList = new List<Byte>(4 * idCount);
            for (Byte i = 0; i < idCount; i++)
            {
                UInt32 id = oid[i];              
                
                if (id <= 0x7F)
                {
                    oidPacketList.Add((Byte)id);
                }
                else if(id  <= 0x3FFF)
                {
                    oidPacketList.Add((Byte)(0x80 | (id >> 7)));
                    oidPacketList.Add((Byte)(id & 0x7F));
                }
                else
                {
                    throw new NotImplementedException(String.Format(
                        "An OID id greater than {0} has not yet been implemented, yours is {1}",
                        0x3FFF, id));
                }
            }
            this.oidPacket = oidPacketList.ToArray();
            this.packetLength = oidPacket.Length;
        }

    }
}
