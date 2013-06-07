using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

using More;

namespace More.Net
{
    [TestClass]
    public class RegressionTests
    {
        const Int32 testUdpPort = 12345;


        [TestMethod]
        public void TestMethod1()
        {
            Cdp.TryStaticInit(Cdp.MaxPayloadWithIDOverUdp);

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback, testUdpPort);
            FixedRetryTimeout timeout = new FixedRetryTimeout(500, 6);

            using (TestServerWrapper server = new TestServerWrapper(serverEndPoint, timeout))
            {

                UdpConnectedClientTransmitter udpTransmitter = new UdpConnectedClientTransmitter(serverEndPoint);
                CdpTransmitter transmitter = new CdpTransmitter(udpTransmitter);


                Int32 offset;
                Byte[] myMessage;
                Byte[] payloadBuffer;


                //
                // Send and wait for ack
                //
                myMessage = Encoding.UTF8.GetBytes("This should be a normal payload with an immediate ack");

                payloadBuffer = transmitter.RequestSendBuffer(myMessage.Length, out offset);
                Array.Copy(myMessage, 0, payloadBuffer, offset, myMessage.Length);
                offset += myMessage.Length;
                Console.WriteLine("[Client] Sending Payload With Ack...");
                transmitter.ControllerSendPayloadWithAck(offset, timeout);

                //
                // Send heartbeat
                //
                Console.WriteLine("[Client] Sending Heartbeat...");
                transmitter.SendHearbeat();

                //
                // Send Random Payload
                //
                myMessage = Encoding.UTF8.GetBytes("This should be a random payload");

                payloadBuffer = transmitter.RequestSendBuffer(myMessage.Length, out offset);
                Array.Copy(myMessage, 0, payloadBuffer, offset, myMessage.Length);
                offset += myMessage.Length;
                Console.WriteLine("[Client] Sending Random Payload...");
                transmitter.ControllerSendRandomPayload(offset);

                //
                // Send Payload no ack
                //
                myMessage = Encoding.UTF8.GetBytes("This should be the first payload with no immediate ack");

                payloadBuffer = transmitter.RequestSendBuffer(myMessage.Length, out offset);
                Array.Copy(myMessage, 0, payloadBuffer, offset, myMessage.Length);
                offset += myMessage.Length;
                Console.WriteLine("[Client] Sending Payload No Ack...");
                transmitter.ControllerSendPayloadNoAck(offset);

                //
                // Send Payload no ack
                //
                myMessage = Encoding.UTF8.GetBytes("This should be the second payload with no immediate ack");

                payloadBuffer = transmitter.RequestSendBuffer(myMessage.Length, out offset);
                Array.Copy(myMessage, 0, payloadBuffer, offset, myMessage.Length);
                offset += myMessage.Length;
                Console.WriteLine("[Client] Sending Payload No Ack...");
                transmitter.ControllerSendPayloadNoAck(offset);

                //
                // Send and wait for ack
                //
                myMessage = Encoding.UTF8.GetBytes("This should be a normal payload with an immediate ack");

                payloadBuffer = transmitter.RequestSendBuffer(myMessage.Length, out offset);
                Array.Copy(myMessage, 0, payloadBuffer, offset, myMessage.Length);
                offset += myMessage.Length;
                Console.WriteLine("[Client] Sending Payload With Ack...");
                transmitter.ControllerSendPayloadWithAck(offset, timeout);

                //
                // Send Halt
                //
                Console.WriteLine("[Client] Sending Halt...");
                transmitter.SendHaltNoPayload();

                server.TestSucceeded();
                Console.WriteLine("[Client] Done (Success)");
            }
        }

        [TestMethod]
        public void TestResendsAfterLosingAcks()
        {
            Cdp.TryStaticInit(Cdp.MaxPayloadWithIDOverUdp);

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback, testUdpPort);
            FixedRetryTimeout timeout = new FixedRetryTimeout(100, 6);

