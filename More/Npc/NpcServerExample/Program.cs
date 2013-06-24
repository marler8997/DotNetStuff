using System;

using System.Net.Sockets;
using System.Net;
using System.Threading;

using TestNamespace;

namespace HP.Libraries.Npc
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Expected 1 argument (the port number) but you gave {0}", args.Length);
                return;
            }

            UInt16 port = UInt16.Parse(args[0]);

            NpcReflector npcReflector = new NpcReflector(
                new Device(),
                new UsbImpl(),
                typeof(StaticClass));

            npcReflector.PrintInformation(Console.Out);

            INpcHtmlGenerator htmlGenerator = new DefaultNpcHtmlGenerator("NpcServicePage", npcReflector);

            Boolean useWrapper = true;
            if (useWrapper)
            {
                WrapperExample(npcReflector, htmlGenerator, port);
            }
            else
            {
                NoWrapperExample(npcReflector, htmlGenerator, port);
            }
        }

        static void WrapperExample(NpcReflector npcReflector, INpcHtmlGenerator htmlGenerator, UInt16 port)
        {
            NpcServerSingleThreaded wrapper = new NpcServerSingleThreaded(NpcLoggerCallback.ConsoleLogger, npcReflector,
                htmlGenerator, port);
            wrapper.Run();
        }

        static void NoWrapperExample(NpcReflector npcReflector, INpcHtmlGenerator htmlGenerator, UInt16 port)
        {
            //
            // Accept Connections Loop
            //
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            listenSocket.Listen(32);

            while (true)
            {
                Socket clientSocket = listenSocket.Accept();

                String clientString = clientSocket.RemoteEndPoint.ToString();
                Console.WriteLine("{0} [ListenThread] New Client '{1}'", DateTime.Now, clientString);

                NpcBlockingThreadHander npcHandler =
                    new NpcBlockingThreadHander(NpcLoggerCallback.ConsoleLogger, clientSocket, npcReflector, htmlGenerator);
                Thread handlerThread = new Thread(npcHandler.Run);
                handlerThread.Start();
            }
        }
    }
}
