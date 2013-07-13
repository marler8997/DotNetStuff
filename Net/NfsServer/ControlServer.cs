using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace More.Net
{
    public class ControlServer : StreamSelectServerCallback
    {
        public void ServerListening(Socket listenSocket)
        {
        }
        public void ServerStopped()
        {
        }
        public ServerInstruction ListenSocketClosed(UInt32 clientCount)
        {
            return ServerInstruction.NoInstruction;
        }
        public ServerInstruction ClientOpenCallback(UInt32 clientCount, Socket socket)
        {
            return ServerInstruction.NoInstruction;
        }
        public ServerInstruction ClientCloseCallback(UInt32 clientCount, Socket socket)
        {
            return ServerInstruction.NoInstruction;
        }
        public ServerInstruction ClientDataCallback(Socket socket, Byte[] bytes, UInt32 bytesRead)
        {
            if (bytesRead > 0)
            {
                NetworkStream networkStream = new NetworkStream(socket);
                Byte b = bytes[0];

                if(b == 'd' || b == 'D')
                {
                    StreamWriter writer = new StreamWriter(networkStream);
                    NfsServerLog.PrintNfsCalls(writer);
                    writer.Flush();
                }
                else
                {
                    Byte[] helpBytes = Encoding.UTF8.GetBytes("Commands: dump, help\r\n");
                    networkStream.Write(helpBytes, 0, helpBytes.Length);
                }

                networkStream.Flush();

                return ServerInstruction.CloseClient;
            }

            return ServerInstruction.NoInstruction;
        }
    }
}
