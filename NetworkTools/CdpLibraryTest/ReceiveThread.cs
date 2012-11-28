using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Marler.NetworkTools
{

    public class ReceiveThread
    {
        public readonly String name;
        public readonly Int32 timeoutMillis;
        public readonly Byte[] receiveBuffer;
        readonly IConnectedDatagramTransmitter receiveTransmitter;

        Int64 stopwatchStartTicks;
        public ReceiveThread(String name, IConnectedDatagramTransmitter receiveTransmitter, Int32 timeoutMillis, byte[] receiveBuffer)
        {
            this.name = name;
            this.timeoutMillis = timeoutMillis;
            this.receiveBuffer = receiveBuffer;
            this.receiveTransmitter = receiveTransmitter;

            this.stopwatchStartTicks = 0;
        }
        public void SetStopwatchStartTicks(Int64 stopwatchStartTicks)
        {
            this.stopwatchStartTicks = stopwatchStartTicks;
        }
        public void ReceiveAndExpectTimeout()
        {
            try
            {
                Console.WriteLine("[{0} {1} millis] ReceiveBlocking, timeout is {2} milliseconds (Expecting TimeoutException)...",
                    name, (Stopwatch.GetTimestamp() - stopwatchStartTicks).StopwatchTicksAsInt64Milliseconds(), timeoutMillis);
                receiveTransmitter.ReceiveBlocking(receiveBuffer, 0, receiveBuffer.Length, timeoutMillis);
                Assert.Fail("Expected TimeoutException");
            }
            catch (TimeoutException)
            {
                Console.WriteLine("[{0} {1} millis] TimeOut Exception", name, (Stopwatch.GetTimestamp() - stopwatchStartTicks).StopwatchTicksAsInt64Milliseconds());
            }
        }
        public void ReceiveExpectSuccess(byte[] expectedReceiveBuffer)
        {
            Console.WriteLine("[{0} {1} millis] ReceiveBlocking, timeout is {2} milliseconds (Expecting Success)...",
                name, (Stopwatch.GetTimestamp() - stopwatchStartTicks).StopwatchTicksAsInt64Milliseconds(), timeoutMillis);
            Int32 bytesRead = receiveTransmitter.ReceiveBlocking(receiveBuffer, 0, receiveBuffer.Length, timeoutMillis);
            CdpTest.AssertEqual(expectedReceiveBuffer, receiveBuffer, bytesRead);
        }
    }
}
