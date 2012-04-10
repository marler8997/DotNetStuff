using System;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Diagnostics;

namespace Marler.NetworkTools
{
    public static class SocketExtensions
    {
        public static Boolean ConnectWithTimeout(this Socket socket, EndPoint endPoint, TimeSpan timeout)
        {
            IAsyncResult asyncResult = socket.BeginConnect(endPoint, null, null);
            if (asyncResult.AsyncWaitHandle.WaitOne(timeout))
            {
                return socket.Connected;
            }

            try
            {
                socket.EndConnect(asyncResult);
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }
            return false;
        }

        public static Boolean ConnectWithTimeout(this Socket socket, String host, UInt16 port, TimeSpan timeout)
        {
            IAsyncResult asyncResult = socket.BeginConnect(host, port, null, null);
            if (asyncResult.AsyncWaitHandle.WaitOne(timeout))
            {
                return socket.Connected;
            }
            try
            {
                //socket.EndConnect(asyncResult);
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }
            return false;
        }
    }


    public static class GenericUtilities
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source) action(element);
        }

        public static String GetString<T>(this List<T> list)
        {
            StringBuilder stringBuilder = new StringBuilder("{ ");

            IEnumerator<T> enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
            {
                T item = enumerator.Current;

                stringBuilder.Append(item);
                stringBuilder.Append(' ');
            }

            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }

        public static String Proxy4Name(byte[] userID, IDirectSocketConnector proxyConnector, IPAddress hostIP, UInt16 hostPort)
        {
            return String.Format("%proxy4%{0}{1}>{2}:{3}", (userID == null) ? String.Empty : (Encoding.UTF8.GetString(userID) + "@"),
                proxyConnector.HostAndPort, hostIP, hostPort);
        }

        public static String Proxy4aName(byte[] userID, IDirectSocketConnector proxyConnector, String host, UInt16 hostPort)
        {
            return String.Format("%proxy4a%{0}{1}>{2}:{3}", (userID == null) ? String.Empty : (Encoding.UTF8.GetString(userID) + "@"),
                proxyConnector.HostAndPort, host, hostPort);
        }


        public static void Insert(this byte[] arr, ref Int32 offset, byte[] insert)
        {
            if (offset + insert.Length > arr.Length) throw new ArgumentOutOfRangeException("offset");

            for (int i = 0; i < insert.Length; i++)
            {
                arr[offset++] = insert[i];
            }
        }


        public static String GetString(this Dictionary<String, String> dictionary)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append('{');
            foreach (KeyValuePair<String, String> pair in dictionary)
            {
                stringBuilder.Append(" [");
                stringBuilder.Append(pair.Key);
                stringBuilder.Append(',');
                stringBuilder.Append(pair.Value);
                stringBuilder.Append(']');
            }
            stringBuilder.Append(" }");
            return stringBuilder.ToString();
        }

        public static void PrintStack<T>(this Stack<T> stack)
        {
            if (stack.Count <= 0)
            {
                Console.WriteLine("Empty");
            }
            else
            {
                foreach(T item in stack)
                {
                    Console.WriteLine(item);            
                }
            }
        }
        

        public static void SendFile(this Socket socket, String filename, Int32 bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            if (filename == null)
            {
                Console.WriteLine("Please supply a filename");
                return;
            }

            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(filename, FileMode.Open);

                Console.WriteLine("[Sending File '{0}', buffer is {1} bytes]", filename, bufferSize);

                try
                {
                    Int32 bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        Console.WriteLine("[Sending {0} bytes]", bytesRead);
                        socket.Send(buffer, SocketFlags.None);
                    }
                    Console.WriteLine("[Success]");
                }
                catch (Exception e)
                {
                    Console.WriteLine("[FAILED: {0}]", e.Message);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("[FAILED: Could not open file '{0}': {1}]", filename, e.Message);
            }
            finally
            {
                if (fileStream != null) fileStream.Close();
            }
        }

        public static void ReadFullSize(this Socket socket, byte[] buffer, int offset, int size)
        {
            int lastBytesRead;

            do
            {
                lastBytesRead = socket.Receive(buffer, offset, size, SocketFlags.None);
                size -= lastBytesRead;
                if (size <= 0)
                {
                    return;
                }
                offset += lastBytesRead;
            } while (lastBytesRead > 0);

            throw new IOException(String.Format("reached end of stream: still needed {0} bytes", size));
        }

        public static void ReadFullSize(this Stream stream, byte[] buffer, int offset, int size)
        {
            int lastBytesRead;

            do
            {
                lastBytesRead = stream.Read(buffer, offset, size);
                size -= lastBytesRead;
                if (size <= 0)
                {
                    return;
                }
                offset += lastBytesRead;
            } while (lastBytesRead > 0);

            throw new IOException("reached end of stream");
        }

        public static byte[] ReadFile(String filename)
        {
            //
            // 1. Get file size
            //
            FileInfo fileInfo = new FileInfo(filename);
            Int32 fileLength = (Int32)fileInfo.Length;
            byte[] buffer = new byte[fileLength];

            //
            // 2. Read the file contents
            //
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(filename, FileMode.Open);
                ReadFile(fileStream, buffer, 0, fileLength);
            }
            finally
            {
                if (fileStream != null) fileStream.Dispose();
            }

            return buffer;
        }

        public static void ReadFile(this FileStream fileStream, byte[] buffer, Int32 offset, Int32 size)
        {
            int lastBytesRead;

            do
            {
                lastBytesRead = fileStream.Read(buffer, offset, size);
                size -= lastBytesRead;
                if (size <= 0)
                {
                    return;
                }
                offset += lastBytesRead;
            } while (lastBytesRead > 0);

            throw new IOException("reached end of file");
        }

        public static String GetString(this EndPoint endPoint)
        {
            //DnsEndPoint dnsEndPoint = endPoint as DnsEndPoint;
            //if(dnsEndPoint != null)
            //{
            //	return String.Format("{0}:{1}",dnsEndPoint.Host,dnsEndPoint.Port);
            //}
            IPEndPoint ipEndPoint = endPoint as IPEndPoint;
            if (ipEndPoint != null)
            {
                return String.Format("{0}", ipEndPoint.Address.ToString());
            }
            return "?.?.?.?";
        }

        public static Stream Connect(String host, Int32 port)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(host, port);
            return new NetworkStream(socket);
        }


        public static IPAddress ResolveHost(String host)
        {
            IPAddress ipAddress;
            if (IPAddress.TryParse(host, out ipAddress))
            {
                return ipAddress;
            }

            Console.WriteLine("Host \"{0}\" could not be parsed as an IP Address...attempting to resolve it as a host name...", host);

            IPHostEntry hostEntry = Dns.GetHostEntry(host);
            if (hostEntry.AddressList == null || hostEntry.AddressList.Length <= 0)
            {
                throw new InvalidOperationException(String.Format("Could not resolve host \"{0}\"", host));
            }
            ipAddress = hostEntry.AddressList[0];

            // Create Address List String
            String addressListString = ipAddress.ToString();
            for (int i = 1; i < hostEntry.AddressList.Length; i++)
            {
                addressListString += "," + hostEntry.AddressList[i].ToString();
            }

            Console.WriteLine("Host \"{0}\" has {1} addresses ({2})...using {3}", host,
                hostEntry.AddressList.Length, addressListString, ipAddress);

            return ipAddress;
        }

    }
}
