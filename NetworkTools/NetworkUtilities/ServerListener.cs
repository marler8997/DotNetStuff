using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace Marler.NetworkTools
{
    public class ServerInitializer
    {
        public readonly UInt32 handlerID;
        public readonly Stream stream;

        public ServerInitializer(UInt32 handlerID, Stream stream)
        {
            this.handlerID = handlerID;
            this.stream = stream;
        }
    }

    public class ServerListener
    {
        private UInt32 serverCount;
        private Socket listenSocket;

        public ServerListener(Int32 listeningPort)
        {
            this.serverCount = 0;
            this.listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.listenSocket.Bind(new IPEndPoint(IPAddress.Any, listeningPort));
            this.listenSocket.Listen(10);
        }

        public ServerInitializer Accept()
        {
            UInt32 nextHandlerID = serverCount;
            serverCount++;

            Console.WriteLine("[Server {0}: Listening]", nextHandlerID);
            Socket newClientSocket = listenSocket.Accept();

            Console.WriteLine("[Server {0}: Accepted client \"{1}\"]",
                nextHandlerID, newClientSocket.RemoteEndPoint);

            return new ServerInitializer(nextHandlerID, new NetworkStream(newClientSocket));
        }
    }
}
