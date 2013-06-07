using System;
using System.Collections.Generic;
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
}
