using System;
using System.Net.Sockets;
using System.Text;

namespace Marler.NetworkTools
{
    public interface ITelnetHandler
    {
        void ReceivedEor();
        Byte ReceivedWill(Byte option);
        Byte ReceivedWont(Byte option);
        Byte ReceivedDo(Byte option);
        Byte ReceivedDont(Byte option);
    }

    public class DefaultTelnetHandler : ITelnetHandler
    {
        private readonly static Object syncObject = new Object();
        private static DefaultTelnetHandler instance = null;

        public static ITelnetHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncObject)
                    {
                        if (instance == null)
                        {
                            instance = new DefaultTelnetHandler();
                        }
                    }
                }
                return instance;
            }
        }

        private DefaultTelnetHandler() { }
        public void ReceivedEor() { }
        public byte ReceivedWill(Byte option)
        {
            return (option == Telnet.OPT_SUPPRESS_GO_AHEAD) ? Telnet.CMD_DO : Telnet.CMD_DONT;
        }
        public byte ReceivedWont(Byte option) { return               0; }
        public byte ReceivedDo  (Byte option) { return Telnet.CMD_WONT; }
        public byte ReceivedDont(Byte option) { return               0; }
    }

    public class TelnetStream : IDisposable
    {
        delegate Byte TelnetOptionHandler(Byte option);

        private const byte STATE_DATA            = 0;
        private const byte STATE_IAC             = 1;
        private const byte STATE_IAC_SB          = 2;
        private const byte STATE_IAC_WILL        = 3;
        private const byte STATE_IAC_WONT        = 4;
        private const byte STATE_IAC_DO          = 5;
        private const byte STATE_IAC_DONT        = 6;
        private const byte STATE_IAC_SB_IAC      = 7;
        private const byte STATE_IAC_SB_DATA     = 8;
        private const byte STATE_IAC_SB_DATA_IAC = 9;

        public readonly Socket socket;
        private readonly ITelnetHandler handler;
        private readonly Boolean ownsSocket;

        private Byte state;

        private byte[] receivedWillWont;
        private byte[] sentWillWont;
        private byte[] receivedDoDont;
        private byte[] sentDoDont;

        public TelnetStream(Socket socket)
            : this(socket, null, true)
        {
        }
        public TelnetStream(Socket socket, Boolean ownsSocket)
            : this(socket, null, ownsSocket)
        {
        }
        public TelnetStream(Socket socket, ITelnetHandler handler)
            : this(socket, handler, true)
        {
        }
        public TelnetStream(Socket socket, ITelnetHandler handler, Boolean ownsSocket)
        {
            if (socket == null) throw new ArgumentNullException("socket");

            this.socket = socket;
            this.handler = (handler == null) ? DefaultTelnetHandler.Instance : handler;
            this.ownsSocket = ownsSocket;

            Initialize();
        }

        private void Initialize()
        {
            this.state = 0;

            this.receivedWillWont = new Byte[0x100];
            this.sentWillWont = new Byte[0x100];
            this.receivedDoDont = new Byte[0x100];
            this.sentDoDont = new Byte[0x100];
        }

        public void Dispose()
        {
            if (ownsSocket)
            {
                if (socket.Connected)
                {
                    try
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }
                    catch { }
                }
                socket.Close();
            }
        }

        public Int32 Receive(Byte[] buffer, Int32 offset, Int32 length)
        {
            Int32 firstEscapeOffset;

            while (true)
            {
                Int32 bytesRead = socket.Receive(buffer, offset, length, SocketFlags.None);
                if (bytesRead <= 0) return bytesRead;

                if (state == STATE_DATA)
                {
                    Int32 limit = offset + length;
                    for (firstEscapeOffset = offset; firstEscapeOffset < limit; firstEscapeOffset++)
                    {
                        if (buffer[firstEscapeOffset] == Telnet.CMD_IAC)
                        {
                            goto HANDLE_ESCAPED_BYTES;
                        }
                    }

                    return bytesRead;
                }

                firstEscapeOffset = offset;

            HANDLE_ESCAPED_BYTES:

                bytesRead = HandleEscapes(buffer, firstEscapeOffset, bytesRead);
                if (bytesRead > 0) return bytesRead;
            }
        }

        public Int32 Receive(Byte[] buffer)
        {
            Int32 firstEscapeOffset;

            while (true)
            {
                Int32 bytesRead = socket.Receive(buffer);
                if (bytesRead <= 0) return bytesRead;

                if (state == STATE_DATA)
                {
                    for (firstEscapeOffset = 0; firstEscapeOffset < bytesRead; firstEscapeOffset++)
                    {
                        if (buffer[firstEscapeOffset] == Telnet.CMD_IAC)
                        {
                            goto HANDLE_ESCAPED_BYTES;
                        }
                    }

                    return bytesRead;
                }

                firstEscapeOffset = 0;

            HANDLE_ESCAPED_BYTES:

                bytesRead = HandleEscapes(buffer, firstEscapeOffset, bytesRead);
                if (bytesRead > 0) return bytesRead;
            }
        }

        private Int32 HandleEscapes(Byte[] buffer, Int32 offset, Int32 offsetLimit)
        {
            // offset     : is the current offset into the buffer of actual data
            // highOffset : is the offset of the actual buffer that is being processed
            // The more telnet commands that are present, the bigger the difference between
            // offset and highOffset
            //
            // Memory
            //
            // [ 0 ] [ 1 ] [ 2 ] ... [ x ] [x+1]
            //         ^               ^
            //         offset          highOffset
            //
            //

            Byte cmd;
            Byte[] cmdBuffer = new Byte[3]; // buffer for sending response commands
            cmdBuffer[0] = Telnet.CMD_IAC;

            Int32 highOffset = offset;

            while (highOffset < offsetLimit)
            {
                byte nextByte = buffer[highOffset++];
                switch (state)
                {
                    case STATE_DATA:
                        if (nextByte != Telnet.CMD_IAC)
                        {
                            buffer[offset++] = nextByte;
                            continue;
                        }

                        //Console.WriteLine();
                        //Console.Write("RECV IAC,");
                        state = STATE_IAC;
                        continue;
                    case STATE_IAC:
                        switch (nextByte)
                        {
                            case Telnet.CMD_EOR:
                                //Console.WriteLine("EOR");
                                handler.ReceivedEor();
                                state = STATE_DATA;
                                continue;
                            case Telnet.CMD_SB:
                                //Console.WriteLine("SB,");
                                state = STATE_IAC_SB;
                                continue;
                            case Telnet.CMD_WILL:
                                //Console.Write("WILL,");
                                state = STATE_IAC_WILL;
                                continue;
                            case Telnet.CMD_WONT:
                                //Console.Write("WONT,");
                                state = STATE_IAC_WONT;
                                continue;
                            case Telnet.CMD_DO:
                                //Console.Write("DO,");
                                state = STATE_IAC_DO;
                                continue;
                            case Telnet.CMD_DONT:
                                //Console.Write("DONT,");
                                state = STATE_IAC_DONT;
                                continue;
                            case Telnet.CMD_IAC:
                                //Console.Write("IAC");
                                state = STATE_DATA;
                                buffer[offset++] = Telnet.CMD_IAC;
                                continue;
                        }

                        Console.WriteLine(Telnet.CmdToString(nextByte));
                        state = STATE_DATA;
                        continue;
                        
                    case STATE_IAC_SB: // Context: IAC,SB

                        if (nextByte == Telnet.CMD_SE)
                        {
                            //Console.WriteLine("SE", nextByte);
                            state = STATE_DATA;
                        }
                        else
                        {
                            //Console.Write("0x{0:X2},", nextByte);
                        }
                        continue;

                    case STATE_IAC_WILL:
                        //Console.WriteLine(Telnet.OptToString(nextByte));

                        cmd = handler.ReceivedWill(nextByte);

                        if (
                            (cmd == Telnet.CMD_DO || cmd == Telnet.CMD_DONT) &&
                            (receivedWillWont[nextByte] != Telnet.CMD_WILL || sentDoDont[nextByte] != cmd))
                        {
                            receivedWillWont[nextByte] = Telnet.CMD_WILL;

                            //Console.WriteLine("RESP IAC,{0},{1}", Telnet.CmdToString(cmd), Telnet.OptToString(nextByte));
                            cmdBuffer[1] = cmd;
                            cmdBuffer[2] = nextByte;
                            socket.Send(cmdBuffer);

                            sentDoDont[nextByte] = cmd;
                        }

                        state = STATE_DATA;
                        continue;
                    case STATE_IAC_WONT: // Context: IAC,WONT
                        //Console.WriteLine(Telnet.OptToString(nextByte));

                        cmd = handler.ReceivedWont(nextByte);

                        if (
                            (cmd == Telnet.CMD_DO || cmd == Telnet.CMD_DONT) &&
                            (receivedWillWont[nextByte] != Telnet.CMD_WONT || sentDoDont[nextByte] != cmd))
                        {
                            receivedWillWont[nextByte] = Telnet.CMD_WONT;

                            //Console.WriteLine("RESP IAC,{0},{1}", Telnet.CmdToString(cmd), Telnet.OptToString(nextByte));
                            cmdBuffer[1] = cmd;
                            cmdBuffer[2] = nextByte;
                            socket.Send(cmdBuffer);

                            sentDoDont[nextByte] = cmd;
                        }

                        state = STATE_DATA;
                        continue;
                    case STATE_IAC_DO:
                        //Console.WriteLine(Telnet.OptToString(nextByte));

                        cmd = handler.ReceivedDo(nextByte);

                        if (
                            (cmd == Telnet.CMD_WILL || cmd == Telnet.CMD_WONT) &&
                            (receivedDoDont[nextByte] != Telnet.CMD_DO || sentWillWont[nextByte] != cmd))
                        {
                            receivedDoDont[nextByte] = Telnet.CMD_DO;

                            //Console.WriteLine("RESP IAC,{0},{1}", Telnet.CmdToString(cmd), Telnet.OptToString(nextByte));
                            cmdBuffer[1] = cmd;
                            cmdBuffer[2] = nextByte;
                            socket.Send(cmdBuffer);

                            sentWillWont[nextByte] = cmd;
                        }

                        state = STATE_DATA;
                        continue;
                    case STATE_IAC_DONT:
                        //Console.WriteLine(Telnet.OptToString(nextByte));

                        cmd = handler.ReceivedDont(nextByte);

                        if (
                            (cmd == Telnet.CMD_WILL || cmd == Telnet.CMD_WONT) &&
                            (receivedDoDont[nextByte] != Telnet.CMD_DONT || sentWillWont[nextByte] != cmd))
                        {
                            receivedDoDont[nextByte] = Telnet.CMD_DONT;

                            //Console.WriteLine("RESP IAC,{0},{1}", Telnet.CmdToString(cmd), Telnet.OptToString(nextByte));
                            cmdBuffer[1] = cmd;
                            cmdBuffer[2] = nextByte;
                            socket.Send(cmdBuffer);

                            sentWillWont[nextByte] = cmd;
                        }

                        state = STATE_DATA;
                        continue;
                   
                    default:
                        state = STATE_DATA;
                        continue;
                }
            }
            return offset;
        }

        public void SendLine(String str)
        {
            //
            // TODO: Change algorithm to loop through the string until an escape
            //       character is found, if none is found, send the string...otherwise
            //       create a new array that can hold the worst case string and continue
            //       looping at the first escape character coping the string and translating
            //       escape characters.
            //

            Byte[] telnetBytes = new Byte[str.Length * 2 + 2]; // add 2 for carraige return
            Int32 telnetBytesOffset = 0;

            if (receivedDoDont[0x80 + Telnet.OPT_BINARY] != Telnet.CMD_DONT)
            {
                //
                // Translate '\n', '\r' and 0xFF to escaped characters
                //
                for (Int32 i = 0; i < str.Length; i++)
                {
                    Char currentChar = str[i];

                    if (currentChar != (byte)'\r' && currentChar != (byte)'\n' && currentChar != Telnet.CMD_IAC)
                    {
                        telnetBytes[telnetBytesOffset++] = (Byte)currentChar;
                        continue;
                    }

                    if (currentChar == (byte)'\n')
                    {
                        telnetBytes[telnetBytesOffset++] = (byte)'\r';
                        telnetBytes[telnetBytesOffset++] = (byte)'\n';
                        continue;
                    }

                    if (currentChar == (byte)'\r')
                    {
                        telnetBytes[telnetBytesOffset++] = (byte)'\r';
                        telnetBytes[telnetBytesOffset++] = (byte)'\0';
                        continue;
                    }

                    // currentByte == Telnet.CMD_IAC
                    telnetBytes[telnetBytesOffset++] = Telnet.CMD_IAC;
                    telnetBytes[telnetBytesOffset++] = Telnet.CMD_IAC;
                }
            }
            else
            {
                //
                // Translate 0xFF to 0xFF,0xFF
                //
                for (Int32 i = 0; i < str.Length; i++)
                {
                    if (str[i] != 0xFF)
                    {
                        telnetBytes[telnetBytesOffset++] = (Byte)str[i];
                    }
                    else
                    {
                        telnetBytes[telnetBytesOffset++] = Telnet.CMD_IAC;
                        telnetBytes[telnetBytesOffset++] = Telnet.CMD_IAC;
                    }
                }

            }

            //
            // Add Carriage Return
            //
            telnetBytes[telnetBytesOffset++] = (byte)'\r';
            telnetBytes[telnetBytesOffset++] = (byte)'\0';

            //
            // Send
            //
            socket.Send(telnetBytes, 0, telnetBytesOffset, SocketFlags.None);
        }
    }
}
