using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using More;

class Program
{
    static void Main(String[] args)
    {
        Console.WriteLine("Note: Pipe the output of PacketCat to this program");

        PacketCatParser packetCatParser = new PacketCatParser(Console.OpenStandardInput());

        while (true)
        {
            UInt32 packetSize;
            Byte[] packet = packetCatParser.GetPacket(out packetSize);
            if (packet == null) break;

            //Console.WriteLine(BitConverter.ToString(packetArray, 0, (Int32)packetSize));

            //
            // Make sure packet has enough headers
            //  Ethernet Header : 14 bytes
            //  IP Header: at least 20 bytes
            //
            if (packetSize <= 34)
            {
                Console.WriteLine("OmitPacket: Packet Length {0} is too short (must be at least 34)", packetSize);
                continue;
            }

            Byte ipVersion = (Byte)(packet[14] >> 4);
            //Console.WriteLine("IPVer {0}", ipVersion);
            //Console.WriteLine("{0}.{1}.{2}.{3} {4}.{5}.{6}.{7}",
            //    packetArray[26], packetArray[27], packetArray[28], packetArray[29],
            //    packetArray[30], packetArray[31], packetArray[32], packetArray[33]);

            //
            // Check if it is IPv4
            //
            if(ipVersion != 4)
            {
                Console.WriteLine("OmitPacket: Unhandled IP Version {0}", ipVersion);
                continue;
            }

            Console.WriteLine("{0}.{1}.{2}.{3} {4}.{5}.{6}.{7}",
                packet[26], packet[27], packet[28], packet[29],
                packet[30], packet[31], packet[32], packet[33]);


        }
    }
}