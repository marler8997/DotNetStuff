//
// TMP: Tunnel Manipulation Protocol
//
// Scenario: One end point, call it the 'Hidden End Point', can make outgoing connections, but
// other end points cannot connect to it (usually because it is behind some sort of a NAT / Proxy).
// The TMP can provide access to the Hidden End Point and any other end point accessible by the Hidden
// end point to any end point outside that network.  It can also restrict access and encrypt traffic.
//
// In order to accomplish this, the Hidden End Point will be running the TmpServer program.
// The TmpServer program will maintain a connection with the 'Accessor End Point'. This connection
// will facilitate communication using the Tunnel Manipulation Protocol, which allows the accessor
// to open secure or insecure tunnels to any end point that is accessible by the Hidden End Point.
//
// Security
// ------------------------------------
// The TmpServer is the one exposing functionality, and therefore, is the first one who needs
// to authenticate it's users.  It's user is some sort of Accessor program.  If you are exposing this interface
// over an open network and do not want to share functionality, then the TmpServer needs to authenticate
// the Accessor it connects to, and also encrypt the data transferred.
//
// To authenticate the accessor, the TmpServer must require that it's Tmp connection use Ssl/Tls.  Then it can maintain
// a list of public keys that it accepts.
//
// The Accessor can also choose to authenticate the TmpServer, it would also do this by requiring a Ssl/Tls connection
// and maintaining a list of authenticated public keys.  In order to prevent a Man-In-The-Middle attacks, each end point
// must authenticate the other.
// 
// Tunnel Manipulation Protocol
// -------------------------------------
//
//
//                 TmpServer                      |                   Accessor
// ----------------------------------------------------------------------------------------------------
//                                                | Waiting for connections (default port 2029)...
//                                                |
//  Connect to Accessor (default port 2029)       |
//                                                |
//                                                | Accept Connection, then wait for ConnectionInfo
//                                                |
//  Send ConnectionInfo indicating:               |
//    1. whether the connection is a tunnel and   |
//    2. whether Tls is required                  |
//                                                |
//                                                | Accept ConnectionInfo packet
//                                                |
//  |--------------------------------------------------------------------------------------------------
//  | If Tls was required by TmpServer
//  |--------------------------------------------------------------------------------------------------
//  |                                             |
//  | Negotiate Tls                               | Negotiate Tls
//  |                                             |
//  |--------------------------------------------------------------------------------------------------
//
//
//  |--------------------------------------------------------------------------------------------------
//  | If Connection is a tunnel
//  |--------------------------------------------------------------------------------------------------
//  |                                             |
//  |                                             | Wait for TunnelKey...
//  |                                             |
//  | Send TunnelKey                              |
//  |   Note: If the TmpServer does not require   |
//  |   Tls then the ConnectionInfo and TunnelKey |
//  |   should be sent in the same packet         |
//  |                                             |
//  |                                             |
//  |                                             |
//  |                                             | Accept TunnelKey
//  |                                             |   If Tls is not setup, and the Accessor specified that
//  |                                             |   the tunnel matching this key required Tls, then the
//  |                                             |   Accessor should close the connection.  It is important
//  |                                             |   that the Accessor does not indicate the reason for closing
//  |                                             |   as this could provide information to an attacker.
//  |                                             |
//  | Start serving tunnel...                     | Start serving tunnel...
//  |                                             |
//  |--------------------------------------------------------------------------------------------------
//  | Else (Connection is a TMP Control connection)
//  |--------------------------------------------------------------------------------------------------
//  |                                             |
//  |    |---------------------------------------------------------------------------------------------
//  |    | If Tls was not required by TmpServer (Tls is not set up yet)
//  |    |---------------------------------------------------------------------------------------------
//  |    |                                        |
//  |    | Wait for TlsRequirement...             |
//  |    |                                        |
//  |    |                                        | Send TlsRequirement
//  |    |                                        |
//  |    | Accept TlsRequirement                  |
//  |    |                                        |
//  |    |   |---------------------------------------------------------------------------------------------
//  |    |   | If Accessor requires Tls
//  |    |   |---------------------------------------------------------------------------------------------
//  |    |   |                                    |
//  |    |   | Negotiate Tls                      | Negotiate Tls
//  |    |   |                                    |
//  |    |   |---------------------------------------------------------------------------------------------
//  |    |                                        |
//  |    |---------------------------------------------------------------------------------------------
//  |                                             |
//  |                                             | Wait for ServerInfo packet
//  |                                             |
//  | Send ServerInfo packet                      |
//  |                                             |
//  | Enter TmpServerLoop                         | Enter AccessorLoop
//  |   1. Accept TMP commands                    |   1. Send TMP commands
//  |   2. Send heartbeats                        |   2. Accept heartbeats (??? Should the Accessor respond to heartbeats ???)
//  |                                             |
//  |--------------------------------------------------------------------------------------------------
//
//
// Tmp Commands
// -------------------------------------
// The TMP protocol piggybacks of the FrameAndHeartbeat protocol.
// The FrameAndHeartbeat protocol is a simple protocol that allows a stream interface to send heartbeats
// and also delimit frames.
//
// Either end point can send a heartbeat or a command in a single Frame.
// 
// OpenAccessorTunnel Request
// --------------------------
// Requests the TmpServer to open a tunnel to the accessor
// 
//



