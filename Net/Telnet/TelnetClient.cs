using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace More.Net
{
    public class TelnetClient : ITelnetHandler
    {
        public readonly Boolean wantServerEcho;

        private Boolean stillConnected;

        public TelnetClient(Boolean wantServerEcho)
        {
            this.wantServerEcho = wantServerEcho;
        }

        public void Run(Socket connectedSocket)
        {
            using (TelnetStream stream = new TelnetStream(connectedSocket, this, true))
            {
                stillConnected = true;

                ConsolePrintCallback receiver = new ConsolePrintCallback(this, stream);
                new Thread(receiver.Run).Start();

                while (stillConnected)
                {
                    stream.SendLine(Console.ReadLine());
                }
            }
        }

        public void ReceiveLoopDone(Exception e)
        {
            stillConnected = false;
            if (e != null)
            {
                Console.WriteLine("{0} in Receive Loop: {0}", e.GetType(), e.Message);
            }
            Console.WriteLine("[Disconnected]");
            Console.WriteLine("Press <enter> to exit");
        }

        public void ReceivedEor()
        {
            throw new NotImplementedException();
        }

        public byte ReceivedWill(byte option)
        {
            switch (option)
            {
                case Telnet.OPT_BINARY:
                    return Telnet.CMD_DO;
                case Telnet.OPT_ECHO:
                    return wantServerEcho ? Telnet.CMD_DO : Telnet.CMD_DONT;
                case Telnet.OPT_RECONNECTION:
                    return Telnet.CMD_DONT;
                case Telnet.OPT_SUPPRESS_GO_AHEAD:
                    return Telnet.CMD_DO;
            }

            return Telnet.CMD_DONT;
        }

        public byte ReceivedWont(byte option)
        {
            return 0;
        }

        public byte ReceivedDo(byte option)
        {
            return Telnet.CMD_WONT;
        }

        public byte ReceivedDont(byte option)
        {
            return 0;
        }

        class ConsolePrintCallback
        {
            private readonly TelnetClient client;
            private readonly TelnetStream stream;

            public ConsolePrintCallback(TelnetClient client, TelnetStream stream)
            {
                this.client = client;
                this.stream = stream;
            }

            public void Run()
            {
                Exception potentialException = null;
                try
                {
                    Byte[] buffer = new Byte[4096];

                    while (true)
                    {
                        Int32 bytesRead = stream.Receive(buffer);

                        if (bytesRead <= 0) break;

                        Console.Write(Encoding.ASCII.GetString(buffer, 0, bytesRead));

                    }
                }
                catch (Exception e)
                {
                    potentialException = e;
                }
                finally
                {
                    client.ReceiveLoopDone(potentialException);
                }
            }

        }

    }
}
