using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

namespace Marler.NetworkTools
{
    public delegate void ReceiveHandler(Socket socket, Byte[] buffer, Int32 bytesRead);

    class SelectBasedServer
    {
        private readonly ReceiveHandler receiveHandler;
        private readonly UInt16 listenPort;
        private readonly Int32 bufferSize;
        private readonly Int32 socketBackLog;

        private Thread thread;

        public SelectBasedServer(ReceiveHandler receiveHandler, UInt16 listenPort, Int32 bufferSize, Int32 socketBackLog)
        {
            this.receiveHandler = receiveHandler;
            this.bufferSize = bufferSize;
            this.listenPort = listenPort;
            this.socketBackLog = socketBackLog;

            thread = null;
        }


        public void Start()
        {
            if (thread != null) throw new InvalidOperationException("Already Started");

            thread = new Thread(Run);
            thread.IsBackground = true;
            thread.Start();
        }

        public void Run()
        {
            byte[] buffer = new byte[bufferSize];
            List<Socket> readSockets = new List<Socket>();
            List<Socket> connectedSockets = new List<Socket>();
            try
            {
                Socket listenSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(new IPEndPoint(IPAddress.Any, listenPort));
                listenSocket.Listen(socketBackLog);

                while (true)
                {
                    // Fill the read list
                    readSockets.Add(listenSocket);
                    readSockets.AddRange(connectedSockets);

                    // Wait for something to do
                    Socket.Select(readSockets, null, null, -1);

                    if (readSockets.Count > 0)
                    {
                        // Process each socket that has something to do
                        foreach (Socket currentSocket in readSockets)
                        {
                            if (currentSocket == listenSocket)
                            {
                                // Accept and store the new client's socket
                                Socket newSocket = currentSocket.Accept();
                                connectedSockets.Add(newSocket);
                            }
                            else
                            {
                                // Read and process the data as appropriate
                                int bytesRead = currentSocket.Receive(buffer);
                                if (bytesRead == 0)
                                {
                                    connectedSockets.Remove(currentSocket);
                                    if (currentSocket.Connected) currentSocket.Shutdown(SocketShutdown.Both);
                                    currentSocket.Close();
                                }
                                else
                                {
                                    receiveHandler(currentSocket, buffer, bytesRead);
                                }
                            }
                        }
                        readSockets.Clear();
                    }
                }
            }
            catch (SocketException exc)
            {
                Console.WriteLine("Socket exception: " + exc.SocketErrorCode);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception: " + exc);
            }
            finally
            {
                foreach (Socket socket in connectedSockets)
                {
                    if (socket.Connected) socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                connectedSockets.Clear();
            }
        }
    }
}
