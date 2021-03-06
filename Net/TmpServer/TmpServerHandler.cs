﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

using More.Net.TmpCommand;

namespace More.Net
{
    public class TmpServerHandler : IDataHandler
    {
        readonly AccessorConnection accessorConnection;
        readonly TlsSettings tlsSettings;
        //readonly SelectTunnelsThread tunnelsThread;
        public TmpServerHandler(AccessorConnection accessorConnection, TlsSettings tlsSettings/*, SelectTunnelsThread tunnelsThread*/)
        {
            this.accessorConnection = accessorConnection;
            this.tlsSettings = tlsSettings;
            //this.tunnelsThread = tunnelsThread;
        }
        public void HandleHeartbeat()
        {
            Console.WriteLine("{0} Got heartbeat", DateTime.Now);
        }
        public void HandleData(Byte[] data, UInt32 offset, UInt32 length)
        {
            byte id = data[offset];

            switch(id)
            {
                case Tmp.ToServerOpenTunnelRequestID:
                    Console.WriteLine("{0} Got OpenTunnelRequest", DateTime.Now);

                    break;
                case Tmp.ToServerOpenAccessorTunnelRequestID:
                    Console.WriteLine("{0} Got OpenAccessorTunnelRequest", DateTime.Now);

                    try
                    {
                        OpenAccessorTunnelRequest request;
                        OpenAccessorTunnelRequest.Serializer.Deserialize(data, offset + 1, offset + length, out request);

                        throw new NotImplementedException();
#if COMMENT
                        //
                        // Connect to accessor
                        //
                        Socket socketToAccessor = accessorConnection.MakeNewSocketAndConnect();

                        //
                        // Handle encryption options
                        //
                        IDataHandler sendHandler = new SocketSendDataHandler(socketToAccessor);

                        if ((request.Options & TunnelOptions.RequireTls) == TunnelOptions.RequireTls)
                        {
                            //
                            // Send Connection Info
                            //
                            Byte[] connectionInfo = new Byte[] {Tmp.CreateConnectionInfoFromTmpServerToAccessor(true, true)};
                            socketToAccessor.Send(connectionInfo);

                            throw new NotImplementedException("Ssl not implemented for tunnels yet");
                        }
                        else
                        {
                            //
                            // Send connection info and tunnel key in the same packet
                            //
                            Byte[] connectionInfoAndTunnelKey = new Byte[2 + request.TunnelKey.Length];
                            connectionInfoAndTunnelKey[0] = Tmp.CreateConnectionInfoFromTmpServerToAccessor(false, true);
                            connectionInfoAndTunnelKey[1] = (Byte)request.TunnelKey.Length;
                            Array.Copy(request.TunnelKey, 0, connectionInfoAndTunnelKey, 2, request.TunnelKey.Length);
                            socketToAccessor.Send(connectionInfoAndTunnelKey);
                        }

                        //
                        // Connect to target
                        //
                        String targetIPOrHost = Encoding.ASCII.GetString(request.TargetHost);
                        IPEndPoint targetEndPoint = new IPEndPoint(EndPoints.ParseIPOrResolveHost(targetIPOrHost, DnsPriority.IPv4ThenIPv6), request.TargetPort);

                        Socket socketToTarget = new Socket(targetEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        socketToTarget.Connect(targetEndPoint);

                        tunnelsThread.Add(socketToAccessor, socketToTarget);
#endif
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    
                    break;
                default:
                    Console.WriteLine("{0} Got Frame with unknown id {1}", DateTime.Now, id);
                    break;
            }
        }
        public void Dispose()
        {
        }
    }
}
