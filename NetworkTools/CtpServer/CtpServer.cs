using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CtpServer
{
    //
    // CTP: Controller Transfer Protocol operates on top of CDP: Controller Datagram Protocol
    // CTPS: Controller Transfer Protocol Secure (CTP on top of SSL)
    //
    // Unlike HTTP, CTP can be statefull. The reason for this is that a CDP connection uses much less resources
    // than a TCP connection.  So in theory, it is not unreasonable for a single machine to have thousands of
    // simultaneous connections open.
    //
    //
    //----------------------------------------------------
    //              Request Connection
    //----------------------------------------------------
    // Client                                 Server
    //----------------------------------------------------
    //       -------------------------------------
    //              >>> RequestPayload (Ctp.KeepAlive=1)
    //              [ <<< Cdp.Ack ] (The first request of a connection should probably wait for an ack)
    //                ...
    //              >>> RequestPayload (Cdp.GiveControl)
    //              [ <<< Cdp.Ack ]           
    //              <<< ResponsePayload
    //              [ >>> Cdp.Ack ]
    //                ...
    //              <<< ResponsePayload (Cdp.GiveControl)
    //              [ >>> Cdp.Ack ] (This one is probably not needed since the client will not take long to send the next request)
    //       -------------------------------------
    //                  ...
    //       -------------------------------------
    //              >>> RequestPayload (Ctp.KeepAlive=0)
    //              [ <<< Cdp.Ack ]
    //                ...
    //              >>> RequestPayload (Cdp.GiveControl)
    //              [ <<< Cdp.Ack ]  (The last payload of a request should probably request and wait for an immediate ack)    
    //              <<< ResponsePayload
    //              [ >>> Cdp.Ack ]
    //                ...
    //              <<< ResponsePayload (Cdp.Close)
    //              >>> Cdp.Close
    //              <<< Cdp.Halt
    //----------------------------------------------------
    //
    //
    // What the request can include
    //    1. Keep Alive (One Bit)
    //       * Tells the server if it should to close the connection after its next response
    //    2. Muliple Resources (One Bit)
    //       * Request multiple resources at once
    //    3. Cached Resources (One Bit)
    //       * Request resources that have been cached by the client
    //    3. Resource Identifer(s) (String, for now each resource up to 65535)
    //    4. The Host Name the client used to get the server's IP Address (Max Dns Name, 255 I think)
    //       * This allows the server to switch what it responds with based on the host name
    //       * Unlike HTTP, this is not intended to be used by proxies to forward packets. Instead, a client should use another protocol for that like SOCKS.
    //    5. Cookies
    //       * Allow the server to track state on the client across connections
    //    6. UMI (Unique Machine Identifier)
    //       * This is not possible right now, but it would be very useful if it was put in place in the future
    //    7. Authentication
    //       * It may be usefull to include an authentication procedure
    //
    // What the response can include
    //    1. Events (One Bit)
    //       * Rells the client that the server is listening on the CTP event port to send events to the client.
    //    2. Cookies
    //       * Set cookies
    //
    //
    // Implementation:
    //    A request must be broken into multiple payloads if it is too large.
    //    The client simply keeps control after it sends each payload until the last payload.
    //
    // Request {
    //    Flags Byte flags {
    //       KeepAlive            0x01
    //       MultipleResources    0x02
    //       HasCachedResources   0x04
    //    }
    //    if flags.MultipleResources {
    //      Object[Byte] resources {
    //        Char[UInt16] resourceName
    //      }
    //    } else {
    //      Byte[UInt16] resource
    //    }
    //    if flags.CachedResources {
    //      Object[Byte] cachedResources {
    //        Byte[HASH_LENGTH] hash(sha1 or md5?)
    //        Char[UInt16]resourceName
    //      }
    //    }
    //    Char[Byte] host
    //    Object[Byte] cookies {
    //       Byte cookieID
    //       Byte[Byte] cookie
    //    }
    //    Byte[] data // Since this is the last item, it does not need to have a size
    // }
    // Response {
    //    Flags Byte flags {   
    //       Events 0x01 //Tells the client that the server is listening for events
    //    }
    //    Enum Byte encoding {
    //       None 0
    //       deflate 1
    //       gzip 2
    //    }
    //    Object[Byte] SetCookies {
    //       Byte cookieID
    //       UInt64 expireDateTime
    //       Byte[Byte] cookie
    //    }
    //    Object[Byte] Resources {
    //       Flags Byte resourceFlags {
    //         Cacheable 0x01
    //       }
    //       Byte[UInt32] content
    //    } 
    // }
    //
    // Note: the resource length should never go accros datagram boundaries
    //
    //
    //----------------------------------------------------
    //              Event Connection
    //----------------------------------------------------
    // Client                                 Server
    //----------------------------------------------------
    //                >>> StartEventsPayload
    //                <<< Ack
    //
    //                <<< EventPayload
    //                >>> Ack
    //                <<< EventPayload
    //                >>> Ack
    //                <<< EventPayload
    //                >>> Ack
    //                <<< EventPayload
    //                >>> Ack
    //                <<< EventPayload
    //                >>> Ack
    //                ...
    //
    //                <<< Heartbeat (Unordered Any Time)
    //                >>> Heartbeat (Unordered Any Time)
    //               
    //
    //    Every so often, the client or server can send empty unordered packets as heartbeats.
    //    The reason for the heartbeats is to make sure the routers/nats/proxies in between the client and host
    //    maintain the route from the server to the client.
    //
    //
    // StartEvents {
    //    Char[Byte] host
    //    Object[Byte] cookies {
    //       Byte cookieID
    //       Byte[Byte] cookie
    //    }
    // }
    // Event {
    //    UInt16 eventID
    //    Byte[UInt16] eventValue
    // }
    // 
    // Hearbeat is an empty unordered packet
    //
    //
    class CtpServer
    {
        static void Main(string[] args)
        {
        }
    }
}
