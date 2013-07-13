using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using More.Net.TlsCommand;

namespace More.Net
{
    /// <summary>
    /// Summary description for TestTls
    /// </summary>
    [TestClass]
    public class TestTls
    {
        [TestMethod]
        public void TestTlsHandshake()
        {
            TestTlsHandshake(new DnsEndPoint("localhost", 22));
        }
        public void TestTlsHandshake(EndPoint endPoint)
        {
            Int32 bytesRead;
            Byte[] receiveBuffer = new Byte[1024];

            System.Random random = new System.Random();

            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endPoint);

            ByteBuffer sendBuffer = new ByteBuffer(512, 256);

            Byte[] randomBytes = new Byte[28];
            random.NextBytes(randomBytes);

            TlsRecord record = new TlsRecord(
                TlsRecord.ContentTypeEnum.Handshake,
                Tls.Tls10MajorVersion,
                Tls.Tls10MinorVersion,
                new TlsHandshakeRecord(
                    TlsHandshakeRecord.TypeEnum.ClientHello,
                    new ClientHello(
                        Tls.Tls10MajorVersion,
                        Tls.Tls10MinorVersion,
                        DateTime.Now.ToUniversalTime().ToUnixTime(),
                        Tls.CreateRandom(random),
                        null,
                        new CipherSuite[] {
                            CipherSuite.TlsRsaWithAes256CbcSha,
                            CipherSuite.TlsRsaWithAes128CbcSha,
                        },
                        new CompressionMethod[] {
                            CompressionMethod.Null,
                        },
                        null//new Extension[] {}
                        ).CreateSerializerAdapater()).CreateSerializerAdapater());

            UInt32 packetLength = TlsRecord.Serializer.SerializationLength(record);
            sendBuffer.EnsureCapacity(packetLength);
            TlsRecord.Serializer.Serialize(sendBuffer.array, 0, record);

            socket.Send(sendBuffer.array, 0, (Int32)packetLength, SocketFlags.None);

            bytesRead = socket.Receive(receiveBuffer);
            Assert.IsTrue(bytesRead > 0);

            Console.WriteLine("Received '{0}'", Encoding.ASCII.GetString(receiveBuffer, 0, bytesRead));
            Console.WriteLine("Bytes: {0}", BitConverter.ToString(receiveBuffer, 0, bytesRead));
        }
    }
}
