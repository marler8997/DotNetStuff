using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace More.Net
{
    class DebugClientData : IDisposable
    {
        public readonly LineParser lineParser;
        public readonly StreamWriter writer;
        public DebugClientData(Socket socket)
        {
            this.lineParser = new LineParser(Encoding.ASCII, 64, 128);
            this.writer = new StreamWriter(new NetworkStream(socket));
        }
        public void Dispose()
        {
            writer.Dispose();
        }
    }

    public class ControlServer : StreamSelectServerCallback
    {
        readonly Dictionary<Socket, DebugClientData> clientMap = new Dictionary<Socket, DebugClientData>();

        public ControlServer()
        {
        }

        void SendPrompt(DebugClientData clientData)
        {
            clientData.writer.Write("NfsServerDebug> ");
            clientData.writer.Flush();
        }

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
            DebugClientData clientData = new DebugClientData(socket);
            clientMap.Add(socket, new DebugClientData(socket));

            SendPrompt(clientData);

            return ServerInstruction.NoInstruction;
        }
        public ServerInstruction ClientCloseCallback(UInt32 clientCount, Socket socket)
        {
            DebugClientData clientData;
            if (clientMap.TryGetValue(socket, out clientData))
            {
                clientData.Dispose();
                clientMap.Remove(socket);
            }
            return ServerInstruction.NoInstruction;
        }
        public ServerInstruction ClientDataCallback(Socket socket, Byte[] bytes, UInt32 bytesRead)
        {
            DebugClientData clientData;
            if (!clientMap.TryGetValue(socket, out clientData)) return ServerInstruction.CloseClient;

            clientData.lineParser.Add(bytes, 0, bytesRead);

            while (true)
            {
                String line = clientData.lineParser.GetLine();
                if (line == null) break;


                if (line[0] == 'd' || line[0] == 'D')
                {
                    if (NfsServerLog.performanceLog == null)
                    {
                        clientData.writer.Write("Cannot dump performance log because it was not enabled");
                    }
                    else
                    {
                        NfsServerLog.performanceLog.DumpLog(clientData.writer);
                    }
                }
                else if (line[0] == 'h' || line[0] == 'H')
                {
                    clientData.writer.WriteLine("Commands: dump, help");
                }
                else if (line[0] == 'e' || line[0] == 'E')
                {
                    return ServerInstruction.CloseClient;
                }
                else
                {
                    clientData.writer.WriteLine("UnknownCommand '{0}'", line);
                    clientData.writer.WriteLine("Commands: dump, help, exit");
                }
                SendPrompt(clientData);
            }

            return ServerInstruction.NoInstruction;
        }
    }
}
