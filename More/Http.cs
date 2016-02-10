using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace More.Net
{
    public static class Http
    {
        public const String GetMethod     = "GET";
        public const String PostMethod    = "POST";
        public const String OptionsMethod = "OPTIONS";
        public const String HeadMethod    = "HEAD";
        public const String ConnectMethod = "CONNECT";
        public const String TraceMethod   = "TRACE";
        public const String DeleteMethod  = "DELETE";
        public const String PutMethod     = "PUT";

        public const UInt16 ResponseOK                  = 200;
        public const UInt16 ResponseMovedPermanently    = 301;
        public const UInt16 ResponseRedirection         = 302;
        public const UInt16 ResponseBadRequest          = 400;
        public const UInt16 ResponseNotFound            = 404;
        public const UInt16 ResponseInternalServerError = 500;

        public const String ResponseNotFoundMessage = "Not Found";

        public static readonly Byte[] HostHeaderPrefix          = Encoding.ASCII.GetBytes("Host: ");
        public static readonly Byte[] ContentLengthHeaderPrefix = Encoding.ASCII.GetBytes("Content-Length: ");
        public static readonly Byte[] ContentTypeHeaderPrefix   = Encoding.ASCII.GetBytes("Content-Type: ");
        public static readonly Byte[] ConnectionHeaderPrefix    = Encoding.ASCII.GetBytes("Connection: ");
        public static readonly Byte[] ConnectionKeepAlive       = Encoding.ASCII.GetBytes("Keep-Alive");
        public static readonly Byte[] ConnectionClose           = Encoding.ASCII.GetBytes("close");

        public static readonly Byte[] Newline       = Encoding.ASCII.GetBytes("\r\n");
        public static readonly Byte[] DoubleNewline = Encoding.ASCII.GetBytes("\r\n\r\n");

        public static readonly Byte[] VersionBytes = new Byte[]
        {
            (Byte)'H', (Byte)'T', (Byte)'T', (Byte)'P', (Byte)'/', (Byte)'1', (Byte)'.', (Byte)'1'
        };

        static Dictionary<UInt16, String> responseMessageMap = null;
        public static Dictionary<UInt16, String> ResponseMessageMap
        {
            get
            {
                if (responseMessageMap == null)
                {
                    responseMessageMap = new Dictionary<UInt16, String>() {
                        {200, "Ok"},
                        {201, "Created"},
                        {202, "Accepted"},
                        {204, "No Content"},

                        {301, "Moved Permanently"},
                        {302, "Redirection"},
                        {304, "Not Modified"},

                        {400, "Bad Request"},
                        {401, "Unauthorized"},
                        {403, "Forbidden"},
                        {ResponseNotFound, ResponseNotFoundMessage},

                        {500, "Internal Server Error"},
                        {501, "Not Implemented"},
                        {502, "Bad Gateway"},
                        {503, "Service Unavailable"},
                    };

                }
                return responseMessageMap;
            }
        }
        public static String GetParentUrl(this String url)
        {
            if (url == null || url.Length <= 1) return null;

            Int32 index = url.Length - 1;

            if (url[index] == '/') index--;

            while (true)
            {
                if (index < 0) return null;
                if (url[index] == '/') return url.Substring(0, index + 1);
                index--;
            }
        }
        public static String GetUrlFilename(this String url)
        {
            if (url == null || url.Length <= 0) return null;

            Int32 lengthWithoutSlash = url.Length;
            Int32 index = url.Length - 1;

            if (url[index] == '/')
            {
                lengthWithoutSlash--;
                index--;
            }

            while (true)
            {
                if (index < 0) return (url.Length == lengthWithoutSlash) ? url : url.Substring(0, lengthWithoutSlash);
                if (url[index] == '/') return url.Substring(index + 1, lengthWithoutSlash - index - 1);
                index--;
            }
        }
        struct ByteAndCharStringBuilder
        {
            readonly Char[] chars;
            int charCount;

            Byte[] bytes;
            int byteCount;

            public ByteAndCharStringBuilder(Int32 bufferSize)
            {
                this.chars = new Char[bufferSize];
                this.charCount = 0;
                this.bytes = null;
                this.byteCount = 0;
            }
            void FlushBytes()
            {
                this.charCount += Encoding.ASCII.GetChars(this.bytes, 0, this.byteCount, this.chars, this.charCount);
                this.byteCount = 0;
            }

            public void AddChar(Char c)
            {
                if (this.byteCount > 0) FlushBytes();
                this.chars[this.charCount++] = c;
            }
            public void AddByte(Byte b)
            {
                if (bytes == null) bytes = new Byte[chars.Length];
                bytes[byteCount++] = b;
            }

            public string CreateString()
            {
                if (byteCount > 0) FlushBytes();
                return (charCount <= 0) ? "" : new String(this.chars, 0, this.charCount);
            }
        }
        public static String UrlDecode(String s)
        {
            Int32 length = s.Length;
            ByteAndCharStringBuilder builder = new ByteAndCharStringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                char ch = s[i];
                if (ch == '+')
                {
                    ch = ' ';
                }
                else if ((ch == '%') && (i < (length - 2)))
                {
                    if ((s[i + 1] == 'u') && (i < (length - 5)))
                    {
                        int num3 = s[i + 2].HexValue();
                        int num4 = s[i + 3].HexValue();
                        int num5 = s[i + 4].HexValue();
                        int num6 = s[i + 5].HexValue();
                        if (((num3 < 0) || (num4 < 0)) || ((num5 < 0) || (num6 < 0)))
                        {
                            goto Label_0106;
                        }
                        ch = (char)((((num3 << 12) | (num4 << 8)) | (num5 << 4)) | num6);
                        i += 5;
                        builder.AddChar(ch);
                        continue;
                    }
                    int num7 = s[i + 1].HexValue();
                    int num8 = s[i + 2].HexValue();
                    if ((num7 >= 0) && (num8 >= 0))
                    {
                        byte b = (byte)((num7 << 4) | num8);
                        i += 2;
                        builder.AddByte(b);
                        continue;
                    }
                }
            Label_0106:
                if ((ch & 0xff80) == 0)
                {
                    builder.AddByte((byte)ch);
                }
                else
                {
                    builder.AddChar(ch);
                }
            }
            return builder.CreateString();
        }
    }
}
