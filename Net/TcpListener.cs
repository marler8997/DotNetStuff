using System;
using System.Net;
using System.Net.Sockets;

namespace Marler.Net
{

    public interface TcpClientHandler
    {
        void NewClient(Socket socket);
    }

    public class TcpListener
    {
        readonly TcpClientHandler clientHandler;
        readonly Socket listenSocket;
        Boolean keepRunning;

        public TcpListener(TcpClientHandler clientHandler, Int32 port, Int32 backlog)
            : this(clientHandler, new IPEndPoint(IPAddress.Any, port), backlog)
        {
        }
        public TcpListener(TcpClientHandler clientHandler, IPEndPoint endPoint, Int32 backlog)
        {
            this.clientHandler = clientHandler;
            this.listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.listenSocket.Bind(endPoint);
            this.listenSocket.Listen(backlog);
            this.keepRunning = false;
        }
        public void PrepareToRun()
        {
            this.keepRunning = true;
        }
        public void StopRunning()
        {
            this.keepRunning = false;
            this.listenSocket.Close();
        }
        public void Run()
        {
            while (keepRunning)
            {
                clientHandler.NewClient(listenSocket.Accept());
            }
        }
    }
}
