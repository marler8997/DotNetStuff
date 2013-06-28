//
// Tmp stands for Tunnel Manipulation Protocol
//
// Scenario: One end point is designated as the 'Hidden End Point'.  The Hidden End Point
// can make outgoing connections but is usually behind some sort of a NAT / Proxy and is not reachable
// from the outside.  The goal of this protocol is to allow access to the Hidden End Point.
//
// In order to accomplish this, the Hidden End Point will be running the TmpHiddenServer program.
// The TmpHiddenServer program will keep a connection with a TmpAccessor program, which will expose the TmpHiddenServer to
// anyone who has access to the TmpAccessor program.
//
//
// 
// The Hidden End Point must run the TmpServer program.
// The TmpServer program must do the following.
//   1. 
// The following need to be in place in order to accomplish this
// 1. Keep a connection open 
// Protocol
//
// 
using System;


namespace More.Net
{
    public class Tmp
    {
        public const UInt16 TmpAccessorPort = 2029;
        public const UInt16 TmpAccessorProxyPort = 2030;

        public const UInt16 DefaultHeartbeatSeconds = 60;
        public const UInt16 DefaultReconnectWaitSeconds = 60;

        
        //
        // Tmp Protocol
        // The first byte is always the length
        //
        // 

        //
        //Command IDs
        //

    }
}