using System;
using System.Net.Sockets;


namespace More.Net
{
    public class TlsSettings
    {
        public readonly Boolean requireTlsForTmpConnections;

        public TlsSettings(Boolean requireTlsForTmpConnections)
        {
            this.requireTlsForTmpConnections = requireTlsForTmpConnections;
        }
    }

    /*
    [Flags]
    public enum TmpConnectionFlags
    {
        TlsRequiredFlag    = 0x01, // 0 means tls not required, 1 means tls is required
        ConnectionTypeFlag = 0x02, // 0 means tmp control connection, 1 means tunnel connection
    }
    */

    //
    // The TunnelManipulationProtocol
    // operates above the FrameAndHeartbeat protocol
    //
    public static class Tmp
    {
        public const UInt16 DefaultPort                = 2029;
        public const UInt16 DefaultAccessorControlPort = 2030;


        public const UInt16 DefaultHeartbeatSeconds = 60;
        public const UInt16 DefaultReconnectWaitSeconds = 60;




        //
        // Connection Info Bytes
        //

        // Accessor to TmpServer Flags
        public const Byte TmpConnectionsRequireTlsFlag    = 0x01;
        public const Byte TunnelConnectionsRequireTlsFlag = 0x02;


        // TmpServer to Accessor Flags
        public const Byte RequireTlsFlag = 0x01;
        public const Byte IsTunnelFlag   = 0x02;


        public static Byte CreateTlsRequirementFromAccessorToTmpServer(Boolean requireTls)
        {
            return requireTls ? (Byte)1 : (Byte)0;
        }
        public static Byte CreateConnectionInfoFromTmpServerToAccessor(Boolean requireTls, Boolean isTunnel)
        {
            return (Byte)(
                (requireTls ? RequireTlsFlag : 0) |
                (isTunnel ? IsTunnelFlag : 0));
        }
        public static Boolean ReadTlsRequirementFromAccessorToTmpServer(Byte tlsRequirement)
        {
            if(tlsRequirement == 0) return false;
            if(tlsRequirement == 1) return true;
            throw new FormatException(String.Format("Expected TlsRequirement to be 0 or 1 but was {0}", tlsRequirement));
        }
        public static void ReadConnectionInfoFromTmpServer(Byte connectionInfo, out Boolean requireTls, out Boolean isTunnel)
        {
            requireTls = (RequireTlsFlag & connectionInfo) == RequireTlsFlag;
            isTunnel = (IsTunnelFlag & connectionInfo) == IsTunnelFlag;
        }


        //
        // Command IDs
        //
        public const Byte ToAccessorServerInfoID = 0;

        public const Byte ToServerOpenTunnelRequestID         = 0;
        public const Byte ToServerOpenAccessorTunnelRequestID = 1;

        /*
        public static Byte[] CreateCommandPacket<T>(Byte commandID, IReflector reflector, T command, UInt32 offset)
        {
            Int32 commandLength = 1 + reflector.SerializationLength(command);
            Byte[] commandPacket = FrameAndHeartbeatProtocol.AllocateFrame(offset, commandLength);
            commandPacket[offset + 3] = commandID;
            reflector.Serialize(command, commandPacket, offset + 4);
            return commandPacket;
        }

        public static void SendTmpCommand(this Socket socket, IReflector reflector, Byte commandID, Object command)
        {
            Int32 commandLength = 1 + reflector.SerializationLength(command);
            Byte[] commandPacket = FrameAndHeartbeatProtocol.AllocateFrame(0, commandLength);
            commandPacket[3] = commandID;
            reflector.Serialize(command, commandPacket, 4);
            socket.Send(commandPacket);
        }
        */
    }
}
