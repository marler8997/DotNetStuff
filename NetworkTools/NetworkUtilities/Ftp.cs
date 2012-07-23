using System;

namespace Marler.NetworkTools
{
    public class Ftp
    {
        public const UInt16 ResponseErrorSyntax         = 500;
        public const UInt16 ResponseErrorParameter      = 501;
        public const UInt16 ResponseErrorNotImplemented = 502;

        public enum TransferType
        {
            Ascii,Ebcdic,Image,Local,
        }
        public enum TransferType2
        {
            NonPrint,Telnet,Asa,
        }



    }
}
