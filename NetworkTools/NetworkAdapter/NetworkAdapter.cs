using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Marler.NetworkTools
{
    public class NetworkAdapter
    {
        public static Byte[] ConnectionRequesterSequence = new Byte[]
        {
            (byte) 29, (byte)240, (byte)198, (byte)96,
            (byte)123, (byte)234, (byte) 47, (byte)150,
            (byte)150, (byte) 62, (byte)116, (byte)206,
            (byte)178, (byte)197, (byte) 31, (byte)218
        };
        public static Byte[] ConnectionReceiverSequence = new Byte[]
        {
            (byte) 71, (byte)132, (byte)149, (byte) 99,
            (byte) 22, (byte)169, (byte)243, (byte)218,
            (byte) 44, (byte) 39, (byte)163, (byte)212,
            (byte)120, (byte)205, (byte)170, (byte) 44
        };

        /*
        public static NetworkAdapter ParseNetworkAdapter(List<String> args, ref Int32 offset)
        {
            if (offset + 1 >= args.Count)
            {
                throw new FormatException(String.Format("Not enough arguments, need {0} but got {1}",
                    offset + 2, args.Count));
            }

            AdapterType adapterType = AdapterUtilities.GetAdapterType(args[offset++]);

            if (adapterType == AdapterType.Client)
            {
                if (offset + 1 >= args.Count)
                {
                    throw new FormatException(String.Format("Not enough arguments, need {0} but got {1}",
                        offset + 2, args.Count));
                }
                String host = args[offset++];
                UInt16 port = UInt16.Parse(args[offset++]);

                return new NetworkAdapter(true, host, port);
            }
            else if (adapterType == AdapterType.Server)
            {
                UInt16 port = UInt16.Parse(args[offset++]);
                return new NetworkAdapter(false, null, port);
            }
            else
            {
                throw new FormatException(String.Format("Invalid Enum '{0}' ({1})", adapterType, (Int32)adapterType));
            }
        }


        public readonly Boolean isClient;
        public readonly String host;
        public readonly UInt16 port;

        public NetworkAdapter(Boolean isClient, String host, UInt16 port)
        {
            this.isClient = isClient;
            this.host = host;
            this.port = port;
        }
        */
    }

}
