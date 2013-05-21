using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Diagnostics;

using Marler.Common;

namespace Marler.Net
{
    [TestClass]
    public class VirtualDatagramTransmitterTests
    {
        [TestMethod]
        public void SimpleTestMethod()
        {
            Cdp.TryStaticInit(Cdp.MaxPayloadWithIDOverUdp);
            
            VirtualDatagramTransmitter transmitter = new VirtualDatagramTransmitter();
            transmitter.Print(Console.Out);


            byte[] sendBuffer = new Byte[] { 0, 1, 2, 3, 4 };
            byte[] receiveBuffer;
            Int32 length;

            //
            // Send Buffer
            //
            transmitter.Send(sendBuffer, 0, sendBuffer.Length);
            Assert.AreEqual(1, transmitter.DatagramsInSendQueue);

            //
            // Receive Buffer
            //
            receiveBuffer = new Byte[sendBuffer.Length];
            length = transmitter.otherTransmitter.ReceiveNonBlocking(receiveBuffer, 0, receiveBuffer.Length);
            Assert.AreEqual(sendBuffer.Length, length);
            Assert.AreEqual(0, transmitter.DatagramsInSendQueue);
            CdpTest.AssertEqual(sendBuffer, receiveBuffer);

            length = transmitter.otherTransmitter.ReceiveNonBlocking(receiveBuffer, 0, sendBuffer.Length);
            Assert.AreEqual(-1, length);

            //
            // Send the buffer 5 times
            //
            for (int i = 0; i < 5; i++)
            {
                sendBuffer[0] = (Byte)i;
                transmitter.Send(sendBuffer, 0, sendBuffer.Length);
                Assert.AreEqual(i + 1, transmitter.DatagramsInSendQueue);
            }
            transmitter.Print(Console.Out);
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine("Receiving datagram {0}", i + 1);
                receiveBuffer = new Byte[sendBuffer.Length];
                length = transmitter.otherTransmitter.ReceiveNonBlocking(receiveBuffer, 0, receiveBuffer.Length);
                Assert.AreEqual(sendBuffer.Length, length);
                Assert.AreEqual(4-i, transmitter.DatagramsInSendQueue);

                sendBuffer[0] = (Byte)i;
                CdpTest.AssertEqual(sendBuffer, receiveBuffer);
            }

            length = transmitter.otherTransmitter.ReceiveNonBlocking(receiveBuffer, 0, sendBuffer.Length);
            Assert.AreEqual(-1, length);
        }


        [TestMethod]
        public void TestTimeoutExceptionHappens()
        {
            Cdp.TryStaticInit(Cdp.MaxPayloadWithIDOverUdp);

            VirtualDatagramTransmitter transmitter = new VirtualDatagramTransmitter();
            transmitter.Print(Console.Out);

            Int32 sendLength = 32;
            Byte[] sendBuffer = new Byte[sendLength];
            for(int i = 0; i < sendLength; i++)
            {
                sendBuffer[i] = (Byte)i;
            }           
            byte[] receiveBuffer = new byte[sendLength];


            ReceiveThread receiver = new ReceiveThread("Receiver", transmitter.otherTransmitter, 0, receiveBuffer);

            Int64 startTicks = Stopwatch.GetTimestamp();
            receiver.SetStopwatchStartTicks(startTicks);


            new Thread(() =>
            {
                Int32 waitTime = 2000;
                Console.WriteLine("[Sender {0} millis] Waiting for {1} millis...",
                    (Stopwatch.GetTimestamp() - startTicks).StopwatchTicksAsInt64Milliseconds(), waitTime);
                Thread.Sleep(waitTime);
                Console.WriteLine("[Sender {0} millis] Done Waiting, Now Sending...", (Stopwatch.GetTimestamp() - startTicks).StopwatchTicksAsInt64Milliseconds());
                transmitter.Send(sendBuffer, 0, sendBuffer.Length);

            }).Start();

            receiver.ReceiveAndExpectTimeout();
        }

        [TestMethod]
        public void TestTimeoutDoesntHappen()
        {
            Cdp.TryStaticInit(Cdp.MaxPayloadWithIDOverUdp);

            Int32 sendLength = 32;

            VirtualDatagramTransmitter transmitter = new VirtualDatagramTransmitter();
            transmitter.Print(Console.Out);

            Byte[] sendBuffer = new Byte[sendLength];
            for (int i = 0; i < sendLength; i++)
            {
                sendBuffer[i] = (Byte)i;
            }


            byte[] receiveBuffer = new byte[sendLength];
            ReceiveThread receiver = new ReceiveThread("Receiver", transmitter.otherTransmitter, 400, receiveBuffer);

            Int64 startTicks = Stopwatch.GetTimestamp();
            receiver.SetStopwatchStartTicks(startTicks);


            new Thread(() =>
            {
                Int32 waitTime = 200;
                Console.WriteLine("[Sender {0} millis] Waiting for {1} millis...",
                    (Stopwatch.GetTimestamp() - startTicks).StopwatchTicksAsInt64Milliseconds(), waitTime);
                Thread.Sleep(waitTime);
                Console.WriteLine("[Sender {0} millis] Done Waiting, Now Sending...", (Stopwatch.GetTimestamp() - startTicks).StopwatchTicksAsInt64Milliseconds());
                transmitter.Send(sendBuffer, 0, sendBuffer.Length);

            }).Start();

            receiver.ReceiveExpectSuccess(sendBuffer);
        }

        [TestMethod]
        public void TwoReceiveBlockingThreads()
        {
            Cdp.TryStaticInit(Cdp.MaxPayloadWithIDOverUdp);

            Int32 sendLength = 32;

            VirtualDatagramTransmitter transmitter = new VirtualDatagramTransmitter();
            transmitter.Print(Console.Out);

            Byte[] sendBuffer = new Byte[sendLength];
            for (int i = 0; i < sendLength; i++)
            {
                sendBuffer[i] = (Byte)i;
            }


            byte[] receiveBuffer1 = new byte[sendLength];
            byte[] receiveBuffer2 = new byte[sendLength];
            ReceiveThread backgroundReceiver = new ReceiveThread("BackgroundReceiver", transmitter.otherTransmitter, 400, receiveBuffer1);
            ReceiveThread mainReceiver = new ReceiveThread("MainReceiver", transmitter.otherTransmitter, 600, receiveBuffer2);

            Int64 startTicks = Stopwatch.GetTimestamp();
            backgroundReceiver.SetStopwatchStartTicks(startTicks);
            mainReceiver.SetStopwatchStartTicks(startTicks);

            new Thread(() =>
            {
                Int32 waitTime = 200;
                Console.WriteLine("[Sender {0} millis] Waiting for {1} millis...",
                    (Stopwatch.GetTimestamp() - startTicks).StopwatchTicksAsInt64Milliseconds(), waitTime);
                Thread.Sleep(waitTime);
                Console.WriteLine("[Sender {0} millis] Done Waiting, Now Sending...", (Stopwatch.GetTimestamp() - startTicks).StopwatchTicksAsInt64Milliseconds());
                transmitter.Send(sendBuffer, 0, sendBuffer.Length);

            }).Start();
            new Thread(backgroundReceiver.ReceiveAndExpectTimeout).Start();

            mainReceiver.ReceiveExpectSuccess(sendBuffer);

            Thread.Sleep(500);
        }
    }
}
