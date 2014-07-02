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
        public readonly Boolean enableColorDecoding;

        private Boolean stillConnected;

        public TelnetClient(Boolean wantServerEcho, Boolean enableColorDecoding)
        {
            this.wantServerEcho = wantServerEcho;
            this.enableColorDecoding = enableColorDecoding;
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
            readonly TelnetClient client;
            readonly TelnetStream stream;

            readonly Char[] charBuffer = new Char[2048];

            public ConsolePrintCallback(TelnetClient client, TelnetStream stream)
            {
                this.client = client;
                this.stream = stream;
            }
            void AnsiEscapeHandler(Byte[] data, UInt32 offset, UInt32 length)
            {
                /*
                Console.WriteLine("\r\nEscape: '{0}' {1}",
                    Encoding.ASCII.GetString(data, (int)offset, (int)length),
                    data.ToHexString((int)offset, (int)length));
                */
                if (data[offset] == '[')
                {
                    if (data[offset + length - 1] == 'm')
                    {
                        for (int i = 1; i < length; i += 3)
                        {
                            Byte firstDigit = data[offset + i];
                            Byte secondDigit = data[offset + i + 1];
                            
                            ConsoleColor color;

                            //Console.WriteLine("Digits: '{0}' '{1}' {2} {3}",
                            //    (Char)firstDigit, (Char)secondDigit, firstDigit, secondDigit);
                            if (secondDigit < '0' || secondDigit > '9')
                            {
                                // Reset Formatting
                                if (firstDigit == '0')
                                {
                                    Console.ForegroundColor = TelnetProgram.DefaultConsoleForegroundColor;
                                    Console.BackgroundColor = TelnetProgram.DefaultConsoleBackgroundColor;
                                }
                            }
                            else
                            {
                                switch (firstDigit)
                                {
                                    case (Byte)'3':
                                    case (Byte)'4':
                                        switch (secondDigit)
                                        {
                                            case (Byte)'0': color = ConsoleColor.Black; break;
                                            case (Byte)'1': color = ConsoleColor.Red; break;
                                            case (Byte)'2': color = ConsoleColor.Green; break;
                                            case (Byte)'3': color = ConsoleColor.Yellow; break;
                                            case (Byte)'4': color = ConsoleColor.Blue; break;
                                            case (Byte)'5': color = ConsoleColor.Magenta; break;
                                            case (Byte)'6': color = ConsoleColor.Cyan; break;
                                            case (Byte)'7': color = ConsoleColor.White; break;
                                            case (Byte)'9':
                                                if(firstDigit == (Byte)'3') {Console.ForegroundColor = TelnetProgram.DefaultConsoleForegroundColor; }
                                                else                        {Console.BackgroundColor = TelnetProgram.DefaultConsoleBackgroundColor; }
                                                goto SKIP_SET_COLOR;
                                            default: goto SKIP_SET_COLOR;
                                        }

                                        if(firstDigit == (Byte)'3') { Console.ForegroundColor = color; }
                                        else                        { Console.BackgroundColor = color; }

                                SKIP_SET_COLOR:
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            void AnsiDataHandler(Byte[] data, UInt32 offset, UInt32 length)
            {
                Int32 charCount = Encoding.ASCII.GetChars(data, (Int32)offset, (Int32)length, charBuffer, 0);
                Console.Write(charBuffer, 0, charCount);
            }
            public void Run()
            {
                AnsiEscapeDecoder ansiDecoder = null;
                if (client.enableColorDecoding)
                {
                    ansiDecoder = new AnsiEscapeDecoder(AnsiEscapeHandler, AnsiDataHandler);
                }

                Exception potentialException = null;
                try
                {
                    Int32 offset = 0;
                    Byte[] byteBuffer = new Byte[2048];

                    while (true)
                    {
                        Int32 bytesRead = stream.Receive(byteBuffer, offset, byteBuffer.Length - offset);
                        if (bytesRead <= 0)
                        {
                            if(offset > 0)
                            {
                                Console.Write(Encoding.ASCII.GetString(byteBuffer, 0, offset));
                                offset = 0;
                            }
                            break;
                        }

                        if (client.enableColorDecoding)
                        {
                            UInt32 processed = ansiDecoder.Decode(byteBuffer, 0, (UInt32)bytesRead);

                            UInt32 bytesLeft = (UInt32)bytesRead - processed;
                            if (bytesLeft > 0)
                            {
                                Array.Copy(byteBuffer, offset, byteBuffer, 0, bytesLeft);
                                offset = (Int32)bytesLeft;
                            }
                        }
                        else
                        {
                            Int32 charCount = Encoding.ASCII.GetChars(byteBuffer, 0, bytesRead, charBuffer, 0);
                            Console.Write(charBuffer, 0, charCount);
                        }
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
