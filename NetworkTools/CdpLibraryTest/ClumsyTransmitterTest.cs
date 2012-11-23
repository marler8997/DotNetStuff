using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading;

namespace Marler.NetworkTools
{
    [TestClass]
    public class ClumsyTransmitterTest
    {
        [TestMethod]
        public void TestTimeoutHappendBecauseOfDroppedPacket()
        {
            Cdp.TryStaticInit(Cdp.MaxPayloadWithIDOverUdp);

            VirtualDatagramTransmitter virtualTransmitter = new VirtualDatagramTransmitter();
            ClumsyTransmitter clumsyTransmitter = new ClumsyTransmitter(virtualTransmitter, Console.Out);
            virtualTransmitter.Print(Console.Out);

            Int32 sendLength = 32;            
            Byte[] sendBuffer = new Byte[sendLength];
            for(int i = 0; i < sendLength; i++)
            {
                sendBuffer[i] = (Byte)i;
            }
            
            byte[] receiveBuffer = new byte[sendLength];
            ReceiveThread receiver = new ReceiveThread("Receiver", virtualTransmitter.otherTransmitter, 200, receiveBuffer);

            Int64 startTicks = Stopwatch.GetTimestamp();
            receiver.SetStopwatchStartTicks(startTicks);

            new Thread(() =>
            {
                Console.WriteLine("[Sender {0} millis] Setting clumsy dropper..", (Stopwatch.GetTimestamp() - startTicks).StopwatchTicksAsInt64Milliseconds());
                clumsyTransmitter.DropAllSentDatagramsForTheNext(200);

                Console.WriteLine("[Sender {0} millis] Sending...", (Stopwatch.GetTimestamp() - startTicks).StopwatchTicksAsInt64Milliseconds());
                clumsyTransmitter.Send(sendBuffer, 0, sendBuffer.Length);
                Console.WriteLine("[Sender {0} millis] Done Sending...", (Stopwatch.GetTimestamp() - startTicks).StopwatchTicksAsInt64Milliseconds());

            }).Start();

            receiver.ReceiveAndExpectTimeout();

        }
    }
}
