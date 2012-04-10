using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Diagnostics;

namespace Marler.NetworkTools
{
    public static class SnmpManagerProgram
    {
        public enum SnmpRequestType
        {
            GetRequest = 0xA0,
            GetNextRequest = 0xA1
        };

        public static void Main(String[] args)
        {
            Byte[] packet = new Byte[1024];
            EndPoint emulatorEndPoint = new IPEndPoint(Dns.GetHostAddresses("utah4063.boi.hp.com")[0], 161);
            Console.WriteLine("IPAddress = {0}", emulatorEndPoint);

            byte[] receiveBuffer = new byte[1024];


            Oid oid = new Oid("1.3.6.1.4.1.11.2.3.9.4.2.1.1.1.1.0");
            for (int i = 0; i < oid.idCount; i++)
            {
                Console.WriteLine("[{0}] {1}", i, oid.Id(i));
            }
            Console.WriteLine("Packet:");
            for (int i = 0; i < oid.packetLength; i++)
            {
                Console.WriteLine("[{0}] {1}", i, oid.Packet(i));
            }

            Int32 packetLength = packet.InsertSnmpRequestPacket(0, SnmpRequestType.GetRequest, null, oid);

            //Send packet to destination
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);
            Console.WriteLine("Packet:");
            for (int i = 0; i < packetLength; i++)
            {
                Console.WriteLine("[{0}] {1}", i, packet[i]);
            }

            socket.SendTo(packet, packetLength, SocketFlags.None, emulatorEndPoint);
            //Receive response from packet
            int received = 0;
            received = socket.ReceiveFrom(receiveBuffer, ref emulatorEndPoint);

