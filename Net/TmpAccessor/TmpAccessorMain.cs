using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

using More;

namespace More.Net
{
    class TmpAccessorOptions : CLParser
    {
        public readonly CLGenericArgument<UInt16> accessorListenPort;
        public readonly CLGenericArgument<UInt16> proxyListenPort;

        public TmpAccessorOptions()
        {
            accessorListenPort = new CLGenericArgument<UInt16>(UInt16.Parse, 'l', "port", "The accessor listen port");
            accessorListenPort.SetDefault(Tmp.TmpAccessorPort);
            Add(accessorListenPort);

            proxyListenPort = new CLGenericArgument<UInt16>(UInt16.Parse, 'p', "proxy-port", "The accessor proxy listen port");
            proxyListenPort.SetDefault(Tmp.TmpAccessorProxyPort);
            Add(proxyListenPort);
        }

        public override void PrintUsageHeader()
        {
            Console.WriteLine("TmpAccessor.exe [options]");
        }
    }

    /*
    class CommonRequestResponseLock : IDataHandler
    {
        static Socket currentSocketCommunicating;

        Socket accessorSocket;
        Socket accessorProxySocket;
        Boolean aToB;

        public CommonRequestResponseLock(Socket accessorSocket, Socket accessorProxySocket, out CommonRequestResponseLock bToADataHandler)
        {
            this.accessorSocket = accessorSocket;
            this.accessorProxySocket = accessorProxySocket;
            this.aToB = true;
            bToADataHandler = new CommonRequestResponseLock(accessorSocket, accessorProxySocket
        }

        CommonRequestResponseLock(Socket accessorSocket, Socket accessorProxySocket)
        {
            this.accessorSocket = accessorSocket;
            this.accessorProxySocket = accessorProxySocket;
            this.aToB = false;
        }

        public void HandleData(byte[] data, int offset, int length)
        {


            if (aToB)
            {

            }
            else
            {

            }
        }
    }
    */


    class SocketDataAndHeartbeatHandler : IDataAndHeartbeatHandler
    {
        readonly Socket accessorsocket;
        Socket proxySocket;
        public SocketDataAndHeartbeatHandler(Socket accessorsocket)
        {
            this.accessorsocket = accessorsocket;
        }
        public void SetProxySocket(Socket proxySocket)
        {
            this.proxySocket = proxySocket;
        }
        public void HandleHeartbeat()
        {
            Console.WriteLine("{0} Received heartbeat from accessor", DateTime.Now);
            accessorsocket.Send(FrameAndHeartbeatData.HeartBeatPacket);
        }
        public void HandleData(byte[] data, int offset, int length)
        {
            Socket proxySocket = this.proxySocket;
            if (proxySocket == null)
            {
                Console.WriteLine("{0} Throwing away {1} bytes from accessor", DateTime.Now, length);
            }
            else
            {
                proxySocket.Send(data, offset, length, SocketFlags.None);
            }
        }
        public void Dispose()
        {
        }
    }


