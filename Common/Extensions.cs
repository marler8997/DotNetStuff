using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace Marler.Common
{
#if WindowsCE
    public static class IPParser
    {
        public static Boolean TryParse(String ipString, out IPAddress address)
        {
            try
            {
                address = IPAddress.Parse(ipString);
                return true;
            }
            catch (FormatException)
            {
                address = null;
                return false;
            }
        }
    }
    public static class EnumReflectionWrapper
    {
        public static String[] GetNames(Type enumType)
        {
            List<String> names = new List<String>();
            foreach (FieldInfo fieldInfo in enumType.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                names.Add(fieldInfo.Name);
            }
            return names.ToArray();
        }
    }
#endif
    public static class StringExtensions
    {
        public static String Peel(this String str, out String rest)
        {
            return Peel(str, 0, out rest);
        }

        // Peel the first string until whitespace
        public static String Peel(this String str, Int32 offset, out String rest)
        {
            if (str == null)
            {
                rest = null;
                return null;
            }

            Char c;

            //
            // Skip beginning whitespace
            //
            while (true)
            {
                if (offset >= str.Length)
                {
                    rest = null;
                    return null;
                }
                c = str[offset];
                if (!Char.IsWhiteSpace(c)) break;
                offset++;
            }

            Int32 startOffset = offset;

            //
            // Find next whitespace
            //
            while (true)
            {
                offset++;
                if (offset >= str.Length)
                {
                    rest = null;
                    return str.Substring(startOffset);
                }
                c = str[offset];
                if (Char.IsWhiteSpace(c)) break;
            }

            Int32 peelLimit = offset;

            //
            // Remove whitespace till rest
            //
            while (true)
            {
                offset++;
                if (offset >= str.Length)
                {
                    rest = null;
                }
                if (!Char.IsWhiteSpace(str[offset]))
                {
                    rest = str.Substring(offset);
                    break;
                }
            }
            return str.Substring(startOffset, peelLimit - startOffset);
        }
        public static Boolean SubstringEquals(String str, Int32 offset, String compare)
        {
            if (offset + compare.Length > str.Length)
            {
                return false;
            }

            for (int i = 0; i < compare.Length; i++)
            {
                if (str[offset] != compare[i])
                {
                    if (Char.IsUpper(str[offset]))
                    {
                        if (Char.IsUpper(compare[i])) return false;
                        if (Char.ToUpper(compare[i]) != str[offset]) return false;
                    }
                    else
                    {
                        if (Char.IsLower(compare[i])) return false;
                        if (Char.ToLower(compare[i]) != str[offset]) return false;
                    }
                }
                offset++;
            }
            return true;
        }
#if WindowsCE
        public static String ToLowerInvariant(this String str)
        {
            return str.ToLower(System.Globalization.CultureInfo.InvariantCulture);
        }
#endif
    }
    public static class ListExtensions
    {
        public static String ToDataString<T>(this List<T> list)
        {
            StringBuilder builder = new StringBuilder();
            ToDataString<T>(list, builder);
            return builder.ToString();
        }
        public static void ToDataString<T>(this List<T> list, StringBuilder builder)
        {
            builder.Append('[');
            Boolean atFirst = true;
            for (int i = 0; i < list.Count; i++)
            {
                if (atFirst) { atFirst = false; } else { builder.Append(','); }
                builder.Append(list[i]);
            }
            builder.Append("]");
        }
    }
    public static class DictionaryExtensions
    {
        public static String ToDataString<T,U>(this Dictionary<T,U> dictionary)
        {
            StringBuilder builder = new StringBuilder();
            ToDataString<T,U>(dictionary, builder);
            return builder.ToString();
        }
        public static void ToDataString<T,U>(this Dictionary<T,U> dictionary, StringBuilder builder)
        {
            builder.Append('{');
            Boolean atFirst = true;
            foreach (KeyValuePair<T,U> pair in dictionary)
            {
                if (atFirst) { atFirst = false; } else { builder.Append(','); }
                builder.Append(pair.Key);
                builder.Append(':');
                builder.Append(pair.Value);
            }
            builder.Append('}');
        }
    }

    public static class StackExtensions
    {
        public static void Print<T>(this Stack<T> stack)
        {
            if (stack.Count <= 0)
            {
                Console.WriteLine("Empty");
            }
            else
            {
                foreach (T item in stack)
                {
                    Console.WriteLine(item);
                }
            }
        } 
    }


    public static class DateTimeExtensions
    {
        public static readonly DateTime UnixZeroTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static double ToUnixTime(this DateTime dateTime)
        {
            return (double)(dateTime - UnixZeroTime).TotalSeconds;
        }
    }

    public static class StopwatchExtensions
    {
        private static Double StopwatchTicksPerMillisecondAsDouble = 1000.0 / Stopwatch.Frequency;
        //private static Double StopwatchTicksPerMicrosecondAsDouble = 1000000.0 / Stopwatch.Frequency;

        private static String stopwatchTickString = null;
        public static String StopwatchTickString
        {
            get
            {
                if (stopwatchTickString == null)
                {
                    Int64 frequency = Stopwatch.Frequency;
                    switch (frequency)
                    {
                        case 1000: stopwatchTickString = "milliseconds"; break;
                        case 1000000: stopwatchTickString = "microsecond"; break;
                        case 1000000000: stopwatchTickString = "nanoseconds"; break;
                        default: throw new InvalidOperationException(String.Format("Unknown stopwatch frequency: '{0}' (Expected 1000, 1000000 or 1000000000)", frequency));
                    }
                }
                return stopwatchTickString;
            }
        }

        public static Int64 MillisToStopwatchTicks(this Int32 millis)
        {
            return Stopwatch.Frequency * (Int64)millis / 1000L;
        }

        public static Int64 StopwatchTicksAsMicroseconds(this Int64 stopwatchTicks)
        {
            return stopwatchTicks * 1000000L / Stopwatch.Frequency;
        }
        public static Int32 StopwatchTicksAsInt32Milliseconds(this Int64 stopwatchTicks)
        {
            return (Int32)(stopwatchTicks * 1000 / Stopwatch.Frequency);
        }
        public static Int64 StopwatchTicksAsInt64Milliseconds(this Int64 stopwatchTicks)
        {
            return (Int64)(stopwatchTicks * 1000 / Stopwatch.Frequency);
        }
        public static Double StopwatchTicksAsDoubleMilliseconds(this Int64 stopwatchTicks)
        {
            return StopwatchTicksPerMillisecondAsDouble * stopwatchTicks;
        }
    }

    public static class SocketExtensions
    {
        public static String SafeRemoteEndPointString(this Socket socket)
        {
            try { return socket.RemoteEndPoint.ToString(); } catch (Exception) { return "<disconnected>";  }
        }
        public static void ReadFullSize(this Socket socket, byte[] buffer, int offset, int size)
        {
            int lastBytesRead;

            do
            {
                lastBytesRead = socket.Receive(buffer, offset, size, SocketFlags.None);
                size -= lastBytesRead;

                if (size <= 0) return;

                offset += lastBytesRead;
            } while (lastBytesRead > 0);

            throw new IOException(String.Format("reached end of stream: still needed {0} bytes", size));
        }
        public static void SendFile(Socket socket, String filename, Byte[] transferBuffer)
        {
            if (filename == null)
            {
                Console.WriteLine("Please supply a filename");
                return;
            }

            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(filename, FileMode.Open);
                Int32 bytesRead;
                while ((bytesRead = fileStream.Read(transferBuffer, 0, transferBuffer.Length)) > 0)
                {
                    socket.Send(transferBuffer, 0, bytesRead, SocketFlags.None);
                }
            }
            finally
            {
                if (fileStream != null) fileStream.Close();
            }
        }
    }

    public static class StreamExtensions
    {
        public static void ReadFullSize(this Stream stream, byte[] buffer, int offset, int size)
        {
            int lastBytesRead;

            do
            {
                lastBytesRead = stream.Read(buffer, offset, size);
                size -= lastBytesRead;

                if (size <= 0) return;

                offset += lastBytesRead;
            } while (lastBytesRead > 0);

            throw new IOException(String.Format("Reached end of stream but still expected {0} bytes", size));
        }

        /*
        public static String ReadLine(this Stream stream, StringBuilder builder)
        {
            builder.Length = 0;
            while (true)
            {
                int next = stream.ReadByte();
                if (next < 0)
                {
                    if (builder.Length == 0) return null;
                    return builder.ToString();
                }

                if (next == '\n') return builder.ToString();
                if (next == '\r')
                {
                    do
                    {
                        next = stream.ReadByte();
                        if (next < 0)
                        {
                            if (builder.Length == 0) return null;
                            return builder.ToString();
                        }
                        if (next == '\n') return builder.ToString();
                        builder.Append('\r');
                    } while (next == '\r');
                }

                builder.Append((char)next);
            }
        }
        */
    }

    public static class FileExtensions
    {
        public static Byte[] ReadFile(String filename)
        {
            //
            // 1. Get file size
            //
            FileInfo fileInfo = new FileInfo(filename);
            Int32 fileLength = (Int32)fileInfo.Length;
            Byte[] buffer = new Byte[fileLength];

            //
            // 2. Read the file contents
            //
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(filename, FileMode.Open);
                fileStream.ReadFullSize(buffer, 0, fileLength);
            }
            finally
            {
                if (fileStream != null) fileStream.Dispose();
            }

            return buffer;
        }

        public static Int32 ReadFile(FileInfo fileInfo, Int32 fileOffset, Byte[] buffer, FileShare shareOptions, out Boolean reachedEndOfFile)
        {
            fileInfo.Refresh();

            Int64 fileSize = fileInfo.Length;

            if (fileOffset >= fileSize)
            {
                reachedEndOfFile = true;
                return 0;
            }

            Int64 fileSizeFromOffset = fileSize - fileOffset;

            Int32 readLength;
            if (fileSizeFromOffset > (Int64)buffer.Length)
            {
                reachedEndOfFile = false;
                readLength = buffer.Length;
            }
            else
            {
                reachedEndOfFile = true;
                readLength = (Int32)fileSizeFromOffset;
            }
            if (readLength <= 0) return 0;

            using (FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, shareOptions))
            {
                fileStream.Position = fileOffset;
                fileStream.ReadFullSize(buffer, 0, readLength);
            }

            return readLength;
        }
    }
}
