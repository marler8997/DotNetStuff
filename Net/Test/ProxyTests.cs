using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Sockets;

namespace More.Net
{
    [TestClass]
    public class ProxyTests
    {
        /*
        [TestMethod]
        public void TestHttpProxyAtHP()
        {
            HttpProxy proxy = new HttpProxy(new DnsEndPoint("proxy.houston.hp.com", 8080, -1));

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            proxy.Connect(socket, new DnsEndPoint("www.google.com", 80, false));

            socket.Send(Encoding.UTF8.GetBytes("GET / HTTP/1.1\r\nHost: www.google.com\r\n\r\n"));

            NetworkStream stream = new NetworkStream(socket);
            Byte[] buffer = new Byte[1024];

            while (true)
            {
                Int32 bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead <= 0) return;
                Console.Write(Encoding.UTF8.GetString(buffer, 0, bytesRead));
            }

        }

        [TestMethod]
        public void TestHttpsProxyAtHP()
        {
            HttpsProxy proxy = new HttpsProxy(new DnsEndPoint("proxy.houston.hp.com", 8080, -1));

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            proxy.Connect(socket, new DnsEndPoint("www.google.com", 80, false));

            socket.Send(Encoding.UTF8.GetBytes("GET / HTTP/1.1\r\nHost: www.google.com\r\n\r\n"));

            NetworkStream stream = new NetworkStream(socket);
            Byte[] buffer = new Byte[1024];

            while (true)
            {
                Int32 bytesRead = stream.Read(buffer, 0, buffer.Length);
                if(bytesRead <= 0) return;
                Console.Write(Encoding.UTF8.GetString(buffer, 0, bytesRead));
            }

        }
         */
    }
}