            using (TestServerWrapper server = new TestServerWrapper(serverEndPoint, timeout))
            {
                Cdp.TryStaticInit(Cdp.MaxPayloadWithIDOverUdp);

                //
                //
                //
                UdpConnectedClientTransmitter udpTransmitter = new UdpConnectedClientTransmitter(serverEndPoint);
                ClumsyTransmitter clumsyTransmitter = new ClumsyTransmitter(udpTransmitter, Console.Out);
                CdpTransmitter transmitter = new CdpTransmitter(clumsyTransmitter);

                Int64 startTicks = Stopwatch.GetTimestamp();
                Console.WriteLine("[Sender {0} millis] Sending...", (Stopwatch.GetTimestamp() - startTicks).StopwatchTicksAsInt64Milliseconds());

                Int32 offset;
                Byte[] datagram = transmitter.RequestSendBuffer(10, out offset);
                for (Byte i = 0; i < 10; i++)
                {
                    datagram[offset++] = (Byte)('A' + i);
                }
                clumsyTransmitter.DropAllReceivedDatagramsForTheNext(400);
                transmitter.ControllerSendPayloadWithAck(offset, timeout);
                server.TestSucceeded();
            }
        }

        [TestMethod]
        public void TestOutOfOrder()
        {
            Cdp.TryStaticInit(Cdp.MaxPayloadWithIDOverUdp);

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback, testUdpPort);
            FixedRetryTimeout timeout = new FixedRetryTimeout(100, 6);

            using (TestServerWrapper server = new TestServerWrapper(serverEndPoint, timeout))
            {
                Cdp.TryStaticInit(Cdp.MaxPayloadWithIDOverUdp);

                //
                //
                //
                UdpConnectedClientTransmitter udpTransmitter = new UdpConnectedClientTransmitter(serverEndPoint);
                ClumsyTransmitter clumsyTransmitter = new ClumsyTransmitter(udpTransmitter, Console.Out);
                CdpTransmitter transmitter = new CdpTransmitter(clumsyTransmitter);

                Int64 startTicks = Stopwatch.GetTimestamp();
                Console.WriteLine("[Sender {0} millis] Sending...", (Stopwatch.GetTimestamp() - startTicks).StopwatchTicksAsInt64Milliseconds());

                clumsyTransmitter.DropAllSentDatagramsForTheNext(200);

                for (int i = 0; i < 3; i++)
                {
                    Int32 offset;
                    Byte[] datagram = transmitter.RequestSendBuffer(10, out offset);
                    for (Byte j = 0; j < 10; j++)
                    {
                        datagram[offset++] = (Byte)('A' + j);
                    }
                    transmitter.ControllerSendPayloadNoAck(offset);
                }

                Thread.Sleep(200);
                {
                    Int32 offset;
                    Byte[] datagram = transmitter.RequestSendBuffer(10, out offset);
                    for (Byte j = 0; j < 10; j++)
                    {
                        datagram[offset++] = (Byte)('A' + j);
                    }
                    transmitter.ControllerSendPayloadWithAck(offset, timeout);
                }

                server.TestSucceeded();
            }

        }




        public class TestServerWrapper : IDisposable
        {
            Boolean success;
            CdpServerOverUdp server;

            public TestServerWrapper(IPEndPoint serverEndPoint, ICdpTimeout timeout)
            {
                this.success = false;
                this.server = null;

                CdpServerForTests cdpServer = new CdpServerForTests();
                server = new CdpServerOverUdp(serverEndPoint);

                new Thread(() =>
                {
                    try
                    {
                        server.Run(cdpServer, new Byte[256], timeout);
                    }
                    catch (Exception e)
                    {
                        if (!success) Console.WriteLine("[Server] '{0}': {1}", e.GetType().Name, e);
                    }
                    finally
                    {
                        Console.WriteLine("[Server] Stopped");
                    }
                }).Start();
                Thread.Sleep(100); // wait for server to start
            }
            public void TestSucceeded()
            {
                this.success = true;
            }
            public void Dispose()
            {
                if (server != null) server.udpSocket.Close();
            }
        }

        public class FixedRetryTimeout : ICdpTimeout
        {
            public readonly Int32 retry, maxRetries;
            public FixedRetryTimeout(Int32 retry, Int32 maxRetries)
            {
                this.retry = retry;
                this.maxRetries = maxRetries;
            }
            public int WaitForAckInitialRetryTimeout(int averageLatency)
            {
                return retry;
            }
            public int WaitForAckRetryOrTimeout(int retries, int averageLatency, int elapsedMillis, int lastWaitForAckTimeout)
            {
                return (retries < maxRetries) ? retry : 0;
            }
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
                Console.WriteLine("[Server] Heartbeat from unknown client '{0}'", endPoint);
                return false;
            }

            public Boolean NewConnection(CdpTransmitter transmitter, out ICdpServerHandler handler, out int maxSendBeforeAck)
            {
                connectionCount++;

                Console.WriteLine("[Server] New Connection from '{0}' ({1} connections)", transmitter.RemoteEndPoint, connectionCount);

                handler = new CdpServerHandlerForTests(transmitter);
                maxSendBeforeAck = 0xFF;
                return false;
            }

            public void ConnectionClosed(EndPoint endPoint)
            {
                connectionCount--;
                Console.WriteLine("[Server] Close Connection ({0} connections)", connectionCount);
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
                Console.WriteLine("[Server] Random Payload From '{0}' ({1} Bytes): '{2}'", transmitter.RemoteEndPoint, length, Encoding.UTF8.GetString(readBytes, offset, length));
                return false;
            }

            bool ICdpClientHandler.Payload(byte[] readBytes, int offset, int length)
            {
                Console.WriteLine("[Server] Payload From '{0}' ({1} Bytes): '{2}'", transmitter.RemoteEndPoint, length, Encoding.UTF8.GetString(readBytes, offset, length));
                return false;
            }

            public Boolean Close()
            {
                return false; // just halt
            }

            public void Halt()
            {
                Console.WriteLine("[Server] Halt");
            }
        }
    }
}
