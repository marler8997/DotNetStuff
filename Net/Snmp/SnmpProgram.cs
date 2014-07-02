using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace More
{
    public class Options : CLParser
    {
        public readonly CLStringArgument community;
        public Options()
        {
            community = new CLStringArgument('c', "community", "The SNMP community");
            community.SetDefault("public");
            Add(community);
        }
        public override void PrintUsageHeader()
        {
            Console.WriteLine("Snmp.exe get <host> <mib>");
            Console.WriteLine("Snmp.exe set <host> <mib> <type> <value>");
        }
    }
    public class SnmpProgram
    {
        static Int32 Main(String[] args)
        {
            Options options = new Options();

            List<String> nonOptionArgs = options.Parse(args);

            if (nonOptionArgs.Count == 0)
            {
                options.PrintUsage();
                return 1;
            }

            String command = nonOptionArgs[0];

            if (nonOptionArgs.Count < 3)
            {
                return options.ErrorAndUsage("Error: Missing command line arguments");
            }
            String hostString = nonOptionArgs[1];
            String oidString = nonOptionArgs[2];
            
            EndPoint endPoint = EndPoints.EndPointFromIPOrHostAndOptionalPort(hostString, 161);

            List<Byte> oidBytes = new List<Byte>();
            Snmp.ParseOid(oidString, oidBytes);
            Byte[] oid = oidBytes.ToArray();

            if (command.Equals("get", StringComparison.CurrentCultureIgnoreCase))
            {
                if (nonOptionArgs.Count != 3)
                {
                    return options.ErrorAndUsage("Error: operation 'get' requires 2 arguments but there are {0}", nonOptionArgs.Count - 1);
                }

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                Byte[] packet = new Byte[Snmp.MaximumBufferForGet(
                    (UInt32)options.community.ArgValue.Length, (UInt32)oid.Length)];

                UInt32 snmpContentLength = Snmp.SerializeGet(packet, 0, options.community.ArgValue, Snmp.NextID(), oid);

                socket.SendTo(packet, (Int32)snmpContentLength, 0, endPoint);
            }
            else if (command.Equals("set", StringComparison.CurrentCultureIgnoreCase))
            {
                if (nonOptionArgs.Count != 5)
                {
                    return options.ErrorAndUsage("Error: operation 'set' requires 4 arguments but there are {0}", nonOptionArgs.Count - 1);
                }

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                Byte[] value = Asn1.ParseValue(nonOptionArgs[3], nonOptionArgs[4]);

                Byte[] packet = new Byte[Snmp.MaximumBufferForSet(
                    (UInt32)options.community.ArgValue.Length, (UInt32)oid.Length, (UInt32)value.Length)];

                UInt32 snmpContentLength = Snmp.SerializeSet(packet, 0, options.community.ArgValue, Snmp.NextID(), oid, value);

                socket.SendTo(packet, (Int32)snmpContentLength, 0, endPoint);
            }
            else
            {
                Console.WriteLine("Error: Unknown operation '{0}'", command);
                return 1;
            }

            return 0;
        }
    }
}
