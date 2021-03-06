﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;

namespace More.Net
{
    public class HttpToCtpSelectServerHandler
    {
        //readonly Dictionary<Socket, HttpToCdpClient> clients;
        //public readonly Dictionary<Socket, Socket> serverSocketsToClientSockets;

        public HttpToCtpSelectServerHandler()
        {
            //this.clients = new Dictionary<Socket, HttpToCdpClient>();
            //this.serverSocketsToClientSockets = new Dictionary<Socket, Socket>();
        }

        public void AcceptCallback(ref SelectControl control, Socket listenSock, Buf safeBuffer)
        {
            Socket newSock = listenSock.Accept();
            String clientEndPointString = newSock.SafeRemoteEndPointString();
            Console.WriteLine("[{0}] New Connection", clientEndPointString);
            //clients.Add(socket, newClient);
            control.AddReceiveSocket(newSock, new HttpToCdpClient(clientEndPointString, newSock, this).TcpRecvHandler);
        }

        /*
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
            HttpToCdpClient newClient = new HttpToCdpClient(socket, this);

            clients.Add(socket, newClient);
            Console.WriteLine("[{0}] New Connection ({1} clients)", newClient.clientEndPointString, clientCount);
            return ServerInstruction.NoInstruction;
        }

        public void PrintDictionaries()
        {
            foreach (KeyValuePair<Socket, HttpToCdpClient> pair in clients)
            {
                Console.WriteLine("Client Socket '{0}'", pair.Key.RemoteEndPoint);
            }
            foreach (KeyValuePair<Socket, Socket> pair in serverSocketsToClientSockets)
            {
                Console.WriteLine("Server Socket '{0}'", pair.Key.RemoteEndPoint);
            }
        }

        public ServerInstruction ClientCloseCallback(UInt32 clientCount, Socket socket)
        {
            Socket clientSocket;
            if (serverSocketsToClientSockets.TryGetValue(socket, out clientSocket))
            {
                Console.WriteLine("Server socket closed");

                
                serverSocketsToClientSockets.Remove(socket);
                
                // Close and remove the client socket as well                
                if (clientSocket.Connected)
                {
                    try { clientSocket.Shutdown(SocketShutdown.Both); }
                    catch(IOException) { }
                }
                //try { clientSocket.Close(); }
                //catch(IOException) { }

                return ServerInstruction.CloseClient;
            }

            HttpToCdpClient client;
            if (clients.TryGetValue(socket, out client))
            {
                Console.WriteLine("Client socket closed");


                clients.Remove(socket);
                if (client.serverSocket != null)
                {
                    if (client.serverSocket.Connected)
                    {
                        try { client.serverSocket.Shutdown(SocketShutdown.Both); }
                        catch (IOException) { }
                    }
                    //try { client.serverSocket.Close(); }
                    //catch (IOException) { }
                }

                return ServerInstruction.CloseClient;
            }

            Console.WriteLine("[Warning] Received close for a client '{0}' that wasn't in the client dictionary or the server dictionary", socket.RemoteEndPoint);
            return ServerInstruction.NoInstruction;   
        }
        public ServerInstruction ClientDataCallback(Socket socket, Byte[] bytes, UInt32 bytesRead)
        {
            HttpToCdpClient client;
            if (clients.TryGetValue(socket, out client))
            {
                return client.Data(bytes, bytesRead);
            }

            Socket clientSocket;
            if (serverSocketsToClientSockets.TryGetValue(socket, out clientSocket))
            {
                try
                {
                    clientSocket.Send(bytes, (Int32)bytesRead, SocketFlags.None);
                    return ServerInstruction.NoInstruction;
                }
                catch (SocketException)
                {
                    serverSocketsToClientSockets.Remove(socket);
                    try { socket.Shutdown(SocketShutdown.Both); }
                    catch (IOException) { }
                    try { socket.Close(); }
                    catch (IOException) { }
                    clients.Remove(clientSocket);
                    return ServerInstruction.CloseClient;
                }
            }

            Console.WriteLine("[Warning] Received close for a client '{0}' that wasn't in the client dictionary or the server dictionary", socket.RemoteEndPoint);
            return ServerInstruction.CloseClient;
        }
        */
    }
}