            Console.WriteLine("Recieved {0} bytes: {1}", received, BitConverter.ToString(receiveBuffer, 0, received));

        }
        /*
        public static void Main(string[] argv)
        {
            //
            // 1. Get Host EndPoint and Socket
            //
            String host = argv[0];
            
            // Get IP Address
            IPAddress ipAddress;
            if (!IPAddress.TryParse(host, out ipAddress))
            {
                IPHostEntry ipHostEntry = Dns.GetHostEntry(host);
                ipAddress = ipHostEntry.AddressList[0];
                if (ipAddress == null) throw new InvalidOperationException(String.Format("Could not resolve host '{0}'", host));
            }

            // Get EndPoint
            EndPoint hostEndPoint = new IPEndPoint(ipAddress, 161);

            // Get Socket
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);
            socket.Connect(hostEndPoint);

            //
            // 2. Get Community
            //
            byte[] community = Encoding.Default.GetBytes(argv[1]);



            int commlength, miblength, datatype, datalength, datastart;
            int uptime = 0;
            string output;
            Byte[] mibBytes = new Byte[255];
            Byte mibBytesLength;

            Byte[] response = new Byte[1024];
            Int32 responseLength;


            Console.WriteLine("Device SNMP information:");

            // Send sysName SNMP request
            mibBytesLength = MIBStringToBytes("1.3.6.1.2.1.1.5.0", mibBytes);
            socket.SendSnmpRequest(SnmpRequestType.GetRequest, community, mibBytes, mibBytesLength);
            //Receive response from packet   
            responseLength = socket.ReceiveFrom(response, ref hostEndPoint);

            // If response, get the community name and MIB lengths
            commlength = Convert.ToInt16(response[6]);
            miblength = Convert.ToInt16(response[23 + commlength]);

            // Extract the MIB data from the SNMP response
            datatype = Convert.ToInt16(response[24 + commlength + miblength]);
            datalength = Convert.ToInt16(response[25 + commlength + miblength]);
            datastart = 26 + commlength + miblength;
            output = Encoding.ASCII.GetString(response, datastart, datalength);
            Console.WriteLine("  sysName - Datatype: {0}, Value: {1}", datatype, output);

            // Send a sysLocation SNMP request
            mibBytesLength = MIBStringToBytes("1.3.6.1.2.1.1.6.0", mibBytes);
            socket.SendSnmpRequest(SnmpRequestType.GetRequest, community, mibBytes, mibBytesLength);
            //Receive response from packet   
            responseLength = socket.ReceiveFrom(response, ref hostEndPoint);

            // If response, get the community name and MIB lengths
            commlength = Convert.ToInt16(response[6]);
            miblength = Convert.ToInt16(response[23 + commlength]);

            // Extract the MIB data from the SNMP response
            datatype = Convert.ToInt16(response[24 + commlength + miblength]);
            datalength = Convert.ToInt16(response[25 + commlength + miblength]);
            datastart = 26 + commlength + miblength;
            output = Encoding.ASCII.GetString(response, datastart, datalength);
            Console.WriteLine("  sysLocation - Datatype: {0}, Value: {1}", datatype, output);

            // Send a sysContact SNMP request
            mibBytesLength = MIBStringToBytes("1.3.6.1.2.1.1.4.0", mibBytes);
            socket.SendSnmpRequest(SnmpRequestType.GetRequest, community, mibBytes, mibBytesLength);
            //Receive response from packet   
            responseLength = socket.ReceiveFrom(response, ref hostEndPoint);

            // Get the community and MIB lengths
            commlength = Convert.ToInt16(response[6]);
            miblength = Convert.ToInt16(response[23 + commlength]);

            // Extract the MIB data from the SNMP response
            datatype = Convert.ToInt16(response[24 + commlength + miblength]);
            datalength = Convert.ToInt16(response[25 + commlength + miblength]);
            datastart = 26 + commlength + miblength;
            output = Encoding.ASCII.GetString(response, datastart, datalength);
            Console.WriteLine("  sysContact - Datatype: {0}, Value: {1}",
                    datatype, output);

            // Send a SysUptime SNMP request
            mibBytesLength = MIBStringToBytes("1.3.6.1.2.1.1.3.0", mibBytes);
            socket.SendSnmpRequest(SnmpRequestType.GetRequest, community, mibBytes, mibBytesLength);
            //Receive response from packet   
            responseLength = socket.ReceiveFrom(response, ref hostEndPoint);

            // Get the community and MIB lengths of the response
            commlength = Convert.ToInt16(response[6]);
            miblength = Convert.ToInt16(response[23 + commlength]);

            // Extract the MIB data from the SNMp response
            datatype = Convert.ToInt16(response[24 + commlength + miblength]);
            datalength = Convert.ToInt16(response[25 + commlength + miblength]);
            datastart = 26 + commlength + miblength;

            // The sysUptime value may by a multi-byte integer
            // Each byte read must be shifted to the higher byte order
            while (datalength > 0)
            {
                uptime = (uptime << 8) + response[datastart++];
                datalength--;
            }
            Console.WriteLine("  sysUptime - Datatype: {0}, Value: {1}",
                   datatype, uptime);

        }
        */

