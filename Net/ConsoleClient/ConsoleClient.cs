using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

using More;

namespace More.Net
{
    public class ConsoleClient
    {
        delegate void CommandFunction(String line);

        readonly Object syncObject = new Object();

        readonly Int32 sendFileBufferSize;
        readonly Int32 recvBufferSize;

        ISocketConnector connector;
        EndPoint serverEndPoint;

        MessageLogger messageLogger;
        IConnectionDataLogger connectionLogger;

        readonly Dictionary<String,CommandFunction> commandDictionary;

        private Socket socket;
        Boolean keepRunning;

        public ConsoleClient(Int32 sendFileBufferSize, Int32 recvBufferSize, ISocketConnector connector, EndPoint serverEndPoint,
            MessageLogger messageLogger, IConnectionDataLogger connectionLogger)
        {
            this.sendFileBufferSize = sendFileBufferSize;
            this.recvBufferSize = recvBufferSize;

            this.connector = connector;
            this.serverEndPoint = serverEndPoint;

            this.messageLogger = messageLogger;
            this.connectionLogger = connectionLogger;

            commandDictionary = new Dictionary<String, CommandFunction>();
            commandDictionary.Add("open", OpenCommand);
            commandDictionary.Add("close", CloseCommand);
            commandDictionary.Add("send", SendCommand);
            commandDictionary.Add("sendfile", SendFileCommand);
            commandDictionary.Add("proxy", ProxyCommand);
            commandDictionary.Add("help", HelpCommand);
            commandDictionary.Add("exit", ExitCommand);
            commandDictionary.Add("echo", EchoCommand);
        }

        private byte[] GetData(String str, Int32 offset, out Int32 outLength)
        {
            if (offset + 1 >= str.Length)
            {
                Console.WriteLine("Please supply data");
                outLength = 0;
                return null;
            }
            if (str[offset] != ' ')
            {
                Console.WriteLine("I expected a space ' ' after the command, but got '{0}'", str[offset]);
                outLength = 0;
                return null;
            }

            offset++;
            byte[] data = str.ParseStringLiteral(offset, out outLength);
            if(outLength <= 0)
            {
                Console.WriteLine("Please supply data");
                return null;
            }
            return data;
        }
        private void Disconnect()
        {
            lock (syncObject)
            {
                if (socket != null)
                {
                    if (socket.Connected) try { socket.Shutdown(SocketShutdown.Both); }
                        catch (SystemException) { }
                    socket.Close();
                    socket = null;
                }
            }
        }
        public void RunPrepare()
        {
            this.keepRunning = true;
        }
        public void Run()
        {
            if (serverEndPoint != null)
            {
                Console.WriteLine("[Connecting to {0}...]", serverEndPoint);

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                if (connector == null) socket.Connect(serverEndPoint); else connector.Connect(socket, serverEndPoint);
                Console.WriteLine("[Connected]");
            }

            try
            {
                while (keepRunning)
                {
                    String prompt;
                    lock (syncObject)
                    {
                        prompt = (socket == null || !socket.Connected) ? "NotConnected" : socket.RemoteEndPoint.ToString();
                    }
                    Console.Write(prompt + ">");
                    String line = Console.ReadLine();

                    String command = line.Peel(out line);
                    if (String.IsNullOrEmpty(command)) continue;

                    CommandFunction function;
                    if (!commandDictionary.TryGetValue(command, out function))
                    {
                        Console.WriteLine("Unknown command '{0}'", command);
                    }
                    else
                    {
                        function(command);
                    }
                }
            }
            finally
            {
                Disconnect();
            }
        }

        public void ReceiveThread()
        {
            try
            {
                byte[] buffer = new byte[recvBufferSize];
                while (true)
                {
                    Int32 bytesRead = socket.Receive(buffer, SocketFlags.None);
                    if (bytesRead <= 0) return;
                    if (messageLogger != null) messageLogger.Log("[RecvThread] Received {0} bytes", bytesRead);
                    if (connectionLogger != null) connectionLogger.LogDataBToA(buffer, 0, bytesRead);
                }
            }
            catch (SocketException e)
            {
                if (messageLogger != null) messageLogger.Log("Receive thread SocketException: {0}", e.Message);
            }
            finally
            {
                lock (syncObject)
                {
                    if (socket != null) socket.Close();
                }
                if (messageLogger != null) messageLogger.Log("Receive thread stopped");
            }
        }
        public void OpenCommand(String line)
        {
            String server = line.Peel(out line);
            if (String.IsNullOrEmpty(server))
            {
                if (serverEndPoint == null)
                {
                    Console.WriteLine("Error: missing server and port");
                    return;
                }
            }
            else
            {
                line = (line == null) ? null : line.Trim();
                if (String.IsNullOrEmpty(line))
                {
                    Console.WriteLine("Error: missing port");
                    return;
                }
                UInt16 port;
                try
                {
                    port = UInt16.Parse(line);
                }
                catch (FormatException)
                {
                    Console.WriteLine("Error: invalid port '{0}'", line);
                    return;
                }
                serverEndPoint = EndPoints.EndPointFromIPOrHost(server, port);
            }
            Open();
        }
        void Open()
        {
            lock (syncObject)
            {
                if (socket != null && socket.Connected)
                {
                    Console.WriteLine("Already connected");
                    return;
                }

                Console.WriteLine("[Connecting to {0}...]", serverEndPoint.ToString());
                try
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    if (connector == null) socket.Connect(serverEndPoint); else connector.Connect(socket, serverEndPoint);
                    new Thread(ReceiveThread).Start();

                    Console.WriteLine("[Connected]");
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
        public void CloseCommand(String line)
        {
            lock (syncObject)
            {
                if (socket == null || !socket.Connected)
                {
                    Console.WriteLine("[Not-Connected]");
                }
                else
                {
                    Disconnect();
                }
            }
        }
        public void SendCommand(String line)
        {
            lock (syncObject)
            {
                if (socket == null || !socket.Connected)
                {
                    Console.WriteLine("[Not-Connected]");
                }
                else
                {
                    Int32 dataLength;
                    Byte[] dataAsBytes = GetData(line, 0, out dataLength);
                    if (dataAsBytes != null && dataLength > 0)
                    {
                        //dataAsBytes = Encoding.UTF8.GetBytes(data);
                        socket.Send(dataAsBytes, dataLength, SocketFlags.None);
                    }
                }
            }
        }
        public void SendFileCommand(String line)
        {
            lock (syncObject)
            {
                if (socket == null || !socket.Connected)
                {
                    Console.WriteLine("[Not-Connected]");
                    return;
                }

                line = (line == null) ? null : line.Trim();
                if (String.IsNullOrEmpty(line))
                {
                    Console.WriteLine("Missing file");
                    return;
                }

                socket.SendFile(line);
            }
        }
        public void ProxyCommand(String line)
        {
            connector = ConnectorParser.ParseProxy(line);
        }
        public void HelpCommand(String line)
        {
            foreach (KeyValuePair<String, CommandFunction> pair in commandDictionary)
            {
                Console.WriteLine("{0}", pair.Key);
            }
        }
        public void ExitCommand(String line)
        {
            keepRunning = false;
        }
        public void EchoCommand(String line)
        {
            Int32 dataLength;
            Byte[] dataAsBytes = GetData(line, 0, out dataLength);
            if (dataAsBytes != null && dataLength > 0)
            {
                Console.WriteLine(Encoding.UTF8.GetString(dataAsBytes, 0, dataLength));
            }
        }
    }
}
