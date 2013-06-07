using System;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Diagnostics;

namespace More.Net
{
    /*
    public static class SocketExtensions
    {
        public static Boolean ConnectWithTimeout(Socket socket, EndPoint endPoint, TimeSpan timeout)
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

        public static Boolean ConnectWithTimeout(Socket socket, String host, UInt16 port, TimeSpan timeout)
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
    */

    public static class GenericUtilities
    {

        /*
        public static void ForEach<T>(IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source) action(element);
        }

        */ 
        /*
#if !WindowsCE
        public static String Proxy4Name(byte[] userID, IDirectSocketConnector proxyConnector, IPAddress hostIP, UInt16 hostPort)
        {
            return String.Format("%proxy4%{0}{1}>{2}:{3}", (userID == null) ? String.Empty : (Encoding.UTF8.GetString(userID) + "@"),
                proxyConnector.ConnectionSpecifier, hostIP, hostPort);
        }

        public static String Proxy4aName(byte[] userID, IDirectSocketConnector proxyConnector, String host, UInt16 hostPort)
        {
            return String.Format("%proxy4a%{0}{1}>{2}:{3}", (userID == null) ? String.Empty : (Encoding.UTF8.GetString(userID) + "@"),
                proxyConnector.ConnectionSpecifier, host, hostPort);
        }
#endif
        */ 

        /*
        public static void Insert(byte[] arr, ref Int32 offset, byte[] insert)
        {
            if (offset + insert.Length > arr.Length) throw new ArgumentOutOfRangeException("offset");

            for (int i = 0; i < insert.Length; i++)
            {
                arr[offset++] = insert[i];
            }
        }


      
        */
 
        /*
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
        */

        /*
        public static void WriteFile(this FileInfo fileInfo, Int32 fileOffset, Byte[] buffer, Int32 bufferOffset, Int32 length)
        {
            fileInfo.Refresh();

            using (FileStream fileStream = fileInfo.Open(FileMode.Open))
            {
                fileStream.Position = fileOffset;
                fileStream.Write(buffer, bufferOffset, length);
            }
        }
        */
    }
}
