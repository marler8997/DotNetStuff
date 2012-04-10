using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace Marler.NetworkTools
{
    public class ClientAcceptor
    {
        public readonly UInt16 listenPort;
        public readonly Int32 socketBackLog;

        private Socket listenSocket;
        private Int32 acceptCount;

        public ClientAcceptor(UInt16 listenPort, Int32 socketBackLog)
        {
            this.listenPort = listenPort;
            this.socketBackLog = socketBackLog;

            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Any, listenPort));
            listenSocket.Listen(socketBackLog);
            acceptCount = 0;
        }

        public NetworkStream Accept(out MessageLogger logger)
        {
            logger = new ConsoleMessageLogger(String.Format("Handler {0}", acceptCount.ToString()));

            logger.Log("Listening");
            Socket newClientSocket = listenSocket.Accept();

            logger.Log("Accepted {0}", newClientSocket.RemoteEndPoint.GetString());
            acceptCount++;

            return new NetworkStream(newClientSocket);
        }

        public override string ToString()
        {
            return String.Format("Server Handler ListenPort: {0} SocketBackLog: {1}",
                listenPort, socketBackLog);
        }

    }
}