    class TmpAccessorMain
    {
        static void Main(string[] args)
        {
            TmpAccessorOptions options = new TmpAccessorOptions();
            List<String> nonOptionArgs = options.Parse(args);

            if(nonOptionArgs.Count != 0)
            {
                options.ErrorAndUsage("Expected no non-option arguments but got {0}", nonOptionArgs.Count);
                return;
            }

            Socket accessorListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            accessorListenSocket.Bind(new IPEndPoint(IPAddress.Any, options.accessorListenPort.ArgValue));
            accessorListenSocket.Listen(0);

            while(true)
            {
                Socket accessorSocket = null;
                try
                {
                    accessorSocket = accessorListenSocket.Accept();
                    Console.WriteLine("{0} Received an accessor connection '{1}'", DateTime.Now, accessorSocket.SafeRemoteEndPointString());

                    using (Socket proxyListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        proxyListenSocket.Bind(new IPEndPoint(IPAddress.Any, options.proxyListenPort.ArgValue));
                        proxyListenSocket.Listen(16);

                        SocketDataAndHeartbeatHandler socketDataAndHeartbeatHandler = new SocketDataAndHeartbeatHandler(accessorSocket);
                        FrameAndHeartbeatDataReceiver accessorToProxyDataHandler = new FrameAndHeartbeatDataReceiver(socketDataAndHeartbeatHandler);
                        List<Socket> selectSockets = new List<Socket>(2);
                        Byte[] receiveBuffer = new Byte[1024];

                        //
                        // Switch between the wait for proxy connection loop
                        // and the proxy forward data loop
                        //
                        Boolean connectedToAccessor = true;
                        while (connectedToAccessor)
                        {
                            //
                            // The wait for a proxy connection loop
                            //
                            Socket proxySocket = null;
                            while (proxySocket == null)
                            {
                                selectSockets.Clear();
                                selectSockets.Add(accessorSocket);
                                selectSockets.Add(proxyListenSocket);

                                Socket.Select(selectSockets, null, null, Int32.MaxValue);

                                if (selectSockets.Count > 0)
                                {
                                    for (int i = 0; i < selectSockets.Count; i++)
                                    {
                                        Socket socket = selectSockets[i];

                                        if (socket == accessorSocket)
                                        {
                                            Int32 bytesRead = 0;
                                            try
                                            {
                                                bytesRead = socket.Receive(receiveBuffer);
                                            }
                                            catch (Exception)
                                            {
                                            }

                                            if (bytesRead <= 0)
                                            {
                                                connectedToAccessor = false;
                                                break;
                                            }

                                            accessorToProxyDataHandler.HandleData(receiveBuffer, 0, bytesRead);
                                        }
                                        else
                                        {
                                            proxySocket = proxyListenSocket.Accept();
                                            Console.WriteLine("{0} Received a proxy connection '{1}'", DateTime.Now, proxySocket.SafeRemoteEndPointString());
                                            socketDataAndHeartbeatHandler.SetProxySocket(proxySocket);
                                        }
                                    }
                                }
                            }

                            if (!connectedToAccessor) break;


                            //
                            // The proxy forward data loop
                            //
                            try
                            {
                                while (true)
                                {
                                    selectSockets.Clear();
                                    selectSockets.Add(accessorSocket);
                                    selectSockets.Add(proxySocket);

                                    Socket.Select(selectSockets, null, null, Int32.MaxValue);

                                    if (selectSockets.Count > 0)
                                    {
                                        for (int i = 0; i < selectSockets.Count; i++)
                                        {
                                            Socket socket = selectSockets[i];

                                            Int32 bytesRead = 0;
                                            try
                                            {
                                                bytesRead = socket.Receive(receiveBuffer, 3, receiveBuffer.Length - 3, SocketFlags.None);
                                            }
                                            catch (Exception)
                                            {
                                            }

                                            if (bytesRead <= 0)
                                            {
                                                if (socket == accessorSocket) connectedToAccessor = false;
                                                break;
                                            }

                                            if (socket == accessorSocket)
                                            {
                                                accessorToProxyDataHandler.HandleData(receiveBuffer, 3, bytesRead);
                                            }
                                            else
                                            {
                                                FrameAndHeartbeatDataSender.InsertLength(receiveBuffer, 0, bytesRead);
                                                accessorSocket.Send(receiveBuffer, bytesRead + 3, SocketFlags.None);
                                            }
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                if (proxySocket != null)
                                {
                                    if (proxySocket.Connected)
                                    {
                                        try { proxySocket.Shutdown(SocketShutdown.Both); }
                                        catch (Exception) { }
                                    }
                                    proxySocket.Close();
                                }
                            }
                        }
                    }
                }
                finally
                {
                    if (accessorSocket != null) 
                    {
                        if(accessorSocket.Connected)
                        {
                            try { accessorSocket.Shutdown(SocketShutdown.Both); } catch(Exception) { }
                        }
                        accessorSocket.Close();
                    }
                }
            }
        }
    }
}
