using System;
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
    public readonly CLSwitch listenMode;

    public readonly CLGenericArgument<UInt16> localPort;
    public readonly CLStringArgument localHost;

    public readonly CLInt32Argument bufferSizes;
    public readonly CLInt32Argument tcpSendWindow;
    public readonly CLInt32Argument tcpReceiveWindow;

    public Options()
        : base()
    {
        listenMode = new CLSwitch('l', "listen mode", "Specifies that NetCat will listen for a tcp connection");
        Add(listenMode);

        localPort = new CLGenericArgument<UInt16>(UInt16.Parse, 'p', "the local port to bind to");
        localPort.SetDefault(0);
        Add(localPort);

        localHost = new CLStringArgument('i', "the local host or ip address to bind to");
        localHost.SetDefault("0.0.0.0");
        Add(localHost);

        bufferSizes = new CLInt32Argument('s', "tunnel buffer sizes");
        bufferSizes.SetDefault(8192);
        Add(bufferSizes);

        tcpSendWindow = new CLInt32Argument("send-window", "Size of TCP send window (0 means network stack window size)");
        tcpSendWindow.SetDefault(0);
        Add(tcpSendWindow);
        tcpReceiveWindow = new CLInt32Argument("recv-window", "Size of TCP recv window (0 means network stack window size)");
        tcpReceiveWindow.SetDefault(0);
        Add(tcpReceiveWindow);
    }

    public override void PrintUsageHeader()
    {
        Console.WriteLine("Outbound Connection: NetCat.exe [options] <host-connector> <port>");
        Console.WriteLine("InBound Connection : NetCat.exe [options] -l -p <listen-port>");
    }
}
public class NetCat
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

    static Socket dataSocket;
    static Boolean closed;

    static Int32 Main(string[] args)
    {
        Options options = new Options();
        if (args.Length == 0)
        {
            options.PrintUsage();
            return 0;
        }

        List<String> nonOptionArgs = options.Parse(args);

        socketToConsoleBuffer = new Byte[options.bufferSizes.ArgValue];
        consoleToSocketBuffer = new Byte[options.bufferSizes.ArgValue];

        if (options.listenMode.set)
        {
            IPAddress localAddress = EndPoints.ParseIPOrResolveHost(options.localHost.ArgValue, DnsPriority.IPv4ThenIPv6);

            Socket listenSocket = new Socket(localAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(localAddress, options.localPort.ArgValue));
            listenSocket.Listen(1);

            dataSocket = listenSocket.Accept();
        }
        else if (nonOptionArgs.Count != 2)
        {
            if (nonOptionArgs.Count == 0)
            {
                options.PrintUsage();
                return 0;
            }
            return options.ErrorAndUsage("In client/connect mode there should be 2 non-option command line arguments but got {0}", nonOptionArgs.Count);
        }
        else
        {
            String connectorString = nonOptionArgs[0];

            String portString = nonOptionArgs[1];
            UInt16 port = UInt16.Parse(portString);

            InternetHost host;
            {
                Proxy proxy;
                String ipOrHost = Proxy.StripAndParseProxies(connectorString, DnsPriority.IPv4ThenIPv6, out proxy);
                host = new InternetHost(ipOrHost, port, DnsPriority.IPv4ThenIPv6, proxy);
            }

            dataSocket = new Socket(host.GetAddressFamilyForTcp(), SocketType.Stream, ProtocolType.Tcp);

            if(options.localPort.set)
            {
                dataSocket.Bind(new IPEndPoint(IPAddress.Any, options.localPort.ArgValue));
            }

            BufStruct leftOverData = new BufStruct(socketToConsoleBuffer);
            host.Connect(dataSocket, DnsPriority.IPv4ThenIPv6, ProxyConnectOptions.None, ref leftOverData);
            if (leftOverData.contentLength > 0)
            {
                PrepareConsole();
                consoleOutputStream.Write(leftOverData.buf, 0, (int)leftOverData.contentLength);
            }
        }

        // Note: I'm not sure these options are actually working
        if (options.tcpSendWindow.ArgValue != 0)
        {
            dataSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, options.tcpSendWindow.ArgValue);
        }
        if (options.tcpReceiveWindow.ArgValue != 0)
        {
            dataSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, options.tcpReceiveWindow.ArgValue);
        }

        PrepareConsole();

        Thread consoleReadThread = new Thread(ConsoleReceiveThread);
        consoleReadThread.Name = "Console Read Thread";

        // Don't know how to cancel a console input stream read, so I have to
        // put the console read thread as a background thread so it doesn't keep
        // the program running.
        consoleReadThread.IsBackground = true;

        consoleReadThread.Start();

        try
        {
            while (true)
            {
                Int32 bytesRead = dataSocket.Receive(socketToConsoleBuffer, socketToConsoleBuffer.Length, SocketFlags.None);
                if (bytesRead <= 0)
                {
                    lock (typeof(NetCat))
                    {
                        if (!closed)
                        {

                            closed = true;
                        }
                    }
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
            lock (typeof(NetCat))
            {
                if (!closed)
                {
                    consoleInputStream.Dispose();
                    closed = true;
                }
            }
            // Do not join other thread, just exit
        }

        return 0;
    }
    static void ConsoleReceiveThread()
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
                dataSocket.Send(consoleToSocketBuffer, 0, bytesRead, SocketFlags.None);
            }
        }
        finally
        {
            lock (typeof(NetCat))
            {
                if (!closed)
                {
                    dataSocket.ShutdownAndDispose();
                    closed = true;
                }
            }
        }
    }
}
