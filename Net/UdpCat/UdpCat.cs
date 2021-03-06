﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using More;
using More.Net;

//
// Note: Maybe in the future it would be nice to have an option for single threaded using select or multi threaded
//
public class Options : CLParser
{
    public readonly CLGenericArgument<UInt16> localPort;
    public readonly CLStringArgument localHost;

    public readonly CLInt32Argument bufferSizes;

    public Options()
        : base()
    {
        localPort = new CLGenericArgument<UInt16>(UInt16.Parse, 'p', "the local port to bind to");
        localPort.SetDefault(0);
        Add(localPort);

        localHost = new CLStringArgument('i', "the local host or ip address to bind to");
        localHost.SetDefault("0.0.0.0");
        Add(localHost);

        bufferSizes = new CLInt32Argument('s', "tunnel buffer sizes");
        bufferSizes.SetDefault(8192);
        Add(bufferSizes);
    }

    public override void PrintUsageHeader()
    {
        Console.WriteLine("UdpCat.exe [options] [<host-connector> <port>]");
    }
}
public class UdpCat
{
    static Byte[] socketToConsoleBuffer;
    static Byte[] consoleToSocketBuffer;

    static Stream consoleOutputStream;
    static Stream consoleInputStream;
    static void PrepareConsole()
    {
        if (consoleOutputStream == null)
        {
            Console.Out.Flush();
            consoleOutputStream = Console.OpenStandardOutput();
            consoleInputStream = Console.OpenStandardInput();
        }
    }

    static Socket udpSocket;
    static Boolean closed;

    static EndPoint remoteEndPoint;

    static Int32 Main(String[] args)
    {
        Options options = new Options();
        if (args.Length == 0)
        {
            options.PrintUsage();
            return 0;
        }

        List<String> nonOptionArgs = options.Parse(args);

        ThreadStart consoleReceiveFunction;
        ThreadStart socketReceiveFunction;
        IPEndPoint serverEndPoint;
        if (nonOptionArgs.Count == 0)
        {
            consoleReceiveFunction = DisconnectedConsoleReceiveThread;
            socketReceiveFunction = DisconnectedSocketReceiveThread;
            remoteEndPoint = null;
        }
        else if (nonOptionArgs.Count == 2)
        {
            consoleReceiveFunction = ClientConsoleReceiveThread;
            socketReceiveFunction = ClientSocketReceiveThread;

            String ipOrHost = nonOptionArgs[0];
            UInt16 port = UInt16.Parse(nonOptionArgs[1]);

            // TODO: implement connect through proxy
            remoteEndPoint = new IPEndPoint(EndPoints.ParseIPOrResolveHost(ipOrHost, DnsPriority.IPv4ThenIPv6), port);
        }
        else
        {
            return options.ErrorAndUsage("Expected 0 or 2 non-option arguments but got {0}", nonOptionArgs.Count);
        }

        socketToConsoleBuffer = new Byte[options.bufferSizes.ArgValue];
        consoleToSocketBuffer = new Byte[options.bufferSizes.ArgValue];

        //
        // Create the UDP Socket
        //
        IPEndPoint localEndPoint = new IPEndPoint(
            EndPoints.ParseIPOrResolveHost(options.localHost.ArgValue, DnsPriority.IPv4ThenIPv6),
            options.localPort.ArgValue);

        udpSocket = new Socket(localEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        udpSocket.Bind(localEndPoint);

        PrepareConsole();

        Thread consoleReadThread = new Thread(consoleReceiveFunction);
        consoleReadThread.Name = "Console Read Thread";

        // Don't know how to cancel a console input stream read, so I have to
        // put the console read thread as a background thread so it doesn't keep
        // the program running.
        consoleReadThread.IsBackground = true;

        consoleReadThread.Start();

        socketReceiveFunction();

        return 0;
    }


    static void ClientSocketReceiveThread()
    {
        EndPoint from = new IPEndPoint(IPAddress.Any, 0);
        try
        {
            while (true)
            {
                Int32 bytesRead = udpSocket.ReceiveFrom(socketToConsoleBuffer, socketToConsoleBuffer.Length, SocketFlags.None, ref from);
                if (bytesRead < 0)
                {
                    udpSocket.Close();
                    break;
                }
                consoleOutputStream.Write(socketToConsoleBuffer, 0, bytesRead);
            }
        }
        catch (SocketException e)
        {
        }
        finally
        {
            lock (typeof(UdpCat))
            {
                if (!closed)
                {
                    consoleInputStream.Dispose();
                    closed = true;
                }
            }
            // Do not join other thread, just exit
        }
    }
    static void ClientConsoleReceiveThread()
    {
        try
        {
            while (true)
            {
                Int32 bytesRead = consoleInputStream.Read(consoleToSocketBuffer, 0, consoleToSocketBuffer.Length);
                if (bytesRead <= 0)
                {
                    break;
                }
                udpSocket.SendTo(consoleToSocketBuffer, 0, bytesRead, SocketFlags.None, remoteEndPoint);
            }
        }
        finally
        {
            lock (typeof(UdpCat))
            {
                if (!closed)
                {
                    udpSocket.Close();
                    closed = true;
                }
            }
        }
    }


    static void DisconnectedSocketReceiveThread()
    {
        EndPoint from = new IPEndPoint(IPAddress.Any, 0);
        try
        {
            while (true)
            {
                Int32 bytesRead = udpSocket.ReceiveFrom(socketToConsoleBuffer, socketToConsoleBuffer.Length, SocketFlags.None, ref from);
                if (bytesRead < 0)
                {
                    lock (typeof(UdpCat))
                    {
                        if (!closed)
                        {

                            closed = true;
                        }
                    }
                    break;
                }
                lock (typeof(UdpCat))
                {
                    remoteEndPoint = from;
                }
                consoleOutputStream.Write(socketToConsoleBuffer, 0, bytesRead);
            }
        }
        catch (SocketException e)
        {
        }
        finally
        {
            lock (typeof(UdpCat))
            {
                if (!closed)
                {
                    consoleInputStream.Dispose();
                    closed = true;
                }
            }
            // Do not join other thread, just exit
        }
    }
    static void DisconnectedConsoleReceiveThread()
    {
        try
        {
            while (true)
            {
                Int32 bytesRead = consoleInputStream.Read(consoleToSocketBuffer, 0, consoleToSocketBuffer.Length);
                if (bytesRead <= 0)
                {
                    break;
                }
                lock (typeof(UdpCat))
                {
                    if (remoteEndPoint != null)
                    {
                        udpSocket.SendTo(consoleToSocketBuffer, 0, bytesRead, SocketFlags.None, remoteEndPoint);
                    }
                }
            }
        }
        finally
        {
            lock (typeof(UdpCat))
            {
                if (!closed)
                {
                    udpSocket.ShutdownAndDispose();
                    closed = true;
                }
            }
        }
    }
}