        public static Int32 InsertSnmpRequestPacket(this Byte [] packet, Int32 offset,
            SnmpRequestType snmpRequestType, Byte[] community, Oid oid)
        {
            Debug.Assert(community == null || community.Length <= Byte.MaxValue);
            Debug.Assert(packet.Length >= 256);

            Int32 originalOffset = offset;
            Int32 totalPacketLengthOffset, mibLengthOffset;

            // SNMP Sequence Start
            packet[offset++] = 0x30;                   // Sequence start
            totalPacketLengthOffset = offset;
            offset++;                                  // Sequence Byte Length (Save Spot)

            // SNMP version
            packet[offset++] = 0x02;                   // Integer type
            packet[offset++] = 0x01;                   // Integer Byte Length
            packet[offset++] = 0x00;                   // SNMP version 1

            // Community name
            if (community == null || community.Length < 1)
            {
                packet[offset++] = 0x05;                   // Null type
            }
            else
            {
                packet[offset++] = 0x04;                   // String type
                packet[offset++] = (Byte)community.Length; //length
                for (Byte i = 0; i < community.Length; i++)
                {
                    packet[offset++] = community[i];
                }
            }

            // Add GetRequest or GetNextRequest value
            packet[offset++] = (Byte)snmpRequestType;
            mibLengthOffset = offset;
            offset++;                                   // MIB Byte Length (Save Spot)

            // Request ID
            packet[offset++] = 0x02; // Integer type
            packet[offset++] = 0x01; // Integer Byte Length
            packet[offset++] = 0x01; // SNMP request ID

            //Error status
            packet[offset++] = 0x02; // Integer type
            packet[offset++] = 0x01; // Integer Byte Length
            packet[offset++] = 0x00; // SNMP error status

            //Error index
            packet[offset++] = 0x02; // Integer type
            packet[offset++] = 0x01; // Integer Byte Length
            packet[offset++] = 0x00; // SNMP error index

            //Start of variable bindings
            packet[offset++] = 0x30; //Start of variable bindings sequence

            packet[offset++] = (Byte)(6 + oid.packetLength - 1);     //Size of variable binding

            packet[offset++] = 0x30;                               //Start of first variable bindings sequence
            packet[offset++] = (Byte)(6 + oid.packetLength - 1 - 2); //size
            packet[offset++] = 0x06;                               //Object type
            packet[offset++] = (Byte)(oid.packetLength - 1);         //length


            packet[offset++] = 0x2b; //Start of MIB
            //Place MIB array in packet
            for (Byte i = 2; i < oid.packetLength; i++)
            {
                packet[offset++] = oid.Packet(i);
            }
            packet[offset++] = 0x05; //Null object value
            packet[offset++] = 0x00; //Null

            Console.WriteLine("Packet Length = {0}", offset);
            packet[totalPacketLengthOffset] = (Byte)(offset - originalOffset - 2);
            packet[mibLengthOffset] = (Byte)(offset - mibLengthOffset - 1);

            return offset - originalOffset;
        }

        /*
        public static Byte MIBStringToBytes(String mibString, Byte [] mibBytes)
        {
            String[] mibSubstrings = mibString.Split('.');

            Byte offset = 0;
            Int16 temp;

            // Convert the string MIB into a byte array of integer values
            // Unfortunately, values over 128 require multiple bytes
            // which also increases the MIB length
            for (Byte i = 0; i < mibSubstrings.Length; i++)
            {
                temp = Convert.ToInt16(mibSubstrings[i]);
                if (temp > 127)
                {
                    mibBytes[offset] = Convert.ToByte(128 + (temp / 128));
                    mibBytes[offset + 1] = Convert.ToByte(temp - ((temp / 128) * 128));
                    offset += 2;
                }
                else
                {
                    mibBytes[offset] = Convert.ToByte(temp);
                    offset++;
                }
            }
            return offset;
        }
        */

        public static string getnextMIB(byte[] mibin)
        {
            string output = "1.3";
            int commlength = mibin[6];
            int mibstart = 6 + commlength + 17; //find the start of the mib section
            //The MIB length is the length defined in the SNMP packet
            // minus 1 to remove the ending .0, which is not used
            int miblength = mibin[mibstart] - 1;
            mibstart += 2; //skip over the length and 0x2b values
            int mibvalue;

            for (int i = mibstart; i < mibstart + miblength; i++)
            {
                mibvalue = Convert.ToInt16(mibin[i]);
                if (mibvalue > 128)
                {
                    mibvalue = (mibvalue / 128) * 128 + Convert.ToInt16(mibin[i + 1]);
                    //ERROR here, it should be mibvalue = (mibvalue-128)*128 + Convert.ToInt16(mibin[i+1]);
                    //for mib values greater than 128, the math is not adding up correctly   

                    i++;
                }
                output += "." + mibvalue;
            }
            return output;
        }
    }
}
