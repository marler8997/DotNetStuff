using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.Net
{

    public class CtpServer : ICdpServer, ICdpServerHandler
    {
        readonly Dictionary<CdpTransmitter, CtpRequest> clients;

        public CtpServer()
        {
            this.clients = new Dictionary<CdpTransmitter, CtpRequest>();
        }



        public bool NewConnection(CdpTransmitter transmitter, out ICdpServerHandler handler, out int maxSendBeforeAck)
        {
            handler = this;
            maxSendBeforeAck = 32;
            return false;
        }
        public void ConnectionClosed(System.Net.EndPoint endPoint)
        {
        }
        public bool SocketException(System.Net.Sockets.SocketException e)
        {
            return false;
        }
        public bool HeartbeatFromUnknown(System.Net.EndPoint endPoint)
        {
            return false;
        }



        public bool RandomPayload(byte[] readBytes, int offset, int length)
        {
            throw new InvalidOperationException("Random payloads are no supported by the CTP protocol");
        }
        public bool Payload(byte[] readBytes, int offset, int length)
        {
            throw new NotImplementedException();
        }
        public ServerInstruction GotControl(CdpTransmitter transmitter, out int sendBufferOffsetLimit, out bool requestImmediateAck)
        {
            CtpRequest request;
            if(!clients.TryGetValue(transmitter, out request))
                throw new InvalidOperationException(String.Format("Got control from client '{0}' without an entry in the dictionary", transmitter.RemoteEndPoint));



            CtpResponse response = new CtpResponse();
            response.flags      = 0;
            response.encoding   = CtpEncoding.None;
            response.setCookies = null;




            if(request.resources != null)
            {
                response.resources = new Resource[request.resources.Length];
            }



            //transmitter.RequestSendBuffer(




            sendBufferOffsetLimit = 0;
            requestImmediateAck = false;
            return ServerInstruction.NoInstruction;
        }
        public Boolean Close() { return false; }
        public void Halt() { }
    }
}
