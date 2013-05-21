using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace Marler.Net
{
    public class CdpCatProgram
    {
        static Int32 Main(string[] args)
        {
            CdpCatOptions optionsParser = new CdpCatOptions();

            if (args.Length <= 0)
            {
                optionsParser.PrintUsage();
                return -1;
            }

            List<String> nonOptionArgs = optionsParser.Parse(args);

            Cdp.StaticInit(optionsParser.maxPayload.ArgValue);

            ICdpTimeout timeout = new MyCdpTimeout();

            if (optionsParser.listenPort.set)
            {
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, optionsParser.listenPort.ArgValue);
                MyCdpServer cdpServer = new MyCdpServer();
                Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                Cdp.UdpServerLoop(cdpServer, udpSocket, localEndPoint, new Byte[optionsParser.maxPayload.ArgValue], timeout);
                return -1;
            }
            else
            {

                if (nonOptionArgs.Count < 2)
                {
                    Console.WriteLine("Missing command line arguments");
                    optionsParser.PrintUsage();
                    return -1;
                }

                UInt16 port = UInt16.Parse(nonOptionArgs[1]);
                EndPoint serverEndPoint = EndPoints.EndPointFromIPOrHost(nonOptionArgs[0], port);

                UdpConnectedClientTransmitter udpTransmitter = new UdpConnectedClientTransmitter(serverEndPoint);
                CdpTransmitter transmitter = new CdpTransmitter(udpTransmitter);


                Int32 offset;
                Byte[] myMessage;
                Byte[] payloadBuffer;



                transmitter.SendHearbeat();



                myMessage = Encoding.UTF8.GetBytes("This should be a random payload");

                payloadBuffer = transmitter.RequestSendBuffer(myMessage.Length, out offset);
                Array.Copy(myMessage, 0, payloadBuffer, offset, myMessage.Length);
                transmitter.ControllerSendRandomPayload(offset + myMessage.Length);




                myMessage = Encoding.UTF8.GetBytes("This should be the first payload with no immediate ack");

                payloadBuffer = transmitter.RequestSendBuffer(myMessage.Length, out offset);
                Array.Copy(myMessage, 0, payloadBuffer, offset, myMessage.Length);
                transmitter.ControllerSendPayloadNoAck(offset + myMessage.Length);

                myMessage = Encoding.UTF8.GetBytes("This should be the second payload with no immediate ack");

                payloadBuffer = transmitter.RequestSendBuffer(myMessage.Length, out offset);
                Array.Copy(myMessage, 0, payloadBuffer, offset, myMessage.Length);
                transmitter.ControllerSendPayloadNoAck(offset + myMessage.Length);


                //
                // Send and wait for ack
                //
                myMessage = Encoding.UTF8.GetBytes("This should be a normal payload with an immediate ack");

                payloadBuffer = transmitter.RequestSendBuffer(myMessage.Length, out offset);
                Array.Copy(myMessage, 0, payloadBuffer, offset, myMessage.Length);
                offset += myMessage.Length;
                transmitter.ControllerSendPayloadWithAck(offset, timeout);




                transmitter.SendHaltNoPayload();


                return 0;
            }
        }
    }

    public class MyCdpTimeout : ICdpTimeout
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

    public class MyCdpServer : ICdpServer
    {
        private Int32 connectionCount;

        public MyCdpServer()
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

            handler = new MyCdpPayloadHandler(transmitter);
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

    public class MyCdpPayloadHandler : ICdpServerHandler
    {
        public readonly CdpTransmitter transmitter;
        public MyCdpPayloadHandler(CdpTransmitter transmitter)
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
