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
//    6. ClientType
//       * Analagous to the UserAgent Header
//    6. UMI (Unique Machine Identifier)
//       * This is not possible right now, but it would be very useful if it was put in place in the future
//    7. Authentication
//       * It may be usefull to include an authentication procedure
//    8. Extra meta information (maybe?)
//       * Allow clients to send requests with extra meta information
//
// What the response can include
//    1. Events (One Bit)
//       * Rells the client that the server is listening on the CTP event port to send events to the client.
//    2. Cookies
//       * Set cookies
//       * It may be usefull to include an authentication procedure
//    3. Extensions
//       * Allow extensions to the response header
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
//      Char[UInt16] resourceName
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
//                <<< Cdp.Heartbeat (Unordered Any Time)
//                >>> Cdp.Heartbeat (Unordered Any Time)
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
//
//
//----------------------------------------------------
// Running over TCP
//----------------------------------------------------
// The CTP protocol can also be on top of TCP.
// The following modifications must be made to do this.
// 1. A Payload length must be prepended to each payload that was a CDP packet
//     [Payload Length] [Payload]
// 2. The GiveControl flag must also be prepended to the payload
//     [GiveControl] [Payload Length] [Payload]
//
using System;
using System.Collections.Generic;
using System.Text;

namespace More.Net
{
    public static class Ctp
    {
        public const Int32 HashLength = 20;
    }

    [Flags]
    public enum CtpRequestFlags
    {
        KeepAlive          = 0x01,
        MultipleResources  = 0x02,
        HasCachedResources = 0x03,
    }

    [Flags]
    public enum CtpResponseFlags
    {
        Events = 0x01,
    }
    [Flags]
    public enum CtpResourceFlags
    {
        Cacheable = 0x01,
    }

    public enum CtpEncoding
    {
        None = 0,
        Deflate = 1,
        Gzip = 2,
    }

    struct CtpCachedResource
    {
        public Byte[] hash;
        public String name;
        public CtpCachedResource(Byte[] hash, String name)
        {
            this.hash = hash;
            this.name = name;
        }
    }
    struct CtpCookie
    {
        public Byte id;
        public Byte[] value;
        public CtpCookie(Byte id, Byte[] value)
        {
            this.id = id;
            this.value = value;
        }
    }


    public class CtpBadRequestException : SystemException
    {
        public CtpBadRequestException(String message)
            : base(message)
        {
        }
    }

    class CtpRequest
    {
        public CtpRequestFlags flags;

        public String singleResource;
        public String[] resources;

        public CtpCachedResource[] cachedResources;

        public String host;


        public Byte[] data;

        public CtpRequest(Byte[] payload)
        {
            try
            {
                this.flags = (CtpRequestFlags)payload[0];

                //
                // Parse resources
                //
                Int32 offset;
                if ((this.flags & CtpRequestFlags.MultipleResources) == 0)
                {
                    Int32 length = (0xFF00 & (payload[1] << 8)) | (0x00FF & payload[2]);
                    this.singleResource = Encoding.UTF8.GetString(payload, 3, length);
                    offset = 3 + length;
                }
                else
                {
                    this.singleResource = null;
                    this.resources = new String[payload[1]];

                    offset = 1;
                    for (int i = 0; i < resources.Length; i++)
                    {
                        Int32 length = (0xFF00 & (payload[offset] << 8)) | (0x00FF & payload[offset+1]);
                        this.singleResource = Encoding.UTF8.GetString(payload, offset + 2, length);
                        offset += 2 + length;
                    }
                }

                //
                // Parse cached resources
                //
                if ((this.flags & CtpRequestFlags.HasCachedResources) != 0)
                {
                    throw new NotImplementedException();
                    /*
                    this.cachedResources = new CtpCachedResource[payload[offset]];
                    offset++;

                    for(int i = 0; i < cachedResources.Length; i++)
                    {
                        cachedResources[i].hash = new Byte[Ctp.HashLength
                    }
                    */
                }

                //
                // Parse Cookies
                //





            }
            catch(IndexOutOfRangeException e)
            {
                throw new CtpBadRequestException(String.Format("Got Index out of bounds exception '{0}'", e.Message));
            }
        }
    }


    class SetCookie
    {
        Byte id;
        Byte[] value;
        UInt64 expireDateTime;
        public SetCookie(Byte id, Byte[] value, UInt64 expireDateTime)
        {
            this.id = id;
            this.value = value;
            this.expireDateTime = expireDateTime;
        }
    }



    class Resource
    {
        public CtpResourceFlags flags;
        public Byte[] content;
        public Resource(CtpResourceFlags flags, Byte[] content)
        {
            this.flags = flags;
            this.content = content;
        }
    }

    class CtpResponse
    {
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
        public CtpResponseFlags flags;
        public CtpEncoding encoding;
        public SetCookie[] setCookies;
        public Resource[] resources;

        public CtpResponse()
        {
        }
        public CtpResponse(Byte[] payload)
        {
            this.flags = (CtpResponseFlags)payload[0];
            this.encoding = (CtpEncoding)payload[1];
            this.setCookies = new SetCookie[payload[2]];

            /*
            for(int i = 0; i < setCookies.Length; i++)
            {
                this.set
            }

            */
        }

    }

}
