using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Marler.NetworkTools
{
    [TestClass]
    public class RegressionTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            Cdp.TryStaticInit(Cdp.MaxPayloadWithIDOverUdp);

            CdpTimeoutForTests timeout = new CdpTimeoutForTests();
            CdpServerForTests cdpServer = new CdpServerForTests();
            IPEndPoint serverListenEndPoint = new IPEndPoint(IPAddress.Loopback, 1234);

            new Thread(() =>
            {
                try
                {
                    Cdp.UdpServerLoop(serverListenEndPoint, new Byte[256], cdpServer, timeout);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }).Start();


            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.Connect(serverListenEndPoint);
            CdpTransmitter transmitter = new CdpTransmitter(new UdpClientTransmitter(udpSocket));



            transmitter.SendHearbeat();


            Int32 offset;
            Byte[] myMessage;
            Byte[] payloadBuffer;


            myMessage = Encoding.UTF8.GetBytes("This should be a random payload");

            payloadBuffer = transmitter.RequestSendBuffer(myMessage.Length, out offset);
            Array.Copy(myMessage, 0, payloadBuffer, offset, myMessage.Length);
            offset += myMessage.Length;
            transmitter.ControllerSendRandomPayload(offset);




            myMessage = Encoding.UTF8.GetBytes("This should be the first payload with no immediate ack");

            payloadBuffer = transmitter.RequestSendBuffer(myMessage.Length, out offset);
            Array.Copy(myMessage, 0, payloadBuffer, offset, myMessage.Length);
            offset += myMessage.Length;
            transmitter.ControllerSendPayloadNoAck(offset);

            myMessage = Encoding.UTF8.GetBytes("This should be the second payload with no immediate ack");

            payloadBuffer = transmitter.RequestSendBuffer(myMessage.Length, out offset);
            Array.Copy(myMessage, 0, payloadBuffer, offset, myMessage.Length);
            offset += myMessage.Length;
            transmitter.ControllerSendPayloadNoAck(offset);






            //
            // Send and wait for ack
            //
            myMessage = Encoding.UTF8.GetBytes("This should be a normal payload with an immediate ack");

            payloadBuffer = transmitter.RequestSendBuffer(myMessage.Length, out offset);
            Array.Copy(myMessage, 0, payloadBuffer, offset, myMessage.Length);
            offset += myMessage.Length;
            transmitter.ControllerSendPayloadWithAck(offset, timeout);




            transmitter.SendHaltNoPayload();






        }

        public class CdpTimeoutForTests : ICdpTimeout
        {
            public int WaitForAckInitialRetryTimeout(int averageLatency)
            {
                Int32 calculatedRetryTimeout = (averageLatency <= 0) ? 5000 : averageLatency * 3;
                return (calculatedRetryTimeout > 5000) ? 5000 :
                    ((calculatedRetryTimeout < 1000) ? 1000 : calculatedRetryTimeout);
            }

            public int WaitForAckRetryOrTimeout(int retries, int averageLatency, int elapsedMillis, int lastWaitForAckTimeout)
            {
                if (retries > 3) return 0;
                if (elapsedMillis > 10000) return 0;
                return WaitForAckInitialRetryTimeout(averageLatency);
            }
        }

        public class CdpServerForTests : ICdpServer
        {
            public Int32 connectionCount;

            public CdpServerForTests()
            {
                this.connectionCount = 0;
            }

            public Boolean HeartbeatFromUnknown(EndPoint endPoint)
            {
                Console.WriteLine("Heartbeat from unknown client '{0}'", endPoint);
                return false;
            }

            public Boolean NewConnection(CdpTransmitter transmitter, out ICdpServerHandler handler, out int maxSendBeforeAck)
            {
                connectionCount++;

                Console.WriteLine("New Connection from '{0}' ({1} connections)", transmitter.RemoteEndPoint, connectionCount);

                handler = new CdpServerHandlerForTests(transmitter);
                maxSendBeforeAck = 0xFF;
                return false;
            }

            public void ConnectionClosed(EndPoint endPoint)
            {
                connectionCount--;
                Console.WriteLine("Close Connection ({0} connections)", connectionCount);
            }

            public Boolean SocketException(SocketException e)
            {
                throw e;
            }
        }

        public class CdpServerHandlerForTests : ICdpServerHandler
        {
            public readonly CdpTransmitter transmitter;
            public CdpServerHandlerForTests(CdpTransmitter transmitter)
            {
                this.transmitter = transmitter;
            }

            public ServerInstruction GotControl(CdpTransmitter transmitter, out int sendBufferOffsetLimit, out bool requestImmediateAck)
            {
                throw new NotImplementedException();
            }

            public Boolean RandomPayload(byte[] readBytes, int offset, int length)
            {
                Console.WriteLine("Random Payload From '{0}' ({1} Bytes): '{2}'", transmitter.RemoteEndPoint, length, Encoding.UTF8.GetString(readBytes, offset, length));
                return false;
            }

            bool ICdpClientHandler.Payload(byte[] readBytes, int offset, int length)
            {
                Console.WriteLine("Payload From '{0}' ({1} Bytes): '{2}'", transmitter.RemoteEndPoint, length, Encoding.UTF8.GetString(readBytes, offset, length));
                return false;
            }

            public Boolean Close()
            {
                return false; // just halt
            }

            public void Halt()
            {
                Console.WriteLine("Halt");
            }
        }
    }
}
