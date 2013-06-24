using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace More.Net
{
    public static class Telnet
    {
        public const Byte CMD_EOR  = 0xEF; // End of record
        public const Byte CMD_SE   = 0xF0; // 240 End of subnegotiation parameters
        public const Byte CMD_NOP  = 0xF1; // 241 No operation
        public const Byte CMD_DM   = 0xF2; // 242 Data mark (Indicates the position of a Synch event within the data stream. Should always be accompanied by a TCP urgent notification)
        public const Byte CMD_BRK  = 0xF3; // 243 Break (Indicates that the 'break' or 'attention' key was hit)
        public const Byte CMD_IP   = 0xF4; // 244 Suspend (Interrupt or abort the process to which NVT is connected)
        public const Byte CMD_AO   = 0xF5; // 245 Abort output (Allows current process to run to completion, but stops sending output back)
        public const Byte CMD_AYT  = 0xF6; // 246 Are You There (Send back to the NVT some visible evidence that AYT was received)
        public const Byte CMD_EC   = 0xF7; // 247 Erase Character (The receiver should delete the last preceding undeleted character from the data stream)
        public const Byte CMD_EL   = 0xF8; // 248 Erase Line (Delete characters from the data stream back to, but not including the previous CRLF)
        public const Byte CMD_GA   = 0xF9; // 249 Go ahead (Under certain circumstances used to tell the other end that it can transmit)
        public const Byte CMD_SB   = 0xFA; // 250 Subnegotiation (Subnegotiation of the indicated option follows)
        public const Byte CMD_WILL = 0xFB; // 251 Will (Indicates the desire to begin performing or comfirmation that you are now performing the indicated option)
        public const Byte CMD_WONT = 0xFC; // 252 Wont (Indicates the refusal to perform, or continue performing the indicated option
        public const Byte CMD_DO   = 0xFD; // 253 Do (Indicates the request that the other party perform, or confirmation that you are expecting the other party to perform the indicated option)
        public const Byte CMD_DONT = 0xFE; // 254 Dont (Indicates the demand that the other party stop performing, or confirmation that youa re no longer expecting the other party to perform the indicated option)
        public const Byte CMD_IAC  = 0xFF; // 255 Iterpret as command

        public static String CmdToString(Byte cmd)
        {
            switch (cmd)
            {
                case Telnet.CMD_SE:
                    return "SE";
                case Telnet.CMD_NOP:
                    return "NOP";
                case Telnet.CMD_DM:
                    return "DM";
                case Telnet.CMD_BRK:
                    return "BRK";
                case Telnet.CMD_IP:
                    return "IP";
                case Telnet.CMD_AO:
                    return "AO";
                case Telnet.CMD_AYT:
                    return "AYT";
                case Telnet.CMD_EC:
                    return "EC";
                case Telnet.CMD_EL:
                    return "EL";
                case Telnet.CMD_GA:
                    return "GA";
                case Telnet.CMD_SB:
                    return "SB";
                case Telnet.CMD_WILL:
                    return "WILL";
                case Telnet.CMD_WONT:
                    return "WONT";
                case Telnet.CMD_DO:
                    return "DO";
                case Telnet.CMD_DONT:
                    return "DONT";
                case Telnet.CMD_IAC:
                    return "IAC";
            }
            return String.Format("0x{0:X2} ({0})", cmd);
        }

        public const Byte OPT_BINARY                             = 0;
        public const Byte OPT_ECHO                               = 1;
        public const Byte OPT_RECONNECTION                       = 2;
        public const Byte OPT_SUPPRESS_GO_AHEAD                  = 3;
        public const Byte OPT_APPROX_MESSAGE_SIZE_NEGOTIATION    = 4;
        public const Byte OPT_STATUS                             = 5;
        public const Byte OPT_TIMING_MARK                        = 6;
        public const Byte OPT_REMOTE_CONTROLLED_TRANS_AND_ECHO   = 7;
        public const Byte OPT_OUTPUT_LINE_WIDTH                  = 8;
        public const Byte OPT_OUTPUT_PAGE_SIZE                   = 9;
        public const Byte OPT_OUTPUT_CARRIAGE_RETURN_DISPOSITION = 10;
        public const Byte OPT_OUTPUT_HORIZONTAL_TAB_STOPS        = 11;
        public const Byte OPT_OUTPUT_HORIZONTAL_TAB_DISPOSITION  = 12;
        public const Byte OPT_OUTPUT_FORMFEED_DISPOSITION        = 13;
        public const Byte OPT_OUTPUT_VERTICAL_TAB_STOPS          = 14;
        public const Byte OPT_OUTPUT_VERTICAL_TAB_DISPOSITION    = 15;
        public const Byte OPT_OUTPUT_LINEFEED_DISPOSITION        = 16;
        public const Byte OPT_EXTENDED_ASCII                     = 17;
        public const Byte OPT_LOGOUT                             = 18;
        public const Byte OPT_BYTE_MACRO                         = 19;
        public const Byte OPT_DATA_ENTRY_TERMINAL                = 20;
        public const Byte OPT_SUPDUP                             = 21;
        public const Byte OPT_SUPDUP_OUTPUT                      = 22;
        public const Byte OPT_SEND_LOCATION                      = 23;
        public const Byte OPT_TERMINAL_TYPE                      = 24;
        public const Byte OPT_END_OF_RECORD                      = 25;
        public const Byte OPT_TACACS_USER_IDENTIFICATION         = 26;
        public const Byte OPT_OUTPUT_MARKING                     = 27;
        public const Byte OPT_TERMINAL_LOCATION_NUMBER           = 28;
        public const Byte OPT_TELNET_3270_REGIME                 = 29;
        public const Byte OPT_X3_PAD                             = 30;
        public const Byte OPT_NEGOTIATE_ABOUT_WINDOW_SIZE        = 31;
        public const Byte OPT_TERMINAL_SPEED                     = 32;
        public const Byte OPT_REMOTE_FLOW_CONTROL                = 33;
        public const Byte OPT_LINEMODE                           = 34;
        public const Byte OPT_X_DISPLAY_LOCATION                 = 35;
        public const Byte OPT_ENVIRONMENT_OPTIONS                = 36;
        public const Byte OPT_AUTHENTICATION_OPTION              = 37;
        public const Byte OPT_ENCRYPTION_OPTION                  = 38;
        public const Byte OPT_NEW_ENVIRONMENT_OPTION             = 39;
        //public const Byte OPT_                 = ;

        public static String OptToString(Byte opt)
        {
            switch (opt)
            {
                case Telnet.OPT_BINARY:
                    return "BINARY";
                case Telnet.OPT_ECHO:
                    return "Echo";
                case Telnet.OPT_RECONNECTION:
                    return "Reconnection";
                case Telnet.OPT_SUPPRESS_GO_AHEAD:
                    return "SuppressGoAhead";
                case Telnet.OPT_APPROX_MESSAGE_SIZE_NEGOTIATION:
                    return "ApproxMesssageSizeNegotiation";
                case Telnet.OPT_STATUS:
                    return "Status";
                case Telnet.OPT_TIMING_MARK:
                    return "TimingMark";
                case Telnet.OPT_REMOTE_CONTROLLED_TRANS_AND_ECHO:
                    return "RemoteControlledTransAndEcho";
                case Telnet.OPT_OUTPUT_LINE_WIDTH:
                    return "OutputLineWidth";
                case Telnet.OPT_OUTPUT_PAGE_SIZE:
                    return "OutputPageSize";
                case Telnet.OPT_OUTPUT_CARRIAGE_RETURN_DISPOSITION:
                    return "OutputCarriageReturnDisposition";
                case Telnet.OPT_OUTPUT_HORIZONTAL_TAB_STOPS:
                    return "OutputHorizontalTabStops";
                case Telnet.OPT_OUTPUT_HORIZONTAL_TAB_DISPOSITION:
                    return "OutputHorizontalTabDisposition";
                case Telnet.OPT_OUTPUT_VERTICAL_TAB_STOPS:
                    return "OuptutVerticalTabStops";
                case Telnet.OPT_OUTPUT_VERTICAL_TAB_DISPOSITION:
                    return "OutputVerticalTabDisposition";
                case Telnet.OPT_OUTPUT_LINEFEED_DISPOSITION:
                    return "OuptutLinefeedDisposition";
                case Telnet.OPT_EXTENDED_ASCII:
                    return "ExtendedASCII";
                case Telnet.OPT_LOGOUT:
                    return "Logout";
                case Telnet.OPT_BYTE_MACRO:
                    return "ByteMacro";
                case Telnet.OPT_DATA_ENTRY_TERMINAL:
                    return "DataEntryTerminal";
                case Telnet.OPT_SUPDUP:
                    return "SUPDUP";
                case Telnet.OPT_SUPDUP_OUTPUT:
                    return "SUPDUPOutput";
                case Telnet.OPT_SEND_LOCATION:
                    return "SendLocation";
                case Telnet.OPT_TERMINAL_TYPE:
                    return "TerminalType";
                case Telnet.OPT_END_OF_RECORD:
                    return "EndOfRecord";
                case Telnet.OPT_TACACS_USER_IDENTIFICATION:
                    return "TACASUserIdentification";
                case Telnet.OPT_OUTPUT_MARKING:
                    return "OutputMarking";
                case Telnet.OPT_TERMINAL_LOCATION_NUMBER:
                    return "TerminalLocationNumber";
                case Telnet.OPT_TELNET_3270_REGIME:
                    return "Telnet3270Regime";
                case Telnet.OPT_X3_PAD:
                    return "X3Pad";
                case Telnet.OPT_NEGOTIATE_ABOUT_WINDOW_SIZE:
                    return "NegotiateAboutWindowSize";
                case Telnet.OPT_TERMINAL_SPEED:
                    return "TerminalSpeed";
                case Telnet.OPT_REMOTE_FLOW_CONTROL:
                    return "RemoteFlowControl";
                case Telnet.OPT_LINEMODE:
                    return "Linemode";
                case Telnet.OPT_X_DISPLAY_LOCATION:
                    return "XDisplayLocation";
                case Telnet.OPT_ENVIRONMENT_OPTIONS:
                    return "EnvironmentOptions";
                case Telnet.OPT_AUTHENTICATION_OPTION:
                    return "AuthenticationOption";
                case Telnet.OPT_ENCRYPTION_OPTION:
                    return "EncryptionOption";
                case Telnet.OPT_NEW_ENVIRONMENT_OPTION:
                    return "NewEnvironmentOption";
            }
            return String.Format("0x{0:X2} ({0})", opt);
        }
    }
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
        private static DefaultTelnetHandler instance = null;
        public static DefaultTelnetHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DefaultTelnetHandler();
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
        public byte ReceivedWont(Byte option) { return 0; }
        public byte ReceivedDo(Byte option) { return Telnet.CMD_WONT; }
        public byte ReceivedDont(Byte option) { return 0; }
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
